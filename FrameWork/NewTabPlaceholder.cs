using FrameWork.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FrameWork
{
    class NewTabPlaceholder : ClosableTab
    {
        public event NewTabCreationRequestEventHandler NewTabCreationRequest;
        public delegate void NewTabCreationRequestEventHandler(object caller, EventArgs args);

        public NewTabPlaceholder()
        {
            Header = "+";
            FontSize = 25;
            FontFamily = new FontFamily("Copperplate Gothic Light");
            FontWeight = FontWeights.UltraLight;
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            OnNewTabCreationRequest(new EventArgs());
        }

        protected virtual void OnNewTabCreationRequest(EventArgs args)
        {
            NewTabCreationRequest?.Invoke(this, args);
        }
    }
}
