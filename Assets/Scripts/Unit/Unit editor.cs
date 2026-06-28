#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Unit))]
public class Uniteditor : Editor {
    SerializedProperty soldierCount;
    SerializedProperty width;

    SerializedProperty extraVariableDeleteLater;

    private void OnEnable() {
        soldierCount = serializedObject.FindProperty("startingSoldierTotal");
        width = serializedObject.FindProperty("currentWidth");
        extraVariableDeleteLater = serializedObject.FindProperty("a");
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        HasSoldierCountChanged();
        serializedObject.ApplyModifiedProperties();
    }
    void HasSoldierCountChanged() {
        int priorSoldierCount = soldierCount.intValue;
        if (soldierCount.intValue > ((Unit)(target)).transform.childCount) soldierCount.intValue = ((Unit)(target)).transform.childCount;
        EditorGUILayout.IntSlider(soldierCount, 0, 180);
        if (priorSoldierCount != soldierCount.intValue) {
            ((Unit)target).SetSoldierCount(soldierCount.intValue);
            width.intValue = soldierCount.intValue / 5;
        }

        if (soldierCount.intValue - 1 < extraVariableDeleteLater.intValue)
            extraVariableDeleteLater.intValue = soldierCount.intValue - 1;
        EditorGUILayout.IntSlider(extraVariableDeleteLater, 0, soldierCount.intValue - 1);
        if (GUILayout.Button("Button")) {
            ((Unit)target).DebugSoldierInfo(extraVariableDeleteLater.intValue);
        }

    }

}
#endif