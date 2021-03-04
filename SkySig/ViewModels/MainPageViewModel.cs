using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySig.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        public MainPageViewModel()
        {
            SDRViewModel = new SDRViewModel();
            ConnectionViewModel = new ConnectionViewModel(SDRViewModel);
        }
        private ConnectionViewModel _connectionViewModel;
        private SDRViewModel _sdrViewModel;

        public SDRViewModel SDRViewModel
        {
            get => _sdrViewModel;
            set => Set(ref _sdrViewModel, value);
        }

        public ConnectionViewModel ConnectionViewModel
        {
            get => _connectionViewModel;
            set => Set(ref _connectionViewModel, value);
        }

    }
}
