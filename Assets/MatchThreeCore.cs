﻿using System;

public class GridState
{
  
  public int MatchCount;
  public int LockCnt;
  public Gem m_Gem;

  public GridState()
  {
    m_Gem = null;
    MatchCount = 0;
    LockCnt = 0;
  }

  public void GenGem(int Color)
  {
    if (m_Gem == null)
      m_Gem = new Gem(Color);
  }
}

public class Gem
{
  private int m_Color;

  public int Color { get { return m_Color; } }

  public Gem(int Color)
  {
    m_Color = Color;
  }
}

public class MatchThreeCore
{
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

  private GridState[,] m_Grid;
  private int m_Row, m_Col;
  private int m_ColorCount;
  private bool m_IsHasMatch;

  private Random m_Rand;

  public MatchThreeCore(int Col, int Row, int ColorCount)
  {
    m_Grid = new GridState[Col, Row];
    m_Row = Row;
    m_Col = Col;
    m_ColorCount = ColorCount;

    m_Rand = new Random();
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
    return m_Grid[Col, Row].m_Gem.Color;
  }

  public int GetClearCount(int Col, int Row)
  {
    return m_Grid[Col, Row].MatchCount;
  }

  public int GetLockCount(int Col, int Row)
  {
    return m_Grid[Col, Row].LockCnt;
  }

  public void Update(int TimeUnit)
  {
    bool IsUnlock = false;
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        if (m_Grid[i, j].LockCnt > 0)
        {
          m_Grid[i, j].LockCnt -= TimeUnit;
          if (m_Grid[i, j].LockCnt < 0)
          {
            m_Grid[i, j].LockCnt = 0;
            IsUnlock = true;
          }

          //if (m_CBLock != null) m_CBLock(i, j, m_Grid[i, j].LockCnt);
        }
      }
    }

    if (IsUnlock)
    {
      Scan();
    }
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
            m_CBGenerate(i, j, GetColor(i, j));
          }
        }
      }
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

    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {

        if (GetColor(i, j) == -1)
        {
          //TODO: 檢查上方掉落
          // 先補再產, 兩輪

          // 上方沒有的話要產出
          m_Grid[i, j].Color = m_Rand.Next(0, m_ColorCount);
          m_Grid[i, j].LockCnt = 1000;
          if (m_CBGenerate != null)
          {
            m_CBGenerate(i, j, GetColor(i, j));
          }
        }
      }
    }
    */
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

    if (m_Grid[Col, Row].LockCnt > 0) return false;
    if (m_Grid[TargetCol, TargetRow].LockCnt > 0) return false;

    int TargetColor = GetColor(TargetCol, TargetRow);

    ChangeGem(m_Grid[Col, Row], m_Grid[TargetCol, TargetRow]);
    ///m_CBMove(TargetCol, TargetRow, Col, Row, MOVE_TYPE_SWITCH);

    //Scan();

    //if (IsHasClearState())
    {
      m_CBMove(TargetCol, TargetRow, Col, Row, MOVE_TYPE_SWITCH);
      //m_Grid[Col, Row].LockCnt = 1000;
      //m_Grid[TargetCol, TargetRow].LockCnt = 1000;

      //CleanMatchState();
      //Generate(false);
    }
    /*else
    {
      m_CBMove(TargetCol, TargetRow, Col, Row, MOVE_TYPE_SWITCHBACK);

      ChangeGem(m_Grid[Col, Row], m_Grid[TargetCol, TargetRow]);

      m_Grid[Col, Row].LockCnt = 1000;
      m_Grid[TargetCol, TargetRow].LockCnt = 1000;
      return false;
    }*/


    return true;
  }

  public void Scan()
  {
    m_IsHasMatch = false;
    CleanMatchState();

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

  public void CleanMatchState()
  {
    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {
        // TODO:產出特殊寶石
        if (m_Grid[i, j].MatchCount > 0)
        {
          //m_Grid[i, j].Color = -1;
          m_Grid[i, j].GenGem(1);
          m_Grid[i, j].MatchCount = 0;
          m_Grid[i, j].LockCnt = 0;
          /*if (m_CBLock != null)
          {
            m_CBLock(i, j, m_Grid[i, j].LockCnt);
          }*/
          if (m_CBClear != null)
          {
            m_CBClear(i, j);
          }
        }

      }
    }
  }

  bool CheckMatch(int Col, int Row)
  {
    int CurrColor = GetColor(Col, Row);
    if (CurrColor == -1) return false;

    if (m_Grid[Col, Row].LockCnt > 0)
      return false;

    bool IsMatch = false;

    //橫豎各自檢查

    bool LSame, RSame, USame, DSame;

    LSame = RSame = USame = DSame = false;

    int LCol = Col - 1, LRow = Row;
    if (LCol >= 0)
    {
      if (m_Grid[LCol, LRow].LockCnt == 0)
        LSame = (CurrColor == GetColor(LCol, LRow));
    }
    int RCol = Col + 1, RRow = Row;
    if (RCol < m_Col)
    {
      if (m_Grid[RCol, RRow].LockCnt == 0)
        RSame = (CurrColor == GetColor(RCol, RRow));
    }

    int UCol = Col, URow = Row - 1;
    if (URow >= 0)
    {
      if (m_Grid[UCol, URow].LockCnt == 0)
        USame = (CurrColor == GetColor(UCol, URow));
    }
    int DCol = Col, DRow = Row + 1;
    if (DRow < m_Row)
    {
      if (m_Grid[DCol, DRow].LockCnt == 0)
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
