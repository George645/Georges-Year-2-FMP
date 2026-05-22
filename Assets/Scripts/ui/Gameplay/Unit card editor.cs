using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateUnitSelectionCards)), CanEditMultipleObjects]
public class Unitcardeditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Reset the cards"))
            ((CreateUnitSelectionCards)target).ReplaceUnitCards();
        serializedObject.ApplyModifiedProperties();
    }
}
