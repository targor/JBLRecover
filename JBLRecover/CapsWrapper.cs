using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBLRecover
{
    public  class CapsWrapper
    {
        public WaveOutCapabilities? Cpas;
        public MMDevice Device;
        public int deviceIndex;

        public CapsWrapper(MMDevice device)
        {
            this.Device = device;
        }

        public CapsWrapper(WaveOutCapabilities caps, int index)
        {
            this.deviceIndex = index;
            this.Cpas = caps;
        }

        public override string ToString()
        {
            if (this.Cpas != null)
            {
                return Cpas.Value.ProductName;
            }
            else if (this.Device != null)
            {
                return Device.FriendlyName;
            }
            return "";


        }
    }
}
