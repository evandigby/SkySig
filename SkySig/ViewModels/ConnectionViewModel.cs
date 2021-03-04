using SkySigModels;
using SkySigService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkySig.ViewModels
{
    public class ConnectionViewModel : BindableBase//, IEditableObject
    {
        public ICommand ConnectCommand { get; private set; }

        private readonly SDRViewModel _sdrViewModel;
        private ISDR _sdr;

        public ConnectionViewModel(SDRViewModel sdrViewModel)
        {
            ConnectCommand = new ConnectCommand(this);
            _sdrViewModel = sdrViewModel;
            Connection = new ConnectionModel
            {
                HostName = "rtlsdr.local",
                Port = 1234
            };
        }

        public ISDR SDR 
        {
            get => _sdr;
            set 
            {
                Set(ref _sdr, value);
                _sdrViewModel.SDR = value;
            }
        }

        public ConnectionModel Connection { get; private set; }

        public string HostName
        {
            get => Connection.HostName;
            set
            {
                Connection.HostName = value;
                OnPropertyChanged();
            }
        }

        public string Port {
            get
            {
                return Connection.Port.ToString();
            }
            set
            {
                if (int.TryParse(value, out int result))
                {
                    Connection.Port = result;
                    OnPropertyChanged();
                }
            }
        }

        public void BeginEdit()
        {
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            throw new NotImplementedException();
        }

        public void EndEdit()
        {
            throw new NotImplementedException();
        }
    }
}
