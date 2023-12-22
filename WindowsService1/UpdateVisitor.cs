using LibreHardwareMonitor.Hardware;

namespace WindowsService1
{
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            // 访问计算机
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            // 更新硬件
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
