// Editor/MissionClearerEditor.cs
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionClearer))]
public class MissionClearerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var stageProp = serializedObject.FindProperty("stage");
        var missionIndexProp = serializedObject.FindProperty("missionIndexToClear");

        EditorGUILayout.PropertyField(stageProp);

        if (stageProp.objectReferenceValue is Stage stage && stage.StageSO != null)
        {
            var missions = stage.StageSO.Missions;
            var options = System.Array.ConvertAll(missions, m => m.missionTitle);
            int current = System.Array.FindIndex(missions, m => m.missionIndex == missionIndexProp.intValue);
            int selected = EditorGUILayout.Popup("Mission To Clear", Mathf.Max(current, 0), options);
            missionIndexProp.intValue = missions[selected].missionIndex;
        }
        else
        {
            EditorGUILayout.HelpBox("Stage를 먼저 할당해주세요.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}