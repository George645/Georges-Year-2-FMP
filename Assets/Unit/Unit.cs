using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.PlayerLoop;

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
    private void Start() {
        InitializePositions();
    }



    #region General unit questions
    void InitializePositions() {
        Debug.Log(NumberOfSoldiers);
        unitPositions = new Vector3[childSoldiers.Count()];
        for (int i = 0; i < childSoldiers.Count(); i++) {
            unitPositions[i] = childSoldiers[i].transform.position;
            Debug.Log(unitPositions[i]);
        }
    }
    Vector3[] unitPositions = { };
    /// <summary>
    /// Checks if there is a soldier from this unit in a given position
    /// </summary>
    /// <param name="position"> the position that is being checked </param>
    /// <returns> returns true if there is a soldier in the given position </returns>
    public bool SoldierInPosition(Vector3 position) {
        foreach (Soldier child in childSoldiers) {
            if (Vector3.SqrMagnitude(child.transform.position - position) < 1) {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Sets the position so the unit knows where all of the soldiers in a unit are
    /// </summary>
    /// <param name="unitIndexInChildren"> Use transform.getindex for this </param>
    /// <param name="newPosition"> This is the position that the unit is attempting to get to </param>
    /// <returns> this returns whether or not you can set the position to that position based off of the other soldiers in the area </returns>
    public bool SetNewPositionOfSoldier(int unitIndexInChildren, Vector3 newPosition) {
        int listIndex = ChildIndexToListIndex(unitIndexInChildren);
        for (int i = 0; i < childSoldiers.Count; i++) {
            if (i == listIndex) continue;
            Soldier current = childSoldiers[i];
            if (Vector3.Magnitude(current.transform.position - childSoldiers[listIndex].transform.position) < 1) {
                return false;
            }
        }
        return true;
    }

    public void Push(int siblingIndex, Vector3 direction) {
        throw new NotImplementedException();
    }

    int ChildIndexToListIndex(int siblingIndex) {
        return siblingIndex / 2;

    }
    #endregion

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
    public Vector3 OffsetPerRow {
        get { return offsetPerRow; }
    }// Make a thing in the unit editor like with starting soldier total
    [SerializeField]
    Vector3 offsetPerRow;

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
    GameObject StoredItem {
        get {
            if (staticItem == null) {
                staticItem = serializedItem;
            }
            return staticItem;
        }
    }
    [SerializeField]
    GameObject serializedItem;
    public static GameObject staticItem;

    void AddSoldierAndTargetPosition() {
        GameObject addedSoldier = Instantiate(serializedItem);
        addedSoldier.transform.parent = transform;
        addedSoldier.transform.name = "Soldier " + childSoldiers.Count;
        addedSoldier.AddComponent<CapsuleCollider>();
        addedSoldier.gameObject.layer = LayerMask.NameToLayer("soldierDetection");

        childSoldiers.Add(addedSoldier.AddComponent<Soldier>());
        childSoldiers[^1].unit = this;

        GameObject addedTargetPosition = Instantiate(serializedItem);
        addedTargetPosition.transform.parent = transform;
        addedTargetPosition.transform.name = "Target position for soldier " + targetPositions.Count;

        targetPositions.Add(addedTargetPosition.AddComponent<TargetPosition>());
        targetPositions[^1].thisSoldier = childSoldiers[^1];
    }
    #endregion
}
