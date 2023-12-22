using App.ViewModels;
using App.Views;
using Avalonia.Controls;
using Core.Commons;
using Core.Helpers;
using MQTTnet;
using MQTTnet.Client;

namespace App.ViewManager
{
    public class CPUViewManage : ISensorManage
    {
        //与其他监控项的排序
        public int Sort => 1;
        public CPUViewManage(string cpuName, string title)
        {
            Name = cpuName;
            Title = title;
            View = new CPUView() { Margin = new Avalonia.Thickness(2) };
            if (View.DataContext is CPUViewModel vm)
            {
                vm.CpuName = title;
            }
        }

        public UserControl View { get; set; }
        public string Name { get; set; }
        public string Title { get; }

        public void Update(MqttApplicationMessageReceivedEventArgs args)
        {
            if (args == null || args.ApplicationMessage == null)
                return;
            if (View.DataContext is CPUViewModel vm)
            {
                if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType.Cpu}/{Name}/{SensorType.Load}/CPU Total")))
                {
                    vm.CpuUsage = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
                else if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType.Cpu}/{Name}/{SensorType.Temperature}/CPU Package")))
                {
                    vm.Temperature = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
                else if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType.Cpu}/{Name}/{SensorType.Clock}/Total")))
                {
                    vm.CpuFrequency = float.Parse(args.ApplicationMessage.ConvertPayloadToString()) / 1000;
                }

            }
        }

        public void SubscribeTopic(IMqttClient client)
        {
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Cpu}/{Name}/{SensorType.Load}/CPU Total"));
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Cpu}/{Name}/{SensorType.Temperature}/CPU Package"));
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Cpu}/{Name}/{SensorType.Clock}/Total"));
        }
    }
}
