using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySigService.RTLTCP
{
    public enum RTLTCPCommandType : byte
    {
        TuneToFrequency = 0x01,
        SetSampleRate = 0x02,
        SetAutoGainControl = 0x03,
        SetTunerGainOffset = 0x0D
    }

    public class RTLTCPCommand
    {
        public RTLTCPCommandType Type { get; set; }
        public uint Argument { get; set; }
    }
}
