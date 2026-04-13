#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraScript), false)]
public class MoveViewportEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif