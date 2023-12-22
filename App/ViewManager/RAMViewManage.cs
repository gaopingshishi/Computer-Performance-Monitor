using App.ViewModels;
using App.Views;
using Avalonia.Controls;
using Core.Commons;
using Core.Helpers;
using MQTTnet;
using MQTTnet.Client;

namespace App.ViewManager
{
    public class RAMViewManage : ISensorManage
    {
        //与其他监控项的排序
        public int Sort => 3;
        public RAMViewManage(string RAMName, string title)
        {
            Name = RAMName;
            View = new RAMView() { Margin = new Avalonia.Thickness(2) };
            if (View.DataContext is RAMViewModel vm)
            {
                vm.RAMName = title;
            }
        }

        public UserControl View { get; set; }
        public string Name { get; set; }

        public void Update(MqttApplicationMessageReceivedEventArgs args)
        {
            if (args == null || args.ApplicationMessage == null)
                return;
            if (View.DataContext is RAMViewModel vm)
            {
                if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType.Memory}/{Name}/{SensorType.Load}/Memory")))
                {
                    vm.RAMUsage = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
                else if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType.Memory}/{Name}/{SensorType.Data}/Memory Used")))
                {
                    vm.Used = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
                else if (args.ApplicationMessage.Topic.Contains(MQTTHelper.FormatPublishTopic($"{HardwareType.Memory}/{Name}/{SensorType.Data}/Total")))
                {
                    vm.Total = float.Parse(args.ApplicationMessage.ConvertPayloadToString());
                }
            }
        }

        public void SubscribeTopic(IMqttClient client)
        {
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Memory}/{Name}/{SensorType.Load}/Memory"));
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Memory}/{Name}/{SensorType.Data}/Memory Used"));
            client.SubscribeAsync(MQTTHelper.FormatPublishTopic($"{HardwareType.Memory}/{Name}/{SensorType.Data}/Total"));
        }
    }
}
