using System;
using System.Collections.Generic;



public class MatchThreeCore
{
  enum GemType{
    NormalStart = 0,
    LineStart = 6,
    BombStart = 18,
    SameAllStart = 24,
  }

  public class GridState
  {

    public int MatchCount;
    public Gem m_Gem;

    public GridState()
    {
      m_Gem = null;
      MatchCount = 0;
    }

    public void GenGem(int Color, GridPos Pos)
    {
      if (m_Gem == null)
      {
        m_Gem = new Gem(Color, Pos);
      }
    }

    public bool IsCanMove()
    {
      if (m_Gem == null) return false;
      return m_Gem.IsCanMove();
    }

    public void Clear()
    {
      //m_Gem = null;
      m_Gem.SetClear();
    }
  }

  public class Gem
  {
    enum State
    {
      Idle,
      Moving,
      Clear,
    }

    public GridPos m_TempPos;
    private int m_Color;
    private State m_State = State.Idle;
    private Action<Gem> m_Callback;

    private int m_Countdown;

    public int Color
    {
      get
      {
        return m_Color;
      }
    } 

    public int MatchColor
    {
      get
      {
        if (m_Color < (int)GemType.LineStart)
          return m_Color;
        else if (m_Color < (int)GemType.BombStart)
          return (m_Color - (int)GemType.LineStart) / 2;
        else if (m_Color < (int)GemType.SameAllStart)
          return (m_Color - (int)GemType.BombStart);
        else 
          return m_Color;
      }
    }

    public Gem(int Color, GridPos Pos)
    {
      m_Color = Color;
      m_TempPos = Pos;
    }

    public void Update(int DeltaTime)
    {
      m_Countdown -= DeltaTime;
      if (m_Countdown <= 0)
      {
        m_State = State.Idle;
        if (m_Callback != null)
          m_Callback(this);

      }
    }

    public bool IsCanMove()
    {
      return m_State == State.Idle;
    }

    public void SetCountdown(int Cnt, Action<Gem> Callback = null)
    {
      m_Countdown = Cnt;
      m_State = State.Moving;
      m_Callback = null;
      m_Callback = Callback;
    }


    public void SetClear()
    {
      m_State = State.Clear;
    }

    public bool IsClear()
    {
      return m_State == State.Clear;
    }
  }

  public struct GridPos
  {
    public int x;
    public int y;

    public GridPos(int vx, int vy) 
    { x = vx; y = vy; }
  }

  class SwipeRecord
  {
    public int m_x1, m_y1, m_x2, m_y2;
    public SwipeRecord(int x1, int y1, int x2, int y2)
    {
      m_x1 = x1;
      m_y1 = y1;
      m_x2 = x2;
      m_y2 = y2;
    }

    
  };

  class MoveRecord
  {
    private static readonly string FormatStr = "[{0}, {1} : {2}]";

    int m_x1, m_y1;
    Direction m_Direction;
    public MoveRecord(int x1, int y1, Direction Direction)
    {
      m_x1 = x1;
      m_y1 = y1;
      m_Direction = Direction;
    }

    public override string ToString()
    {
      return string.Format(FormatStr, m_x1, m_y1, m_Direction);
    }
  };

  class MatchRecord{

    public int m_x1, m_y1, m_x2, m_y2;
    public MatchRecord(int x1, int y1, int x2, int y2)
    {
      m_x1 = x1;
      m_y1 = y1;
      m_x2 = x2;
      m_y2 = y2;
    
    }

    public bool IsInside(int x, int y)
    {
      return x >= m_x1 && x <= m_x2 && y >= m_y1 && y <= m_y2;
    }

    public bool IsVertical()
    {
      return m_y1 == m_y2;
    }

    public List<GridPos> GetGridPos()
    {
      List<GridPos> List = new List<GridPos>();
            
      for (int i = m_x1; i <= m_x2; ++i)
      {
        for (int j = m_y1; j <= m_y2; ++j)
        {
          List.Add(new GridPos(i, j) );
        }
      }
      return List;
    }

  };

  enum Direction
  { 
    DOWN,
    UP,
    LEFT,
    RIGHT,
  };

  public int GridPosToIdx(GridPos g) { return g.x + g.y * m_Col; }
  public int GridPosToIdx(int x, int y) { return x + y * m_Col;  }
  public void GridIdxToPos(int idx, ref int x, ref int y) { x =  idx % m_Col;  y = idx / m_Col; }

  public static readonly int MOVE_TYPE_MOVE = 1;
  public static readonly int MOVE_TYPE_SWITCH = 2;
  public static readonly int MOVE_TYPE_SWITCHBACK = 3;

  public delegate void OnGenerate(int Col, int Row, int Color);
  public delegate void OnClear(int Col, int Row);
  public delegate void OnMove(int Col, int Row, int TargetCol, int TargetRow, int Type);
  public delegate void OnLock(int Col, int Row, int Count);
  public delegate void OnLog(string Log);

  public OnGenerate m_CBGenerate;
  public OnClear m_CBClear;
  public OnMove m_CBMove;
  public OnLock m_CBLock;
  public OnLog m_CBLog;
  public OnLog m_CBLog2;

  private GridState[,] m_Grid;
  private int m_Row, m_Col;
  private int m_ColorCount;
  private bool m_IsHasMatch;

  private Random m_Rand;

  private List<SwipeRecord> m_SwipeRecs ;
  private List<MoveRecord> m_PossibleMove;
  private List<MatchRecord> m_MatchRecs;
  private Dictionary<int, int> m_GemAssignList;
  private List<int> m_GemForceRemoveList;

  private bool m_IsMatchDebug = false;
  

  public MatchThreeCore(int Col, int Row, int ColorCount)
  {
    m_Grid = new GridState[Col, Row];
    m_Row = Row;
    m_Col = Col;
    m_ColorCount = ColorCount;

    m_Rand = new Random();

    m_SwipeRecs = new List<SwipeRecord>();
    m_PossibleMove = new List<MoveRecord>();
    m_MatchRecs = new List<MatchRecord>();
    m_GemAssignList = new Dictionary<int, int>();
    m_GemForceRemoveList = new List<int>();
  }

  int Col { get { return m_Col; } }
  int Row { get { return m_Row; } }

  //public void ChangeGem(GridState Grid1, GridState Grid2)
  public void ChangeGem(int x1, int y1, int x2, int y2)
  {
    GridState Grid1 = m_Grid[x1, y1];
    GridState Grid2 = m_Grid[x2, y2];

    var Tmp = Grid1.m_Gem;
    Grid1.m_Gem = Grid2.m_Gem;
    Grid2.m_Gem = Tmp;

    if (Grid1.m_Gem != null)
    {
      Grid1.m_Gem.m_TempPos.x = x1;
      Grid1.m_Gem.m_TempPos.y = y1;
    }
    if (Grid2.m_Gem != null)
    {
      Grid2.m_Gem.m_TempPos.x = x2;
      Grid2.m_Gem.m_TempPos.y = y2;
    }
  }

  public int GetColor(int Col, int Row)
  {
    if (Col < 0 || Col >= m_Col) return -1;
    if (Row < 0 || Row >= m_Row) return -1;
    if (m_Grid[Col, Row].m_Gem == null)
      return -1;
    return m_Grid[Col, Row].m_Gem.Color;
  }

  public int GetMatchColor(int Col, int Row)
  {
    if (Col < 0 || Col >= m_Col) return -1;
    if (Row < 0 || Row >= m_Row) return -1;
    if (m_Grid[Col, Row].m_Gem == null)
      return -1;
    return m_Grid[Col, Row].m_Gem.MatchColor;
  }

  public int GetClearCount(int Col, int Row)
  {
    return m_Grid[Col, Row].MatchCount;
  }

  public void Update(int TimeUnit)
  {
    // 執行swipe行為.
    for(int i=0; i< m_SwipeRecs.Count; ++i)
    {
      var Rec = m_SwipeRecs[i];

      ChangeGem(Rec.m_x1, Rec.m_y1, Rec.m_x2, Rec.m_y2);
      m_CBMove(Rec.m_x2, Rec.m_y2, Rec.m_x1, Rec.m_y1, MOVE_TYPE_SWITCH);

      m_Grid[Rec.m_x1, Rec.m_y1].m_Gem.SetCountdown(300);
      m_Grid[Rec.m_x2, Rec.m_y2].m_Gem.SetCountdown(300);
    }
    m_SwipeRecs.Clear();

    // 更新珠子, 清除已消除資料.
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        if (m_Grid[i, j].m_Gem != null)
        {
          m_Grid[i, j].m_Gem.Update(TimeUnit);
          if (m_Grid[i, j].m_Gem.IsClear())
            m_Grid[i, j].m_Gem = null;
        }
      
      }
    }

    if (!m_IsMatchDebug)
    {
      ScanMatch();
      CleanMatchState();
    }
    AssignGemGen();
    GemDrop();
    Generate(false);
    ScanMatchPossible();
    CheckReset();
  }

  void AssignGemGen()
  {
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {

        if (m_Grid[i, j].m_Gem == null)
        {
          //TODO: 檢查上方掉落
          // 先補再產, 兩輪

          // 上方沒有的話要產出

          var Idx = GridPosToIdx(i, j);
          if (m_GemAssignList.ContainsKey(Idx))
          {
            m_Grid[i, j].GenGem(m_GemAssignList[Idx], new GridPos(i, j));
            m_GemAssignList.Remove(Idx);

            m_Grid[i, j].m_Gem.SetCountdown(300);
            if (m_CBGenerate != null)
            {
              m_CBGenerate(i, j, GetColor(i, j));
            }
          }
          


          
        }
      }
    }

  }

  void CheckReset()
  {
    if (m_SwipeRecs.Count > 0) return;
    if (m_PossibleMove.Count > 0) return;
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        if (!m_Grid[i, j].IsCanMove()) return;
        if (m_Grid[i, j].MatchCount >0) return;
      }
    }

    Reset();
  }

  void ScanMatchPossible()
  {
    m_PossibleMove.Clear();
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        CheckMatchPossible(i, j);
      }
    }

    /*System.Text.StringBuilder m_TempBuilder = new System.Text.StringBuilder();
    m_TempBuilder.Remove(0, m_TempBuilder.Length);
    for (int i=0; i< m_PossibleMove.Count; ++i)
    {
      m_TempBuilder.Append(m_PossibleMove[i]);
    }
    m_CBLog2(m_TempBuilder.ToString());*/
  }

  int CheckMatchPossible(int Col, int Row)
  {
    //  上移　　　　左移　     右移      下移
    // ...|...   .......   .......   .......
    // ...|...   ..|....   ....|..   .......
    // .--+--.   ..|....   ....|..   .......
    // ...O...   --+O...   ...O+--   ...O...
    // .......   ..|....   ....|..   .--+--.
    // .......   ..|....   ....|..   ...|...
    // .......   .......   .......   ...|...

    if (!m_Grid[Col, Row].IsCanMove())
      return 0;

    int CurrColor = GetColor(Col, Row);
    if (CurrColor == -1) return 0;

    int Cnt = 0;

    do
    {
      // 左移左
      if (CurrColor == GetColor(Col - 2, Row) && CurrColor == GetColor(Col - 3, Row))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.LEFT));
        break;
      }
      // 左移上
      if (CurrColor == GetColor(Col - 1, Row - 1) && CurrColor == GetColor(Col - 1, Row - 2))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.LEFT));
        break;
      }
      // 左移下
      if (CurrColor == GetColor(Col - 1, Row + 1) && CurrColor == GetColor(Col - 1, Row + 2))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.LEFT));
        break;
      }
      // 左移中
      if (CurrColor == GetColor(Col - 1, Row + 1) && CurrColor == GetColor(Col - 1, Row - 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.LEFT));
        break;
      }
    } while (false);

    do
    {
      // 右移右
      if (CurrColor == GetColor(Col + 2, Row) && CurrColor == GetColor(Col + 3, Row))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.RIGHT));
        break;
      }
      // 右移上
      if (CurrColor == GetColor(Col + 1, Row - 1) && CurrColor == GetColor(Col + 1, Row - 2))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.RIGHT));
        break;
      }
      // 右移下
      if (CurrColor == GetColor(Col + 1, Row + 1) && CurrColor == GetColor(Col + 1, Row + 2))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.RIGHT));
        break;
      }
      // 右移中
      if (CurrColor == GetColor(Col + 1, Row + 1) && CurrColor == GetColor(Col + 1, Row - 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.RIGHT));
        break;
      }
    } while (false);

    do
    {
      // 上移上
      if (CurrColor == GetColor(Col, Row - 2) && CurrColor == GetColor(Col, Row - 3))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.UP));
        break;
      }
      // 上移左
      if (CurrColor == GetColor(Col - 1, Row - 1) && CurrColor == GetColor(Col - 2, Row - 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.UP));
        break;
      }
      // 上移右
      if (CurrColor == GetColor(Col + 1, Row - 1) && CurrColor == GetColor(Col + 2, Row - 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.UP));
        break;
      }
      // 上移中
      if (CurrColor == GetColor(Col + 1, Row - 1) && CurrColor == GetColor(Col - 1, Row - 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.UP));
        break;
      }
    } while (false);

    do
    {
      // 下移下
      if (CurrColor == GetColor(Col, Row + 2) && CurrColor == GetColor(Col, Row + 3))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.DOWN));
        break;
      }
      // 下移左
      if (CurrColor == GetColor(Col - 1, Row + 1) && CurrColor == GetColor(Col - 2, Row + 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.DOWN));
        break;
      }
      // 下移右
      if (CurrColor == GetColor(Col + 1, Row + 1) && CurrColor == GetColor(Col + 2, Row + 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.DOWN));
        break;
      }
      // 下移中
      if (CurrColor == GetColor(Col + 1, Row + 1) && CurrColor == GetColor(Col - 1, Row + 1))
      {
        m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.DOWN));
        break;
      }
    } while (false);

    return Cnt;
  }

  public void Generate(bool IsInit = false)
  {
    if (IsInit)
    {
      for (int i = 0; i < m_Col; ++i)
      {
        for (int j = 0; j < m_Row; ++j)
        {
          if (IsInit)
          {
            m_Grid[i, j] = new GridState();
            m_Grid[i, j].GenGem(m_Rand.Next(0, m_ColorCount), new GridPos(i, j));
            m_Grid[i, j].m_Gem.SetCountdown(300);
            m_CBGenerate(i, j, GetColor(i, j));
          }
        }
      }
      return;
    }


    //for (int j = m_Row - 1; j >= 0; --j)
    /*for (int j = 0; j < m_Row; ++j)
    {
      for (int i = 0; i < m_Col; ++i)
      {
        if (GetColor(i, j) == -1)
        {
          // 先補再產
          // 上方沒有的話要產出.
          // 先只看正上方.

          //for(int k = j-1;  k >=0 ; --k)
          for (int k = j + 1; k < m_Row; ++k)
          {
            if (GetColor(i, k) != -1)
            {
              m_Grid[i, j].Color = GetColor(i, k);
              m_Grid[i, k].Color = -1;
              m_Grid[i, j].LockCnt = 1000 ;
              if (m_CBMove != null)
              {
                m_CBMove(i, k, i, j, MOVE_TYPE_MOVE);
              }
              break;
            }
           
          }
        }
      }
    }
    */
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {

        if (m_Grid[i, j].m_Gem == null)
        {
          //TODO: 檢查上方掉落
          // 先補再產, 兩輪

          // 上方沒有的話要產出

          m_Grid[i, j].GenGem(m_Rand.Next(0, m_ColorCount), new GridPos(i, j));

          
          m_Grid[i, j].m_Gem.SetCountdown(300);
          if (m_CBGenerate != null)
          {
            m_CBGenerate(i, j, GetColor(i, j));
          }
        }
      }
    }
    
  }

  public bool Swipe(int Col, int Row, int Direction)
  {
    int CurrColor = GetColor(Col, Row);
    int TargetCol = Col, TargetRow = Row;

    switch (Direction)
    {
      case 0:
        TargetRow++; break;
      case 1:
        TargetRow--; break;
      case 2:
        TargetCol--; break;
      case 3:
        TargetCol++; break;
    }

    if (TargetCol < 0) return false;
    if (TargetCol >= m_Col) return false;
    if (TargetRow < 0) return false;
    if (TargetRow >= m_Row) return false;

    //if (m_Grid[Col, Row].LockCnt > 0) return false;
    if(!m_Grid[Col, Row].IsCanMove()) return false;
    //if (m_Grid[TargetCol, TargetRow].LockCnt > 0) return false;
    if (!m_Grid[TargetCol, TargetRow].IsCanMove()) return false;

    m_SwipeRecs.Add(new SwipeRecord(Col, Row, TargetCol, TargetRow));
    
    return true;
  }

  public void ScanMatch()
  {
    
    m_IsHasMatch = false;
    m_MatchRecs.Clear();

    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        if (CheckMatch(i, j))
        {
          m_IsHasMatch = true;
        }
      }
    }

    test();
  }

  public bool IsHasClearState()
  {
    return m_IsHasMatch;
  }


  
  class MatchGrid : IComparable<MatchGrid>
  {
    GridPos m_BasePos;

    public int m_v;
    public int m_h;

    List<MatchRecord> Recs;
    List<GridPos> m_AllGrid;
    public MatchGrid(int x, int y)
    {
      m_BasePos = new GridPos(x, y);
      Recs = new List<MatchRecord>();
      m_AllGrid = new List<GridPos>();
    }

    public void AddMatchRecord(MatchRecord Rec)
    {
      Recs.Add(Rec);
      if (Rec.IsVertical()) 
        m_v++;
      else 
        m_h++;

      var PosList = Rec.GetGridPos();
      for(int i=0; i< PosList.Count; ++i)
      {
        if(!m_AllGrid.Contains( PosList[i]))
        {
          m_AllGrid.Add(PosList[i]);
        }
      }
    }

    public GridPos BasePos { get { return m_BasePos; } }
    public List<GridPos> AllPos { get {return  m_AllGrid; } }

    public bool IsMatch5()
    {
      return m_v == 3 || m_h == 3;
    }

    public bool IsMatchT()
    {
      return m_v >= 1 && m_h >= 1;
    }

    public bool IsMatch4()
    {
      return m_v >= 2 || m_h >= 2;
    }

    public bool IsMatch3()
    {
      return m_v >= 1 || m_h >= 1;
    }

    public int CompareTo(MatchGrid other)
    {
      if (other == null) return -1;
      int Compare = Recs.Count.CompareTo(other.Recs.Count);
      if(Compare == 0)
      {
        if (Math.Abs(m_v - m_h) < Math.Abs(other.m_v - other.m_h))
          Compare = 1;
        else
          Compare = 0;
      }
      return -Compare;
    }

    public override string ToString()
    {
      return "" + m_BasePos.x + "," + m_BasePos.y + "," + m_v + "," + m_h;
    }
  };

  void test()
  {
    // 建立格子的索引
    Dictionary<int, MatchGrid> List = new Dictionary<int, MatchGrid>();
    for (int i = 0; i < m_MatchRecs.Count; ++i)
    {
      var Grids = m_MatchRecs[i].GetGridPos();
      for (int j = 0; j < Grids.Count; ++j)
      {
        int Idx = this.GridPosToIdx(Grids[j]);

        if(!List.ContainsKey(Idx))
        {
          List[Idx] = new MatchGrid(Grids[j].x, Grids[j].y);
        }
        List[Idx].AddMatchRecord(m_MatchRecs[i]);
      }
    }
    // 依照該格的消除次數做排序.
    List<MatchGrid> List2 = new List<MatchGrid>();
    foreach (KeyValuePair<int, MatchGrid> entry in List)
    {
      List2.Add(entry.Value);
      
    }
    List2.Sort();

    // 
    foreach (MatchGrid entry in List2)
    {
      int BaseIdx = GridPosToIdx(entry.BasePos);
      if (!List.ContainsKey(BaseIdx)) continue;

      if (entry.IsMatch5())
      {
        // 移除該組合中, 每一格在索引中的MatchGrid.
        var Pos = entry.AllPos;
        m_GemAssignList[BaseIdx] = 24;
        for (int i=0; i < Pos.Count; ++i)
        {
          int Idx = GridPosToIdx(Pos[i]);
          if (List.Remove(Idx))
          {
            m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetCountdown(300, OnGemMatchClear);
            if (m_CBClear != null)
            {
              m_CBClear(Pos[i].x, Pos[i].y);
            }
            m_Grid[Pos[i].x, Pos[i].y].MatchCount = 0;
          }
        }
        continue;
      }

      if (entry.IsMatchT())
      {
        // 移除該組合中, 每一格在索引中的MatchGrid.
        var Pos = entry.AllPos;
        m_GemAssignList[BaseIdx] = GetMatchColor( entry.BasePos.x, entry.BasePos.y) + m_ColorCount * 3 ;
        for (int i = 0; i < Pos.Count; ++i)
        {
          int Idx = GridPosToIdx(Pos[i]);
          if (List.Remove(Idx))
          {
            m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetCountdown(300, OnGemMatchClear);
            if (m_CBClear != null)
            {
              m_CBClear(Pos[i].x, Pos[i].y);
            }
            m_Grid[Pos[i].x, Pos[i].y].MatchCount = 0;
          }
        }
        continue;
      }

      if (entry.IsMatch4())
      {
        // 移除該組合中, 每一格在索引中的MatchGrid.
        var Pos = entry.AllPos;
        m_GemAssignList[BaseIdx] = m_ColorCount + GetMatchColor(entry.BasePos.x, entry.BasePos.y) * 2 + ((entry.m_v > entry.m_h)?0:1);
        for (int i = 0; i < Pos.Count; ++i)
        {
          int Idx = GridPosToIdx(Pos[i]);
          if (List.Remove(Idx))
          {
            m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetCountdown(300, OnGemMatchClear);
            if (m_CBClear != null)
            {
              m_CBClear(Pos[i].x, Pos[i].y);
            }
            m_Grid[Pos[i].x, Pos[i].y].MatchCount = 0;
          }
        }
        continue;
      }

      if (entry.IsMatch3())
      {
        // 移除該組合中, 每一格在索引中的MatchGrid.
        var Pos = entry.AllPos;
        for (int i = 0; i < Pos.Count; ++i)
        {
          int Idx = GridPosToIdx(Pos[i]);
          if (List.Remove(Idx))
          {
            m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetCountdown(300, OnGemMatchClear);
            if (m_CBClear != null)
            {
              m_CBClear(Pos[i].x, Pos[i].y);
            }
            m_Grid[Pos[i].x, Pos[i].y].MatchCount = 0;
          }
        }
        continue;
      }
      



    }
  }
  public void CleanMatchState(bool IsRemoveGem = true)
  {
    return;
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        // TODO:產出特殊寶石
        if (m_Grid[i, j].MatchCount > 0)
        {
          if (IsRemoveGem)
          {
            m_Grid[i, j].m_Gem.SetCountdown(300, OnGemOnlyClear);
            if (m_CBClear != null)
            {
              m_CBClear(i, j);
            }
          }
          m_Grid[i, j].MatchCount = 0;
        }

      }
    }
  }

  bool CheckMatch(int Col, int Row)
  {
    if (!m_Grid[Col, Row].IsCanMove())
      return false;

    int CurrColor = GetMatchColor(Col, Row);
    if (CurrColor == -1) return false;

    

    bool IsMatch = false;

    //橫豎各自檢查

    bool LSame, RSame, USame, DSame;

    LSame = RSame = USame = DSame = false;

    int LCol = Col - 1, LRow = Row;
    if (LCol >= 0)
    {
      if (m_Grid[LCol, LRow].IsCanMove())
        LSame = (CurrColor == GetMatchColor(LCol, LRow));
    }
    int RCol = Col + 1, RRow = Row;
    if (RCol < m_Col)
    {
      if (m_Grid[RCol, RRow].IsCanMove())
        RSame = (CurrColor == GetMatchColor(RCol, RRow));
    }

    int UCol = Col, URow = Row - 1;
    if (URow >= 0)
    {
      if (m_Grid[UCol, URow].IsCanMove())
        USame = (CurrColor == GetMatchColor(UCol, URow));
    }
    int DCol = Col, DRow = Row + 1;
    if (DRow < m_Row)
    {
      if (m_Grid[DCol, DRow].IsCanMove())
        DSame = (CurrColor == GetMatchColor(DCol, DRow));
    }

    if (LSame && RSame)
    {
      ///m_Grid[Col, Row].MatchCount++;
      ///m_Grid[LCol, LRow].MatchCount++;
      ///m_Grid[RCol, RRow].MatchCount++;
      m_MatchRecs.Add(new MatchRecord(LCol, LRow, RCol, RRow));
      IsMatch = true;
    }

    if (USame && DSame)
    {
      ///m_Grid[Col, Row].MatchCount++;
      ///m_Grid[UCol, URow].MatchCount++;
      ///m_Grid[DCol, DRow].MatchCount++;
      m_MatchRecs.Add(new MatchRecord(UCol, URow, DCol, DRow));
      IsMatch = true;
    }

    return IsMatch;
  }

  public void GemDrop()
  {
    for (int i = 0; i < m_Col; ++i)
    {
      int EmptyCnt = 0;
      for (int j = m_Row - 1; j >= 0; --j)
      {
        if (m_Grid[i, j].m_Gem == null)
        {
          EmptyCnt++;
        }
        else
        {
          if (!m_Grid[i, j].m_Gem.IsCanMove())
            EmptyCnt = 0;

          if (EmptyCnt == 0) 
            continue;

          if (m_Grid[i, j + EmptyCnt].m_Gem == null ||
            m_Grid[i, j + EmptyCnt].m_Gem.IsCanMove())
          {
            ChangeGem(i, j, i, j + EmptyCnt);
            if (m_Grid[i, j].m_Gem != null)
            {
              m_Grid[i, j].m_Gem.SetCountdown(300);
            }
            if (m_Grid[i, j + EmptyCnt].m_Gem != null)
            {
              m_Grid[i, j + EmptyCnt].m_Gem.SetCountdown(300);
            }
            m_CBMove(i, j, i, j + EmptyCnt, MOVE_TYPE_SWITCH);
          }
          else
          {
            break;
          }
        }
      }
    }
  }

  public void Reset()
  {
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        m_Grid[i, j].m_Gem.SetCountdown(300, OnGemOnlyClear);
        if (m_CBClear != null)
        {
          m_CBClear(i, j);
        }
        m_Grid[i, j].MatchCount = 0;
      }
    }
  }

  public void OnGemMatchClear(Gem TargetGem)
  {
    TargetGem.SetClear();

    if(TargetGem.Color >= (int)GemType.LineStart  && TargetGem.Color < (int)GemType.BombStart)
    {
      // test
      if (TargetGem.Color % 2 == 0)
      {
        for (int i = 0; i < m_Col; ++i)
        {
          if (m_Grid[i, TargetGem.m_TempPos.y].m_Gem != null && m_Grid[i, TargetGem.m_TempPos.y].m_Gem.IsCanMove())
          {
            m_Grid[i, TargetGem.m_TempPos.y].m_Gem.SetCountdown(300, OnGemMatchClear);
            if (m_CBClear != null)
            {
              m_CBClear(i, TargetGem.m_TempPos.y);
            }
            m_Grid[i, TargetGem.m_TempPos.y].MatchCount = 0;
          }
        }
      }
      else
      {
        for (int i = 0; i < m_Row; ++i)
        {
          if (m_Grid[TargetGem.m_TempPos.x, i].m_Gem != null && m_Grid[TargetGem.m_TempPos.x, i].m_Gem.IsCanMove())
          {
            m_Grid[TargetGem.m_TempPos.x, i].m_Gem.SetCountdown(300, OnGemMatchClear);
            if (m_CBClear != null)
            {
              m_CBClear(TargetGem.m_TempPos.x, i);
            }
            m_Grid[TargetGem.m_TempPos.x, i].MatchCount = 0;
          }
        }
      }
    }

    if (TargetGem.Color >= (int)GemType.BombStart && TargetGem.Color < (int)GemType.SameAllStart)
    {
      int ColMin = TargetGem.m_TempPos.x - 2;
      if (ColMin < 0) ColMin = 0;
      int ColMax = TargetGem.m_TempPos.x + 2;
      if (ColMax >= m_Col) ColMax = m_Col - 1;
      int RowMin = TargetGem.m_TempPos.y - 2;
      if (RowMin < 0) RowMin = 0;
      int RowMax = TargetGem.m_TempPos.y + 2;
      if (RowMax >= m_Row) RowMax = m_Row - 1;

      for (int i = ColMin; i <= ColMax; ++i)
      {
        for (int j = RowMin; j <= RowMax; ++j)
        {
          if (m_Grid[i, j].m_Gem != null && m_Grid[i, j].m_Gem.IsCanMove())
          {
            m_Grid[i, j].m_Gem.SetCountdown(300, OnGemMatchClear);
            if (m_CBClear != null)
            {
              m_CBClear(i, j);
            }
            m_Grid[i, j].MatchCount = 0;
          }
        }
      }
    }
  }

  public void OnGemOnlyClear(Gem TargetGem)
  {
    TargetGem.SetClear();
  }

  public void print()
  {
    System.Text.StringBuilder LogBuilder = new System.Text.StringBuilder();
    for (int j = 0; j < m_Row; ++j)
    {
      for (int i = 0; i < m_Col; ++i)
      {
        LogBuilder.Append(GetColor(i, j));
        LogBuilder.Append(" ");
      }
      LogBuilder.Append("\n");
    }
    if(m_CBLog != null)
      m_CBLog(LogBuilder.ToString());
  }

  public void printClearState()
  {
    System.Text.StringBuilder LogBuilder = new System.Text.StringBuilder();
    for (int j = 0; j < m_Row; ++j)
    {
      for (int i = 0; i < m_Col; ++i)
      {
        LogBuilder.Append(m_Grid[i, j].MatchCount);
        LogBuilder.Append(" ");
      }
      LogBuilder.Append("\n");
    }
    if (m_CBLog != null)
      m_CBLog(LogBuilder.ToString());
  }

}

