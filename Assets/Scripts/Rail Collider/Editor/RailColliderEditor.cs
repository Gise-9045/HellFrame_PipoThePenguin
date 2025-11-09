#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailCollider))]
public class RailColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        RailCollider railCollider = (RailCollider)target;
        
        // Paste from Clipboard button
        if (GUILayout.Button("Paste Rail Info from Clipboard", GUILayout.Height(25)))
        {
            try
            {
                string json = EditorGUIUtility.systemCopyBuffer;
                RailInfo pastedRailInfo = JsonUtility.FromJson<RailInfo>(json);
                
                if (pastedRailInfo != null)
                {
                    Undo.RecordObject(railCollider, "Paste Rail Info");
                    
                    var field = typeof(RailCollider).GetField("railInfo", 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                    
                    if (field != null)
                    {
                        field.SetValue(railCollider, pastedRailInfo);
                        EditorUtility.SetDirty(railCollider);
                        Debug.Log("Rail Info pasted successfully!");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to paste Rail Info. Make sure you copied valid Rail Info data.\n" + e.Message);
            }
        }
        
        EditorGUILayout.HelpBox(
            "Use 'Paste Rail Info from Clipboard' to paste Rail Info copied from RailCamera Editor.",
            MessageType.Info
        );
    }
}
#endif