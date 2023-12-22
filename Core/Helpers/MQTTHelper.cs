namespace Core.Helpers
{
    public class MQTTHelper
    {
        public static string FormatPublishTopic(string topic)
        {
            return topic.Replace("#", "_").Replace("+", "_");
        }
        public static string FormatPublishTopic(string hardwareType, string hardwareName, string sensorType, string sensorName)
        {
            return FormatPublishTopic($"{hardwareType}/{hardwareName}/{sensorType}/{sensorName}");
        }
    }
}
