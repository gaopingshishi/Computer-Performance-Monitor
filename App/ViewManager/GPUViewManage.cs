using App.ViewModels;
using App.Views;
using Avalonia.Controls;
using Core.Commons;
using Core.Helpers;
using MQTTnet;
using MQTTnet.Client;

namespace App.ViewManager
{
    public class GPUViewManage : ISensorManage
    {
        //与其他监控项的排序
        public int Sort => 2;
        public GPUViewManage(string gpuName, HardwareType hardwareType, string title)
        {
            Name = gpuName;
            HardwareType = hardwareType;
            Title = title;
            View = new GPUView() { Margin = new Avalonia.Thickness(2) };
            if (View.DataContext is GPUViewModel vm)
            {
                vm.GpuName = title;
            }
        }

        public UserControl View { get; set; }
        public string Name { get; set; }
        public HardwareType HardwareType { get; }
        public string Title { get; }

        public void Update(MqttApplicationMessageReceivedEventArgs args)
        {
            if (args == null || args.ApplicationMessage == null)
                return;
            if (View.DataContext is GPUViewModel vm)
            {
                if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Load}/GPU Core")))
                {
                    vm.GpuUsage = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
                else if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Temperature}/GPU Core")))
                {
                    vm.Temperature = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
                else if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Power}/GPU Package")))
                {
                    vm.PowerWaste = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
                else if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Load}/GPU Memory")))
                {
                    vm.GPUMemory = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
            }
        }

        public void SubscribeTopic(IMqttClient client)
        {
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Load}/GPU Core"));
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Temperature}/GPU Core"));
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Power}/GPU Package"));
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType}/{Name}/{SensorType.Load}/GPU Memory"));
        }
    }
}
