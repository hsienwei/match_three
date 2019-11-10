namespace Match3
{
  // 方位的定義.
  public enum Direction
  {
    DOWN,
    UP,
    LEFT,
    RIGHT,
    LD,
    RD,
    LU,
    RU,
    MAX
  };

  public struct IntVector2
  {
    int m_x;
    int m_y;

    public int GemColor { get { return m_x; } }
    public int GemType { get { return m_y; } }

    public IntVector2(int x, int y)
    {
      m_x = x;
      m_y = y;
    }
  }

  public struct GridPos
  {
    public int x;
    public int y;

    public GridPos(int vx, int vy)
    { x = vx; y = vy; }
  }
}
