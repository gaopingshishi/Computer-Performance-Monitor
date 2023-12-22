using Topshelf;

namespace WindowsService1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<Service1>(s =>
                {
                    s.ConstructUsing(name => new Service1());
                    s.WhenStarted(tc => tc.OnStart());
                    s.WhenStopped(tc => tc.OnStop());
                });
                x.RunAsLocalSystem();//本地系统账号运行
                x.SetDescription("本服务负责定时获取系统数据，并创建mqtt服务端。");
                x.SetDisplayName("GT_获取系统数据");
                x.SetServiceName("GT_SystemDataByMqtt");
                x.StartAutomatically();
            });
        }
    }
}
