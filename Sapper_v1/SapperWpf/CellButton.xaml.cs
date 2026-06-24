using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class CellButton : Button
{
    private Grid _contentGrid;
    private Rectangle _topRect;
    private Rectangle _bottomRect;
    private TextBlock _flagText;

    public CellButton(Cell cell)
    {
        Tag = cell;
        BorderBrush = Brushes.Black;
        BorderThickness = new Thickness(1);
        Background = Brushes.LightGray;

        _contentGrid = new Grid();
        _contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        _topRect = new Rectangle();
        _bottomRect = new Rectangle();

        Grid.SetRow(_topRect, 0);
        Grid.SetRow(_bottomRect, 1);

        _contentGrid.Children.Add(_topRect);
        _contentGrid.Children.Add(_bottomRect);

        _flagText = new TextBlock()
        {
            Text = "🚩",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16,
            Visibility = Visibility.Collapsed
        };
        _contentGrid.Children.Add(_flagText);

        Content = _contentGrid;

        ContextMenu = new ContextMenu();

        var goHereItem = new MenuItem { Header = "Идти сюда" };
        var flagItem = new MenuItem { Header = "Поставить флажок" };

        goHereItem.Click += (s, e) => RaiseGoHere();
        flagItem.Click += (s, e) => RaisePlaceFlag();

        ContextMenu.Items.Add(goHereItem);
        ContextMenu.Items.Add(flagItem);

        MouseRightButtonDown += (s, e) =>
        {
            if (!cell.IsVisited)
                ContextMenu.IsOpen = true;
        };
    }

    public event Action GoHere;
    public event Action PlaceFlag;

    private void RaiseGoHere() => GoHere?.Invoke();
    private void RaisePlaceFlag() => PlaceFlag?.Invoke();

    public void SetColor(Brush color)
    {
        _topRect.Fill = color;
        _bottomRect.Fill = color;
    }

    public void SetDualColor(Brush topColor, Brush bottomColor)
    {
        _topRect.Fill = topColor;
        _bottomRect.Fill = bottomColor;
    }

    public void SetFlagVisible(bool visible)
    {
        _flagText.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }
}