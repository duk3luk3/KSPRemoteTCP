using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JoystickTool.JoystickBinding;
using SlimDX.DirectInput;

namespace JoystickTool.GUI
{
    /// <summary>
    /// Interaction logic for AxisControl.xaml
    /// </summary>
    public partial class AxisControl : UserControl
    {
        ManagedJoystick joystick;
        UsagePage axis;

        public AxisControl(ManagedJoystick joystick, UsagePage axis)
        {
            InitializeComponent();

            this.joystick = joystick;
            this.axis = axis;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var di in joystick.DeviceObjects)
            {
                if (di.Usage == (int)axis && ((di.ObjectType & ObjectDeviceType.Axis) != 0))
                {
                    AxisNameLabel.Content = di.Name;
                    break;
                }
            }

            AxisNameLabel.Content = joystick.Name + " " + AxisNameLabel.Content;

            MinValueBox.Text = joystick.Ranges[axis].Minimum.ToString();
            MaxValueBox.Text = joystick.Ranges[axis].Maximum.ToString();

            joystick.OnJoystickEvent += JoystickEvent;
        }

        private void JoystickEvent(ManagedJoystick sender, JoystickState state)
        {
            
            CurrentValueBox.Dispatcher.BeginInvoke(
                new Action(
                    delegate()
                    {
                        int value = 0;

                        switch (axis)
                        {
                            case UsagePage.X:
                                value = state.X;
                                break;
                            case UsagePage.Y:
                                value = state.Y;
                                break;
                            case UsagePage.Z:
                                value = state.Z;
                                break;
                            case UsagePage.Rx:
                                value = state.RotationX;
                                break;
                            case UsagePage.Ry:
                                value = state.RotationY;
                                break;
                            case UsagePage.Rz:
                                value = state.RotationZ;
                                break;
                            case UsagePage.Vx:
                                value = state.VelocityX;
                                break;
                            case UsagePage.Vy:
                                value = state.VelocityY;
                                break;
                            case UsagePage.Vz:
                                value = state.VelocityZ;
                                break;
                            default:
                                break;
                        }

                        CurrentValueBox.Text = value.ToString();
                    }
            ));
        }
    }
}
