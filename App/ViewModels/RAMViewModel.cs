namespace App.ViewModels
{
    public class RAMViewModel : ViewModelBase
    {
        private string ramName = "未知";
        /// <summary>
        /// ram名称
        /// </summary>
        public string RAMName
        {
            get { return ramName; }
            set { SetProperty(ref ramName, value); }
        }

        private float ramUsage;
        /// <summary>
        /// RAM占有率
        /// </summary>
        public float RAMUsage
        {
            get { return ramUsage; }
            set { SetProperty(ref ramUsage, value); }
        }

        private float used;
        /// <summary>
        /// 已用内存
        /// </summary>
        public float Used
        {
            get { return used; }
            set { SetProperty(ref used, value); }
        }

        private float total;
        /// <summary>
        /// 总内存
        /// </summary>
        public float Total
        {
            get { return total; }
            set { SetProperty(ref total, value); }
        }



    }
}
