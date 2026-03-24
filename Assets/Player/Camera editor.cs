#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraScript), false)]
public class MoveViewportEditor : Editor {
    SerializedProperty positionsList;
    SerializedProperty positionsCount;
    SerializedProperty currentIndex;
    private void OnEnable() {
        positionsList = serializedObject.FindProperty("positions");
        positionsCount = serializedObject.FindProperty("positionsCount");
        currentIndex = serializedObject.FindProperty("currentIndex");
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        ChangeListSize();
        ChangeDisplayingIndex();
        SetIndexedPosition();
        serializedObject.ApplyModifiedProperties();
    }

    void ChangeListSize() {
        int checkCount = positionsCount.intValue;
        EditorGUILayout.PropertyField(positionsCount);
        if (checkCount != positionsCount.intValue)
            positionsList.arraySize = positionsCount.intValue;
    }
    // < [x] Label
    void ChangeDisplayingIndex() {
        int checkSelectedIndex = currentIndex.intValue;
        currentIndex.intValue = EditorGUILayout.IntField(currentIndex.displayName, currentIndex.intValue);
        if (checkSelectedIndex != currentIndex.intValue) {
            ((CameraScript)target).SetCameraPosition(currentIndex.intValue);
        }
    }
    void SetIndexedPosition() {
        if (GUILayout.Button("set current index in the list to position?")) {
            ((CameraScript)target).SetElementAtIndexToPosition(currentIndex.intValue);
        }
    }
}
#endif