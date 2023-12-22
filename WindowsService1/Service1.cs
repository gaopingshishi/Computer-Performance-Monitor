using Core.Helpers;
using LibreHardwareMonitor.Hardware;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsService1
{
    public class Service1
    {
        const string aesKey = "gaotian123456789";
        const string StrRequestTopicName = "broker";
        readonly string StrSplic = ASCIIEncoding.ASCII.GetString(new byte[] { 0x00 });

        Computer computer = null;
        MqttServer mqttServer = null;
        IMqttClient mqttClient = null;
        Timer timer = null;//定时刷新
        UDPHelper udp = new UDPHelper();

        //CPU,"i9-13900KF",Load,"CPU Total","4.1"
        Dictionary<HardwareType, Dictionary<string, Dictionary<SensorType, Dictionary<string, string>>>> dicSensor =
            new Dictionary<HardwareType, Dictionary<string, Dictionary<SensorType, Dictionary<string, string>>>>();
        public Service1()
        {

        }

        public void OnStart()
        {
            //启动mqtt服务
            var optionsBuilderServer = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(1883);
            var options = optionsBuilderServer.Build();
            mqttServer = new MqttFactory().CreateMqttServer(options);
            mqttServer.StartAsync().Wait();

            // 创建MQTT客户端实例
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1", 1883)
                .Build();
            mqttClient = new MqttFactory().CreateMqttClient();
            mqttClient.ConnectAsync(mqttClientOptions).Wait();
            mqttClient.SubscribeAsync(StrRequestTopicName).Wait();
            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            //初始化监控
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true
            };
            computer.Open();
            computer.Accept(new UpdateVisitor());
            //定时获取硬件数据
            timer = new Timer(new TimerCallback(o =>
            {
                DateTime startTime = DateTime.Now;
                TimerMethod(o);
                DateTime endTime = DateTime.Now;
                Console.WriteLine($"耗时：{(endTime - startTime).TotalMilliseconds}ms");
            }), null, 3000, 1000);

        }

        public void OnStop()
        {
            timer.Dispose();
            mqttClient.ApplicationMessageReceivedAsync -= MqttClient_ApplicationMessageReceivedAsync;
            mqttClient.DisconnectAsync().Wait();
            mqttClient.Dispose();
            mqttServer.StopAsync().Wait();
            mqttServer.Dispose();

        }

        private void TimerMethod(object state)
        {
            #region 广播本机IP
            string message = GetPCMessage();
            message = AESHelper.Encrypt(message, aesKey);
            udp.Send(message);
            #endregion
            if (!mqttClient.IsConnected)
                return;
            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();
                if (!dicSensor.ContainsKey(hardware.HardwareType))
                {
                    dicSensor[hardware.HardwareType] = new Dictionary<string, Dictionary<SensorType, Dictionary<string, string>>>() { };
                }
                if (!dicSensor[hardware.HardwareType].ContainsKey(hardware.Name))
                {
                    dicSensor[hardware.HardwareType][hardware.Name] = new Dictionary<SensorType, Dictionary<string, string>>() { };
                }
                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        if (!dicSensor[hardware.HardwareType][hardware.Name].ContainsKey(sensor.SensorType))
                        {
                            dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType] = new Dictionary<string, string>();
                        }
                        if (!dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType].ContainsKey(sensor.Name))
                        {
                            dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType][sensor.Name] = sensor.Value?.ToString();
                        }
                        //float.TryParse(dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType][sensor.Name], out var oldValue);
                        //float.TryParse(sensor.Value?.ToString(), out var value);
                        //if (oldValue != value)//数据变化或者请求数据
                        //{
                        dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType][sensor.Name] = sensor.Value?.ToString();
                        mqttClient.PublishStringAsync(MQTTHelper.FormatPublishTopic($"{hardware.HardwareType}/{hardware.Name}/{sensor.SensorType}/{sensor.Name}"), sensor.Value?.ToString());
                        //}
                    }
                }
                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (!dicSensor[hardware.HardwareType][hardware.Name].ContainsKey(sensor.SensorType))
                    {
                        dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType] = new Dictionary<string, string>();
                    }
                    if (!dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType].ContainsKey(sensor.Name))
                    {
                        dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType][sensor.Name] = sensor.Value?.ToString();
                    }
                    //float.TryParse(dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType][sensor.Name], out var oldValue);
                    //float.TryParse(sensor.Value?.ToString(), out var value);
                    //if (oldValue != value)//数据变化或者请求数据
                    //{
                    dicSensor[hardware.HardwareType][hardware.Name][sensor.SensorType][sensor.Name] = sensor.Value?.ToString();
                    mqttClient.PublishStringAsync(MQTTHelper.FormatPublishTopic($"{hardware.HardwareType}/{hardware.Name}/{sensor.SensorType}/{sensor.Name}"), sensor.Value?.ToString());
                    //}
                }
            }

            //个性化定制数据
            #region CPU频率
            if (dicSensor.ContainsKey(HardwareType.Cpu))
            {
                dicSensor.TryGetValue(HardwareType.Cpu, out var dicName);
                foreach (var name in dicName)
                {
                    //CPU里只有1个Clock对象,但是有多个内核，每个内核都有自己的频率，所以要计算平均频率，首先要排基准频率
                    var clocks = name.Value.Where(x => x.Key == SensorType.Clock).FirstOrDefault().Value.Where(x => x.Key != "Bus Speed");
                    float total = clocks.Average(x => float.Parse(x.Value));
                    mqttClient.PublishStringAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Cpu}/{name.Key}/{SensorType.Clock}/Total"), total.ToString());
                }
            }
            #endregion
            #region 总内存
            if (dicSensor.TryGetValue(HardwareType.Memory, out var hardwareType))
            {
                if (hardwareType.TryGetValue("Generic Memory", out var name))
                {
                    if (name.TryGetValue(SensorType.Data, out var sensorType))
                    {
                        float used = 0f;
                        float available = 0f;
                        if (sensorType.TryGetValue("Memory Used", out var memoryUsed))
                        {
                            float.TryParse(memoryUsed, out used);
                        }
                        if (sensorType.TryGetValue("Memory Available", out var memoryAvailable))
                        {
                            float.TryParse(memoryAvailable, out available);
                        }
                        float total = used + available;
                        mqttClient.PublishStringAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Memory}/Generic Memory/{SensorType.Data}/Total"), total.ToString());
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            //接收到请求，返回所有的topic名称
            if (args.ApplicationMessage.Topic == StrRequestTopicName && args.ApplicationMessage.ConvertPayloadToString().ToLower() == "request")
            {
                TimerMethod(null);//强制刷新一次数据
                string strTopics = JsonConvert.SerializeObject(dicSensor);
                mqttClient.PublishStringAsync(MQTTHelper.FormatPublishTopic("broker/subscriptions"), strTopics);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取电脑描述信息
        /// </summary>
        /// <returns></returns>
        private string GetPCMessage()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

            // 获取本机的 IPv4 地址
            IPAddress ipv4 = Array.Find(ipEntry.AddressList,
                a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            return $"{hostName};{ipv4}";
        }
    }
}
