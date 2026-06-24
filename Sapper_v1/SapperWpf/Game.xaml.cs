using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SapperWpf
{
    public class Game
    {
        private readonly MainWindow _window;
        private Cell[,] _cells;
        private int _rows;
        private int _cols;
        private int _totalBombs;
        private int _score;
        private int _lives = 3;
        private int _scoreMultiplier;
        private HashSet<(int, int)> _visitedPositions = new HashSet<(int, int)>();
        private List<(int, int)> _exits = new List<(int, int)>();

        public Game(MainWindow window, int size, int totalBombs, int scoreMultiplier)
        {
            _window = window;
            _rows = size;
            _cols = size;
            _totalBombs = totalBombs;
            _scoreMultiplier = scoreMultiplier;
        }

        public void Start()
        {
            GenerateBoard();
            _window.UpdateBoard(_cells, _rows, _cols);
            _window.UpdateStatus($"Очки: {_score}, Жизни: {_lives}");
        }

        private void GenerateBoard()
        {
            _cells = new Cell[_rows, _cols];

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    _cells[r, c] = new Cell { Row = r, Col = c };
                }
            }

            var rand = new Random();
            int placed = 0;
            while (placed < _totalBombs)
            {
                int r = rand.Next(1, _rows);
                int c = rand.Next(0, _cols);
                if (!_cells[r, c].IsBomb)
                {
                    _cells[r, c].IsBomb = true;
                    placed++;
                }
            }

            GeneratePath();
        }

        private void GeneratePath()
        {
            var rand = new Random();
            int startCol;
            do
            {
                startCol = rand.Next(0, _cols);
            } while (_cells[0, startCol].IsBomb);

            var startCell = _cells[0, startCol];
            startCell.IsStart = true;

            int targetRow = _rows - 1;
            int targetCol = rand.Next(0, _cols);
            _cells[targetRow, targetCol].IsEnd = true;
            _exits.Add((targetRow, targetCol));

            int row = 0, col = startCol;
            while (row < _rows - 1)
            {
                int dir = rand.Next(0, 3);
                if (dir == 0 && col > 0) col--;
                else if (dir == 1 && col < _cols - 1) col++;
                else if (dir == 2) { }

                row++;
                _cells[row, col].IsPath = true;
            }
        }

        public void OnCellClick(Cell cell)
        {
            if (cell.IsVisited || (_visitedPositions.Count > 0 && !IsAdjacent(cell)))
                return;

            cell.IsVisited = true;
            _visitedPositions.Add((cell.Row, cell.Col));

            if (cell.IsBomb && !cell.IsFlagged)
            {
                _lives--;
                if (_lives <= 0)
                {
                    MessageBox.Show("Вы проиграли!");
                    ResetGame();
                    return;
                }
            }

            Brush color = Brushes.Gray;
            var adjacentBombs = CountAdjacentBombs(cell);
            var horizontalBombs = CountHorizontalBombs(cell);
            var frontBomb = HasFrontBomb(cell);

            if (cell.IsBomb && !cell.IsFlagged)
            {
                foreach (CellButton btn in _window.BoardGrid.Children)
                {
                    if ((btn.Tag as Cell)?.Row == cell.Row && (btn.Tag as Cell)?.Col == cell.Col)
                    {
                        btn.SetColor(Brushes.Red);
                        break;
                    }
                }
            }
            else if (horizontalBombs > 0 && frontBomb)
            {
                foreach (CellButton btn in _window.BoardGrid.Children)
                {
                    if ((btn.Tag as Cell)?.Row == cell.Row && (btn.Tag as Cell)?.Col == cell.Col)
                    {
                        btn.SetDualColor(Brushes.Pink, Brushes.Yellow);
                        break;
                    }
                }
            }
            else if (horizontalBombs > 0)
            {
                color = Brushes.Yellow;
            }
            else if (frontBomb)
            {
                color = Brushes.Pink;
            }
            else
            {
                color = Brushes.Violet;
            }

            if (!(horizontalBombs > 0 && frontBomb) && !(cell.IsBomb && !cell.IsFlagged))
            {
                foreach (CellButton btn in _window.BoardGrid.Children)
                {
                    if ((btn.Tag as Cell)?.Row == cell.Row && (btn.Tag as Cell)?.Col == cell.Col)
                    {
                        btn.SetColor(color);
                        break;
                    }
                }
            }

            if (cell.IsEnd && !_exits.Contains((cell.Row, cell.Col)))
            {
                _score += 1000;
                _exits.Add((cell.Row, cell.Col));
            }

            _window.UpdateStatus($"Очки: {_score}, Жизни: {_lives}");

            if (CheckWin())
            {
                MessageBox.Show("Вы выиграли!");
                ResetGame();
            }
        }

        public void OnPlaceFlag(CellButton btn, Cell cell)
        {
            if (cell.IsVisited || cell.IsFlagged)
                return;

            cell.IsFlagged = true;
            btn.SetFlagVisible(true);
            _score += _scoreMultiplier;
            _window.UpdateStatus($"Очки: {_score}, Жизни: {_lives}");
        }

        public void GiveUp()
        {
            foreach (CellButton btn in _window.BoardGrid.Children)
            {
                var cell = btn.Tag as Cell;
                if (cell.IsBomb && !cell.IsFlagged)
                {
                    btn.SetColor(Brushes.Red);
                }
            }

            MessageBox.Show("Вы сдались. Игра окончена.");
            ResetGame();
        }

        public bool HasExitBeenFound() => _exits.Count > 0;

        private bool CheckWin()
        {
            return _visitedPositions.Count >= _rows * _cols;
        }

        private bool IsAdjacent(Cell cell)
        {
            foreach (var pos in _visitedPositions)
            {
                if (Math.Abs(pos.Item1 - cell.Row) <= 1 &&
                    Math.Abs(pos.Item2 - cell.Col) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private int CountAdjacentBombs(Cell cell)
        {
            int count = 0;
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <= 1; c++)
                {
                    int nr = cell.Row + r;
                    int nc = cell.Col + c;
                    if (nr >= 0 && nr < _rows && nc >= 0 && nc < _cols && (r != 0 || c != 0))
                    {
                        if (_cells[nr, nc].IsBomb)
                            count++;
                    }
                }
            }
            return count;
        }

        private int CountHorizontalBombs(Cell cell)
        {
            int count = 0;
            int[] dc = { -1, 1 };
            foreach (var d in dc)
            {
                int nc = cell.Col + d;
                if (nc >= 0 && nc < _cols && _cells[cell.Row, nc].IsBomb)
                    count++;
            }
            return count;
        }

        private bool HasFrontBomb(Cell cell)
        {
            int row = cell.Row;
            int col = cell.Col;

            if (row < _rows - 1 && _cells[row + 1, col].IsBomb)
                return true;

            return false;
        }

        private void ResetGame()
        {
            _score = 0;
            _lives = 3;
            _visitedPositions.Clear();
            _exits.Clear();
            Start();
        }
    }
}