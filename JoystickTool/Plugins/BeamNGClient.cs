using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using JoystickTool.JoystickBinding;
using SlimDX.DirectInput;
using System.IO;

namespace JoystickTool.Plugins
{
    public class BeamNGClient
    {
        public BeamNGClient(ManagedJoystick joystick, IPEndPoint endpoint)
        {
            this.joystick = joystick;
            this.endpoint = endpoint;
        }

        public bool IsEnabled { get { return isenabled; } set { Enable(value);}}

        public UsagePage AccelAxis { get; set; }
        public UsagePage BrakeAxis { get; set; }
        public UsagePage SteerAxis { get; set; }

        public bool AccelInvert { get; set; }
        public bool BrakeInvert { get; set; }
        public bool SteerInvert { get; set; }


        private bool isenabled = false;
        private IPEndPoint endpoint;
        private ManagedJoystick joystick;
        private UdpClient client;

        private void Enable(bool enable)
        {
            if (enable != isenabled)
            {
                if (enable)
                {
                    client = new UdpClient();
                    client.Connect(endpoint);

                    var s = new MemoryStream();
                    var bw = new BinaryWriter(s);
                    bw.Write("BEAMNGEDD1".ToCharArray());

                    var sent = client.Send(s.ToArray(), (int)s.Length);
                    System.Console.WriteLine("Sent Hello message: " + sent + " bytes");
                    IPEndPoint re = null;
                    var retmsg = client.Receive(ref re);

                    System.Console.WriteLine(retmsg.Length.ToString());
                    System.Console.WriteLine(retmsg.Select(b => (char)b).Aggregate("", (body,next)=>body+next));
                                        
                    joystick.OnJoystickEvent += JoystickEvent;

                    isenabled = true;
                }
                else{

                    joystick.OnJoystickEvent -= JoystickEvent;
                    client.Close();

                    isenabled = false;
                }
            }
        }

        private int axisvalue(JoystickState state, UsagePage axis)
        {
            switch (axis)
            {
                case UsagePage.X:
                    return state.X;
                    
                case UsagePage.Y:
                    return state.Y;
                    
                case UsagePage.Z:
                    return state.Z;
                    
                case UsagePage.Rx:
                    return state.RotationX;
                    
                case UsagePage.Ry:
                    return state.RotationY;
                    
                case UsagePage.Rz:
                    return state.RotationZ;
                    
                case UsagePage.Vx:
                    return state.VelocityX;
                    
                case UsagePage.Vy:
                    return state.VelocityY;
                    
                case UsagePage.Vz:
                    return state.VelocityZ;
                    
                default:
                    return 0;
            }
        }

        public void AddEvent(ManagedJoystick j)
        {
            j.OnJoystickEvent += JoystickEvent;
        }

        private void JoystickEvent(ManagedJoystick sender, JoystickState state)
        {
            float Accel = axisvalue(state, AccelAxis) / 32767.0f;
            float Brake = axisvalue(state, BrakeAxis) / 32767.0f;
            float Steer = axisvalue(state, SteerAxis) / 32767.0f;

            if (AccelInvert)
            {
                Accel *= -1;
            }
            if (BrakeInvert)
            {
                Brake *= -1;
            }
            if (SteerInvert)
            {
                Steer *= -1;
            }

            var ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(new byte[] { (byte)'O', (byte)'R', (byte)'I' });
            bw.Write(Accel);
            bw.Write(Brake);
            bw.Write(Steer);

            client.Send(ms.ToArray(), (int)ms.Length);           
        }
    }
}
