public class Cell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsBomb { get; set; }
    public bool IsVisited { get; set; }
    public bool IsStart { get; set; }
    public bool IsEnd { get; set; }
    public bool IsPath { get; set; }
    public bool IsFlagged { get; set; }
}