using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

public class GridState
{
  public int Color;
  public int MatchCount;
  public int LockCnt;

  public GridState()
  {
    Color = -1;
    MatchCount = 0;
    LockCnt = 0;
  }
}

public class MatchThreeLogic
{
  public static readonly int MOVE_TYPE_MOVE = 1;
  public static readonly int MOVE_TYPE_SWITCH = 2;
  public static readonly int MOVE_TYPE_SWITCHBACK = 3;

  public delegate void OnGenerate(int Col, int Row, int Color);
  public delegate void OnClear(int Col, int Row);
  public delegate void OnMove(int Col, int Row, int TargetCol, int TargetRow, int Type);
  public delegate void OnLock(int Col, int Row, int Count);

  private GridState[,] m_Grid;
  private int m_Row, m_Col;
  private int m_ColorCount;

  private bool m_IsHasMatch;

  public OnGenerate m_CBGenerate;
  public OnClear m_CBClear;
  public OnMove m_CBMove;
  public OnLock m_CBLock;

  public MatchThreeLogic(int Col, int Row, int ColorCount)
  {
    m_Grid = new GridState[Col, Row];
    m_Row = Row;
    m_Col = Col;
    m_ColorCount = ColorCount;


  }

  int Col { get { return m_Col; } }
  int Row { get { return m_Row; } }

  public int GetColor(int Col, int Row)
  {
    return m_Grid[Col, Row].Color;
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
          if(m_Grid[i, j].LockCnt < 0)
          {
            m_Grid[i, j].LockCnt = 0;
            IsUnlock = true;
          }

          //if (m_CBLock != null) m_CBLock(i, j, m_Grid[i, j].LockCnt);
        }
      }
    }

    if(IsUnlock)
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
          }
        }
      }
    }


    //for (int j = m_Row - 1; j >= 0; --j)
    for (int j = 0; j < m_Row; ++j)
    {
      for (int i = 0; i < m_Col; ++i)
      {
        if (m_Grid[i, j].Color == -1)
        {
          // 先補再產
          // 上方沒有的話要產出.
          // 先只看正上方.

          //for(int k = j-1;  k >=0 ; --k)
          for (int k = j + 1; k < m_Row; ++k)
          {
            if (m_Grid[i, k].Color != -1)
            {
              m_Grid[i, j].Color = m_Grid[i, k].Color;
              m_Grid[i, k].Color = -1;
              m_Grid[i, j].LockCnt = 1000 /** Mathf.Abs(j - k)*/;
              if (m_CBMove != null)
              {
                m_CBMove(i, k, i, j, MOVE_TYPE_MOVE);
              }
              break;
            }
            /*else
            {
                m_Grid[i, j].Color = Random.Range(0, m_ColorCount);
                if (m_CBGenerate != null)
                {
                    m_CBGenerate(i, j, m_Grid[i, j].Color);
                }
            }*/
          }
        }
      }
    }

    for (int i = 0; i < m_Col; ++i)
    {
      for (int j = 0; j < m_Row; ++j)
      {

        if (m_Grid[i, j].Color == -1)
        {
          //TODO: 檢查上方掉落
          // 先補再產, 兩輪

          // 上方沒有的話要產出
          m_Grid[i, j].Color = Random.Range(0, m_ColorCount);
          m_Grid[i, j].LockCnt = 1000;
          if (m_CBGenerate != null)
          {
            m_CBGenerate(i, j, m_Grid[i, j].Color);
          }
        }
      }
    }
    /*for (int i= 0; i< m_Col; ++i)
    {
        for (int j = 0; j < m_Row; ++j)
        {

            if (m_Grid[i, j].Color == -1)
            {
                //TODO: 檢查上方掉落
                // 先補再產, 兩輪

                // 上方沒有的話要產出
                m_Grid[i, j].Color = Random.Range(0, m_ColorCount);
                if (m_CBGenerate != null)
                {
                    m_CBGenerate(i, j, m_Grid[i, j].Color);
                }
            }
        }
    }*/
  }

  public bool Swipe(int Col, int Row, int Direction)
  {
    int CurrColor = m_Grid[Col, Row].Color;
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

    int TargetColor = m_Grid[TargetCol, TargetRow].Color;

    m_Grid[Col, Row].Color = TargetColor;
    m_Grid[TargetCol, TargetRow].Color = CurrColor;

    Scan();

    if(IsHasClearState())
    {
      m_CBMove(TargetCol, TargetRow, Col, Row, MOVE_TYPE_SWITCH);
      m_Grid[Col, Row].LockCnt = 1000;
      m_Grid[TargetCol, TargetRow].LockCnt = 1000;

      CleanMatchState();
      Generate(false);
    }
    else
    {
      m_CBMove(TargetCol, TargetRow, Col, Row, MOVE_TYPE_SWITCHBACK);

      m_Grid[Col, Row].Color = CurrColor;
      m_Grid[TargetCol, TargetRow].Color = TargetColor;

      m_Grid[Col, Row].LockCnt = 1000;
      m_Grid[TargetCol, TargetRow].LockCnt = 1000;
      return false;
    }

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
          m_Grid[i, j].Color = -1;
          m_Grid[i, j].MatchCount = 0;
          m_Grid[i, j].LockCnt = 0;
          if (m_CBLock != null)
          {
            m_CBLock(i, j, m_Grid[i, j].LockCnt);
          }
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
    int CurrColor = m_Grid[Col, Row].Color;
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
      if(m_Grid[LCol, LRow].LockCnt == 0)
        LSame = (CurrColor == m_Grid[LCol, LRow].Color);
    }
    int RCol = Col + 1, RRow = Row;
    if (RCol < m_Col)
    {
      if (m_Grid[RCol, RRow].LockCnt == 0)
        RSame = (CurrColor == m_Grid[RCol, RRow].Color);
    }

    int UCol = Col, URow = Row - 1;
    if (URow >= 0)
    {
      if (m_Grid[UCol, URow].LockCnt == 0)
        USame = (CurrColor == m_Grid[UCol, URow].Color);
    }
    int DCol = Col, DRow = Row + 1;
    if (DRow < m_Row)
    {
      if (m_Grid[DCol, DRow].LockCnt == 0)
        DSame = (CurrColor == m_Grid[DCol, DRow].Color);
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
        LogBuilder.Append(m_Grid[i, j].Color);
        LogBuilder.Append(" ");
      }
      LogBuilder.Append("\n");
    }
    Debug.Log(LogBuilder.ToString());
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
    Debug.Log(LogBuilder.ToString());
  }

}

public class MatchThree : MonoBehaviour
{
  public UnityEngine.UI.InputField m_TextTimeScale;
  public UnityEngine.UI.Button m_BtnTimeScaleSet;

  public int m_Row;
  public int m_Colume;
  public Camera m_Camera;

  private int m_Size = 2;

  public SpriteRenderer[] m_GemTmpList;

  private Transform[,] m_GemGrid;
  private Vector3[,] m_GemPos;

  private MatchThreeLogic m_MT;


  // Use this for initialization
  void Start()
  {

    m_TextTimeScale.text = "" + Time.timeScale;
    m_BtnTimeScaleSet.onClick.AddListener(() =>
    {
      Time.timeScale = float.Parse(m_TextTimeScale.text);
    });

    m_MT = new MatchThreeLogic(m_Colume, m_Row, 6);
    m_MT.m_CBGenerate = _OnGenerate;
    m_MT.m_CBClear = _OnClear;
    m_MT.m_CBMove = _OnMove;
    m_MT.m_CBLock = _OnLock;


    m_GemGrid = new Transform[m_Colume, m_Row];
    m_GemPos = new Vector3[m_Colume, m_Row];

    m_MT.Generate(true);

    Generate();
  }



  void Update()
  {
    m_MT.Update((int)(Time.deltaTime * 1000));
  }

  void _OnGenerate(int Col, int Row, int Color)
  {
    Vector3 Offset = new Vector3((m_Colume - 1) * 0.5f * m_Size, (m_Row - 1) * 0.5f * m_Size, 0);

    if (m_GemGrid[Col, Row] == null)
    {
      SpriteRenderer GemInst = GameObject.Instantiate(m_GemTmpList[m_MT.GetColor(Col, Row)]);
      GemInst.transform.position = new Vector3(Col * m_Size, Row * m_Size, 0) - Offset;
      m_GemGrid[Col, Row] = GemInst.transform;
      GemInst.transform.localScale = Vector3.zero;
      GemInst.transform.DOScale(1.3f, 0.3f);
      m_GemPos[Col, Row] = GemInst.transform.position;
    }

  }

  void _OnClear(int Col, int Row)
  {
    var Gem = m_GemGrid[Col, Row].transform;
    m_GemGrid[Col, Row] = null;
    Gem.DOScale(0, 0.3f).OnComplete(() =>
    {
      GameObject.Destroy(Gem.gameObject);
    });
  }

  void _OnMove(int Col, int Row, int TargetCol, int TargetRow, int MoveType)
  {
    var GemA = m_GemGrid[Col, Row];
    var GemB = m_GemGrid[TargetCol, TargetRow];

    int MoveNum = Mathf.Abs(TargetRow - Row) + Mathf.Abs(TargetCol - Col);

    if (MoveType == MatchThreeLogic.MOVE_TYPE_MOVE)
    {
      GemA.DOMove(m_GemPos[TargetCol, TargetRow], 0.5f );

      m_GemGrid[Col, Row] =  null;
      m_GemGrid[TargetCol, TargetRow] = GemA;
    }
    else if (MoveType == MatchThreeLogic.MOVE_TYPE_SWITCH)
    {
      GemA.DOMove(m_GemPos[TargetCol, TargetRow], 0.5f );
      GemB.DOMove(m_GemPos[Col, Row], 0.5f );

      m_GemGrid[Col, Row] = GemB;
      m_GemGrid[TargetCol, TargetRow] = GemA;
    }
    else if (MoveType == MatchThreeLogic.MOVE_TYPE_SWITCHBACK)
    {
      GemA.DOMove(m_GemPos[TargetCol, TargetRow], 0.25f ).SetLoops(2, LoopType.Yoyo);
      GemB.DOMove(m_GemPos[Col, Row], 0.25f ).SetLoops(2, LoopType.Yoyo);
    }
  }

  void _OnLock(int Col, int Row, int Count)
  {

  }

  public void Generate()
  {

    Sequence Seq = DOTween.Sequence();
    Seq.AppendInterval(0.3f);
    Seq.AppendCallback(() =>
    {

      m_MT.Scan();
      if (m_MT.IsHasClearState())
      {
        Clear();
      }
    });

  }

  //
  // for test.
  //

  public void print()
  {
    m_MT.print();
  }

  public void printClearState()
  {
    m_MT.printClearState();
  }

  Vector2 GetGemPos(Vector2 TouchPos)
  {
    Vector3 WorldPos = GetWorldPos(TouchPos);
    Debug.Log(WorldPos);
    int Row = (int)(WorldPos.y + m_Size * (m_Row) * 0.5f) / m_Size;
    int Col = (int)(WorldPos.x + m_Size * (m_Colume) * 0.5f) / m_Size;
    Debug.Log("Row " + Row + " Col " + Col);

    return new Vector2(Col, Row);
  }

  Vector3 GetWorldPos(Vector2 TouchPos)
  {
    Vector3 WorldPos = Vector3.zero;
    Ray ray = m_Camera.ScreenPointToRay(new Vector3(TouchPos.x, TouchPos.y, 0));
    Plane GemPlane = new Plane(-Vector3.forward, Vector3.zero);

    float rayDistance;
    if (GemPlane.Raycast(ray, out rayDistance))
      WorldPos = ray.GetPoint(rayDistance);
    return WorldPos;
  }

  void Swipe(int Col, int Row, int Direction)
  {
    bool IsSwipeSuccess = m_MT.Swipe(Col, Row, Direction);

    if (IsSwipeSuccess)
    {
      Sequence Seq = DOTween.Sequence();
      Seq.AppendInterval(0.3f);
      Seq.AppendCallback(() =>
      {

        Clear();

      });
    }
    

  }

  public void Clear()
  {
    if (!m_MT.IsHasClearState()) return;

    Sequence Seq = DOTween.Sequence();
    Seq.AppendInterval(0.3f);
    Seq.AppendCallback(() =>
    {
      m_MT.CleanMatchState();
      m_MT.Generate(false);
      Generate();

    });


  }

  public void Scan()
  {
    m_MT.Scan();
    Clear();
  }


  #region LeanTouch

  private Vector2 m_TouchGemPos;

  protected virtual void OnEnable()
  {
    // Hook into the events we need
    LeanTouch.OnFingerDown += OnFingerDown;
    LeanTouch.OnFingerSet += OnFingerSet;
    LeanTouch.OnFingerUp += OnFingerUp;
    LeanTouch.OnFingerTap += OnFingerTap;
    LeanTouch.OnFingerSwipe += OnFingerSwipe;
    LeanTouch.OnGesture += OnGesture;
  }

  protected virtual void OnDisable()
  {
    // Unhook the events
    LeanTouch.OnFingerDown -= OnFingerDown;
    LeanTouch.OnFingerSet -= OnFingerSet;
    LeanTouch.OnFingerUp -= OnFingerUp;
    LeanTouch.OnFingerTap -= OnFingerTap;
    LeanTouch.OnFingerSwipe -= OnFingerSwipe;
    LeanTouch.OnGesture -= OnGesture;
  }

  public void OnFingerDown(LeanFinger finger)
  {
    //Debug.Log("Finger " + finger.Index + " began touching the screen");
    var m_TouchPos = finger.ScreenPosition;
    m_TouchGemPos = GetGemPos(m_TouchPos);
  }

  public void OnFingerSet(LeanFinger finger)
  {
    //Debug.Log("Finger " + finger.Index + " is still touching the screen");
  }

  public void OnFingerUp(LeanFinger finger)
  {
    //Debug.Log("Finger " + finger.Index + " finished touching the screen");
  }

  public void OnFingerTap(LeanFinger finger)
  {
    // Debug.Log("Finger " + finger.Index + " tapped the screen");
  }

  public void OnFingerSwipe(LeanFinger finger)
  {
    //Debug.Log("Finger " + finger.Index + " swiped the screen");
    var SwipeDelta = finger.SwipeScreenDelta;
    if (Mathf.Abs(SwipeDelta.x) < Mathf.Abs(SwipeDelta.y))
    {
      if (SwipeDelta.y > 0)
      {
        // 上
        Swipe((int)m_TouchGemPos.x, (int)m_TouchGemPos.y, 0);
      }
      if (SwipeDelta.y < 0)
      {
        // 下
        Swipe((int)m_TouchGemPos.x, (int)m_TouchGemPos.y, 1);
      }
    }
    else if (Mathf.Abs(SwipeDelta.x) > Mathf.Abs(SwipeDelta.y))
    {
      if (SwipeDelta.x > 0)
      {
        // 右
        Swipe((int)m_TouchGemPos.x, (int)m_TouchGemPos.y, 3);
      }
      if (SwipeDelta.x < 0)
      {
        // 左
        Swipe((int)m_TouchGemPos.x, (int)m_TouchGemPos.y, 2);
      }
    }
  }

  public void OnGesture(List<LeanFinger> fingers)
  {
    /*Debug.Log("Gesture with " + fingers.Count + " finger(s)");
    Debug.Log("    pinch scale: " + LeanGesture.GetPinchScale(fingers));
    Debug.Log("    twist degrees: " + LeanGesture.GetTwistDegrees(fingers));
    Debug.Log("    twist radians: " + LeanGesture.GetTwistRadians(fingers));
    Debug.Log("    screen delta: " + LeanGesture.GetScreenDelta(fingers));*/
  }

  #endregion
}
