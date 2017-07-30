using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

public class MatchThree : MonoBehaviour {
    public int m_Row;
    public int m_Colume;
    public Camera m_Camera;

    private int m_Size = 2;

    public SpriteRenderer[] m_GemTmpList;

    private Transform[,] m_GemGrid;
    private Vector3[,] m_GemPos;

    // Use this for initialization
    void Start () {
		Vector3 Offset = new Vector3((m_Colume -1) * 0.5f * m_Size, (m_Row - 1) * 0.5f * m_Size, 0);

        m_GemGrid = new Transform[m_Colume, m_Row];
        m_GemPos = new Vector3[m_Colume, m_Row];

        for (int i=0; i< m_Row; ++i )
        {
            for (int j = 0; j < m_Colume; ++j)
            {
                SpriteRenderer GemInst = GameObject.Instantiate(m_GemTmpList[Random.Range(0, m_GemTmpList.Length)]);
                GemInst.transform.position = new Vector3(j * m_Size, i * m_Size, 0) - Offset;
                m_GemGrid[j, i] = GemInst.transform;
                m_GemPos[j, i] = GemInst.transform.position;
            }
        }

	}
	

    Vector2 GetGemPos(Vector2 TouchPos)
    {
        Vector3 WorldPos = GetWorldPos(TouchPos);
        Debug.Log(WorldPos);
        int Row = (int)( WorldPos.y +  m_Size * (m_Row ) * 0.5f) / m_Size;
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
        int TargetCol = Col, TargetRow = Row;
        if( Direction == 0) TargetRow++;
        if (Direction == 1) TargetRow--;
        if (Direction == 2) TargetCol--;
        if (Direction == 3) TargetCol++;

        if (TargetRow >= m_Row) return;
        if (TargetRow < 0) return;
        if (TargetCol >= m_Colume) return;
        if (TargetCol < 0) return;

        var TargetGemGrid = m_GemGrid[TargetCol, TargetRow];
        var FromGemGrid = m_GemGrid[Col, Row];

        TargetGemGrid.transform.DOMove(m_GemPos[Col, Row], 0.2f);
        FromGemGrid.transform.DOMove(m_GemPos[TargetCol, TargetRow], 0.2f);

        m_GemGrid[TargetCol, TargetRow] = FromGemGrid;
        m_GemGrid[Col, Row] = TargetGemGrid;
    }


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
        if(Mathf.Abs(SwipeDelta.x) < Mathf.Abs(SwipeDelta.y))
        {
            if(SwipeDelta.y > 0)
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
}
