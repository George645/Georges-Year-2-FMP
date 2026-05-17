using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Xml;

public class Unit : MonoBehaviour {
    List<Soldier> childSoldiers = new();
    List<TargetPosition> targetPositions = new();
    List<Vector3> soldierPositions;


    BoundingBox BoundingBox;
    public bool selected = false;
    public bool playersUnit;

    int offsetDistance = 4;

    [HideInInspector]
    public int potentialNextWidth;
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
    private void Start() {
        InitializePositions();
        AssigndirectionMagnitudes();
        currentlyFighting = new();
        if (playersUnit)
            foreach (Soldier soldier in childSoldiers)
                FindFirstObjectByType<Highlightedtargetpositions>().CreateHighlightedPosition();
    }
    private void Update() {
        if (transform.childCount == 0) {
            Destroy(this);
        }
        if (Input.GetKey(KeyCode.Space)) foreach (TargetPosition targetPosition in targetPositions) targetPosition.Enable();
        else foreach (TargetPosition targetPosition in targetPositions) targetPosition.Disable();

        if (!inCombat && currentlyFighting.Count() != 0) {
            if (IsFacingOpponent() && !isAssigningPositions) {
                Vector3[] newArrayOfPositions = new Vector3[childSoldiers.Count()];
                NewUnitFormation(battlePoint - 0.1f * offsetPerRow, offsetPerTroop, offsetPerRow, currentWidth);
            }
        }
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
    float forwardsMagnitude;
    float rightMagnitude;

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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    /// <returns>returns front, right, back or left</returns>
    string QuadrantOfPoint(Vector3 point) {
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

    void AssigndirectionMagnitudes() {
        forwardsMagnitude = offsetPerRow.magnitude;
        rightMagnitude = offsetPerTroop.magnitude;
    }
    #endregion

    #region Soldier questions
    void InitializePositions() {
        soldierPositions = new();
        BoundingBox = new();
        for (int i = 0; i < NumberOfSoldiers; i++) {
            soldierPositions.Add(childSoldiers[i].transform.position);
            BoundingBox.Encapsulate(transform.GetChild(i * 2).transform.position);
        }
        SetNewTargetSolderPositions(soldierPositions);

    }

    public int NumberOfKills {
        get { return NumberOfKills; }
        set {
            numberOfKills = value;
            StatTracker.instance.AddKill(this);
        }
    }
    int numberOfKills;
    public void SoldierDeath(int siblingIndex) {
        int indexInList = ChildIndexToListIndex(siblingIndex);
        Soldier removingSoldier = null;
        removingSoldier = childSoldiers[indexInList];
        //remove from the grid system
        CustomGrid.instance.RemoveSoldier(removingSoldier);

        //remove from highlighted target positions (in the camera)
        if (selected)
            CameraScript.instance.RemoveSoldier(this);

        //Removal from lists and others in script

        BoundingBox.RemovePoint(indexInList);
        TargetPositionBoundingBox.RemovePoint(indexInList);
        childSoldiers.RemoveAt(indexInList);
        targetPositions.RemoveAt(indexInList);
        soldierPositions.RemoveAt(indexInList);

        if (!IsFacingOpponent()) {
            TurnToFaceOpponent();
        }
        if (!isAssigningPositions)
            NewUnitFormation(battlePoint, offsetPerTroop, offsetPerRow, CurrentWidth);
        //if (targetSoldierPositions != null && targetSoldierPositions.Count != 0)
        //    targetSoldierPositions.RemoveAt(indexInList);
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
        Vector3 childPosition;
        for (int i = 0; i < nearbySoldiers.Length; i++) {
            childPosition = nearbySoldiers[i].transform.position;
            if (excludedPosition == childPosition) continue;
            Vector3 directionAndDistanceBetweenSoldiers = childPosition - position;
            float magnitude = directionAndDistanceBetweenSoldiers.sqrMagnitude;
            if (magnitude < offsetDistance) {
                if (nearbySoldiers[i].unit.playersUnit != playersUnit && doNotEngageTimer > 0) {
                    Soldier checkingChild = transform.GetChild(excludedIndex).GetComponent<Soldier>();
                    if (!inCombat)
                        CollidedWithOpponent(nearbySoldiers[i]);
                    if (!checkingChild.isFighting)
                        new SoldierLevelCombat(checkingChild, nearbySoldiers[i]);
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
            current.transform.rotation = Quaternion.LookRotation(-offsetPerRow, Vector3.up);
        }
        return true;
    }


    int ChildIndexToListIndex(int siblingIndex) {
        try {
            return childSoldiers.IndexOf(transform.GetChild(siblingIndex).GetComponent<Soldier>());
        }
        catch (System.Exception e) {
            Debug.Log("sibling index: " + siblingIndex);
            Debug.Log(transform.GetChild(siblingIndex).gameObject.name);
            Debug.Log(transform.GetChild(siblingIndex).GetComponent<Soldier>());
            throw e;
        }
    }
    #endregion

    #region combat
    #region combatStatistics
    [SerializeField, Tooltip("value between 0 and 50")]
    public int attack = 25;
    [SerializeField, Tooltip("value between 0 and 10")]
    public int defense = 5;
    [SerializeField, Tooltip("value between 0 and 10")]
    public int armour = 5;
    #endregion

    bool IsFacingOpponent() {
        return QuadrantOfPoint(currentlyFighting[0].targetPositionBoundingBox.Center) == "front";
    }
    public void MovedByPlayer() {
        BreakCombat();
        doNotEngageTimerStart = Time.time + timeWithoutCombatCollision;
    }
    Vector3 battlePoint;
    [SerializeField]
    float timeWithoutCombatCollision = 5;
    float doNotEngageTimerStart;
    float doNotEngageTimer {
        get { return Time.time - doNotEngageTimerStart; }
    }
    void CollidedWithOpponent(Soldier soldier) {
        Unit collidedUnit = soldier.unit;
        string quadrant = collidedUnit.QuadrantOfPoint(CenterPoint);
        Vector3 localUnitStartPosition = Vector3.zero;
        if (quadrant == "left") {
            offsetPerTroop = collidedUnit.offsetPerRow.normalized * rightMagnitude;
            offsetPerRow = collidedUnit.offsetPerTroop.normalized * forwardsMagnitude;
            localUnitStartPosition = collidedUnit.CenterPoint + (collidedUnit.offsetPerTroop * collidedUnit.CurrentWidth) / 2 - offsetPerTroop * CurrentWidth / 2;
        }
        else if (quadrant == "right") {
            offsetPerTroop = -collidedUnit.offsetPerRow.normalized * rightMagnitude;
            offsetPerRow = -collidedUnit.offsetPerTroop.normalized * forwardsMagnitude;
            localUnitStartPosition = collidedUnit.CenterPoint - (collidedUnit.offsetPerTroop * collidedUnit.CurrentWidth) / 2 - offsetPerTroop * CurrentWidth / 2;
        }
        else if (quadrant == "front") {
            offsetPerTroop = -collidedUnit.offsetPerTroop.normalized * rightMagnitude;
            offsetPerRow = -collidedUnit.offsetPerRow.normalized * forwardsMagnitude;
            localUnitStartPosition = collidedUnit.CenterPoint - (collidedUnit.offsetPerRow * (collidedUnit.NumberOfSoldiers / collidedUnit.CurrentWidth)) / 2 - offsetPerTroop * (CurrentWidth - 3) / 2;
        }
        else if (quadrant == "back") {
            offsetPerTroop = collidedUnit.offsetPerTroop.normalized * rightMagnitude;
            offsetPerRow = collidedUnit.offsetPerRow.normalized * forwardsMagnitude;
            localUnitStartPosition = collidedUnit.CenterPoint + (collidedUnit.offsetPerRow * (collidedUnit.NumberOfSoldiers / collidedUnit.CurrentWidth)) / 2 - offsetPerTroop * (CurrentWidth - 3) / 2;
        }
        else {
            throw new System.Exception("quadrant not found" + quadrant); //<- if this ever gets run, I will eat a hat, like I do not believe it.
        }

        NewUnitFormation(localUnitStartPosition, offsetPerTroop, offsetPerRow, currentWidth);

        battlePoint = localUnitStartPosition;

        EngageInCombat(collidedUnit);
    }

    bool inCombat {
        get { return !childSoldiers.TrueForAll(x => !x.isFighting); }
    }
    List<Unit> currentlyFighting;
    public void BreakCombat(Unit engagingUnit) {
        currentlyFighting.Remove(engagingUnit);
    }
    public void BreakCombat() {
        while (currentlyFighting.Count() != 0) {
            currentlyFighting[0].BreakCombat(this);
            currentlyFighting.Remove(currentlyFighting[0]);
        }
    }

    public void EngageInCombat(Unit engagingUnit) {
        if (!currentlyFighting.Contains(engagingUnit)) {
            currentlyFighting.Add(engagingUnit);
            engagingUnit.EngageInCombat(this);
        }
    }

    #endregion

    #region Move unit
    void TurnToFaceOpponent() {
        offsetPerRow = (targetPositionBoundingBox.Center - currentlyFighting[0].targetPositionBoundingBox.Center).normalized * offsetPerRow.magnitude;
        offsetPerTroop = new Vector3(offsetPerRow.z, offsetPerRow.y, -offsetPerRow.x).normalized * offsetPerTroop.magnitude;
        Vector3 plannedOffsetPerRow = offsetPerRow;
        Vector3 plannedOffsetPerTroop = offsetPerTroop;
        Vector3 startingPoint = currentlyFighting[0].targetPositionBoundingBox.Center - currentlyFighting[0].offsetPerRow * currentlyFighting[0].NumberOfSoldiers / currentlyFighting[0].currentWidth / 2 - currentlyFighting[0].offsetPerTroop * currentWidth / 2;

        NewUnitFormation(startingPoint, plannedOffsetPerTroop, plannedOffsetPerRow, currentWidth);
        battlePoint = startingPoint;
    }

    void NewUnitFormation(Vector3 startPosition, Vector3 offsetPerTroop, Vector3 offsetPerRow, int newWidth) {
        Debug.Log("New formation");
        //if (currentWidth < new)
        Vector3[] nextPositions = new Vector3[NumberOfSoldiers];
        int width = 0;
        int depth = 0;
        for (int i = 0; i < nextPositions.Count(); i++) {
            nextPositions[i] = startPosition + width * offsetPerTroop + depth * offsetPerRow;
            width++;
            if (width == newWidth) {
                depth++;
                width = 0;
            }
        }
        NewPositions(nextPositions.ToList());



        potentialOffsetPerRow = offsetPerRow;
        potentialOffsetPerTroop = offsetPerTroop;
        potentialNextWidth = newWidth;
        ApplyPotentials();
    }
    public void ApplyPotentials() { // change this to be when all the soldiers are reported as in position.
        currentWidth = potentialNextWidth;
        offsetPerRow = potentialOffsetPerRow;
        offsetPerTroop = potentialOffsetPerTroop;
    }
    public void NewPositions(List<Vector3> listOfPositions) {
        if (!isAssigningPositions)
            StartCoroutine(nameof(UpdatePosition), listOfPositions);
        else {
            WaitForUpdatePosition(listOfPositions);
        }
    }
    IEnumerator WaitForUpdatePosition(List<Vector3> list) {
        while (isAssigningPositions) {
            yield return null;
        }
        UpdatePosition(list);
    }
    bool isAssigningPositions;
    IEnumerator UpdatePosition(List<Vector3> listOfPositions) {
        isAssigningPositions = true;
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
                    float sqrMagnitude = Vector3.SqrMagnitude(listOfPositions[i] - oldSoldierPositions[j]);
                    if (sqrMagnitude > maxDistance) {
                        indexOfNewPosition = i;
                        maxDistance = sqrMagnitude;
                    }
                }
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
            count++;
            if (oldSoldierPositions.Count > 0 && (180 / oldSoldierPositions.Count) * (180 / oldSoldierPositions.Count) < count) {
                yield return null;
                count = 0;
            }
            if (oldTargetPositions2[indexOfOldPosition] == null) {
                continue;
            }
            oldTargetPositions2[indexOfOldPosition].NewPosition(listOfPositions[indexOfNewPosition]);
            oldTargetPositions2.RemoveAt(indexOfOldPosition);
            oldSoldierPositions.RemoveAt(indexOfOldPosition);
            listOfPositions.RemoveAt(indexOfNewPosition);
        }
        yield return null;
        isAssigningPositions = false;
        StopCoroutine(nameof(UpdatePosition));
    }
    #endregion

    #region Set up unit
    [SerializeField, HideInInspector]
    int startingSoldierTotal;

    [HideInInspector]
    public Vector3 potentialOffsetPerTroop;
    [HideInInspector]
    public Vector3 potentialOffsetPerRow;

    [SerializeField]
    public Vector3 offsetPerTroop;
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
            Debug.Log(targetPositions);
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

        if (currentlyFighting != null && currentlyFighting.Count() != 0) {
            GLFunctions.GLshapes.DrawArrow(CenterPoint, currentlyFighting[0].targetPositionBoundingBox.Center, Color.purple);
        }
    }

    void DisplayDiagonal() {
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
