using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private int[,] board;
        private Button[,] buttons;
        private bool[,] flagged;
        private bool[,] flagEnabled;
        private int[,] flagScoreChange; 
        private int size;
        private int bombs;
        private int score;
        private int lives = 3;
        private int multiplier;
        private bool[,] visited;
        private HashSet<int> reachedExits;
        private Random rand = new Random();
        private bool isGameStarted = false;
        private const string HEART_EMOJI = "❤️";
        private MediaPlayer explosionSound;
        private MediaPlayer victorySound;

        public MainWindow()
        {
            InitializeComponent();
            SetupUI();
            SetupSounds();
        }

        private void SetupSounds()
        {
            explosionSound = new MediaPlayer();
            victorySound = new MediaPlayer();
            explosionSound.Open(new Uri("vine-boom-sound-meme.mp3", UriKind.Relative));
            victorySound.Open(new Uri("dejavu-ear.mp3", UriKind.Relative));
        }

        private void SetupUI()
        {
            DifficultyComboBox.SelectedIndex = 0;
            DifficultyComboBox.SelectionChanged += DifficultyComboBox_SelectionChanged;
            StartButton.Click += StartButton_Click;
            SurrenderButton.Click += SurrenderButton_Click;
            EndGameButton.Click += EndGameButton_Click;
            DifficultyComboBox_SelectionChanged(null, null);
            ClearGameGrid();
            RulesTextBlock.Visibility = Visibility.Visible;
            GameGrid.Visibility = Visibility.Hidden;
            GameBorder.Visibility = Visibility.Hidden;
            DifficultyComboBox.IsEnabled = true;
            StartButton.IsEnabled = true;
            SurrenderButton.Visibility = Visibility.Hidden;
            SurrenderButton.IsEnabled = false;
            LivesLabel.Content = HEART_EMOJI + HEART_EMOJI + HEART_EMOJI;
        }

        private void ClearGameGrid()
        {
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();
            RulesTextBlock.Visibility = Visibility.Visible;
            GameGrid.Visibility = Visibility.Hidden;
            GameBorder.Visibility = Visibility.Hidden;
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DifficultyComboBox.SelectedIndex == -1) return;

            switch (DifficultyComboBox.SelectedIndex)
            {
                case 0:
                    size = 10;
                    bombs = 10;
                    multiplier = 1;
                    break;
                case 1:
                    size = 15;
                    bombs = 15;
                    multiplier = 10;
                    break;
                case 2:
                    size = 20;
                    bombs = 20;
                    multiplier = 100;
                    break;
            }

            if (!isGameStarted)
            {
                ClearGameGrid();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (isGameStarted) return;

            score = 0;
            lives = 3;
            LivesLabel.Content = HEART_EMOJI + HEART_EMOJI + HEART_EMOJI;
            ScoreLabel.Visibility = Visibility.Hidden;
            SurrenderButton.Visibility = Visibility.Visible;
            SurrenderButton.IsEnabled = true;
            EndGameButton.Visibility = Visibility.Hidden;
            StartButton.IsEnabled = false;
            DifficultyComboBox.IsEnabled = false;
            isGameStarted = true;
            RulesTextBlock.Visibility = Visibility.Hidden;
            GameGrid.Visibility = Visibility.Visible;
            GameBorder.Visibility = Visibility.Visible;
            InitializeGame();
        }

        private void InitializeGame()
        {
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < size; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            buttons = new Button[size, size];
            board = new int[size, size];
            flagged = new bool[size, size];
            flagEnabled = new bool[size, size];
            visited = new bool[size, size];
            flagScoreChange = new int[size, size]; 
            reachedExits = new HashSet<int>();

            PlaceBombs();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var button = new Button
                    {
                        Background = i == 0 ? Brushes.LightGray : Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Content = "",
                        IsEnabled = i == 0
                    };
                    button.Click += Button_Click;
                    button.MouseRightButtonDown += Button_Flag;
                    button.Tag = (i, j);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GameGrid.Children.Add(button);
                    buttons[i, j] = button;
                }
            }

            for (int j = 0; j < size; j++)
            {
                flagEnabled[0, j] = true;
            }
        }

        private void PlaceBombs()
        {
            int bombsPlaced = 0;
            while (bombsPlaced < bombs)
            {
                int row = rand.Next(1, size);
                int col = rand.Next(size);
                if (board[row, col] == 0)
                {
                    board[row, col] = 1;
                    bombsPlaced++;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var (row, col) = ((int, int))button.Tag;

            if (flagged[row, col]) return;

            if (!visited[row, col])
            {
                visited[row, col] = true;
                if (board[row, col] == 1)
                {
                    lives--;
                    explosionSound.Stop();
                    explosionSound.Play();
                    LivesLabel.Content = string.Concat(Enumerable.Repeat(HEART_EMOJI, lives));
                    button.Background = Brushes.LightCoral;
                    button.Content = "💣";
                    score += multiplier;
                    if (lives <= 0)
                    {
                        MessageBox.Show($"Игра окончена! Вы проиграли.\nОчки: {score}");
                        ScoreLabel.Visibility = Visibility.Visible;
                        ScoreLabel.Content = $"Очки: {score}";
                        _ = ShowBombsAndClear();
                        return;
                    }
                }
                else
                {
                    UpdateCellColor(button, row, col);
                    if (row == size - 1 && !reachedExits.Contains(col))
                    {
                        reachedExits.Add(col);
                        score += 1000;
                        SurrenderButton.IsEnabled = false;
                        EndGameButton.Visibility = Visibility.Visible;
                    }
                    UpdateEnabledCells();
                    UpdateFlagEnabledCells();
                }
            }
        }

        private void Button_Flag(object sender, MouseButtonEventArgs e)
        {
            var button = (Button)sender;
            var (row, col) = ((int, int))button.Tag;

            if (visited[row, col]) return;

            if (!flagged[row, col]) 
            {
                if (!flagEnabled[row, col]) return;

                flagged[row, col] = true;
                button.Content = "🚩";
                if (board[row, col] == 1)
                {
                    flagScoreChange[row, col] = 100 * multiplier; 
                    score += flagScoreChange[row, col];
                }
                else
                {
                    flagScoreChange[row, col] = -30 * multiplier; 
                    score += flagScoreChange[row, col];
                }
            }
            else 
            {
                flagged[row, col] = false;
                button.Content = "";
                score -= flagScoreChange[row, col]; 
                flagScoreChange[row, col] = 0; 
            }

            UpdateEnabledCells();
            UpdateFlagEnabledCells();
        }

        private void UpdateCellColor(Button button, int row, int col)
        {
            bool hasBombLeft = col > 0 && board[row, col - 1] == 1;
            bool hasBombRight = col < size - 1 && board[row, col + 1] == 1;
            bool hasBombDown = row < size - 1 && board[row + 1, col] == 1;

            if (hasBombDown && (hasBombLeft || hasBombRight))
            {
                LinearGradientBrush gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0, 0);
                gradientBrush.EndPoint = new Point(1, 1);
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Pink, 0.0));
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));
                button.Background = gradientBrush;
            }
            else if (hasBombDown)
            {
                button.Background = Brushes.Pink;
            }
            else if (hasBombLeft || hasBombRight)
            {
                button.Background = Brushes.Yellow;
            }
            else
            {
                button.Background = Brushes.LightGreen;
            }
        }

        private void UpdateEnabledCells()
        {
            for (int r = 1; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (!visited[r, c])
                    {
                        buttons[r, c].IsEnabled = false;
                    }
                }
            }

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (visited[r, c] && (buttons[r, c].Background == Brushes.Pink || buttons[r, c].Background == Brushes.Yellow || buttons[r, c].Background == Brushes.LightGreen || buttons[r, c].Background is LinearGradientBrush))
                    {
                        ActivateAdjacentCellsForSteps(r, c);
                    }
                }
            }

            for (int j = 0; j < size; j++)
            {
                if (!visited[0, j])
                {
                    buttons[0, j].IsEnabled = true;
                }
            }

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (flagged[r, c] && !visited[r, c])
                    {
                        buttons[r, c].IsEnabled = true;
                    }
                }
            }
        }

        private void ActivateAdjacentCellsForSteps(int row, int col)
        {
            var directions = new (int, int)[]
            {
                (row + 1, col),
                (row, col - 1),
                (row, col + 1)
            };

            foreach (var (r, c) in directions)
            {
                if (r >= 0 && r < size && c >= 0 && c < size && !visited[r, c])
                {
                    buttons[r, c].IsEnabled = true;
                }
            }
        }

        private void UpdateFlagEnabledCells()
        {
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (!visited[r, c])
                    {
                        flagEnabled[r, c] = false;
                    }
                }
            }

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (visited[r, c] && (buttons[r, c].Background == Brushes.Pink || buttons[r, c].Background == Brushes.Yellow || buttons[r, c].Background == Brushes.LightGreen || buttons[r, c].Background is LinearGradientBrush))
                    {
                        ActivateAdjacentCellsForFlags(r, c);
                    }
                }
            }

            for (int j = 0; j < size; j++)
            {
                if (!visited[0, j])
                {
                    flagEnabled[0, j] = true;
                }
            }
        }

        private void ActivateAdjacentCellsForFlags(int row, int col)
        {
            var directions = new (int, int)[]
            {
                (row + 1, col),
                (row, col - 1),
                (row, col + 1)
            };

            foreach (var (r, c) in directions)
            {
                if (r >= 0 && r < size && c >= 0 && c < size && !visited[r, c])
                {
                    flagEnabled[r, c] = true;
                }
            }
        }

        private async Task ShowBombsAndClear()
        {
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] == 1 && !visited[r, c])
                    {
                        buttons[r, c].Background = Brushes.LightCoral;
                        buttons[r, c].Content = "💣";
                    }
                }
            }

            await Task.Delay(2000);

            isGameStarted = false;
            StartButton.IsEnabled = true;
            DifficultyComboBox.IsEnabled = true;
            SurrenderButton.Visibility = Visibility.Hidden;
            SurrenderButton.IsEnabled = false;
            EndGameButton.Visibility = Visibility.Hidden;
            ClearGameGrid();
        }

        private async void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            if (reachedExits.Count == 0)
            {
                ScoreLabel.Content = $"Очки: {score}";
                ScoreLabel.Visibility = Visibility.Visible;
                MessageBox.Show($"Вы сдались! Игра окончена.\nОчки: {score}");
                await ShowBombsAndClear();
            }
        }

        private async void EndGameButton_Click(object sender, RoutedEventArgs e)
        {
            victorySound.Stop();
            victorySound.Play();

            ScoreLabel.Content = $"Очки: {score}";
            ScoreLabel.Visibility = Visibility.Visible;
            MessageBox.Show($"Игра окончена!\nОчки: {score}");
            await ShowBombsAndClear();
        }
    }
}