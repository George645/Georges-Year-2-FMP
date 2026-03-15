using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using Unity.VisualScripting;

public class Unit : MonoBehaviour {
    List<Soldier> childSoldiers = new();
    List<TargetPosition> targetPositions = new();
    public bool selected = false;
    public int CurrentWidth {
        get { return currentWidth; }
    }
    [SerializeField, HideInInspector]
    int currentWidth;

    public int NumberOfSoldiers {
        get {
            if (childSoldiers.Count == 0 && transform.GetComponentsInChildren<Soldier>().Length > 0) {
                childSoldiers = transform.GetComponentsInChildren<Soldier>().ToList();
                targetPositions = transform.GetComponentsInChildren<TargetPosition>().ToList();
            }
            return childSoldiers.Count;
        }
    }

    #region Move unit
    internal void NewPositions(List<Vector3> listOfPositions) {
        StartCoroutine(nameof(UpdatePosition), listOfPositions);
    }
    IEnumerator UpdatePosition(List<Vector3> listOfPositions) {
        List<TargetPosition> oldTargetPositions = new(targetPositions);
        int count = 0;
        for (int h = 0; h < targetPositions.Count; h++) {
            if (oldTargetPositions.Count == 0 || listOfPositions.Count == 0) {
                break;
            }

            float maxDistance = -1;
            int indexOfNewPosition = 0;
            for (int i = 0; i < listOfPositions.Count; i++) {
                for (int j = 0; j < oldTargetPositions.Count; j++) {
                    if (Math.Abs(Vector3.SqrMagnitude(listOfPositions[i] - oldTargetPositions[j].transform.position)) > maxDistance) {
                        indexOfNewPosition = i;
                        maxDistance = Vector3.SqrMagnitude(listOfPositions[i] - oldTargetPositions[j].transform.position);
                    }
                }
            }

            int indexOfOldPosition = -1;
            float minDistance = float.MaxValue;
            for (int i = 0; i < oldTargetPositions.Count; i++) {
                if (Math.Abs(Vector3.SqrMagnitude(listOfPositions[indexOfNewPosition] - oldTargetPositions[i].transform.position)) < minDistance) {
                    minDistance = Vector3.SqrMagnitude(listOfPositions[indexOfNewPosition] - oldTargetPositions[i].transform.position);
                    indexOfOldPosition = i;
                }
            }

            oldTargetPositions[indexOfOldPosition].NewPosition(listOfPositions[indexOfNewPosition]);
            oldTargetPositions.RemoveAt(indexOfOldPosition);
            listOfPositions.RemoveAt(indexOfNewPosition);
            count++;
            if (oldTargetPositions.Count > 0 && 180 / oldTargetPositions.Count < count / 2) {
                yield return null;
                count = 0;
            }
        }
        yield return null;
    }
    #endregion

    #region Set up unit
    int StartingSoldierTotal {
        get { return PlayerPrefs.GetInt("Unit Size", 0) != 0 ? PlayerPrefs.GetInt("Unit Size") : startingSoldierTotal; }
    }
    [SerializeField, HideInInspector]
    int startingSoldierTotal;
    [SerializeField]
    public Vector3 offsetPerTroop; // Make a thing in the unit editor like with starting soldier total
    [SerializeField]
    public Vector3 offsetPerRow; // Make a thing in the unit editor like with starting soldier total

    public void InstantArrangeByWidth(int widthCount) {
        currentWidth = widthCount;
        Debug.Log(currentWidth);
        int currentWidthIndex = 0;
        int currentRowIndex = 0;
        Vector3 FirstPosition = (Vector3.forward + Vector3.right) / 2;
        for (int i = 0; i < childSoldiers.Count; i++) {
            Vector3 positionOfThisSoldier = FirstPosition + offsetPerRow * currentRowIndex + offsetPerTroop * currentWidthIndex;

            targetPositions[i].InstantSetPosition(positionOfThisSoldier);

            currentWidthIndex++;
            if (currentWidthIndex == widthCount) {
                currentWidthIndex = 0;
                currentRowIndex++;
            }
        }
    }
    public void SetUnitCount(int quantity) {
        if (childSoldiers.Count == 0 && transform.GetComponentsInChildren<Soldier>().Length > 0) {
            childSoldiers = transform.GetComponentsInChildren<Soldier>().ToList();
        }
        if (targetPositions.Count == 0 && transform.GetComponentsInChildren<Soldier>().Length > 0) {
            targetPositions = transform.GetComponentsInChildren<TargetPosition>().ToList();
        }
        for (int i = childSoldiers.Count - 1; i >= 0; i--) {
            if (childSoldiers[i] == null) {
                childSoldiers.Remove(childSoldiers[i]);
            }
        }
        for (int i = targetPositions.Count - 1; i >= 0; i--) {
            if (targetPositions[i] == null) {
                targetPositions.Remove(targetPositions[i]);
            }
        }

        if (childSoldiers.Count < quantity) {
            for (int i = childSoldiers.Count; i < quantity; i++) {
                AddSoldierAndTargetPosition();
            }
        }
        else if (childSoldiers.Count > quantity) {
            for (int i = childSoldiers.Count; i > quantity; i--) {
                DestroyImmediate(childSoldiers[i - 1].gameObject);
                childSoldiers.RemoveAt(i - 1);
                DestroyImmediate(targetPositions[i - 1].gameObject);
                targetPositions.RemoveAt(i - 1);
            }
        }
        else {
            Debug.LogWarning("tried to set unit count to what it already was? no change but change registered?");
        }

        InstantArrangeByWidth(quantity / 5);
    }
    Mesh StoredMesh {
        get {
            if (staticMesh == null) {
                staticMesh = serializedMesh;
            }
            return staticMesh;
        }
    }
    [SerializeField]
    Mesh serializedMesh;
    public static Mesh staticMesh;
    Material StoredMaterial {
        get {
            if (staticMaterial == null) {
                staticMaterial = serializedMaterial;
            }
            return staticMaterial;
        }
    }
    [SerializeField]
    Material serializedMaterial;
    public static Material staticMaterial;

    void AddSoldierAndTargetPosition() {
        GameObject addedSoldier = new();
        addedSoldier.transform.parent = transform;
        addedSoldier.transform.name = "Soldier " + childSoldiers.Count;
        addedSoldier.AddComponent<MeshFilter>().mesh = StoredMesh;
        addedSoldier.AddComponent<MeshRenderer>().material = StoredMaterial;
        addedSoldier.AddComponent<CapsuleCollider>();
        addedSoldier.gameObject.layer = LayerMask.NameToLayer("soldierDetection");

        childSoldiers.Add(addedSoldier.AddComponent<Soldier>());
        childSoldiers[^1].unit = this;

        GameObject addedTargetPosition = new GameObject();
        addedTargetPosition.transform.parent = transform;
        addedTargetPosition.transform.name = "Target position for soldier " + targetPositions.Count;
        addedTargetPosition.AddComponent<MeshFilter>().mesh = StoredMesh;
        addedTargetPosition.AddComponent<MeshRenderer>().material = StoredMaterial;

        targetPositions.Add(addedTargetPosition.AddComponent<TargetPosition>());
        targetPositions[^1].thisSoldier = childSoldiers[^1];
    }
    #endregion
}
