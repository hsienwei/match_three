using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;


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

  private MatchThreeCore m_MT;


  // Use this for initialization
  void Start()
  {

    m_TextTimeScale.text = "" + Time.timeScale;
    m_BtnTimeScaleSet.onClick.AddListener(() =>
    {
      Time.timeScale = float.Parse(m_TextTimeScale.text);
    });

    m_MT = new MatchThreeCore(m_Colume, m_Row, 6);
    m_MT.m_CBGenerate = _OnGenerate;
    m_MT.m_CBClear = _OnClear;
    m_MT.m_CBMove = _OnMove;
    m_MT.m_CBLock = _OnLock;
    m_MT.m_CBLog = Debug.Log;

    m_GemGrid = new Transform[m_Colume, m_Row];
    m_GemPos = new Vector3[m_Colume, m_Row];

    m_MT.Generate(true);

    //Generate();
  }



  void Update()
  {
    //m_MT.Update((int)(Time.deltaTime * 1000));
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

    if (MoveType == MatchThreeCore.MOVE_TYPE_MOVE)
    {
      GemA.DOMove(m_GemPos[TargetCol, TargetRow], 0.5f );

      m_GemGrid[Col, Row] =  null;
      m_GemGrid[TargetCol, TargetRow] = GemA;
    }
    else if (MoveType == MatchThreeCore.MOVE_TYPE_SWITCH)
    {
      GemA.DOMove(m_GemPos[TargetCol, TargetRow], 0.5f );
      GemB.DOMove(m_GemPos[Col, Row], 0.5f );

      m_GemGrid[Col, Row] = GemB;
      m_GemGrid[TargetCol, TargetRow] = GemA;
    }
    else if (MoveType == MatchThreeCore.MOVE_TYPE_SWITCHBACK)
    {
      GemA.DOMove(m_GemPos[TargetCol, TargetRow], 0.25f ).SetLoops(2, LoopType.Yoyo);
      GemB.DOMove(m_GemPos[Col, Row], 0.25f ).SetLoops(2, LoopType.Yoyo);
    }
  }

  void _OnLock(int Col, int Row, int Count)
  {

  }

  /*public void Generate()
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
  }*/

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

    /*if (IsSwipeSuccess)
    {
      Sequence Seq = DOTween.Sequence();
      Seq.AppendInterval(0.3f);
      Seq.AppendCallback(() =>
      {

        Clear();

      });
    }
    
  */
  }

  public void Clear()
  {
    /*if (!m_MT.IsHasClearState()) return;

    Sequence Seq = DOTween.Sequence();
    Seq.AppendInterval(0.3f);
    Seq.AppendCallback(() =>
    {
      m_MT.CleanMatchState();
      m_MT.Generate(false);
      Generate();

    });

  */
  }

  public void Scan()
  {
    //m_MT.Scan();
    //Clear();
  }

  #region Test_methon
  public void TestClear()
  {
    m_MT.Scan();
  }

  #endregion


  #region LeanTouch

  private Vector2 m_TouchGemPos;

  protected virtual void OnEnable()
  {
    // Hook into the events we need
    LeanTouch.OnFingerDown += OnFingerDown;
    LeanTouch.OnFingerSwipe += OnFingerSwipe;
  }

  protected virtual void OnDisable()
  {
    // Unhook the events
    LeanTouch.OnFingerDown -= OnFingerDown;
    LeanTouch.OnFingerSwipe -= OnFingerSwipe;
  }

  public void OnFingerDown(LeanFinger finger)
  {
    //Debug.Log("Finger " + finger.Index + " began touching the screen");
    var m_TouchPos = finger.ScreenPosition;
    m_TouchGemPos = GetGemPos(m_TouchPos);
  }

  public void OnFingerSwipe(LeanFinger finger)
  {
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


  #endregion
}
