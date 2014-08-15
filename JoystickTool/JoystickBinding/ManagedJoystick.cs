using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectInput;
using System.Threading;

namespace JoystickTool.JoystickBinding
{
    public delegate void JoyStickEventHandler(ManagedJoystick sender, JoystickState state);

    public enum UsagePage
    {
        X = 0x30,
        Y = 0x31,
        Z = 0x32,
        Rx = 0x33,
        Ry = 0x34,
        Rz = 0x35,
        Slider = 0x36,
        HatSwitch = 0x39,
        Vx = 0x40,
        Vy = 0x41,
        Vz = 0x42
    }

    public struct JoyStickCapabilities
    {
        public bool AccelX, AccelY, AccelZ;
        public bool VelX, VelY, VelZ;
        public bool AngularAccelX, AngularAccelY, AngularAccelZ;
        public bool AngularVelX, AngularVelY, AngularVelZ;
        public bool ForceX, ForceY, ForceZ;
        public bool RotX, RotY, RotZ;
        public bool TorqueX, TorqueY, TorqueZ;
        public bool X, Y, Z;
        public int CountSliders;
        public int CountAccelSliders;
        public int CountForceSliders;
        public int CountVelSliders;
        public int PointOfViewControllers;
        public int Buttons;

        public static JoyStickCapabilities Zero = new JoyStickCapabilities()
        {
            AccelX = false, AccelY = false, AccelZ = false,
            VelX = false, VelY = false, VelZ = false,
            AngularAccelX = false, AngularAccelY = false, AngularAccelZ = false,
            AngularVelX = false, AngularVelY = false, AngularVelZ = false,
            ForceX = false, ForceY = false, ForceZ = false,
            RotX = false, RotY = false, RotZ = false,
            TorqueX = false, TorqueY = false, TorqueZ = false,
            X = false, Y = false, Z = false,
            CountAccelSliders = 0, CountForceSliders = 0, CountSliders = 0, CountVelSliders = 0,
            PointOfViewControllers = 0, Buttons = 0
        };
    }

    public class JoyStickRange : Dictionary<UsagePage, InputRange> { }

    public class NamedDeviceObject
    {
        public DeviceObjectInstance doi { get; set; }

        public NamedDeviceObject(DeviceObjectInstance doi)
        {
            this.doi = doi;
        }

        public override string ToString()
        {
            return doi.Name;
        }
        
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

        private JoyStickCapabilities caps = JoyStickCapabilities.Zero;
        public JoyStickCapabilities Capabilities { get { return caps; } }

        private JoyStickRange range = new JoyStickRange();
        public JoyStickRange Ranges { get { return range; } }

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
                    if (j.OnJoystickEvent != null)
                    {
                        //j.joystick.Poll();
                        j.OnJoystickEvent(j, j.joystick.GetCurrentState());
                    }
                }
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
                sb.Append("Name: " + di.Name + "; ");
                sb.Append("ObjectType: " + ParseObjectType(di.ObjectType) + " (0x" + ((int)di.ObjectType).ToString("x") + ")" + "; ");
                sb.Append("DesignatorIndex: " + di.DesignatorIndex.ToString() + "; ");
                sb.Append("Usage: 0x" + di.Usage.ToString("x") + "; ");
                sb.Append("Dimension: " + di.Dimension + "; ");
                sb.Append("Exponent: " + di.Exponent + "; ");
                sb.AppendLine();
                
            }

            return sb.ToString();
        }

        public ManagedJoystick(Joystick device)
        {
            this.joystick = device;

            foreach (var di in DeviceObjects)
            {
                if ((di.ObjectType & ObjectDeviceType.Axis) != 0)
                {
                    var prop =  joystick.GetObjectPropertiesById((int)di.ObjectType);
                    if (prop.LowerRange == 0)
                    {
                        prop.SetRange(-(prop.UpperRange / 2)-1, (prop.UpperRange / 2));
                    }

                    if ((di.Usage == (int)UsagePage.Rx) )
                    {
                        caps.RotX = true;
                        //range[UsagePage.Rx] = prop.LogicalRange;
                        range[UsagePage.Rx] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.Ry) )
                    {
                        caps.RotY = true;
                        range[UsagePage.Ry] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.Rz) )
                    {
                        caps.RotZ = true;
                        range[UsagePage.Rz] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.X) )
                    {
                        caps.X = true;
                        range[UsagePage.X] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.Y) )
                    {
                        caps.Y = true;
                        range[UsagePage.Y] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.Z) )
                    {
                        caps.Z = true;
                        range[UsagePage.Z] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.Vx) )
                    {
                        caps.VelX = true;
                        range[UsagePage.Vx] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.Vy) )
                    {
                        caps.VelY = true;
                        range[UsagePage.Vy] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                    else if ((di.Usage == (int)UsagePage.Vz) )
                    {
                        caps.VelZ = true;
                        range[UsagePage.Vz] = new InputRange(prop.LowerRange, prop.UpperRange);
                    }
                }
            }

            caps.PointOfViewControllers = joystick.Capabilities.PovCount;
            caps.Buttons = joystick.Capabilities.ButtonCount;
        }

        public bool Acquire(IntPtr handle, bool notify = true)
        {
            if (handle != IntPtr.Zero)
            {
                joystick.SetCooperativeLevel(handle, CooperativeLevel.Exclusive | CooperativeLevel.Background);
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

        public override string ToString()
        {
            return Name;
        }
    }
}
