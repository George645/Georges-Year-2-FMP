#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomGrid)), CanEditMultipleObjects]
public class GridEditor : Editor {
    SerializedProperty TotalSizeOfMap;

    SerializedProperty UnitReferences;
    SerializedProperty UnitSquareIndex;
    SerializedProperty SoldierReferences;
    SerializedProperty SoldierSquareIndex;

    SerializedProperty UnitGridSize;
    SerializedProperty DisplayUnitGrid;
    SerializedProperty UnitGrid;
    SerializedProperty SoldierGridSize;
    SerializedProperty DisplaySoldierGrid;
    SerializedProperty SoldierGrid;

    SerializedProperty LineMaterial;

    private void OnEnable() {
        TotalSizeOfMap = serializedObject.FindProperty("totalSizeOfMap");

        UnitReferences = serializedObject.FindProperty("unitReferences");
        UnitSquareIndex = serializedObject.FindProperty("unitSquareIndex");
        SoldierReferences = serializedObject.FindProperty("soldierReferences");
        SoldierSquareIndex = serializedObject.FindProperty("soldierSquareIndex");

        UnitGridSize = serializedObject.FindProperty("unitGridWidthCount");
        DisplayUnitGrid = serializedObject.FindProperty("displayUnitGrid");
        UnitGrid = serializedObject.FindProperty("unitGrid");
        SoldierGridSize = serializedObject.FindProperty("soldierGridWidthCount");
        DisplaySoldierGrid = serializedObject.FindProperty("displaySoldierGrid");
        SoldierGrid = serializedObject.FindProperty("soldierGrid");

        LineMaterial = serializedObject.FindProperty("lineMaterial");
    }

    public override void OnInspectorGUI() {
        TotalSizeOfMap.intValue = EditorGUILayout.IntField(TotalSizeOfMap.displayName, TotalSizeOfMap.intValue);

        int priorUnitGridSize = UnitGridSize.intValue;
        int priorSoldierGridSize = SoldierGridSize.intValue;

        UnitGridSize.intValue = EditorGUILayout.IntField(UnitGridSize.displayName, UnitGridSize.intValue);
        DisplayUnitGrid.boolValue = EditorGUILayout.Toggle(DisplayUnitGrid.displayName, DisplayUnitGrid.boolValue);

        if (UnitGridSize.intValue != priorUnitGridSize) {
            //maybe do something, not sure yet
        }


        SoldierGridSize.intValue = EditorGUILayout.IntField(SoldierGridSize.displayName, SoldierGridSize.intValue);
        DisplaySoldierGrid.boolValue = EditorGUILayout.Toggle(DisplaySoldierGrid.displayName, DisplaySoldierGrid.boolValue);

        if (SoldierGridSize.intValue != priorSoldierGridSize) {
            //maybe do something, not sure yet
        }
        EditorGUILayout.PropertyField(UnitReferences);
        EditorGUILayout.PropertyField(UnitSquareIndex);
        if (SoldierReferences != null) {
            EditorGUILayout.PropertyField(SoldierReferences);
            EditorGUILayout.PropertyField(SoldierSquareIndex);
        }
        serializedObject.ApplyModifiedProperties();
        //serializedObject.Update();
    }
}
#endif
