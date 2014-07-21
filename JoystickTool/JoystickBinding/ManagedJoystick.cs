using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectInput;
using System.Threading;

namespace JoystickTool.JoystickBinding
{
    public delegate void JoyStickEventHandler(ManagedJoystick sender);
    public delegate void AxisEventHandler(ManagedJoystick sender, string axisName, int rangeMin, int rangeMax, int currentValue);

    public enum Usage
    {
        X = 0x30,
        Y = 0x31,
        Z = 0x32,
        Rx = 0x33,
        Ry = 0x34,
        Rz = 0x35,
        Slider = 0x36,
        HatSwitch = 0x39
    }

    public class ManagedJoystick
    {
        Joystick joystick;

        public IList<DeviceObjectInstance> DeviceObjects { get { return joystick.GetObjects(); } }
        public string Name { get { return joystick.Information.InstanceName; } }
        public JoystickState State { get { return joystick.GetCurrentState(); } }

        private static AutoResetEvent joyEvent = new AutoResetEvent(false);
        Thread joyThread = new Thread(new ParameterizedThreadStart(Device_JoystickInput));

        public JoyStickEventHandler OnJoystickEvent;
        public AxisEventHandler OnAxisEvent;

        static void Device_JoystickInput(object self) //object sender, EventArgs e)
        {
            ManagedJoystick j = (ManagedJoystick)self;
            bool appClosing = false;

            var axisObjects = j.joystick.GetObjects().Where(doi => (doi.ObjectType & ObjectDeviceType.Axis) != 0);

            while (!appClosing)
            {
                joyEvent.WaitOne();
                if (!appClosing)
                {
                    j.OnJoystickEvent(j);

                    foreach (var ao in axisObjects)
                    {
                        var name = ao.Name;
                        //j.joystick.GetObjectPropertiesById((int)deviceObject.ObjectType)
                    }
                }
                appClosing = true;
            }
        }

        public static string ParseObjectType(ObjectDeviceType type)
        {

            ObjectDeviceType[] types = {              
                    ObjectDeviceType.RelativeAxis,
                    ObjectDeviceType.AbsoluteAxis,
                    ObjectDeviceType.Axis,
                    ObjectDeviceType.PushButton,
                    ObjectDeviceType.ToggleButton,
                    ObjectDeviceType.Button,
                    ObjectDeviceType.PointOfViewController,
                    ObjectDeviceType.Collection,
                    ObjectDeviceType.NoData,
                    ObjectDeviceType.NoCollection,
                    ObjectDeviceType.ForceFeedbackActuator,
                    ObjectDeviceType.ForceFeedbackEffectTrigger,
                    ObjectDeviceType.VendorDefined,
                    ObjectDeviceType.Alias,
                    ObjectDeviceType.Output
            };

            List<ObjectDeviceType> presentTypes = new List<ObjectDeviceType>();

            foreach (var t in types)
            {
                if ((t & type) != 0)
                {
                    presentTypes.Add(t);
                }
            }

            return String.Join(", ", presentTypes.Select(t => t.ToString()));

        }

        public string ReadProps()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Name);

            foreach (var di in DeviceObjects)
            {
                sb.AppendLine("Name: " + di.Name + "; ObjectType: " + ParseObjectType(di.ObjectType) + " (0x" + ((int)di.ObjectType).ToString("x") + ")" + "; DesignatorIndex: " + di.DesignatorIndex.ToString() + "; Usage: 0x" + di.Usage.ToString("x"));
            }

            return sb.ToString();
        }

        public ManagedJoystick(Joystick device)
        {
            this.joystick = device;
        }

        public bool Acquire(IntPtr handle, bool notify = true)
        {
            if (handle != IntPtr.Zero)
            {
                joystick.SetCooperativeLevel(handle, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);
            }

            if (notify)
            {
                if (joystick.SetNotification(joyEvent).IsFailure)
                    return false;
                joyThread.Start(this);
            }

            if (joystick.Acquire().IsFailure)
                return false;

            return true;
        }

        public static IEnumerable<ManagedJoystick> EnumAll()
        {
            List<ManagedJoystick> list = new List<ManagedJoystick>();

            DirectInput dinput = new DirectInput();

            // search for devices
            foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                // create the device
                try
                {
                    var joystick = new Joystick(dinput, device.InstanceGuid);
                    list.Add(new ManagedJoystick(joystick));
                }
                catch (DirectInputException)
                {
                }
            }

            return list;
        }
    }
}
