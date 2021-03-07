using SkySigModels;
using SkySigService;
using SkySigService.RTLTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkySig.ViewModels
{
    public class ConnectCommand : BindableBase, ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly ConnectionViewModel vm;

        public ConnectCommand(ConnectionViewModel vm)
        {
            this.vm = vm;
            this.vm.PropertyChanged += Vm_PropertyChanged;  
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CanExecuteChanged?.Invoke(sender, e);
        }

        public bool CanExecute(object parameter)
        {
            if (!(parameter is ConnectionModel conn))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(conn.HostName))
            {
                return false;
            }

            if (conn.Port == 0)
            {
                return false;
            }

            return true;
        }

        public void Execute(object parameter)
        {
            if (!(parameter is ConnectionModel conn)) {
                throw new ArgumentException("Expected ConnectionModel");
            }

            vm.SDR = new RTLTCPSDR(new RTLTCPSDRConnection
            {
                HostName = conn.HostName,
                Port = conn.Port
            },
            new EmptySDR
            {
                CenterFrequency = 1420400000,
                AutoGainControl = false,
                SampleRate = 2048000,
                TunerGain = 0,
            });

            vm.IQStreamViewModel = new IQStreamViewModel(vm.SDR);
        }
    }
}
