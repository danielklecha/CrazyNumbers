using System;
using Microsoft.Maui.Controls;
using CrazyNumbers.Resources.Strings;

namespace CrazyNumbers
{
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            HideLoading();
        }

        private async void OnPlayClicked(object? sender, EventArgs e)
        {
            ShowLoading();
            await Task.Delay(50); // Yield to allow spinner rendering
            await Navigation.PushAsync(new SetupPage());
        }

        private async void OnRulesClicked(object? sender, EventArgs e)
        {
            ShowLoading();
            await Task.Delay(50); // Yield to allow spinner rendering
            await Navigation.PushAsync(new RulesPage());
        }

        private async void OnAuthorClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync(AppResources.AuthorAlertTitle, AppResources.AuthorAlertMessage, AppResources.OK);
        }

        private void ShowLoading()
        {
            LoadingOverlay.IsVisible = true;
            Spinner.IsRunning = true;
        }

        private void HideLoading()
        {
            LoadingOverlay.IsVisible = false;
            Spinner.IsRunning = false;
        }
    }
}
