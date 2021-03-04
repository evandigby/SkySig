using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySigService
{
    public class EmptySDR : ISDR
    {
        public TunerType TunerType => TunerType.UNKNOWN;

        public uint TunerGainLevels => 0;

        public uint CenterFrequency { get; set; }
        public uint SampleRate { get; set; }
        public bool AutoGainControl { get; set; }
        public uint TunerGain { get; set; }

        public Stream IQ => null;
    }
}
