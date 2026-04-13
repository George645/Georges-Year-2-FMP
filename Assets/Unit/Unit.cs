using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.PlayerLoop;
using UnityEditor;
using UnityEngine.Rendering;

public class Unit : MonoBehaviour {
    List<Soldier> childSoldiers = new();
    List<TargetPosition> targetPositions = new();
    List<Vector3> soldierPositions;
    BoundingBox BoundingBox;
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

    public Vector3 CenterPoint {
        get {
            if (Application.isEditor && !Application.isPlaying) {
                Bounds bound = new Bounds(transform.GetChild(0).transform.position, Vector3.zero);
                for (int i = 2; i < transform.childCount; i += 2) {
                    bound.Encapsulate(transform.GetChild(i).position);
                }
                return bound.center;
            }
            if (BoundingBox == null) {
                BoundingBox = new BoundingBox(transform.GetChild(0).transform.position);
                for (int i = 2; i < transform.childCount; i += 2) {
                    BoundingBox.Encapsulate(transform.GetChild(i).position);
                }
                return BoundingBox.Center;
            }
            return BoundingBox.Center;
        }
    }

    private void Start() {
        InitializePositions();
    }
    private void Update() {
        if (Input.GetKey(KeyCode.Space)) foreach (TargetPosition targetPosition in targetPositions) targetPosition.Enable();
        else foreach (TargetPosition targetPosition in targetPositions) targetPosition.Disable();
    }

    int offsetDistance = 4;

    #region General unit questions
    void InitializePositions() {
        soldierPositions = new();
        BoundingBox = new();
        for (int i = 0; i < NumberOfSoldiers; i++) {
            soldierPositions.Add(childSoldiers[i].transform.position);
            BoundingBox.Encapsulate(transform.GetChild(i * 2).transform.position);
        }


    }
    void SoldierDeath(int UnitIndex) {
        //remove from all lists and the bounds.
        //remove from the grid system
    }
    /// <summary>
    /// Checks if there is a soldier from this unit in a given position
    /// </summary>
    /// <param name="position"> the position that is being checked </param>
    /// <returns> returns true if there is a soldier in the given position </returns>
    public bool SoldierInPosition(Vector3 position) {
        foreach (Vector3 childPosition in soldierPositions) {
            if (Vector3.SqrMagnitude(childPosition - position) < offsetDistance) {
                return false;
            }
        }
        return true;
    }
    public void UpdateSoldierPosition(Vector3 position, int siblingIndex, Soldier soldier) {
        CustomGrid.instance.UpdateSoldierPosition(soldier); // <- make this a parameter to pass in
        int ListIndex = ChildIndexToListIndex(siblingIndex);
        soldierPositions[ListIndex] = position;
        BoundingBox.ChangePoint(ListIndex, position);
    }
    public bool SoldierInPosition(Vector3 position, int excludedIndex) {
        Soldier[] nearbySoldiers = CustomGrid.instance.RetrieveNearbySoldiers(position);
        Soldier excludedSoldier = transform.GetChild(excludedIndex).GetComponent<Soldier>();
        foreach (Soldier nearbySoldier in nearbySoldiers) {
            if (nearbySoldier.transform == excludedSoldier) {
                continue;
            }
            if (Vector3.SqrMagnitude(nearbySoldier.transform.position - position) < offsetDistance) {
                return false;
            }
        }
        return true;
    }
    public bool SoldierInPosition(Vector3 position, int excludedIndex, out Vector3 soldierRelativeDirection) {
        Soldier[] nearbySoldiers = CustomGrid.instance.RetrieveNearbySoldiers(position);
        Vector3 excludedPosition = soldierPositions[ChildIndexToListIndex(excludedIndex)];
        for (int i = 0; i < nearbySoldiers.Length; i++) {
            Vector3 childPosition = nearbySoldiers[i].transform.position;
            if (excludedPosition == childPosition) continue;
            Vector3 directionAndDistanceBetweenSoldiers = childPosition - position;
            float magnitude = directionAndDistanceBetweenSoldiers.sqrMagnitude;
            if (magnitude < offsetDistance) {
                soldierRelativeDirection = directionAndDistanceBetweenSoldiers;
                return false;
            }
        }
        soldierRelativeDirection = Vector3.zero;
        return true;
    }
    List<Soldier> GetSoldiersInPosition(Vector3 position, int excludeIndex) {
        List<Soldier> returningList = new();
        Vector3 excludedPosition = soldierPositions[ChildIndexToListIndex(excludeIndex)];
        foreach (Vector3 childPosition in soldierPositions) {
            if (childPosition == excludedPosition) {
                continue;
            }
            Vector3 offset = childPosition - position;
            if (Vector3.SqrMagnitude(offset) < offsetDistance) {
                returningList.Add(childSoldiers[soldierPositions.IndexOf(childPosition)]);
            }
        }
        return returningList;
    }
    /// <summary>
    /// Sets the position so the unit knows where all of the soldiers in a unit are
    /// </summary>
    /// <param name="unitIndexInChildren"> Use transform.getindex for this </param>
    /// <param name="newPosition"> This is the position that the unit is attempting to get to </param>
    /// <returns> this returns whether or not you can set the position to that position based off of the other soldiers in the area </returns>
    public bool SetNewPositionOfSoldier(int unitIndexInChildren, Vector3 newPosition) {
        int listIndex = ChildIndexToListIndex(unitIndexInChildren);
        for (int i = 0; i < NumberOfSoldiers; i++) {
            if (i == listIndex) continue;
            Soldier current = childSoldiers[i];
            if (Vector3.Magnitude(current.transform.position - childSoldiers[listIndex].transform.position) < offsetDistance) {
                return false;
            }
        }
        return true;
    }

    public void Push(int siblingIndex, Vector3 position) {
        //foreach (Soldier inTheWay in GetSoldiersInPosition(position, ChildIndexToListIndex(siblingIndex))) {
        //    inTheWay.Pushed(2 * inTheWay.transform.position - position);
        //}
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
        List<Vector3> oldSoldierPositions = new(soldierPositions);
        List<TargetPosition> oldTargetPositions2 = new(targetPositions);
        int count = 0;
        for (int h = 0; h < targetPositions.Count; h++) {
            if (oldSoldierPositions.Count == 0 || listOfPositions.Count == 0) {
                break;
            }

            float maxDistance = -1;
            int indexOfNewPosition = 0;

            for (int i = 0; i < listOfPositions.Count; i++) {
                for (int j = 0; j < oldSoldierPositions.Count; j++) {
                    float sqrMagnitude = Vector3.SqrMagnitude(listOfPositions[indexOfNewPosition] - oldSoldierPositions[i]);
                    if (sqrMagnitude > maxDistance) {
                        indexOfNewPosition = i;
                        maxDistance = sqrMagnitude;
                    }
                }
                //count++;
                //if (count > 30) {
                //    yield return null;
                //    count = 0;
                //}
            }

            int indexOfOldPosition = -1;
            float minDistance = float.MaxValue;
            for (int i = 0; i < oldSoldierPositions.Count; i++) {
                float sqrMagnitude = Vector3.SqrMagnitude(listOfPositions[indexOfNewPosition] - oldSoldierPositions[i]);
                if (sqrMagnitude < minDistance) {
                    minDistance = sqrMagnitude;
                    indexOfOldPosition = i;
                }
            }

            oldTargetPositions2[indexOfOldPosition].NewPosition(listOfPositions[indexOfNewPosition]);
            oldTargetPositions2.RemoveAt(indexOfOldPosition);
            oldSoldierPositions.RemoveAt(indexOfOldPosition);
            listOfPositions.RemoveAt(indexOfNewPosition);
            count++;
            if (oldSoldierPositions.Count > 0 && (180 / oldSoldierPositions.Count) * (180 / oldSoldierPositions.Count) < count) {
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

    [SerializeField]
    GameObject serializedItem;
    [SerializeField]
    GameObject serializedItem2;
    public static GameObject staticItem;

    void AddSoldierAndTargetPosition() {
        GameObject addedSoldier = Instantiate(serializedItem);
        addedSoldier.transform.parent = transform;
        addedSoldier.transform.name = "Soldier " + childSoldiers.Count;
        childSoldiers.Add(addedSoldier.GetComponent<Soldier>());

        childSoldiers[^1].unit = this;

        GameObject addedTargetPosition = Instantiate(serializedItem2);
        addedTargetPosition.transform.parent = transform;
        addedTargetPosition.transform.name = "Target position for soldier " + targetPositions.Count;
        targetPositions.Add(addedTargetPosition.GetComponent<TargetPosition>());

        targetPositions[^1].thisSoldier = childSoldiers[^1];
    }
    #endregion

    #region Unity editor
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        CustomGrid.instance.DisplayUnitCheckingSquares(this);
        if (Application.isPlaying)
            BoundingBox.DisplayBox();
    }
#endif
    #endregion
}
