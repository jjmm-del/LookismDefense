using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Debugger))]
public class DebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //원래 있던 기본 변수 
        base.OnInspectorGUI();
        
        //연결된 Debugger 스크립트를 가져옴
        Debugger debugger = (Debugger)target;
        
        //약간의 여백과 제목 추가
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("치트 및 디버그 버튼", EditorStyles.boldLabel);
        
        //버튼1: 골드 추가
        //GUILayout.Height(30)으로 버튼을 크고 누르기 쉽게 만듭니다.
        if (GUILayout.Button("골드 +1000", GUILayout.Height(30)))
        {
            //게임이 실행중 일 때만 존재하도록 방어(GameManager는 실행중일 때만 존재)
            if (Application.isPlaying) debugger.AddGold(1000);
            else Debug.LogWarning("게임 실행 중에만 작동합니다!");
        }
        EditorGUILayout.Space(5);
        //버튼2: 흔함 랜덤 소환권 추가
        if (GUILayout.Button("흔함 랜덤 소환권 +10", GUILayout.Height(30)))
        {
            if(Application.isPlaying) debugger.AddRandomCommon(10);
            else Debug.LogWarning("게임 실행 중에만 작동합니다!");
        }
        EditorGUILayout.Space(5);
        //버튼3: 흔함 선택 소환권 추가
        if (GUILayout.Button("흔함 선택 소환권 +1", GUILayout.Height(30)))
        {
            if (Application.isPlaying) debugger.AddSelectCommon(1);
            else Debug.LogWarning("게임 실행 중에만 작동합니다!");
        }

    }
}
