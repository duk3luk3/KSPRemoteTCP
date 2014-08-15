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
using JoystickTool.Plugins;

namespace JoystickTool.GUI
{
    /// <summary>
    /// Interaction logic for BeamNGControl.xaml
    /// </summary>
    public partial class BeamNGControl : UserControl
    {
        private BeamNGClient client = null;

        public BeamNGControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           // JoystickSelectBox.ItemsSource = ManagedJoystick.EnumAll();
        }

        private void JoystickSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (JoystickSelectBox.SelectedItem != null)
            {
                var joy = (ManagedJoystick)JoystickSelectBox.SelectedItem;

                client = new BeamNGClient((ManagedJoystick)JoystickSelectBox.SelectedItem, new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 3000));

                var axes = joy.DeviceObjects.Where(doi => (doi.ObjectType & ObjectDeviceType.Axis) != 0).Select(doi => new NamedDeviceObject(doi));

                AccelSelectBox.ItemsSource = axes;
                BrakeSelectBox.ItemsSource = axes;
                SteerSelectBox.ItemsSource = axes;
            }
        }

        private void AxisSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                AccelSelectBox.SelectedItem != null &&
                BrakeSelectBox.SelectedItem != null &&
                SteerSelectBox.SelectedItem != null
                )
            {
                StartButton.IsEnabled = true;
            }
            else
            {
                StartButton.IsEnabled = false;
            }
            
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            AccelSelectBox.IsEnabled = false;
            BrakeSelectBox.IsEnabled = false;
            SteerSelectBox.IsEnabled = false;
            JoystickSelectBox.IsEnabled = false;
            StartButton.IsEnabled = false;
            

            client.AccelAxis = (UsagePage)((NamedDeviceObject)AccelSelectBox.SelectedItem).doi.Usage;
            client.BrakeAxis = (UsagePage)((NamedDeviceObject)BrakeSelectBox.SelectedItem).doi.Usage;
            client.SteerAxis = (UsagePage)((NamedDeviceObject)SteerSelectBox.SelectedItem).doi.Usage;

            client.AccelInvert = (bool)AccelInvertBox.IsChecked;
            client.BrakeInvert = (bool)BrakeInvertBox.IsChecked;
            client.SteerInvert = (bool)SteerInvertBox.IsChecked;

            client.IsEnabled = true;
            StopButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            AccelSelectBox.IsEnabled = true;
            BrakeSelectBox.IsEnabled = true;
            SteerSelectBox.IsEnabled = true;
            JoystickSelectBox.IsEnabled = true;

            StopButton.IsEnabled = false;

            client.IsEnabled = false;
            StartButton.IsEnabled = true;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            client.AddEvent((ManagedJoystick)JoystickSelectBox.SelectedItem);
        }
    }
}
