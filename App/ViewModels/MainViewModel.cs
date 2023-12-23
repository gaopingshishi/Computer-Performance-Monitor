using App.ViewManager;
using Avalonia.Threading;
using Core.Commons;
using Core.Helpers;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    const string aesKey = "gaotian123456789";
    public Action<bool>? ConnectionChangeAction { get; set; }

    /// <summary>
    /// 界面上的监视管理器
    /// </summary>
    public readonly List<ISensorManage> ListSensorManage = new List<ISensorManage>();
    /// <summary>
    /// 当前设备有哪些硬件
    /// </summary>
    Dictionary<HardwareType, Dictionary<string, Dictionary<SensorType, Dictionary<string, string>>>>? dicSensor = null;

    private IMqttClient mqttClient;
    MqttClientOptions? mqttOptions = null;
    UDPHelper udpHelper = new UDPHelper();

    private bool currentWorkStatus;
    /// <summary>
    /// 当前连接状态
    /// </summary>
    public bool CurrentWorkStatus
    {
        get { return currentWorkStatus; }
        set { 
            SetProperty(ref currentWorkStatus, value);
            ConnectionChangeAction?.Invoke(value);
        }
    }

    public MainViewModel()
    {
        var factory = new MqttFactory();
        mqttClient = factory.CreateMqttClient();
    }


    /// <summary>
    /// 软件初始化
    /// </summary>
    public void Start()
    {
        ConnectMqttServer();
    }

    /// <summary>
    /// 软件推出
    /// </summary>
    public void Stop()
    {
        DisconnectMqttServer();
    }

    /// <summary>
    /// 连接MQTT服务器
    /// </summary>
    private void ConnectMqttServer()
    {
        if (!CurrentWorkStatus)
        {
            CurrentWorkStatus = false;
            while (!mqttClient.IsConnected)
            {
                try
                {
                    var pcMessage = udpHelper.Receive();
                    if (!string.IsNullOrEmpty(pcMessage))
                    {
                        pcMessage = AESHelper.Decrypt(pcMessage, aesKey);
                        var strs = pcMessage.Split(";");
                        if (strs.Length == 2)
                        {
                            mqttOptions = new MqttClientOptionsBuilder()
                                 .WithTcpServer(strs[1], 1883)
                                 .Build();
                            mqttClient.ConnectAsync(mqttOptions).Wait();
                        }
                    }
                    Task.Delay(1000).Wait();
                }
                catch (Exception ex)
                {

                }
            }
            mqttClient.DisconnectedAsync += MqttClient_Disconnected;
            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedBrokerAsync;
            CurrentWorkStatus = true;
        }
    }

    /// <summary>
    /// 断开MQTT服务器
    /// </summary>
    private void DisconnectMqttServer()
    {
        if (CurrentWorkStatus)
        {
            mqttClient.DisconnectedAsync -= MqttClient_Disconnected;
            mqttClient.ApplicationMessageReceivedAsync -= MqttClient_ApplicationMessageReceivedAsync;
            mqttClient.ApplicationMessageReceivedAsync -= MqttClient_ApplicationMessageReceivedBrokerAsync;
            mqttClient.DisconnectAsync().Wait();
            CurrentWorkStatus = false;
        }
    }

    private Task MqttClient_Disconnected(MqttClientDisconnectedEventArgs args)
    {
        if (CurrentWorkStatus)
        {
            DisconnectMqttServer();
            ConnectMqttServer();
            SubscribeTopic();
        }
        return Task.CompletedTask;
    }


    public void SubscribeTopic()
    {
        foreach (var manage in ListSensorManage)
        {
            manage.SubscribeTopic(mqttClient);
        }
    }
    /// <summary>
    /// Mqtt消息接收事件
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var manage in ListSensorManage)
            {
                manage.Update(args);
            }
        });
        return Task.CompletedTask;
    }
    /// <summary>
    /// Mqtt消息接收事件
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private Task MqttClient_ApplicationMessageReceivedBrokerAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        if (args.ApplicationMessage.Topic == "broker/subscriptions")
        {
            string value = args.ApplicationMessage.ConvertPayloadToString();
            dicSensor = JsonConvert.DeserializeObject<Dictionary<HardwareType, Dictionary<string, Dictionary<SensorType, Dictionary<string, string>>>>>(value);
        }
        return Task.CompletedTask;
    }
    /// <summary>
    /// 获取电脑硬件信息
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public Dictionary<HardwareType, Dictionary<string, Dictionary<SensorType, Dictionary<string, string>>>>? GetPCSensors(TimeSpan? timeout = null)
    {
        dicSensor?.Clear();
        mqttClient.PublishStringAsync(MQTTHelper.FormatPublishTopic("broker"), "request").Wait();
        mqttClient.SubscribeAsync("broker/subscriptions").Wait();//订阅获取所有主题的消息
        DateTime startTime = DateTime.Now;
        while (dicSensor == null || dicSensor.Count == 0)
        {
            if (timeout.HasValue && DateTime.Now - startTime > timeout)
                break;
            Task.Delay(300).Wait();
        }
        return dicSensor;

    }
}
