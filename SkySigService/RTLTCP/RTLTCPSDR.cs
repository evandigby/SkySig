using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SkySigService.RTLTCP
{
    public class RTLTCPSDR : ISDR, IDisposable
    {
        private readonly TcpClient client;

        private NetworkStream clientStream;
        private RTLTCPDeviceHeader header;

        public RTLTCPSDR(RTLTCPSDRConnection connection, ISDR defaults)
        {
            _centerFrequency = defaults.CenterFrequency;
            _autoGainControl = defaults.AutoGainControl;
            _sampleRate = defaults.SampleRate;
            _tunerGain = defaults.TunerGain;

            var addrs = Dns.GetHostAddresses(connection.HostName);

            var exceptions = new List<Exception>();
            foreach (var ip in addrs)
            {
                client = new TcpClient(ip.AddressFamily);

                try
                {
                    client.Connect(ip, connection.Port);
                    break;
                } 
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    continue;
                }
            }

            if (exceptions.Count == addrs.Count())
            {
                throw new AggregateException("unable to connect", exceptions);
            }

            var stream = client.GetStream();

            header = new RTLTCPDeviceHeader(stream);
            clientStream = stream;

            Init(defaults);
        }


        private void Init(ISDR defaults)
        {
            CenterFrequency = defaults.CenterFrequency;
            SampleRate = defaults.SampleRate;
            AutoGainControl = defaults.AutoGainControl;
        }

        private void SendCommand(RTLTCPCommand cmd)
        {
            var buf = new byte[5];
            buf[0] = (byte)cmd.Type;
            var cmdData = BitConverter.GetBytes(cmd.Argument);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(cmdData);
            }

            Array.Copy(cmdData, 0, buf, 1, cmdData.Length);
            clientStream.Write(buf, 0, cmdData.Length);
        }

        private uint _centerFrequency;
        public uint CenterFrequency {
            get => _centerFrequency;
            set
            {
                SendCommand(new RTLTCPCommand { Type = RTLTCPCommandType.TuneToFrequency, Argument = value });
                _centerFrequency = value;
            }
        }

        private uint _sampleRate;
        public uint SampleRate
        {
            get => _sampleRate;
            set
            {
                SendCommand(new RTLTCPCommand { Type = RTLTCPCommandType.SetSampleRate, Argument = value });
                _sampleRate = value;
            }
        }

        private bool _autoGainControl;
        public bool AutoGainControl
        {
            get => _autoGainControl;
            set
            {
                uint toSend = value ? (uint)1 : 0;
                SendCommand(new RTLTCPCommand { Type = RTLTCPCommandType.SetAutoGainControl, Argument = toSend });
                _autoGainControl = value;
            }
        }

        private uint _tunerGain;
        public uint TunerGain
        {
            get => _tunerGain;
            set
            {
                SendCommand(new RTLTCPCommand { Type = RTLTCPCommandType.SetTunerGainOffset, Argument = value });
                _tunerGain = value;
            }
        }

        public Stream IQ => clientStream;
        public TunerType TunerType => header.TunerType; 
        public uint TunerGainLevels => header.TunerGainLevels;

        public void Dispose()
        {
            clientStream.Dispose();
            client.Dispose();
        }
    }
}
