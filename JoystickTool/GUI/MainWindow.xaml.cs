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
using System.Windows.Threading;
using System.Windows.Interop;
using SlimDX.DirectInput;
using JoystickTool.GUI;

namespace JoystickTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var sticks = ManagedJoystick.EnumAll();
            foreach (var s in sticks)
            {
                if (s.Acquire(new WindowInteropHelper(this).Handle))
                {
                    tbLog.Text += s.Name + "\n";
                }
                s.OnJoystickEvent += JoyEvent;

                var ctrl = new JoystickControl();
                ctrl.Load(s);
                JoystickPanel.Children.Add(ctrl);

            }
            tbLog.Text += "\n";

            BeamNGPanel.JoystickSelectBox.ItemsSource = sticks;
        }

        private void JoyEvent(ManagedJoystick sender, JoystickState state)
        {

            tbLog.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        //tbLog.Text = sender.Name + " " + sender.State.X + "\n";
                        tbLog.Text = tbLog.Text + sender.ReadProps();
                    }
            ));

            sender.OnJoystickEvent -= JoyEvent;
        }
    }
}
