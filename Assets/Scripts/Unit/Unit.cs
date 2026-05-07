using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class Unit : MonoBehaviour {
    List<Soldier> childSoldiers = new();
    List<TargetPosition> targetPositions = new();
    List<Vector3> soldierPositions;


    BoundingBox BoundingBox;
    public bool selected = false;
    public bool playersUnit;

    int offsetDistance = 4;
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




    #region Unity functions
    private void Awake() {
    }
    private void Start() {
        InitializePositions();
        if (playersUnit)
            foreach (Soldier soldier in childSoldiers)
                FindFirstObjectByType<Highlightedtargetpositions>().CreateHighlightedPosition();
    }
    private void Update() {
        if (Input.GetKey(KeyCode.Space)) foreach (TargetPosition targetPosition in targetPositions) targetPosition.Enable();
        else foreach (TargetPosition targetPosition in targetPositions) targetPosition.Disable();
    }
    #endregion

    #region Target soldier positions
    public List<Vector3> TargetSoldierPositions { get { return targetSoldierPositions; } private set { targetSoldierPositions = value; } }
    List<Vector3> targetSoldierPositions;
    public BoundingBox TargetPositionBoundingBox { get { return targetPositionBoundingBox; } private set { targetPositionBoundingBox = value; } }
    BoundingBox targetPositionBoundingBox;
    void SetNewTargetSolderPositions(List<Vector3> inputList) {
        TargetSoldierPositions = inputList;
        TargetPositionBoundingBox = new BoundingBox();
        foreach (Vector3 vector in inputList) {
            TargetPositionBoundingBox.Encapsulate(vector);
        }
    }
    #endregion

    #region Unit functions
    public Vector3 UnitFront {
        get {
            return -offsetPerRow * ((float)NumberOfSoldiers / (float)CurrentWidth) / 2;
        }
    }
    public Vector3 UnitRight {
        get {
            return offsetPerTroop * (float)currentWidth / 2;
        }
    }

    Vector3 GetForwardRightDiagonal() {
        return UnitFront + UnitRight;
    }
    Vector3 GetForwardLeftDiagonal() {
        return UnitFront - UnitRight;
    }
    string QuadrantOfPoint(Vector3 point) {
        Debug.Log(Vector3.Dot(GetForwardRightDiagonal(), point - CenterPoint) + ", " + Vector3.Dot(GetForwardLeftDiagonal(), point - CenterPoint));
        if (Vector3.Dot(Rotate90Degrees(GetForwardRightDiagonal()), point - CenterPoint) < 0) {
            if (Vector3.Dot(Rotate90Degrees(GetForwardLeftDiagonal()), point - CenterPoint) < 0) {
                return "left";
            }
            else {
                return "front";
            }
        }
        else {
            if (Vector3.Dot(Rotate90Degrees(GetForwardLeftDiagonal()), point - CenterPoint) < 0) {
                return "back";
            }
            else {
                return "right";
            }
        }
    }
    Vector3 Rotate90Degrees(Vector3 startVector) {
        return new Vector3(startVector.z, startVector.y, startVector.x);
    } 
    Vector3 UnRotate90Degrees(Vector3 startVector) {
        return new Vector3(startVector.z, startVector.y, startVector.x);
    }
    #endregion

    #region General unit questions
    void InitializePositions() {
        soldierPositions = new();
        BoundingBox = new();
        for (int i = 0; i < NumberOfSoldiers; i++) {
            soldierPositions.Add(childSoldiers[i].transform.position);
            BoundingBox.Encapsulate(transform.GetChild(i * 2).transform.position);
        }
        SetNewTargetSolderPositions(soldierPositions);

    }
    void SoldierDeath(int UnitIndex) {
        //remove from all lists and the bounds.
        //remove from the grid system
        //remove from highlighted target positions (in the camera)
    }
    public void UpdateSoldierPosition(Vector3 position, int siblingIndex, Soldier soldier) {
        CustomGrid.instance.UpdateSoldierPosition(soldier);
        int ListIndex = ChildIndexToListIndex(siblingIndex);
        soldierPositions[ListIndex] = position;
        BoundingBox.ChangePoint(ListIndex, position);
    }

    /// <summary>
    /// Checks if there is a soldier from this unit in a given position
    /// </summary>
    /// <param name="position"> the position that is being checked </param>
    /// <returns> returns true if there is a soldier in the given position </returns>
    public bool SoldierInPosition(Vector3 position, int excludedIndex, out Vector3 soldierRelativeDirection) {
        Soldier[] nearbySoldiers = CustomGrid.instance.RetrieveNearbySoldiers(position);
        soldierRelativeDirection = Vector3.zero;
        if (nearbySoldiers == null) return false;
        Vector3 excludedPosition = soldierPositions[ChildIndexToListIndex(excludedIndex)];
        for (int i = 0; i < nearbySoldiers.Length; i++) {
            Vector3 childPosition = nearbySoldiers[i].transform.position;
            if (excludedPosition == childPosition) continue;
            Vector3 directionAndDistanceBetweenSoldiers = childPosition - position;
            float magnitude = directionAndDistanceBetweenSoldiers.sqrMagnitude;
            if (magnitude < offsetDistance) {
                if (nearbySoldiers[i].unit.playersUnit != playersUnit) {
                    Vector3 diagonalA = nearbySoldiers[i].unit.offsetPerTroop * nearbySoldiers[i].unit.CurrentWidth + nearbySoldiers[i].unit.offsetPerRow * (nearbySoldiers[i].unit.NumberOfSoldiers / nearbySoldiers[i].unit.CurrentWidth);
                    Vector3 diagonalB = nearbySoldiers[i].unit.offsetPerTroop * nearbySoldiers[i].unit.CurrentWidth - nearbySoldiers[i].unit.offsetPerRow * (nearbySoldiers[i].unit.NumberOfSoldiers / nearbySoldiers[i].unit.CurrentWidth);
                    Debug.Log(diagonalA + ", " + (position - nearbySoldiers[i].unit.CenterPoint) + ", " + diagonalB);
                    Debug.Log(Vector3.Dot(diagonalA, position - nearbySoldiers[i].unit.CenterPoint));
                    Debug.Log(Vector3.Dot(diagonalB, position - nearbySoldiers[i].unit.CenterPoint));

                    //Vector3 startingPosition = localUnitStartPosition - currentlySelected.offsetPerTroop * currentlySelected.CurrentWidth / 2;
                    //int currentWidth = 0;
                    //int currentRow = 0;
                    //for (int i = 0; i < localCurrentlyManipulatedPositions.Count; i++) {
                    //    localCurrentlyManipulatedPositions[i].transform.position = startingPosition + currentWidth * currentlySelected.offsetPerTroop + currentRow * currentlySelected.offsetPerRow;
                    //    currentWidth++;
                    //    if (currentWidth == currentlySelected.CurrentWidth) {
                    //        currentRow++;
                    //        currentWidth = 0;
                    //    }
                    //}
                }
                soldierRelativeDirection = directionAndDistanceBetweenSoldiers;
                return false;
            }
        }
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
        SetNewTargetSolderPositions(listOfPositions);
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
    [SerializeField, HideInInspector]
    int startingSoldierTotal;
    [SerializeField]
    public Vector3 offsetPerTroop; // Make a thing in the unit editor like with starting soldier total
    //public Vector3 OffsetPerRow {
    //    get { return offsetPerRow; }
    //}// Make a thing in the unit editor like with starting soldier total
    [SerializeField]
    public Vector3 offsetPerRow;

    public void InstantArrangeByWidth(int widthCount) {
        currentWidth = widthCount;
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
    [SerializeField]
    bool drawPlayerArrow;
    private void OnDrawGizmosSelected() {
        CustomGrid.instance.DisplayUnitCheckingSquares(this);
        if (Application.isPlaying)
            BoundingBox.DisplayBox();
        DisplayDiagonal();
    }

    void DisplayDiagonal() {
        //Debug.Log(CenterPoint + ", " + (CenterPoint + GetForwardLeftDiagonal()) + ", " + GetForwardLeftDiagonal() + ", " + UnitFront + ", ");
        GLFunctions.GLshapes.DrawArrow(CenterPoint, CenterPoint + Rotate90Degrees(GetForwardRightDiagonal()), Color.black);
        Vector3 rotatedDiagonal = new Vector3(GetForwardRightDiagonal().z, GetForwardRightDiagonal().y, GetForwardRightDiagonal().x);
        Debug.DrawLine(CenterPoint - Rotate90Degrees(rotatedDiagonal), CenterPoint + Rotate90Degrees(rotatedDiagonal));
        GLFunctions.GLshapes.DrawArrow(CenterPoint, CenterPoint + Rotate90Degrees(GetForwardLeftDiagonal()), Color.black, 1);
        rotatedDiagonal = new Vector3(GetForwardLeftDiagonal().z, GetForwardLeftDiagonal().y, GetForwardLeftDiagonal().x);
        Debug.DrawLine(CenterPoint - Rotate90Degrees(rotatedDiagonal), CenterPoint + Rotate90Degrees(rotatedDiagonal));
        if (drawPlayerArrow) {
            GLFunctions.GLshapes.DrawArrow(CenterPoint, ScreenPointToGroundPoint(Event.current.mousePosition));
            Debug.Log(QuadrantOfPoint(ScreenPointToGroundPoint(Event.current.mousePosition)));
        }
    }
    Vector3 ScreenPointToGroundPoint(Vector3 screenPoint) {
        Ray raycast = Camera.main.ScreenPointToRay(screenPoint);
        LayerMask groundMask = 1 << LayerMask.NameToLayer("Ground");
        if (Physics.Raycast(raycast.origin, raycast.direction * 1000, out RaycastHit hitInfo, 1000, groundMask.value)) {
            return hitInfo.point;
        }
        return Vector3.zero;
    }
#endif
    #endregion
}
