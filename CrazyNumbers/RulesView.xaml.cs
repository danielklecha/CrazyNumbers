using Microsoft.Maui.Controls;
using System;

namespace CrazyNumbers
{
    public partial class RulesView : ContentView
    {
        public event EventHandler? CloseRequested;

        public RulesView()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
