using SkySig.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SkySig.UserControls
{
    public sealed partial class Signal : UserControl
    {
        private object viewModelLock = new object();
        
        private IQStreamViewModel viewModel;

        public Signal()
        {
            InitializeComponent();

            DataContextChanged += Signal_DataContextChanged;
        }

        private void Signal_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (!(args.NewValue is IQStreamViewModel vm))
            {
                return;
            }

            lock (viewModelLock)
            {
                viewModel = vm;
            }
        }

        private bool HasViewModel
        {
            get
            {
                if (viewModel == null)
                {
                    lock (viewModelLock)
                    {
                        if (viewModel == null)
                            return false;
                    }
                }

                return true;
            }
        }

        // Make the current IQ data part of the view model that notifies of changes somehow?
        private void SignalCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            if (!HasViewModel)
                return;

            viewModel.DrawPoints(args.DrawingSession, sender.Size);
        }

        private void SignalCanvas_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            if (!HasViewModel)
                return;

            viewModel.UpdatePoints(sender, sender.Size);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            signalCanvas.Paused = !signalCanvas.Paused;
        }
    }
}
