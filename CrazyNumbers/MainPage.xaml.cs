using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace CrazyNumbers
{
    public partial class MainPage : ContentPage
    {
        private int[] p = new int[] {
            1, 2, 3, 4, 5, 6,
            7, 8, 9, 10, 12, 14,
            15, 16, 18, 20, 21, 24,
            25, 27, 28, 30, 32, 35,
            36, 40, 42, 45, 48, 49,
            54, 56, 63, 64, 72, 81
        };

        private int krok = 1;
        private int koniec = 16;
        private int zawodnik = 1;
        private int[] punkty = new int[4];
        private int[] premia = new int[4];
        private int liczba1;
        private int liczba2 = 0;
        private int[] z = new int[36];

        private Color Player1Color = Color.FromArgb("#00aaaa");
        private Color Player2Color = Color.FromArgb("#aaaa00");
        private Color Player3Color = Color.FromArgb("#aa0000");

        public MainPage()
        {
            InitializeComponent();
            GenerateBoard();
            GenerateFactors();
            RoundsPicker.SelectedIndex = 0;
        }

        private void GenerateBoard()
        {
            for (int i = 0; i < 36; i++)
            {
                var border = new Border
                {
                    BackgroundColor = Colors.White,
                    Stroke = Colors.Black,
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Padding = 0,
                    Margin = 0,
                    BindingContext = i // store index
                };
                
                var label = new Label
                {
                    Text = p[i].ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Black
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
            for (int i = 1; i <= 9; i++)
            {
                var border = new Border
                {
                    BackgroundColor = Colors.LightGreen,
                    Stroke = Colors.Black,
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Margin = new Thickness(5),
                    BindingContext = i
                };

                var label = new Label
                {
                    Text = i.ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Black
                };
                border.Content = label;

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += OnFactorTapped;
                border.GestureRecognizers.Add(tapGesture);

                FactorsGrid.Children.Add(border);
            }
        }

        private async void OnPlayClicked(object sender, EventArgs e)
        {
            if (RoundsPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Required", "Please select the number of rounds.", "OK");
                return;
            }
            int rounds = 5 + RoundsPicker.SelectedIndex;
            koniec = 3 * rounds;
            StartScreen.IsVisible = false;
            GameScreen.IsVisible = true;
            RestartGame();
        }

        private void OnRulesClicked(object sender, EventArgs e)
        {
            RulesPopup.IsVisible = true;
        }

        private void OnRulesCloseRequested(object sender, EventArgs e)
        {
            RulesPopup.IsVisible = false;
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Reset Game", "Are you sure you want to reset the game and return to the main screen?", "Yes", "No");
            if (answer)
            {
                StartScreen.IsVisible = true;
                GameScreen.IsVisible = false;
            }
        }

        private void OnNewGameClicked(object sender, EventArgs e)
        {
            StartScreen.IsVisible = true;
            GameScreen.IsVisible = false;
        }

        private void RestartGame()
        {
            krok = 1;
            zawodnik = 1;
            punkty[1] = 0;
            punkty[2] = 0;
            punkty[3] = 0;
            premia[1] = 0;
            premia[2] = 0;
            premia[3] = 0;

            for (int i = 0; i < 36; i++) z[i] = 0;

            Random rnd = new Random();
            liczba1 = rnd.Next(1, 10);
            liczba2 = 0;

            GameOverOverlay.IsVisible = false;
            WinnersContainer.Clear();

            UpdateUI();
        }

        private void UpdateUI()
        {
            Score1Lbl.Text = punkty[1].ToString();
            Score2Lbl.Text = punkty[2].ToString();
            Score3Lbl.Text = punkty[3].ToString();

            // Maintain player colors, only highlight current player border
            Player1Header.TextColor = Player1Color;
            Score1Lbl.TextColor = Player1Color;
            Player1ColBorder.Stroke = zawodnik == 1 ? Player1Color : Colors.Transparent;

            Player2Header.TextColor = Player2Color;
            Score2Lbl.TextColor = Player2Color;
            Player2ColBorder.Stroke = zawodnik == 2 ? Player2Color : Colors.Transparent;

            Player3Header.TextColor = Player3Color;
            Score3Lbl.TextColor = Player3Color;
            Player3ColBorder.Stroke = zawodnik == 3 ? Player3Color : Colors.Transparent;

            Color playerColor = zawodnik == 1 ? Player1Color : zawodnik == 2 ? Player2Color : Player3Color;

            int totalRounds = koniec / 3;
            int currentRound = Math.Min((krok - 1) / 3 + 1, totalRounds);
            RoundLbl.Text = $"{currentRound}/{totalRounds}";
            
            Factor1Lbl.Text = liczba1.ToString();
            Factor1Border.BackgroundColor = playerColor;

            if (liczba2 != 0)
            {
                Factor2Lbl.Text = liczba2.ToString();
                Factor2Border.BackgroundColor = playerColor;
                
                TipLbl.Text = "Select the product on the grid.";
                foreach (Border b in FactorsGrid.Children)
                {
                    b.BackgroundColor = Colors.LightGray;
                    b.IsEnabled = false;
                }
            }
            else
            {
                Factor2Lbl.Text = "?";
                Factor2Border.BackgroundColor = Colors.White;
                
                TipLbl.Text = "Select a multiplier from the bottom panel.";
                foreach (Border b in FactorsGrid.Children)
                {
                    b.BackgroundColor = Colors.LightGreen;
                    b.IsEnabled = true;
                }
            }

            // Update board colors
            for (int i = 0; i < 36; i++)
            {
                var border = (Border)BoardGrid.Children[i];
                if (z[i] == 1) border.BackgroundColor = Player1Color;
                else if (z[i] == 2) border.BackgroundColor = Player2Color;
                else if (z[i] == 3) border.BackgroundColor = Player3Color;
                else border.BackgroundColor = Colors.White;
            }
        }

        private void OnFactorTapped(object sender, EventArgs e)
        {
            if (liczba2 != 0) return; // already selected

            var border = (Border)sender;
            int factor = (int)border.BindingContext;
            liczba2 = factor;
            UpdateUI();
        }

        private async void OnGridCellTapped(object sender, EventArgs e)
        {
            if (liczba2 == 0) return; // factor 2 not selected yet

            var border = (Border)sender;
            int x = (int)border.BindingContext;

            if (liczba1 * liczba2 == p[x])
            {
                if (z[x] == 0)
                {
                    z[x] = zawodnik;

                    // points (diagonals logic translated)
                    if ((x > 6 && x < 11) || (x > 12 && x < 17) || (x > 18 && x < 23) || (x > 24 && x < 29)) // center
                    {
                        if (z[x] == z[x - 7]) punkty[zawodnik] += 20;
                        if (z[x] == z[x + 7]) punkty[zawodnik] += 20;
                        if (z[x] == z[x - 5]) punkty[zawodnik] += 20;
                        if (z[x] == z[x + 5]) punkty[zawodnik] += 20;
                    }
                    else if (x == 6 || x == 12 || x == 18 || x == 24) // left
                    {
                        if (z[x] == z[x + 7]) punkty[zawodnik] += 20;
                        if (z[x] == z[x - 5]) punkty[zawodnik] += 20;
                    }
                    else if (x == 11 || x == 17 || x == 23 || x == 29) // right
                    {
                        if (z[x] == z[x - 7]) punkty[zawodnik] += 20;
                        if (z[x] == z[x + 5]) punkty[zawodnik] += 20;
                    }
                    else if (x == 1 || x == 2 || x == 3 || x == 4) // top
                    {
                        if (z[x] == z[x + 7]) punkty[zawodnik] += 20;
                        if (z[x] == z[x + 5]) punkty[zawodnik] += 20;
                    }
                    else if (x == 31 || x == 32 || x == 33 || x == 34) // bottom
                    {
                        if (z[x] == z[x - 7]) punkty[zawodnik] += 20;
                        if (z[x] == z[x - 5]) punkty[zawodnik] += 20;
                    }
                    else if (x == 0) // top left
                    {
                        if (z[x] == z[x + 7]) punkty[zawodnik] += 20;
                    }
                    else if (x == 5) // top right
                    {
                        if (z[x] == z[x + 5]) punkty[zawodnik] += 20;
                    }
                    else if (x == 30) // bottom left
                    {
                        if (z[x] == z[x - 5]) punkty[zawodnik] += 20;
                    }
                    else if (x == 35) // bottom right
                    {
                        if (z[x] == z[x - 7]) punkty[zawodnik] += 20;
                    }

                    // PREMIA punktowa
                    if (premia[zawodnik] == 0)
                    {
                        for (int ii = 0; ii < 36; ii++)
                        {
                            if ((ii > 6 && ii < 11) || (ii > 12 && ii < 17) || (ii > 18 && ii < 23) || (ii > 24 && ii < 29)) // center
                            {
                                if ((z[ii - 1] == zawodnik && z[ii] == zawodnik && z[ii + 1] == zawodnik) || (z[ii - 6] == zawodnik && z[ii] == zawodnik && z[ii + 6] == zawodnik))
                                {
                                    punkty[zawodnik] += 30;
                                    premia[zawodnik] = 1;
                                    break;
                                }
                            }
                            else if (ii == 6 || ii == 12 || ii == 18 || ii == 24 || ii == 11 || ii == 17 || ii == 23 || ii == 29) // left and right
                            {
                                if (z[ii - 6] == zawodnik && z[ii] == zawodnik && z[ii + 6] == zawodnik)
                                {
                                    punkty[zawodnik] += 30;
                                    premia[zawodnik] = 1;
                                    break;
                                }
                            }
                            else if (ii == 1 || ii == 2 || ii == 3 || ii == 4 || ii == 31 || ii == 32 || ii == 33 || ii == 34) // top and bottom
                            {
                                if (z[ii - 1] == zawodnik && z[ii] == zawodnik && z[ii + 1] == zawodnik)
                                {
                                    punkty[zawodnik] += 30;
                                    premia[zawodnik] = 1;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "Correct answer, but the selected field is already taken!", "OK");
                    return;
                }
            }
            else
            {
                await DisplayAlert("Oops", "Unfortunately, that's the wrong answer!", "OK");
            }

            liczba1 = liczba2;
            liczba2 = 0;

            zawodnik++;
            if (zawodnik == 4) zawodnik = 1;

            CheckEndGame();
            UpdateUI();
        }

        private void CheckEndGame()
        {
            if (krok == koniec)
            {
                WinnersContainer.Clear();

                int maxPoints = Math.Max(punkty[1], Math.Max(punkty[2], punkty[3]));
                var winners = new System.Collections.Generic.List<int>();
                for (int i = 1; i <= 3; i++)
                {
                    if (punkty[i] == maxPoints)
                    {
                        winners.Add(i);
                    }
                }

                var winLabel = new Label
                {
                    Text = winners.Count > 1 ? "Winners:" : "Winner:",
                    FontSize = 24,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                WinnersContainer.Add(winLabel);

                foreach (int winnerId in winners)
                {
                    string winnerName = $"Player {winnerId}";
                    Color winnerColor = winnerId == 1 ? Player1Color : winnerId == 2 ? Player2Color : Player3Color;

                    var winnerBorder = new Border
                    {
                        Stroke = winnerColor,
                        StrokeThickness = 2,
                        BackgroundColor = Colors.White,
                        Padding = new Thickness(20, 10),
                        HorizontalOptions = LayoutOptions.Center,
                        Content = new HorizontalStackLayout
                        {
                            Spacing = 10,
                            VerticalOptions = LayoutOptions.Center,
                            Children =
                            {
                                new Label
                                {
                                    Text = winnerName,
                                    TextColor = winnerColor,
                                    FontSize = 22,
                                    FontAttributes = FontAttributes.Bold,
                                    VerticalOptions = LayoutOptions.Center
                                },
                                new Label
                                {
                                    Text = $"({punkty[winnerId]} pts)",
                                    TextColor = Colors.DarkGray,
                                    FontSize = 18,
                                    VerticalOptions = LayoutOptions.Center
                                }
                            }
                        }
                    };
                    WinnersContainer.Add(winnerBorder);
                }

                GameOverOverlay.IsVisible = true;
            }
            krok++;
        }
    }
}
