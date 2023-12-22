namespace App.ViewModels
{
    public class CPUViewModel : ViewModelBase
    {
        private string cpuName = "未知";
        /// <summary>
        /// CPU名称
        /// </summary>
        public string CpuName
        {
            get { return cpuName; }
            set { SetProperty(ref cpuName, value); }
        }

        private float cpuUsage;
        /// <summary>
        /// CPU占有率
        /// </summary>
        public float CpuUsage
        {
            get { return cpuUsage; }
            set
            {
                SetProperty(ref cpuUsage, value);
            }
        }

        private float temperature;
        /// <summary>
        /// CPU温度
        /// </summary>
        public float Temperature
        {
            get { return temperature; }
            set { SetProperty(ref temperature, value); }
        }

        private float cpuFrequency;
        /// <summary>
        /// CPU频率
        /// </summary>
        public float CpuFrequency
        {
            get { return cpuFrequency; }
            set { SetProperty(ref cpuFrequency, value); }
        }

    }
}
