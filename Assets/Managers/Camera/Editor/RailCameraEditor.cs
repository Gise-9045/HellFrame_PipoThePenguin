#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailCamera))]
public class RailCameraEditor : Editor
{
    private RailInfo tempRailInfo;
    
    public override void OnInspectorGUI()
    {
        RailCamera railCamera = (RailCamera)target;
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Rail Configuration Editor", EditorStyles.boldLabel);
        
        if (tempRailInfo == null)
        {
            tempRailInfo = new RailInfo();
        }
        
        EditorGUI.BeginChangeCheck();
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        
        if (GUILayout.Button("Set Rail Info to Camera"))
        {
            railCamera.CopyCurrentToRailInfo(tempRailInfo);
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Copy Rail Info to Clipboard", GUILayout.Height(25)))
        {
            railCamera.CopyCurrentToRailInfo(tempRailInfo);
            string json = JsonUtility.ToJson(tempRailInfo, true);
            EditorGUIUtility.systemCopyBuffer = json;
            Debug.Log("Rail Info copied to clipboard!");
        }
    }
    
    private void OnSceneGUI()
    {
        RailCamera railCamera = (RailCamera)target;
    }
}
#endif