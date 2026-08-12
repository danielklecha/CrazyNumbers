using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CrazyNumbers.Resources.Strings;

namespace CrazyNumbers
{
    public partial class SetupPage : ContentPage
    {
        private int _selectedRounds = 5;
        private Button? _selectedButton;

        private int _selectedPlayers = 2;
        private bool _isVsCpu = true;
        private Button? _selectedPlayerButton;

        private CpuDifficulty _selectedCpuDifficulty = CpuDifficulty.Medium;
        private Button? _selectedCpuDifficultyButton;

        public SetupPage()
        {
            InitializeComponent();
            
            // Pre-select default values: 1 player, Medium CPU difficulty, and 5 rounds
            Dispatcher.Dispatch(() =>
            {
                SelectPlayerButton(OnePlayerBtn);
                SelectCpuDifficultyButton(MediumDifficultyBtn);

                foreach (var child in RoundsGrid.Children)
                {
                    if (child is Button btn && btn.Text == "5")
                    {
                        SelectRoundButton(btn);
                        break;
                    }
                }
            });
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnPlayerCountSelected(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                SelectPlayerButton(btn);
            }
        }

        private void SelectPlayerButton(Button btn)
        {
            if (_selectedPlayerButton != null)
            {
                _selectedPlayerButton.BackgroundColor = Color.FromArgb("#15ffffff");
                _selectedPlayerButton.TextColor = Colors.White;
                _selectedPlayerButton.BorderColor = Color.FromArgb("#33ffffff");
            }

            _selectedPlayerButton = btn;
            _selectedPlayerButton.BackgroundColor = Color.FromArgb("#00e5ff");
            _selectedPlayerButton.TextColor = Color.FromArgb("#0B0F19");
            _selectedPlayerButton.BorderColor = Colors.Transparent;

            if (btn == OnePlayerBtn)
            {
                _isVsCpu = true;
                _selectedPlayers = 2;
            }
            else if (btn == TwoPlayersBtn)
            {
                _isVsCpu = false;
                _selectedPlayers = 2;
            }
            else if (btn == ThreePlayersBtn)
            {
                _isVsCpu = false;
                _selectedPlayers = 3;
            }

            if (CpuDifficultyContainer != null)
            {
                CpuDifficultyContainer.IsVisible = _isVsCpu;
            }

            UpdateRoundsDescription();
        }

        private void OnCpuDifficultySelected(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                SelectCpuDifficultyButton(btn);
            }
        }

        private void SelectCpuDifficultyButton(Button btn)
        {
            if (_selectedCpuDifficultyButton != null)
            {
                _selectedCpuDifficultyButton.BackgroundColor = Color.FromArgb("#15ffffff");
                _selectedCpuDifficultyButton.TextColor = Colors.White;
                _selectedCpuDifficultyButton.BorderColor = Color.FromArgb("#33ffffff");
            }

            _selectedCpuDifficultyButton = btn;
            _selectedCpuDifficultyButton.BackgroundColor = Color.FromArgb("#00e5ff");
            _selectedCpuDifficultyButton.TextColor = Color.FromArgb("#0B0F19");
            _selectedCpuDifficultyButton.BorderColor = Colors.Transparent;

            if (btn == EasyDifficultyBtn)
            {
                _selectedCpuDifficulty = CpuDifficulty.Easy;
            }
            else if (btn == MediumDifficultyBtn)
            {
                _selectedCpuDifficulty = CpuDifficulty.Medium;
            }
            else if (btn == HardDifficultyBtn)
            {
                _selectedCpuDifficulty = CpuDifficulty.Hard;
            }
        }

        private void UpdateRoundsDescription()
        {
            if (RoundsDescriptionLbl != null)
            {
                RoundsDescriptionLbl.Text = string.Format(AppResources.RoundsDescriptionTemplate, _selectedPlayers);
            }
        }

        private void OnRoundSelected(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                SelectRoundButton(btn);
            }
        }

        private void SelectRoundButton(Button btn)
        {
            // Reset previous button style
            if (_selectedButton != null)
            {
                _selectedButton.BackgroundColor = Color.FromArgb("#15ffffff");
                _selectedButton.TextColor = Colors.White;
                _selectedButton.BorderColor = Color.FromArgb("#33ffffff");
            }

            // Apply selected style
            _selectedButton = btn;
            _selectedButton.BackgroundColor = Color.FromArgb("#00e5ff");
            _selectedButton.TextColor = Color.FromArgb("#0B0F19");
            _selectedButton.BorderColor = Colors.Transparent;

            if (int.TryParse(btn.Text, out int rounds))
            {
                _selectedRounds = rounds;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            HideLoading();
        }

        private async void OnStartClicked(object? sender, EventArgs e)
        {
            ShowLoading();
            await System.Threading.Tasks.Task.Delay(50); // Yield to allow spinner rendering
            await Navigation.PushAsync(new GamePage(_selectedRounds, _selectedPlayers, _isVsCpu, _selectedCpuDifficulty));
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
