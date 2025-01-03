namespace Core.Entities;

public class WindowBounds
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int TopLeftX { get; set; }
    public int TopLeftY { get; set; }

    public override string ToString()
    {
        return $"TopLeftX={TopLeftX}, TopLeftY={TopLeftY}, Width={Width}, Height={Height}";
    }
}
