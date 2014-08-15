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

namespace JoystickTool.GUI
{
    /// <summary>
    /// Interaction logic for JoystickControl.xaml
    /// </summary>
    public partial class JoystickControl : UserControl
    {
        public JoystickControl()
        {
            InitializeComponent();
        }

        List<AxisControl> axctrls = new List<AxisControl>();

        public void Load(ManagedJoystick joystick)
        {
            var caps = joystick.Capabilities;

            if (caps.X)
            {
                axctrls.Add(new AxisControl(joystick, UsagePage.X));
            }
            if (caps.Y)
            {
                axctrls.Add(new AxisControl(joystick, UsagePage.Y));
            }
            if (caps.Z)
            {
                axctrls.Add(new AxisControl(joystick, UsagePage.Z));
            }
            if (caps.RotX)
            {
                axctrls.Add(new AxisControl(joystick, UsagePage.Rx));
            }
            if (caps.RotY)
            {
                axctrls.Add(new AxisControl(joystick, UsagePage.Ry));
            }
            if (caps.RotZ)
            {
                axctrls.Add(new AxisControl(joystick, UsagePage.Rz));
            }

            foreach (var x in axctrls)
            {
                Panel.Children.Add(x);
            }
        }
    }
}
