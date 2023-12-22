namespace App.ViewModels
{
    public class GPUViewModel : ViewModelBase
    {
        private string gpuName = "未知";
        /// <summary>
        /// GPU名称
        /// </summary>
        public string GpuName
        {
            get { return gpuName; }
            set { SetProperty(ref gpuName, value); }
        }

        private float gpuUsage;
        /// <summary>
        /// CPU占有率
        /// </summary>
        public float GpuUsage
        {
            get { return gpuUsage; }
            set
            {
                SetProperty(ref gpuUsage, value);
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

        private float powerWaste;
        /// <summary>
        /// 功耗
        /// </summary>
        public float PowerWaste
        {
            get { return powerWaste; }
            set { SetProperty(ref powerWaste, value); }
        }

        private float gpuMemory;
        /// <summary>
        /// 显存
        /// </summary>
        public float GPUMemory
        {
            get { return gpuMemory; }
            set { SetProperty(ref gpuMemory, value); }
        }
    }
}
