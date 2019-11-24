using System;
using System.Collections.Generic;


namespace Match3
{
  public class Match3Core
  {
    public readonly int WAIT_UPDATE_TIME_UNIT = 100;
    public readonly int WAIT_UPDATE_TIME_UNIT2 = 100;

    public class GridState
    {

      public Gem m_Gem;

      public GridState()
      {
        m_Gem = null;
      }

      public void GenGem(int Color, GridPos Pos)
      {
        if (m_Gem == null)
        {
          m_Gem = new Gem(Color, Pos);
        }
      }

      public void GenGem(IntVector2 GemInfo, GridPos Pos)
      {
        if (m_Gem == null)
        {
          m_Gem = new Gem(GemInfo, Pos);
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

      public enum GemType
      {
        Normal = 0,
        LineColumn = 1,
        LineRow = 2,
        Bomb = 3,
        Wildcard = 4,
        Cross = 5,

      }

      public GridPos m_TempPos;
      private int m_Color;
      private GemType m_Type;
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

      public GemType Type
      {
        get
        {
          return m_Type;
        }
      }

      public Gem(int Color, GridPos Pos)
      {
        m_Color = Color;
        m_TempPos = Pos;
        m_Type = GemType.Normal;
      }

      public Gem(IntVector2 State, GridPos Pos)
      {
        m_Color = State.GemColor;
        m_TempPos = Pos;
        m_Type = (GemType)State.GemType;
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

      public void SetMoving(int Cnt, Action<Gem> Callback = null)
      {
        m_Countdown = Cnt;
        m_State = State.Moving;
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

    class ActionRecord
    {
      public enum Type
      {
        SWIPE,
        TAP
      }

      public enum State
      {
        START,
        END
      };

      protected GridPos m_Pos1;
      public GridPos Pos1 { get { return m_Pos1; } }

      protected State m_State;
      protected Type m_Type;

      public Type getType()
      {
        return m_Type;
      }

      public void SetChanged()
      {
        m_State = State.END;
      }

      public bool IsStart()
      {
        return m_State == State.START;
      }

      public bool IsEnd()
      {
        return m_State == State.END;
      }
    }
    class SwipeRecord : ActionRecord
    {
      
      protected GridPos m_Pos2;
      public GridPos Pos2 { get { return m_Pos2; } }
      public SwipeRecord(GridPos Pos1, GridPos Pos2, State s = State.START)
      {
        m_Pos1 = Pos1;
        m_Pos2 = Pos2;
        m_State = s;
        m_Type = Type.SWIPE;
      }

    };

    class TapRecord : ActionRecord
    {
      public TapRecord(GridPos Pos1, State s = State.START)
      {
        m_Pos1 = Pos1;
        m_State = s;
        m_Type = Type.TAP;
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

      public bool IsEqual(int x, int y, Direction Dir)
      {
        return x == m_x1 && y == m_y1 && m_Direction == Dir;
      }

      public override string ToString()
      {
        return string.Format(FormatStr, m_x1, m_y1, m_Direction);
      }
    };

    class MatchRecord
    {

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

      public bool IsHorizontal()
      {
        return m_x1 == m_x2;
      }

      public bool IsSquare()
      {
        return (Math.Abs(m_x1 - m_x2) == 1) && (Math.Abs(m_y1 - m_y2) == 1);
      }

      public List<GridPos> GetGridPos()
      {
        List<GridPos> List = new List<GridPos>();

        for (int i = m_x1; i <= m_x2; ++i)
        {
          for (int j = m_y1; j <= m_y2; ++j)
          {
            List.Add(new GridPos(i, j));
          }
        }
        return List;
      }

    };

    class SpecialRemove
    {
      int m_Style;
      Queue<int> m_PosList;
      public SpecialRemove(int Style)
      {
        m_Style = Style;
        m_PosList = new Queue<int>();
      }

      public void Enqueue(int Idx)
      {
        m_PosList.Enqueue(Idx);
      }

      public void Enqueue(List<int> Idxs)
      {
        for (int i = 0; i < Idxs.Count; ++i)
        {
          m_PosList.Enqueue(Idxs[i]);
        }
      }

      public bool Dequeue(ref int Idx)
      {
        if (m_PosList.Count == 0)
          return false;
        Idx = m_PosList.Dequeue();
        return true;
      }

    }

    public int GridPosToIdx(GridPos g) { return g.x + g.y * m_ColumnCnt; }
    public int GridPosToIdx(int x, int y) { return x + y * m_ColumnCnt; }
    public void GridIdxToPos(int idx, ref int x, ref int y) { x = idx % m_ColumnCnt; y = idx / m_ColumnCnt; }

    public static readonly int MOVE_TYPE_MOVE = 1;
    public static readonly int MOVE_TYPE_SWITCH = 2;
    public static readonly int MOVE_TYPE_SWITCHBACK = 3;

    public delegate void OnGenerate(int Col, int Row, int Color, Gem.GemType Type, int TimeUnit);
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
    private int m_RowCnt, m_ColumnCnt;
    private int m_ColorCount;
    private bool m_IsHasMatch;

    private Random m_Rand;

    private List<ActionRecord> m_ActionRecs;
    private List<MoveRecord> m_PossibleMove;
    private List<MatchRecord> m_MatchRecs;
    private Dictionary<int, IntVector2> m_GemAssignList;
    private List<SpecialRemove> m_GemForceRemoveList;

    public bool m_IsCheckMatch = true;
    public bool m_IsSwapIfMatch = true;


    public static Direction GetDirection(int Fromx1, int Fromy1, int Tox2, int Toy2)
    {
      if (Fromx1 > Tox2)
        return Direction.LEFT;
      else if (Fromx1 < Tox2)
        return Direction.RIGHT;
      else
      {
        if (Fromy1 > Toy2)
          return Direction.UP;
        else
          return Direction.DOWN;
      }
    }

    public Match3Core(int Col, int Row, int ColorCount)
    {
      m_Grid = new GridState[Col, Row];
      m_RowCnt = Row;
      m_ColumnCnt = Col;
      m_ColorCount = ColorCount;

      m_Rand = new Random();

      m_ActionRecs = new List<ActionRecord>();
      m_PossibleMove = new List<MoveRecord>();
      m_MatchRecs = new List<MatchRecord>();
      m_GemAssignList = new Dictionary<int, IntVector2>();
      m_GemForceRemoveList = new List<SpecialRemove>();
    }

    int Col { get { return m_ColumnCnt; } }
    int Row { get { return m_RowCnt; } }

    public void SwipeChangeGem(GridPos Pos1, GridPos Pos2)
    {

      m_CBLog2("" + GetDirection(Pos1.x, Pos1.y, Pos2.x, Pos2.y));

      bool IsCanMatch = false;
      for (int i = 0; i < m_PossibleMove.Count; ++i)
      {
        var PossibleMove = m_PossibleMove[i];
        if (PossibleMove.IsEqual(Pos1.x, Pos1.y, GetDirection(Pos1.x, Pos1.y, Pos2.x, Pos2.y)))
        {
          IsCanMatch = true;
          break;
        }

        if (PossibleMove.IsEqual(Pos2.x, Pos2.y, GetDirection(Pos2.x, Pos2.y, Pos1.x, Pos1.y)))
        {
          IsCanMatch = true;
          break;
        }
      }
      //bool IsCanMatch = true;

      ChangeGem(Pos1.x, Pos1.y, Pos2.x, Pos2.y, IsCanMatch || !m_IsSwapIfMatch);
    }

    public void TapGem(GridPos Pos1)
    {
      if (m_Grid[Pos1.x, Pos1.y].m_Gem != null)
      {
        if (m_Grid[Pos1.x, Pos1.y].m_Gem.Type == Gem.GemType.Normal)
          return;

        SpecialRemove SpMove = new SpecialRemove((int)Gem.GemType.Normal);
        m_GemForceRemoveList.Add(SpMove);
        SpMove.Enqueue(GridPosToIdx(Pos1.x, Pos1.y));
      }

    }

    public void ChangeGem(int x1, int y1, int x2, int y2, bool oneWay = true)
    {
      GridState Grid1 = m_Grid[x1, y1];
      GridState Grid2 = m_Grid[x2, y2];



      if (oneWay)
      {
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

        m_CBMove(x1, y1, x2, y2, MOVE_TYPE_SWITCH);

        if (m_Grid[x1, y1].m_Gem != null)
          m_Grid[x1, y1].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT2);
        if (m_Grid[x2, y2].m_Gem != null)
          m_Grid[x2, y2].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT2);


      }
      else
      {

        m_CBMove(x1, y1, x2, y2, MOVE_TYPE_SWITCHBACK);

        if (m_Grid[x1, y1].m_Gem != null)
          m_Grid[x1, y1].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT * 2);
        if (m_Grid[x2, y2].m_Gem != null)
          m_Grid[x2, y2].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT * 2);
      }

    }

    public int GetColor(int Col, int Row)
    {
      if (Col < 0 || Col >= m_ColumnCnt) return -1;
      if (Row < 0 || Row >= m_RowCnt) return -1;
      if (m_Grid[Col, Row].m_Gem == null)
        return -1;
      return m_Grid[Col, Row].m_Gem.Color;
    }

    public int GetMatchColor(int Col, int Row)
    {
      if (Col < 0 || Col >= m_ColumnCnt) return -1;
      if (Row < 0 || Row >= m_RowCnt) return -1;
      if (m_Grid[Col, Row].m_Gem == null)
        return -1;
      return m_Grid[Col, Row].m_Gem.Color;
    }

    List<int> GetColorGems(Gem gem)
    {
      int Color = gem.Color;
      List<int> Rtn = new List<int>();
      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
        {
          if (m_Grid[i, j].m_Gem.Color == Color && m_Grid[i, j].m_Gem != gem)
          {
            Rtn.Add(GridPosToIdx(i, j));
          }

        }
      }
      return Rtn;
    }

    public void Update(int timeUnit)
    {

      UpdateSwipe();
      UpdateGem(timeUnit);
      AssignGemGen();
      SpecialRemoveGem();
      GemDrop();
      Generate(false);

      if (m_IsCheckMatch)
      {
        ScanMatch();
      }

      CheckReset();
      if (m_IsCheckMatch)
      {
        ScanMatchPossible();
      }
    }

    void UpdateSwipe()
    {
      // 執行swipe行為.
      for (int i = 0; i < m_ActionRecs.Count; ++i)
      {
        var oriRec = m_ActionRecs[i];
        if (oriRec.getType() == ActionRecord.Type.SWIPE)
        {
          var Rec = oriRec as SwipeRecord;
          if (m_Grid[Rec.Pos1.x, Rec.Pos1.y].m_Gem == null || !m_Grid[Rec.Pos1.x, Rec.Pos1.y].m_Gem.IsCanMove())
            continue;
          if (m_Grid[Rec.Pos2.x, Rec.Pos2.y].m_Gem == null || !m_Grid[Rec.Pos2.x, Rec.Pos2.y].m_Gem.IsCanMove())
            continue;

          if (!Rec.IsEnd())
          {
            SwipeChangeGem(Rec.Pos1, Rec.Pos2);
            Rec.SetChanged();
          }
        }
        else
        {
          if (m_Grid[oriRec.Pos1.x, oriRec.Pos1.y].m_Gem == null || !m_Grid[oriRec.Pos1.x, oriRec.Pos1.y].m_Gem.IsCanMove())
            continue;

          if (!oriRec.IsEnd())
          {
            TapGem(oriRec.Pos1);
            oriRec.SetChanged();
          }
        }
      }

      m_ActionRecs.RemoveAll((ActionRecord Rec) => { return Rec.IsEnd(); });
    }

    void UpdateGem(int TimeUnit)
    {
      // 更新珠子, 清除已消除資料.
      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
        {
          if (m_Grid[i, j].m_Gem != null)
          {
            m_Grid[i, j].m_Gem.Update(TimeUnit);
            if (m_Grid[i, j].m_Gem.IsClear())
            m_Grid[i, j].m_Gem = null;
            
          }
        }
      }
    }

    void SpecialRemoveGem()
    {
      for (int i = 0; i < m_GemForceRemoveList.Count; ++i)
      {
        SpecialRemove SpRemove = m_GemForceRemoveList[i];

        int PosIdx = 0, x = 0, y = 0;
        while (SpRemove.Dequeue(ref PosIdx))
        {
          GridIdxToPos(PosIdx, ref x, ref y);
          if (m_Grid[x, y].m_Gem == null || !m_Grid[x, y].m_Gem.IsCanMove())
            continue;
          m_Grid[x, y].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT, OnGemMatchClear);
          if (m_CBClear != null)
          {
            m_CBClear(x, y);
          }
        }
      }

      m_GemForceRemoveList.Clear();
    }

    void AssignGemGen()
    {
      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
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

              m_Grid[i, j].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT2);
              if (m_CBGenerate != null)
              {
                m_CBGenerate(i, j, m_Grid[i, j].m_Gem.Color, m_Grid[i, j].m_Gem.Type, WAIT_UPDATE_TIME_UNIT2);
              }
            }
          }
        }
      }

    }

    void CheckReset()
    {
      if (m_ActionRecs.Count > 0) return;
      if (m_PossibleMove.Count > 0) return;
      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
        {
          if (!m_Grid[i, j].IsCanMove()) return;
        }
      }

      Reset();
    }

    void ScanMatchPossible()
    {
      m_PossibleMove.Clear();
      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
        {
          CheckMatchPossible(i, j);
        }
      }

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

        // 左移上方
        if (CurrColor == GetColor(Col - 1, Row - 1) && CurrColor == GetColor(Col - 2, Row - 1) && CurrColor == GetColor(Col - 2, Row))
        {
          m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.LEFT));
          break;
        }

        // 左移下方
        if (CurrColor == GetColor(Col - 1, Row + 1) && CurrColor == GetColor(Col - 2, Row + 1) && CurrColor == GetColor(Col - 2, Row))
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

        // 右移上方
        if (CurrColor == GetColor(Col + 1, Row - 1) && CurrColor == GetColor(Col + 2, Row - 1) && CurrColor == GetColor(Col + 2, Row))
        {
          m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.RIGHT));
          break;
        }

        // 右移下方
        if (CurrColor == GetColor(Col + 1, Row + 1) && CurrColor == GetColor(Col + 2, Row + 1) && CurrColor == GetColor(Col + 2, Row))
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

        // 上移左方
        if (CurrColor == GetColor(Col - 1, Row - 1) && CurrColor == GetColor(Col - 1, Row - 2) && CurrColor == GetColor(Col, Row - 2))
        {
          m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.UP));
          break;
        }
        // 上移右方
        if (CurrColor == GetColor(Col + 1, Row - 1) && CurrColor == GetColor(Col + 1, Row - 2) && CurrColor == GetColor(Col, Row - 2))
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

        // 下移左方
        if (CurrColor == GetColor(Col - 1, Row + 1) && CurrColor == GetColor(Col - 1, Row + 2) && CurrColor == GetColor(Col, Row + 2))
        {
          m_PossibleMove.Add(new MoveRecord(Col, Row, Direction.DOWN));
          break;
        }
        // 下移右方
        if (CurrColor == GetColor(Col + 1, Row + 1) && CurrColor == GetColor(Col + 1, Row + 2) && CurrColor == GetColor(Col, Row + 2))
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
        for (int i = 0; i < m_ColumnCnt; ++i)
        {
          for (int j = 0; j < m_RowCnt; ++j)
          {
            if (IsInit)
            {
              m_Grid[i, j] = new GridState();
              m_Grid[i, j].GenGem(m_Rand.Next(0, m_ColorCount), new GridPos(i, j));
              m_Grid[i, j].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT);
              if (m_CBGenerate != null)
              {
                m_CBGenerate(i, j, m_Grid[i, j].m_Gem.Color, m_Grid[i, j].m_Gem.Type, WAIT_UPDATE_TIME_UNIT2);
              }
            }
          }
        }
        return;
      }

      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
        {
          // 由上往下掃 空格補珠, 如果遇到阻塞就換column.
          if (m_Grid[i, j].m_Gem == null)
          {
            m_Grid[i, j].GenGem(m_Rand.Next(0, m_ColorCount), new GridPos(i, j));
            m_Grid[i, j].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT2);
            if (m_CBGenerate != null)
            {
              m_CBGenerate(i, j, m_Grid[i, j].m_Gem.Color, m_Grid[i, j].m_Gem.Type, WAIT_UPDATE_TIME_UNIT2);
            }
            break;
          }
          else
          {
            if (!m_Grid[i, j].m_Gem.IsCanMove())
            {
              break;
            }
          }
        }
      }

    }

    public bool Swipe(int Col, int Row, Direction Dir)
    {
      GridPos Target = new GridPos(Col, Row);

      switch (Dir)
      {
        case Direction.DOWN:
          Target.y++; break;
        case Direction.UP:
          Target.y--; break;
        case Direction.LEFT:
          Target.x--; break;
        case Direction.RIGHT:
          Target.x++; break;
      }

      if (Target.x < 0) return false;
      if (Target.x >= m_ColumnCnt) return false;
      if (Target.y < 0) return false;
      if (Target.y >= m_RowCnt) return false;

      if (!m_Grid[Col, Row].IsCanMove()) return false;
      if (!m_Grid[Target.x, Target.y].IsCanMove()) return false;

      m_ActionRecs.Add(new SwipeRecord(new GridPos(Col, Row), Target));

      return true;
    }

    public bool Tap(int Col, int Row)
    {
      if (Col < 0) return false;
      if (Col >= m_ColumnCnt) return false;
      if (Row < 0) return false;
      if (Row >= m_RowCnt) return false;

      m_ActionRecs.Add(new TapRecord(new GridPos(Col, Row)));

      return true;
    }

    public void ScanMatch()
    {

      m_IsHasMatch = false;
      m_MatchRecs.Clear();

      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
        {
          if (CheckMatch(i, j))
          {
            m_IsHasMatch = true;
          }
        }
      }

      ClearGemWithCreateAssign();
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
      public int m_s;

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
        else if (Rec.IsHorizontal())
          m_h++;
        else if (Rec.IsSquare())
          m_s++;

        var PosList = Rec.GetGridPos();
        for (int i = 0; i < PosList.Count; ++i)
        {
          if (!m_AllGrid.Contains(PosList[i]))
          {
            m_AllGrid.Add(PosList[i]);
          }
        }
      }

      public GridPos BasePos { get { return m_BasePos; } }
      public List<GridPos> AllPos { get { return m_AllGrid; } }

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

      public bool IsMatchS()
      {
        return m_s == 4;
      }

      public bool IsMatch3()
      {
        return m_v >= 1 || m_h >= 1;
      }

      public int CompareTo(MatchGrid other)
      {
        if (other == null) return -1;
        int compare = Recs.Count.CompareTo(other.Recs.Count);
        if (compare == 0)
        {
          if (Math.Abs(m_v - m_h) < Math.Abs(other.m_v - other.m_h))
            compare = 1;
          else
            compare = 0;
        }
        return -compare;
      }

      public override string ToString()
      {
        return "" + m_BasePos.x + "," + m_BasePos.y + "," + m_v + "," + m_h;
      }
    };

    void ClearGemWithCreateAssign()
    {
      // 建立格子的索引
      Dictionary<int, MatchGrid> List = new Dictionary<int, MatchGrid>();
      for (int i = 0; i < m_MatchRecs.Count; ++i)
      {
        var Grids = m_MatchRecs[i].GetGridPos();
        for (int j = 0; j < Grids.Count; ++j)
        {
          int Idx = this.GridPosToIdx(Grids[j]);

          if (!List.ContainsKey(Idx))
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
          m_GemAssignList[BaseIdx] = new IntVector2(0, (int)Gem.GemType.Wildcard);
          for (int i = 0; i < Pos.Count; ++i)
          {
            int Idx = GridPosToIdx(Pos[i]);
            if (List.Remove(Idx))
            {
              m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT, OnGemMatchClear);
              if (m_CBClear != null)
              {
                m_CBClear(Pos[i].x, Pos[i].y);
              }
            }
          }
          continue;
        }

        if (entry.IsMatchT())
        {
          // 移除該組合中, 每一格在索引中的MatchGrid.
          var Pos = entry.AllPos;
          m_GemAssignList[BaseIdx] = new IntVector2(GetMatchColor(entry.BasePos.x, entry.BasePos.y), (int)Gem.GemType.Bomb);
          for (int i = 0; i < Pos.Count; ++i)
          {
            int Idx = GridPosToIdx(Pos[i]);
            if (List.Remove(Idx))
            {
              m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT, OnGemMatchClear);
              if (m_CBClear != null)
              {
                m_CBClear(Pos[i].x, Pos[i].y);
              }
            }
          }
          continue;
        }

        if (entry.IsMatch4())
        {
          // 移除該組合中, 每一格在索引中的MatchGrid.
          var Pos = entry.AllPos;
          m_GemAssignList[BaseIdx] = new IntVector2(GetMatchColor(entry.BasePos.x, entry.BasePos.y), (entry.m_v > entry.m_h) ? (int)Gem.GemType.LineColumn : (int)Gem.GemType.LineRow);
          for (int i = 0; i < Pos.Count; ++i)
          {
            int Idx = GridPosToIdx(Pos[i]);
            if (List.Remove(Idx))
            {
              m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT, OnGemMatchClear);
              if (m_CBClear != null)
              {
                m_CBClear(Pos[i].x, Pos[i].y);
              }
            }
          }
          continue;
        }

        if (entry.IsMatchS())
        {
          var Pos = entry.AllPos;
          m_GemAssignList[BaseIdx] = new IntVector2(0, (int)Gem.GemType.Cross);

          for (int i = 0; i < Pos.Count; ++i)
          {
            int Idx = GridPosToIdx(Pos[i]);
            if (List.Remove(Idx))
            {
              m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT, OnGemMatchClear);
              if (m_CBClear != null)
              {
                m_CBClear(Pos[i].x, Pos[i].y);
              }
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
              m_Grid[Pos[i].x, Pos[i].y].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT, OnGemMatchClear);
              if (m_CBClear != null)
              {
                m_CBClear(Pos[i].x, Pos[i].y);
              }
            }
          }
          continue;
        }
      }
    }

    bool CheckMatch(int Col, int Row)
    {
      if (!m_Grid[Col, Row].IsCanMove())
        return false;

      int CurrColor = GetMatchColor(Col, Row);
      if (CurrColor == -1) return false;


      // DOWN, UP, LEFT, RIGHT, LD, RD, LU, RU
      int[,] CheckPos = new int[(int)Direction.MAX, 2] { { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 }, { -1, 1 }, { 1, 1 }, { -1, -1 }, { 1, -1 }, };
      bool[] CheckSameState = new bool[(int)Direction.MAX] { false, false, false, false, false, false, false, false };

      bool IsMatch = false;

      // check 8 way.

      for (int i = 0; i < (int)Direction.MAX; ++i)
      {
        CheckSameState[i] = SameColorCheck_(CurrColor, Col, Row, CheckPos[i, 0], CheckPos[i, 1]);
      }

      // ---
      // ooo
      // ---
      if (CheckSameState[(int)Direction.LEFT] && CheckSameState[(int)Direction.RIGHT])
      {
        m_MatchRecs.Add(new MatchRecord(
          Col + CheckPos[(int)Direction.LEFT, 0],
          Row + CheckPos[(int)Direction.LEFT, 1],
          Col + CheckPos[(int)Direction.RIGHT, 0],
          Row + CheckPos[(int)Direction.RIGHT, 1]));
        IsMatch = true;
      }

      // -o-
      // -o-
      // -o-
      if (CheckSameState[(int)Direction.UP] && CheckSameState[(int)Direction.DOWN])
      {
        m_MatchRecs.Add(new MatchRecord(
          Col + CheckPos[(int)Direction.UP, 0],
          Row + CheckPos[(int)Direction.UP, 1],
          Col + CheckPos[(int)Direction.DOWN, 0],
          Row + CheckPos[(int)Direction.DOWN, 1]));
        IsMatch = true;
      }

      // oo-
      // oo-
      // ---
      if (CheckSameState[(int)Direction.UP] && CheckSameState[(int)Direction.LEFT] && CheckSameState[(int)Direction.LU])
      {
        m_MatchRecs.Add(new MatchRecord(
          Col + CheckPos[(int)Direction.LU, 0],
          Row + CheckPos[(int)Direction.LU, 1],
          Col,
          Row));
        IsMatch = true;
      }

      // -oo
      // -oo
      // ---
      if (CheckSameState[(int)Direction.UP] && CheckSameState[(int)Direction.RIGHT] && CheckSameState[(int)Direction.RU])
      {
        m_MatchRecs.Add(new MatchRecord(
          Col + CheckPos[(int)Direction.UP, 0],
          Row + CheckPos[(int)Direction.UP, 1],
          Col + CheckPos[(int)Direction.RIGHT, 0],
          Row + CheckPos[(int)Direction.RIGHT, 1]));
        IsMatch = true;
      }

      // ---
      // oo-
      // oo-
      if (CheckSameState[(int)Direction.DOWN] && CheckSameState[(int)Direction.LEFT] && CheckSameState[(int)Direction.LD])
      {
        m_MatchRecs.Add(new MatchRecord(
          Col + CheckPos[(int)Direction.LEFT, 0],
          Row + CheckPos[(int)Direction.LEFT, 1],
          Col + CheckPos[(int)Direction.DOWN, 0],
          Row + CheckPos[(int)Direction.DOWN, 1]));
        IsMatch = true;
      }

      // ---
      // -oo
      // -oo
      if (CheckSameState[(int)Direction.DOWN] && CheckSameState[(int)Direction.RIGHT] && CheckSameState[(int)Direction.RD])
      {
        m_MatchRecs.Add(new MatchRecord(
          Col,
          Row,
          Col + CheckPos[(int)Direction.RD, 0],
          Row + CheckPos[(int)Direction.RD, 1]));
        IsMatch = true;
      }

      return IsMatch;
    }

    public bool SameColorCheck_(int TargetColor, int TargetPosX, int TargetPosY, int PosOffsetX, int PosOffsetY)
    {
      int Col = TargetPosX + PosOffsetX, Row = TargetPosY + PosOffsetY;
      if (Col >= 0 && Col < m_ColumnCnt && Row >= 0 && Row < m_RowCnt)
      {
        if (m_Grid[Col, Row].IsCanMove())
          return (TargetColor == GetMatchColor(Col, Row));
      }
      return false;
    }

    public void GemDrop()
    {
      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        int emptyCnt = 0;
        for (int j = m_RowCnt - 1; j >= 0; --j)
        {
          if (m_Grid[i, j].m_Gem == null)
          {
            emptyCnt++;
          }
          else
          {
            if (!m_Grid[i, j].m_Gem.IsCanMove())
              emptyCnt = 0;

            if (emptyCnt == 0)
              continue;

            if (m_Grid[i, j + emptyCnt].m_Gem == null ||
              m_Grid[i, j + emptyCnt].m_Gem.IsCanMove())
            {
              ChangeGem(i, j, i, j + emptyCnt);

            }
          }
        }
      }
    }

    public void Reset()
    {
      for (int i = 0; i < m_ColumnCnt; ++i)
      {
        for (int j = 0; j < m_RowCnt; ++j)
        {
          m_Grid[i, j].m_Gem.SetMoving(WAIT_UPDATE_TIME_UNIT, OnGemOnlyClear);
          if (m_CBClear != null)
          {
            m_CBClear(i, j);
          }
        }
      }
    }

    public void OnGemMatchClear(Gem targetGem)
    {
      targetGem.SetClear();

      if (targetGem.Type == Gem.GemType.LineColumn)
      {
        // test
        SpecialRemove SpMove = new SpecialRemove((int)targetGem.Type);
        m_GemForceRemoveList.Add(SpMove);
        for (int i = 0; i < m_RowCnt; ++i)
        {
          if (m_Grid[targetGem.m_TempPos.x, i].m_Gem != null && m_Grid[targetGem.m_TempPos.x, i].m_Gem.IsCanMove())
          {
            SpMove.Enqueue(GridPosToIdx(targetGem.m_TempPos.x, i));
          }
        }

      }
      if (targetGem.Type == Gem.GemType.LineRow)
      {
        SpecialRemove SpMove = new SpecialRemove((int)targetGem.Type);
        m_GemForceRemoveList.Add(SpMove);

        for (int i = 0; i < m_ColumnCnt; ++i)
        {
          if (m_Grid[i, targetGem.m_TempPos.y].m_Gem != null && m_Grid[i, targetGem.m_TempPos.y].m_Gem.IsCanMove())
          {
            SpMove.Enqueue(GridPosToIdx(i, targetGem.m_TempPos.y));
          }
        }
      }


      if (targetGem.Type == Gem.GemType.Bomb)
      {
        int ColMin = targetGem.m_TempPos.x - 1;
        if (ColMin < 0) ColMin = 0;
        int ColMax = targetGem.m_TempPos.x + 1;
        if (ColMax >= m_ColumnCnt) ColMax = m_ColumnCnt - 1;
        int RowMin = targetGem.m_TempPos.y - 1;
        if (RowMin < 0) RowMin = 0;
        int RowMax = targetGem.m_TempPos.y + 1;
        if (RowMax >= m_RowCnt) RowMax = m_RowCnt - 1;


        SpecialRemove SpMove = new SpecialRemove((int)targetGem.Type);
        m_GemForceRemoveList.Add(SpMove);

        for (int i = ColMin; i <= ColMax; ++i)
        {
          for (int j = RowMin; j <= RowMax; ++j)
          {
            if (m_Grid[i, j].m_Gem != null && m_Grid[i, j].m_Gem.IsCanMove())
            {
              SpMove.Enqueue(GridPosToIdx(i, j));
            }
          }
        }
      }
    }

    public void OnGemOnlyClear(Gem targetGem)
    {
      targetGem.SetClear();
    }

    public void print()
    {
      System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();
      for (int j = 0; j < m_RowCnt; ++j)
      {
        for (int i = 0; i < m_ColumnCnt; ++i)
        {
          logBuilder.Append(GetColor(i, j));
          logBuilder.Append(" ");
        }
        logBuilder.Append("\n");
      }
      if (m_CBLog != null)
        m_CBLog(logBuilder.ToString());
    }


  }
}

