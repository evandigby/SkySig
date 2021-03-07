using SkySigService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SkySig.ViewModels
{
    public class SDRViewModel: INotifyPropertyChanged
    {
        public SDRViewModel()
        {
            _sdr = new EmptySDR();
            IQStreamViewModel = new IQStreamViewModel(null);
        }

        private ISDR _sdr;

        private IQStreamViewModel _iqStreamViewModel;
        public IQStreamViewModel IQStreamViewModel
        {
            get => _iqStreamViewModel;
            set
            {
                _iqStreamViewModel = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string TunerType => SDR.TunerType.ToString();

        public uint TunerGainLevels => SDR.TunerGainLevels;

        public bool ReadOnly => _sdr is EmptySDR;

        public ISDR SDR { 
            get => _sdr; 
            set
            {
                _sdr = value;
                OnPropertyChanged();
                OnPropertyChanged("CenterFrequency");
                OnPropertyChanged("SampleRate");
                OnPropertyChanged("AutoGainControl");
                OnPropertyChanged("TunerGain");
                OnPropertyChanged("TunerType");
                OnPropertyChanged("TunerGainLevels");
                OnPropertyChanged("ReadOnly");
            }
        }


        public UInt32 CenterFrequency { 
            get => SDR.CenterFrequency;
            set
            {
                SDR.CenterFrequency = value;

                OnPropertyChanged();
            }
        }
        public uint SampleRate { 
            get => SDR.SampleRate;
            set
            {
                SDR.SampleRate = value;
                OnPropertyChanged();
            }
        }

        public bool AutoGainControl {
            get => SDR.AutoGainControl;
            set
            {
                SDR.AutoGainControl = value;
                OnPropertyChanged();
            }
        }

        public string TunerGain {
            get => Convert.ToString(SDR.TunerGain);
            set
            {
                SDR.TunerGain = Convert.ToUInt32(value);
                OnPropertyChanged();
            }
        }

        public Stream IQ => SDR.IQ;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
