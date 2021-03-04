using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySigService.RTLTCP
{
    public class RTLTCPDeviceHeader
    {
        public RTLTCPDeviceHeader(Stream reader)
        {
            var buf = new byte[12];
            reader.Read(buf, 0, buf.Length);

            string magic = Encoding.ASCII.GetString(buf, 0, 4);
            if (magic != "RTL0")
            {
                throw new Exception("invalid RTL TCP stream");
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buf, 4, 4);
                Array.Reverse(buf, 8, 4);
            }

            TunerType = (TunerType)BitConverter.ToUInt32(buf, 4);
            TunerGainLevels = BitConverter.ToUInt32(buf, 8);
        }

        public TunerType TunerType { get; private set; }
        public uint TunerGainLevels { get; private set; }
    }
}
