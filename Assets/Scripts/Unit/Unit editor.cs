#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Unit))]
public class Uniteditor : Editor {
    SerializedProperty soldierCount;
    SerializedProperty width;
    private void OnEnable() {
        soldierCount = serializedObject.FindProperty("startingSoldierTotal");
        width = serializedObject.FindProperty("currentWidth");
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        HasSoldierCountChanged();
        serializedObject.ApplyModifiedProperties();
    }
    void HasSoldierCountChanged() {
        int priorSoldierCount = soldierCount.intValue;
        EditorGUILayout.IntSlider(soldierCount, 0, 180);
        if (priorSoldierCount != soldierCount.intValue) {
            ((Unit)target).SetSoldierCount(soldierCount.intValue);
            width.intValue = soldierCount.intValue / 5;

        }

        if (GUILayout.Button("Button")) {
            ((Unit)target).DebugArrayNumber();
        }
    }

}
#endif