using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MatchThree))]
[CanEditMultipleObjects]
public class MatchThreeEditor : Editor
{

  MatchThree Obj;
  void OnEnable()
  {
    Obj = (MatchThree)target;

  }

  // Update is called once per frame
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
    if (GUILayout.Button("Test Clear"))
    {
      Obj.TestClear();
    }


    if (GUILayout.Button("Test Drop"))
    {
      Obj.TestDrop();
    }
    
    if (GUILayout.Button("Test Gen"))
    {
      Obj.TestGen();
    }

    if (GUILayout.Button("Test Clear and Gen"))
    {
      Obj.TestClearAndGen();
    }


    /*
    if (GUILayout.Button("print clear state"))
    {

      Obj.printClearState();
    }

    if (GUILayout.Button("clear"))
    {
      Obj.print();
      Obj.printClearState();
      Obj.Clear();
    }*/
  }
}
