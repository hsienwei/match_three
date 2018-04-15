using System;
using System.Collections.Generic;

public class GridState
{
  
  public int MatchCount;
  public Gem m_Gem;

  public GridState()
  {
    m_Gem = null;
    MatchCount = 0;
  }

  public void GenGem(int Color)
  {
    if (m_Gem == null)
      m_Gem = new Gem(Color);
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

  private int m_Color;
  private State m_State = State.Idle;
  private Action m_Callback;

  private int m_Countdown;

  public int Color { get { return m_Color; } }

  public Gem(int Color)
  {
    m_Color = Color;
  }

  public void Update(int DeltaTime)
  {
    m_Countdown -= DeltaTime;
    if (m_Countdown <= 0)
    {
      m_State = State.Idle;
      if (m_Callback != null)
        m_Callback();
      
    }
  }

  public bool IsCanMove()
  {
    return m_State == State.Idle;
  }

  public void SetCountdown(int Cnt, Action Callback = null)
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

public class MatchThreeCore
{

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

  enum Direction
  { 
    DOWN,
    UP,
    LEFT,
    RIGHT,
  };


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

  public MatchThreeCore(int Col, int Row, int ColorCount)
  {
    m_Grid = new GridState[Col, Row];
    m_Row = Row;
    m_Col = Col;
    m_ColorCount = ColorCount;

    m_Rand = new Random();

    m_SwipeRecs = new List<SwipeRecord>();
    m_PossibleMove = new List<MoveRecord>();
  }

  int Col { get { return m_Col; } }
  int Row { get { return m_Row; } }

  public void ChangeGem(GridState Grid1, GridState Grid2)
  {
    var Tmp = Grid1.m_Gem;
    Grid1.m_Gem = Grid2.m_Gem;
    Grid2.m_Gem = Tmp;
  }

  public int GetColor(int Col, int Row)
  {
    if (Col < 0 || Col >= m_Col) return -1;
    if (Row < 0 || Row >= m_Row) return -1;
    if (m_Grid[Col, Row].m_Gem == null)
      return -1;
    return m_Grid[Col, Row].m_Gem.Color;
  }

  public int GetClearCount(int Col, int Row)
  {
    return m_Grid[Col, Row].MatchCount;
  }

  /*public int GetLockCount(int Col, int Row)
  {
    return m_Grid[Col, Row].LockCnt;
  }*/

  public void Update(int TimeUnit)
  {
    // swipe行為.
    for(int i=0; i< m_SwipeRecs.Count; ++i)
    {
      var Rec = m_SwipeRecs[i];

      ChangeGem(m_Grid[Rec.m_x1, Rec.m_y1], m_Grid[Rec.m_x2, Rec.m_y2]);
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

    ScanMatch();
    CleanMatchState();
    GemDrop();
    Generate(false);
    ScanMatchPossible();
    CheckReset();
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

    System.Text.StringBuilder m_TempBuilder = new System.Text.StringBuilder();
    m_TempBuilder.Remove(0, m_TempBuilder.Length);
    for (int i=0; i< m_PossibleMove.Count; ++i)
    {
      m_TempBuilder.Append(m_PossibleMove[i]);
    }
    m_CBLog2(m_TempBuilder.ToString());
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
            m_Grid[i, j].GenGem(m_Rand.Next(0, m_ColorCount));
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
          m_Grid[i, j].GenGem(m_Rand.Next(0, m_ColorCount));
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
  }

  public bool IsHasClearState()
  {
    return m_IsHasMatch;
  }

  public void CleanMatchState(bool IsRemoveGem = true)
  {
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        // TODO:產出特殊寶石
        if (m_Grid[i, j].MatchCount > 0)
        {
          if (IsRemoveGem)
          {
            m_Grid[i, j].m_Gem.SetCountdown(300, m_Grid[i, j].Clear);
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

    int CurrColor = GetColor(Col, Row);
    if (CurrColor == -1) return false;

    

    bool IsMatch = false;

    //橫豎各自檢查

    bool LSame, RSame, USame, DSame;

    LSame = RSame = USame = DSame = false;

    int LCol = Col - 1, LRow = Row;
    if (LCol >= 0)
    {
      if (m_Grid[LCol, LRow].IsCanMove())
        LSame = (CurrColor == GetColor(LCol, LRow));
    }
    int RCol = Col + 1, RRow = Row;
    if (RCol < m_Col)
    {
      if (m_Grid[RCol, RRow].IsCanMove())
        RSame = (CurrColor == GetColor(RCol, RRow));
    }

    int UCol = Col, URow = Row - 1;
    if (URow >= 0)
    {
      if (m_Grid[UCol, URow].IsCanMove())
        USame = (CurrColor == GetColor(UCol, URow));
    }
    int DCol = Col, DRow = Row + 1;
    if (DRow < m_Row)
    {
      if (m_Grid[DCol, DRow].IsCanMove())
        DSame = (CurrColor == GetColor(DCol, DRow));
    }

    if (LSame && RSame)
    {
      m_Grid[Col, Row].MatchCount++;
      m_Grid[LCol, LRow].MatchCount++;
      m_Grid[RCol, RRow].MatchCount++;
      IsMatch = true;
    }

    if (USame && DSame)
    {
      m_Grid[Col, Row].MatchCount++;
      m_Grid[UCol, URow].MatchCount++;
      m_Grid[DCol, DRow].MatchCount++;
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
            ChangeGem(m_Grid[i, j], m_Grid[i, j + EmptyCnt]);
            if(m_Grid[i, j].m_Gem != null)    
              m_Grid[i, j].m_Gem.SetCountdown(300);
            if (m_Grid[i, j + EmptyCnt].m_Gem != null)   
              m_Grid[i, j + EmptyCnt].m_Gem.SetCountdown(300);
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
        m_Grid[i, j].m_Gem.SetCountdown(300, m_Grid[i, j].Clear);
        if (m_CBClear != null)
        {
          m_CBClear(i, j);
        }
        m_Grid[i, j].MatchCount = 0;
      }
    }
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

