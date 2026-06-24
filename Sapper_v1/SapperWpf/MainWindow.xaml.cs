using System.Windows;

namespace SapperWpf
{
    public partial class MainWindow : Window
    {
        private Game _game;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnEasyClick(object sender, RoutedEventArgs e)
        {
            StartGame(10, 10, 1);
        }

        private void OnMediumClick(object sender, RoutedEventArgs e)
        {
            StartGame(15, 30, 5);
        }

        private void OnHardClick(object sender, RoutedEventArgs e)
        {
            StartGame(20, 60, 10);
        }

        private void StartGame(int size, int bombs, int scoreMultiplier)
        {
            _game = new Game(this, size, bombs, scoreMultiplier);
            _game.Start();
        }

        private void OnGiveUpClick(object sender, RoutedEventArgs e)
        {
            _game.GiveUp();
        }

        public void UpdateBoard(Cell[,] cells, int rows, int cols)
        {
            BoardGrid.Rows = rows;
            BoardGrid.Columns = cols;
            BoardGrid.Children.Clear();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var cell = cells[row, col];
                    var btn = new CellButton(cell);
                    btn.GoHere += () => _game.OnCellClick(cell);
                    btn.PlaceFlag += () => _game.OnPlaceFlag(btn, cell);
                    BoardGrid.Children.Add(btn);
                }
            }
        }

        public void UpdateStatus(string text)
        {
            StatusText.Text = text;
            GiveUpButton.Visibility = (_game.HasExitBeenFound() ? Visibility.Visible : Visibility.Collapsed);
        }
    }
}