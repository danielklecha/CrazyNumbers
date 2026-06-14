using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CrazyNumbers.Resources.Strings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrazyNumbers
{
    public partial class GamePage : ContentPage
    {
        private readonly int _rounds;
        private readonly int _players;
        
        private readonly int[] p = new int[] {
            1, 2, 3, 4, 5, 6,
            7, 8, 9, 10, 12, 14,
            15, 16, 18, 20, 21, 24,
            25, 27, 28, 30, 32, 35,
            36, 40, 42, 45, 48, 49,
            54, 56, 63, 64, 72, 81
        };

        private int step = 1;
        private int maxSteps = 16;
        private int currentPlayer = 1;
        private readonly int[] scores = new int[4];
        private readonly int[] bonuses = new int[4];
        private int number1;
        private int number2 = 0;
        private readonly int[] z = new int[36];

        private readonly Color Player1Color = Color.FromArgb("#00e5ff"); // Cyber Cyan
        private readonly Color Player2Color = Color.FromArgb("#ffd600"); // Yellow/Gold
        private readonly Color Player3Color = Color.FromArgb("#ff1744"); // Bright Rose Red

        public GamePage(int rounds, int players = 3)
        {
            InitializeComponent();
            _rounds = rounds;
            _players = players;
            
            ConfigurePlayerLayout();
            GenerateBoard();
            GenerateFactors();
            
            RestartGame();

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = new Command(async () => await HandleBackAction())
            });
        }

        private void ConfigurePlayerLayout()
        {
            if (_players == 2)
            {
                Player3ColBorder.IsVisible = false;
                
                // Move Round column to index 2 (instead of 3)
                Grid.SetColumn(RoundColBorder, 2);
                
                // Adjust column definitions of TopPanelGrid to have 3 columns instead of 4
                TopPanelGrid.ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                };
            }
        }

        private void GenerateBoard()
        {
            BoardGrid.Children.Clear();
            for (int i = 0; i < 36; i++)
            {
                var border = new Border
                {
                    BackgroundColor = Color.FromArgb("#15ffffff"),
                    Stroke = Color.FromArgb("#25ffffff"),
                    StrokeThickness = 1,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                    {
                        CornerRadius = new CornerRadius(8)
                    },
                    Padding = 0,
                    Margin = 0,
                    BindingContext = i // store index
                };
                
                var label = new Label
                {
                    Text = p[i].ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 14
                };
                border.Content = label;

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += OnGridCellTapped;
                border.GestureRecognizers.Add(tapGesture);

                BoardGrid.Add(border, i % 6, i / 6);
            }
        }

        private void GenerateFactors()
        {
            FactorsGrid.Children.Clear();
            for (int i = 1; i <= 9; i++)
            {
                var border = new Border
                {
                    BackgroundColor = Color.FromArgb("#10ffffff"),
                    Stroke = Color.FromArgb("#ffea00"), // gold-accent stroke
                    StrokeThickness = 1.5,
                    WidthRequest = 45,
                    HeightRequest = 45,
                    Margin = new Thickness(6),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                    {
                        CornerRadius = new CornerRadius(22.5) // fully circular
                    },
                    BindingContext = i
                };

                var label = new Label
                {
                    Text = i.ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16
                };
                border.Content = label;

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += OnFactorTapped;
                border.GestureRecognizers.Add(tapGesture);

                FactorsGrid.Children.Add(border);
            }
        }

        private void OnBoardContainerSizeChanged(object? sender, EventArgs e)
        {
            if (BoardContainer == null || BoardBorder == null || BoardGrid == null) return;

            double availableWidth = BoardContainer.Width;
            double availableHeight = BoardContainer.Height;

            if (availableWidth <= 0 || availableHeight <= 0) return;

            // Compute maximum square size to fit within the container with some margin
            double maxSquareSize = Math.Min(availableWidth, availableHeight) - 10;
            if (maxSquareSize < 100) maxSquareSize = 100;

            // Only perform layout updates if the size has actually changed to avoid layout cycles
            if (Math.Abs(BoardBorder.WidthRequest - maxSquareSize) < 0.5) return;

            BoardBorder.WidthRequest = maxSquareSize;
            BoardBorder.HeightRequest = maxSquareSize;

            // Calculate grid cell size
            // Border padding is 15 on each side = 30 total
            // Grid has 6 columns, so 5 column gaps of 6px = 30 total
            // Total padding and spacing is 60px
            double totalPadding = 60;
            double cellSize = (maxSquareSize - totalPadding) / 6.0;
            if (cellSize < 10) cellSize = 10;

            // Update board grid cells
            foreach (var child in BoardGrid.Children)
            {
                if (child is Border cellBorder)
                {
                    cellBorder.WidthRequest = -1;
                    cellBorder.HeightRequest = -1;
                    cellBorder.HorizontalOptions = LayoutOptions.Fill;
                    cellBorder.VerticalOptions = LayoutOptions.Fill;

                    if (cellBorder.StrokeShape is Microsoft.Maui.Controls.Shapes.RoundRectangle roundRect)
                    {
                        roundRect.CornerRadius = new CornerRadius(cellSize * 0.18);
                    }

                    if (cellBorder.Content is Label cellLabel)
                    {
                        cellLabel.FontSize = cellSize * 0.35;
                    }
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            HideLoading();
        }

        private async void OnRulesClicked(object sender, EventArgs e)
        {
            ShowLoading();
            await System.Threading.Tasks.Task.Delay(50); // Yield to allow spinner rendering
            // Pushes RulesPage so the player can view rules without resetting the game state.
            await Navigation.PushAsync(new RulesPage());
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(AppResources.ResetAlertTitle, AppResources.ResetAlertMessage, AppResources.Yes, AppResources.No);
            if (answer)
            {
                await Navigation.PopToRootAsync();
            }
        }

        private async void OnNewGameClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
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

        private void RestartGame()
        {
            maxSteps = _players * _rounds;
            step = 1;
            currentPlayer = 1;
            scores[1] = 0;
            scores[2] = 0;
            scores[3] = 0;
            bonuses[1] = 0;
            bonuses[2] = 0;
            bonuses[3] = 0;

            for (int i = 0; i < 36; i++) z[i] = 0;

            var rnd = new Random();
            number1 = rnd.Next(1, 10);
            number2 = 0;

            GameOverOverlay.IsVisible = false;
            WinnersContainer.Children.Clear();

            UpdateUI();
        }

        private void UpdateUI()
        {
            Score1Lbl.Text = scores[1].ToString();
            Score2Lbl.Text = scores[2].ToString();
            if (_players >= 3)
            {
                Score3Lbl.Text = scores[3].ToString();
            }

            // Player 1 styling/active indicator
            Player1Header.Text = string.Format(AppResources.PlayerHeaderTemplate, 1);
            Player1Header.TextColor = Player1Color;
            Score1Lbl.TextColor = Player1Color;
            Player1ColBorder.Stroke = currentPlayer == 1 ? Player1Color : Colors.Transparent;
            Player1ColBorder.BackgroundColor = currentPlayer == 1 ? Color.FromArgb("#2500e5ff") : Color.FromArgb("#15ffffff");

            // Player 2 styling/active indicator
            Player2Header.Text = string.Format(AppResources.PlayerHeaderTemplate, 2);
            Player2Header.TextColor = Player2Color;
            Score2Lbl.TextColor = Player2Color;
            Player2ColBorder.Stroke = currentPlayer == 2 ? Player2Color : Colors.Transparent;
            Player2ColBorder.BackgroundColor = currentPlayer == 2 ? Color.FromArgb("#25ffd600") : Color.FromArgb("#15ffffff");

            // Player 3 styling/active indicator
            if (_players >= 3)
            {
                Player3Header.Text = string.Format(AppResources.PlayerHeaderTemplate, 3);
                Player3Header.TextColor = Player3Color;
                Score3Lbl.TextColor = Player3Color;
                Player3ColBorder.Stroke = currentPlayer == 3 ? Player3Color : Colors.Transparent;
                Player3ColBorder.BackgroundColor = currentPlayer == 3 ? Color.FromArgb("#25ff1744") : Color.FromArgb("#15ffffff");
            }

            Color playerColor = currentPlayer == 1 ? Player1Color : currentPlayer == 2 ? Player2Color : Player3Color;

            int totalRounds = maxSteps / _players;
            int currentRound = Math.Min((step - 1) / _players + 1, totalRounds);
            RoundLbl.Text = $"{currentRound}/{totalRounds}";
            
            Factor1Lbl.Text = number1.ToString();
            Factor1Border.BackgroundColor = playerColor;
            Factor1Lbl.TextColor = Color.FromArgb("#0B0F19");

            if (number2 != 0)
            {
                Factor2Lbl.Text = number2.ToString();
                Factor2Border.BackgroundColor = playerColor;
                Factor2Border.Stroke = Colors.Transparent;
                Factor2Lbl.TextColor = Color.FromArgb("#0B0F19");
                
                TipLbl.Text = AppResources.TipSelectProduct;
                foreach (Border b in FactorsGrid.Children)
                {
                    b.BackgroundColor = Color.FromArgb("#05ffffff");
                    b.Stroke = Color.FromArgb("#33ffffff");
                    b.IsEnabled = false;
                }
            }
            else
            {
                Factor2Lbl.Text = "?";
                Factor2Border.BackgroundColor = Colors.Transparent;
                Factor2Border.Stroke = Color.FromArgb("#33ffffff");
                Factor2Lbl.TextColor = Colors.White;
                
                TipLbl.Text = AppResources.TipSelectMultiplier;
                foreach (Border b in FactorsGrid.Children)
                {
                    b.BackgroundColor = Color.FromArgb("#10ffffff");
                    b.Stroke = Color.FromArgb("#ffd600");
                    b.IsEnabled = true;
                }
            }

            // Update board colors
            for (int i = 0; i < 36; i++)
            {
                var border = (Border)BoardGrid.Children[i];
                var lbl = (Label)border.Content;
                if (z[i] == 1)
                {
                    border.BackgroundColor = Player1Color;
                    lbl.TextColor = Color.FromArgb("#0B0F19");
                }
                else if (z[i] == 2)
                {
                    border.BackgroundColor = Player2Color;
                    lbl.TextColor = Color.FromArgb("#0B0F19");
                }
                else if (z[i] == 3)
                {
                    border.BackgroundColor = Player3Color;
                    lbl.TextColor = Color.FromArgb("#0B0F19");
                }
                else
                {
                    border.BackgroundColor = Color.FromArgb("#15ffffff");
                    lbl.TextColor = Colors.White;
                }
            }
        }

        private void OnFactorTapped(object sender, EventArgs e)
        {
            if (number2 != 0) return; // already selected

            var border = (Border)sender;
            int factor = (int)border.BindingContext;
            number2 = factor;
            UpdateUI();
        }

        private async void OnGridCellTapped(object sender, EventArgs e)
        {
            if (number2 == 0) return; // factor 2 not selected yet

            var border = (Border)sender;
            int x = (int)border.BindingContext;

            if (number1 * number2 == p[x])
            {
                if (z[x] == 0)
                {
                    z[x] = currentPlayer;

                    // points (diagonals logic translated)
                    if ((x > 6 && x < 11) || (x > 12 && x < 17) || (x > 18 && x < 23) || (x > 24 && x < 29)) // center
                    {
                        if (z[x] == z[x - 7]) scores[currentPlayer] += 20;
                        if (z[x] == z[x + 7]) scores[currentPlayer] += 20;
                        if (z[x] == z[x - 5]) scores[currentPlayer] += 20;
                        if (z[x] == z[x + 5]) scores[currentPlayer] += 20;
                    }
                    else if (x == 6 || x == 12 || x == 18 || x == 24) // left
                    {
                        if (z[x] == z[x + 7]) scores[currentPlayer] += 20;
                        if (z[x] == z[x - 5]) scores[currentPlayer] += 20;
                    }
                    else if (x == 11 || x == 17 || x == 23 || x == 29) // right
                    {
                        if (z[x] == z[x - 7]) scores[currentPlayer] += 20;
                        if (z[x] == z[x + 5]) scores[currentPlayer] += 20;
                    }
                    else if (x == 1 || x == 2 || x == 3 || x == 4) // top
                    {
                        if (z[x] == z[x + 7]) scores[currentPlayer] += 20;
                        if (z[x] == z[x + 5]) scores[currentPlayer] += 20;
                    }
                    else if (x == 31 || x == 32 || x == 33 || x == 34) // bottom
                    {
                        if (z[x] == z[x - 7]) scores[currentPlayer] += 20;
                        if (z[x] == z[x - 5]) scores[currentPlayer] += 20;
                    }
                    else if (x == 0) // top left
                    {
                        if (z[x] == z[x + 7]) scores[currentPlayer] += 20;
                    }
                    else if (x == 5) // top right
                    {
                        if (z[x] == z[x + 5]) scores[currentPlayer] += 20;
                    }
                    else if (x == 30) // bottom left
                    {
                        if (z[x] == z[x - 5]) scores[currentPlayer] += 20;
                    }
                    else if (x == 35) // bottom right
                    {
                        if (z[x] == z[x - 7]) scores[currentPlayer] += 20;
                    }

                    // PREMIA punktowa
                    if (bonuses[currentPlayer] == 0)
                    {
                        for (int ii = 0; ii < 36; ii++)
                        {
                            if ((ii > 6 && ii < 11) || (ii > 12 && ii < 17) || (ii > 18 && ii < 23) || (ii > 24 && ii < 29)) // center
                            {
                                if ((z[ii - 1] == currentPlayer && z[ii] == currentPlayer && z[ii + 1] == currentPlayer) || (z[ii - 6] == currentPlayer && z[ii] == currentPlayer && z[ii + 6] == currentPlayer))
                                {
                                    scores[currentPlayer] += 30;
                                    bonuses[currentPlayer] = 1;
                                    break;
                                }
                            }
                            else if (ii == 6 || ii == 12 || ii == 18 || ii == 24 || ii == 11 || ii == 17 || ii == 23 || ii == 29) // left and right
                            {
                                if (z[ii - 6] == currentPlayer && z[ii] == currentPlayer && z[ii + 6] == currentPlayer)
                                {
                                    scores[currentPlayer] += 30;
                                    bonuses[currentPlayer] = 1;
                                    break;
                                }
                            }
                            else if (ii == 1 || ii == 2 || ii == 3 || ii == 4 || ii == 31 || ii == 32 || ii == 33 || ii == 34) // top and bottom
                            {
                                if (z[ii - 1] == currentPlayer && z[ii] == currentPlayer && z[ii + 1] == currentPlayer)
                                {
                                    scores[currentPlayer] += 30;
                                    bonuses[currentPlayer] = 1;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert(AppResources.Oops, AppResources.CellAlreadyTakenMessage, AppResources.OK);
                    return;
                }
            }
            else
            {
                await DisplayAlert(AppResources.Oops, AppResources.WrongAnswerMessage, AppResources.OK);
            }

            number1 = number2;
            number2 = 0;

            currentPlayer++;
            if (currentPlayer > _players) currentPlayer = 1;

            CheckEndGame();
            UpdateUI();
        }

        private void CheckEndGame()
        {
            if (step == maxSteps)
            {
                WinnersContainer.Children.Clear();

                int maxPoints = scores[1];
                for (int i = 2; i <= _players; i++)
                {
                    if (scores[i] > maxPoints)
                    {
                        maxPoints = scores[i];
                    }
                }

                var winners = new List<int>();
                for (int i = 1; i <= _players; i++)
                {
                    if (scores[i] == maxPoints)
                    {
                        winners.Add(i);
                    }
                }

                var winLabel = new Label
                {
                    Text = winners.Count > 1 ? AppResources.WinnersLabel : AppResources.WinnerLabel,
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 0, 0, 10),
                    CharacterSpacing = 1
                };
                WinnersContainer.Children.Add(winLabel);

                foreach (int winnerId in winners)
                {
                    string winnerName = string.Format(AppResources.PlayerHeaderTemplate, winnerId);
                    Color winnerColor = winnerId == 1 ? Player1Color : winnerId == 2 ? Player2Color : Player3Color;

                    var winnerBorder = new Border
                    {
                        Stroke = winnerColor,
                        StrokeThickness = 1.5,
                        BackgroundColor = Color.FromArgb("#15ffffff"),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                        {
                            CornerRadius = new CornerRadius(10)
                        },
                        Padding = new Thickness(15, 8),
                        HorizontalOptions = LayoutOptions.Center,
                        Content = new HorizontalStackLayout
                        {
                            Spacing = 10,
                            VerticalOptions = LayoutOptions.Center,
                            Children =
                            {
                                new Label
                                {
                                    Text = winnerName.ToUpper(),
                                    TextColor = winnerColor,
                                    FontSize = 18,
                                    FontAttributes = FontAttributes.Bold,
                                    VerticalOptions = LayoutOptions.Center
                                },
                                new Label
                                {
                                    Text = $"{scores[winnerId]} {AppResources.Pts}",
                                    TextColor = Colors.White,
                                    FontSize = 16,
                                    VerticalOptions = LayoutOptions.Center
                                }
                            }
                        }
                    };
                    WinnersContainer.Children.Add(winnerBorder);
                }

                GameOverOverlay.IsVisible = true;
            }
            step++;
        }

        private async Task HandleBackAction()
        {
            if (GameOverOverlay.IsVisible)
            {
                await Navigation.PopAsync();
                return;
            }

            string title = AppResources.ResourceManager.GetString("ExitAlertTitle") ?? "Exit Game";
            string message = AppResources.ResourceManager.GetString("ExitAlertMessage") ?? "Are you sure you want to leave the game? Your progress will be lost.";
            bool answer = await DisplayAlert(title, message, AppResources.Yes, AppResources.No);
            if (answer)
            {
                await Navigation.PopAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (GameOverOverlay.IsVisible)
            {
                return false;
            }

            Dispatcher.Dispatch(async () => await HandleBackAction());
            return true;
        }
    }
}
