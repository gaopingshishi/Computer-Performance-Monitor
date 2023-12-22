using Avalonia.Controls;
using MQTTnet.Client;

namespace App.ViewManager
{
    public interface ISensorManage
    {
        public string Name { get; set; }
        public int Sort { get; }
        public UserControl View { get; set; }
        public void Update(MqttApplicationMessageReceivedEventArgs args);
        public void SubscribeTopic(IMqttClient client);
    }
}
