﻿using App.ViewManager;
using App.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Core.Commons;
using System.Linq;
using System.Threading.Tasks;

namespace App.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 重写OnLoaded事件，隐藏系统状态栏
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var insetsManager = TopLevel.GetTopLevel(this)?.InsetsManager;

        if (insetsManager != null)
        {
            insetsManager.DisplayEdgeToEdge = true;
            insetsManager.IsSystemBarVisible = false;
        }
    }

    private async void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.DataContext is MainViewModel vm)
        {
            await Task.Factory.StartNew(vm.Start);
            InitForm();
            vm.SubscribeTopic();
        }
    }

    private void InitForm()
    {
        if (this.DataContext is MainViewModel vm)
        {
            pSensors.Children.Clear();
            vm.ListSensorManage.Clear();
            var dicSensors = vm.GetPCSensors();
            if (dicSensors == null || dicSensors.Count == 0)
                return;
            foreach (var hardwareType in dicSensors)
            {
                if (hardwareType.Key == HardwareType.Cpu)
                {
                    foreach (var name in hardwareType.Value)
                    {
                        vm.ListSensorManage.Add(new CPUViewManage(name.Key, name.Key));
                    }
                }
                else if (hardwareType.Key == HardwareType.GpuNvidia || hardwareType.Key == HardwareType.GpuAmd || hardwareType.Key == HardwareType.GpuIntel)
                {
                    foreach (var name in hardwareType.Value)
                    {
                        vm.ListSensorManage.Add(new GPUViewManage(name.Key, hardwareType.Key, name.Key));
                    }
                }
                else if (hardwareType.Key == HardwareType.Memory)
                {
                    foreach (var name in hardwareType.Value)
                    {
                        vm.ListSensorManage.Add(new RAMViewManage(name.Key, name.Key));
                    }
                }
            }
            var sort = vm.ListSensorManage.OrderBy(o => o.Sort);
            foreach (var manage in sort)
            {
                pSensors.Children.Add(manage.View);
            }
        }
    }
}