using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour {

    // ⌜ target position x,             target position y,             target position z,             unit number             ⌝
    // | current position x,            current position y,            current position z,            soldier number          |
    // | current velocity x,            current velocity y,            current velocity z,            fighting unit number    |
    // ⌞ currently fighting position x, currently fighting position y, currently fighting position z, fighting soldier number ⌟

    public int unitNumber;

    /// <summary>
    /// <para>[0, 0] == [0] = target position x       </para>
    /// <para>[1, 0] == [1] = target position z       </para>
    /// <para>[2, 0] == [2] = custom grid index       </para>
    /// <para>[3. 0] == [3] = unit number             </para>
    /// <para>[0, 1] == [4] = current position x      </para>
    /// <para>[1, 1] == [5] = current position y      </para>
    /// <para>[2, 1] == [6] = current position z      </para>
    /// <para>[3. 1] == [7] = soldier number          </para>
    /// <para>[0, 2] == [8] =  current velocity x     </para>
    /// <para>[1, 2] == [9] =  current velocity y     </para>
    /// <para>[2, 2] == [10] = current velocity z     </para>
    /// <para>[3. 2] == [11] = opponent unit number   </para>
    /// <para>[0, 3] == [12] = opponent position x    </para>
    /// <para>[1, 3] == [13] = opponent position y    </para>
    /// <para>[2, 3] == [14] = opponent position z    </para>
    /// <para>[3. 3] == [15] = opponent soldier number</para>
    /// </summary>
    //public Matrix4x4[] soldierInformation {
    //    get { return soldierInformation; }
    //    private set { soldierInformation = value; }
    //}

    Matrix4x4[] soldierInformation;
    public int AIIndex = -1;

    Vector3 position;

    BoundingBox BoundingBox;
    public bool selected = false;
    public bool playersUnit;

    const int offsetDistance = 4;

    [HideInInspector]
    public int potentialNextWidth;
    public int CurrentWidth {
        get { return currentWidth; }
    }
    [SerializeField, HideInInspector]
    int currentWidth;
    public int NumberOfSoldiers {
        get {
            if ((soldierInformation == null || soldierInformation.Length == 0) && (transform.childCount > 0 || Application.isEditor)) {
                if (CustomSettings.instance == null)
                    CustomSettings.AssignInstance();
                soldierInformation = new Matrix4x4[CustomSettings.instance.unitSize];
                for (int i = 0; i < transform.childCount; i++) {

                    SetTargetPosition(i, transform.GetChild(i).position);

                    soldierInformation[i][3, 0] = unitNumber;

                    SetPosition(i, transform.GetChild(i).position);

                    soldierInformation[i][3, 1] = i;

                    SetFacingDirection(i, -offsetPerRow);

                    soldierInformation[i][3, 2] = -1;

                    SetOpponentToNone(i);

                    soldierInformation[i][3, 3] = -1;
                    Debug.Log(i);
                }
                for (int i = transform.childCount; i < soldierInformation.Length; i++) {
                    RemoveSoldier(i);
                }
            }
            return transform.childCount;
        }
    }

    public Vector3 CenterPoint {
        get {
            if (Application.isEditor && !Application.isPlaying) {
                if (transform.childCount == 0) {
                    return transform.position;
                }
                Bounds bound = new(transform.GetChild(0).transform.position, Vector3.zero);
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
        AssignDirectionMagnitudes();
        AssignPositionInListIndexes();
        currentlyFighting = new();
        if (playersUnit)
            for (int i = 0; i < NumberOfSoldiers; i++) {
                FindFirstObjectByType<Highlightedtargetpositions>().CreateHighlightedPosition();
            }
    }
    private void FixedUpdate() {
        MoveSoldiers();

        if (Input.GetKey(KeyCode.Space))
            RenderAllTargetPositions();

        if (transform.childCount == 0 || previousStartingPoint.magnitude > 5000) {
            foreach (Unit unit in currentlyFighting) {
                unit.Defeated(this);
            }
            Destroy(gameObject);
        }

        if (!InCombat && currentlyFighting.Count() != 0) {
            if (IsFacingOpponent() && !isAssigningPositions && System.Array.TrueForAll(soldierInformation, x => x[0, 1] == x[0, 0] && x[2, 0] == x[2, 1])) {
                Debug.Log(Vector3.Magnitude(currentlyFighting[0].CenterPoint - CenterPoint) / 5 * offsetPerRow);
                Debug.Log(previousStartingPoint - Vector3.Magnitude(currentlyFighting[0].CenterPoint - CenterPoint) / 5 * offsetPerRow);
                if (IsFacingOpponent())
                    NewUnitFormation(previousStartingPoint - Vector3.Magnitude(currentlyFighting[0].previousStartingPoint + currentlyFighting[0].offsetPerTroop * currentlyFighting[0].currentWidth / 2 - (previousStartingPoint + offsetPerTroop * currentWidth)) / 5 * offsetPerRow, offsetPerTroop, offsetPerRow, currentWidth);
                else
                    NewUnitFormation(previousStartingPoint - 0.1f * offsetPerRow, offsetPerTroop, offsetPerRow, currentWidth);
            }
        }
    }
    #endregion

    #region interact with soldier infromation variable

    public Matrix4x4 GetSoldier(int soldierIndex) {
        return soldierInformation[soldierIndex];
    }

    void InstantSetPosition(int soldier, Vector3 position) {
        SetPosition(soldier, position);
        SetTargetPosition(soldier, position);
    }

    // target position
    void SetTargetPosition(int soldierNumber, Vector3 position) {
        Debug.Log("Setting target positions: " + position);
        soldierInformation[soldierNumber][0] = position.x;
        soldierInformation[soldierNumber][1] = position.z;
    }
    public Vector3 GetTargetPosition(int soldierNumber) {
        return new Vector3(soldierInformation[soldierNumber][0], 0, soldierInformation[soldierNumber][1]);
    }

    //grid index
    public void SetGridIndex(int soldierNumber, int value) {
        soldierInformation[soldierNumber][2] = value;
    }

    public int GetGridIndex(int soldierNumber) {
        return (int)soldierInformation[soldierNumber][2];
    }

    //current position
    void SetPosition(int soldierNumber, Vector3 position) {
        if (this.position == null || this.position == Vector3.zero)
            this.position = transform.position;
        Debug.Log("Setting positions: " + position);
        try {
            transform.GetChild(soldierNumber).transform.position = position + this.position + Vector3.up * 2;
        }
        catch (Exception e) {
            Debug.Log(transform.childCount);
            throw e;
        }

        soldierInformation[soldierNumber][4] = position.x;
        soldierInformation[soldierNumber][5] = position.y;
        soldierInformation[soldierNumber][6] = position.z;
    }
    public Vector3 GetPosition(int soldierNumber) {
        return new Vector3(soldierInformation[soldierNumber][4], soldierInformation[soldierNumber][5], soldierInformation[soldierNumber][6]);
    }

    //velocity
    void SetVelocity(int soldierNumber, Vector3 velocity) {
        Debug.Log("Setting velocity: " + velocity);
        soldierInformation[soldierNumber][8] = velocity.x;
        soldierInformation[soldierNumber][9] = velocity.y;
        soldierInformation[soldierNumber][10] = velocity.z;
    }
    public Vector3 GetVelocity(int soldierNumber) {
        return new Vector3(soldierInformation[soldierNumber][8], soldierInformation[soldierNumber][9], soldierInformation[soldierNumber][10]);
    }
    void SetFacingDirection(int soldierNumber, Vector3 normalisedDirection) {
        SetVelocity(soldierNumber, new Vector3(10000 * normalisedDirection.x, 0, 10000 * normalisedDirection.z));
        transform.GetChild(soldierNumber).rotation = Quaternion.Euler(normalisedDirection);
    }

    //opponent position
    void SetOpponentPosition(int soldierNumber, Vector3 position) {
        soldierInformation[soldierNumber][12] = position.x;
        soldierInformation[soldierNumber][13] = position.y;
        soldierInformation[soldierNumber][14] = position.z;
    }
    public Vector3 GetOpponentPosition(int soldierNumber) {
        return new Vector3(soldierInformation[soldierNumber][12], soldierInformation[soldierNumber][13], soldierInformation[soldierNumber][14]);
    }

    void SetOpponentToNone(int soldierNumber) {
        SetOpponentPosition(soldierNumber, Vector3.positiveInfinity);
    }

    void SetOpponent(int thisSoldierID, Vector3 opponentPosition, int opponentUnitID, int opponentSoldierID) {
        SetOpponentPosition(thisSoldierID, opponentPosition);
        soldierInformation[thisSoldierID][11] = opponentUnitID;
        soldierInformation[thisSoldierID][15] = opponentSoldierID;
    }

    void RemoveSoldier(int soldierIndex) {
        for (int i = 0; i < 16; i++) {
            try {
                soldierInformation[soldierIndex][i] = float.NaN;
            }
            catch (Exception e) {
                Debug.Log(soldierIndex + ", " + i);
                throw e;
            }
        }
    }
    #endregion

    #region Soldier movement
    [SerializeField]
    int speed = 15;
    [SerializeField]
    float speedOfRotation = 4;
    [SerializeField]
    Vector3 airResistance;

    void MoveSoldiers() {
        Vector3 currentSoldierDirectionOfMovement;
        foreach (Matrix4x4 child in soldierInformation) {
            if (child[8] + child[10] > 5000) return;

            Vector3 currentSoldierPosition = new Vector3(child[0], 21, child[1]);
            Vector3 currentSoldierTargetPosition = new Vector3(child[4], child[5], child[6]);
            Vector3 currentSoldierVelocity = new Vector3(child[8], child[9], child[10]);

            int childID = (int)child[7];
            Transform childTransform = transform.GetChild(childID);

            //sets the position to the destination if close enough
            if (Vector3.SqrMagnitude(currentSoldierPosition - currentSoldierTargetPosition) < .01f) {
                childTransform.transform.position = currentSoldierTargetPosition;
                currentSoldierPosition = currentSoldierTargetPosition;
                UpdateSoldierPosition(currentSoldierPosition, childID);
                if (!IsFacing(childID, -offsetPerRow.normalized)) {
                    RotateTowards(childID, currentSoldierVelocity.normalized, -offsetPerRow.normalized);
                    return;
                }
            }

            //moves towards the destination if possible, if not, tries to move around the unit in front
            if (currentSoldierTargetPosition != currentSoldierPosition) {
                currentSoldierDirectionOfMovement = (currentSoldierTargetPosition - currentSoldierPosition).normalized;
                if (currentSoldierDirectionOfMovement.y != 0) Debug.Log(currentSoldierTargetPosition - currentSoldierPosition);
                if (!IsFacing(childID, currentSoldierDirectionOfMovement)) {
                    RotateTowards(childID, currentSoldierVelocity, currentSoldierDirectionOfMovement);
                    return;
                }
                if (!SoldierInPosition(currentSoldierPosition + currentSoldierVelocity / 100 * speed, childID, out Vector3 soldierInPosition) /*&& !ignoreColliders*/) {
                    //float dotProduct = Vector3.Dot(rightDirection, soldierInPosition);
                    //if (dotProduct < 0) {
                    //    if (rightStuck) {
                    //        StartCoroutine(nameof(TemporarilyIgnoreColliders));
                    //    }
                    //    leftStuck = true;
                    //    currentSoldierDirectionOfMovement = rightDirection;
                    //}
                    //else if (dotProduct > 0) {
                    //    if (leftStuck) {
                    //        StartCoroutine(nameof(TemporarilyIgnoreColliders));
                    //    }
                    //    rightStuck = true;
                    //    currentSoldierDirectionOfMovement = -rightDirection;
                    //}
                    //else {
                    //    currentSoldierDirectionOfMovement = Vector3.zero;
                    //}
                }
                else {
                    //leftStuck = false;
                    //rightStuck = false;
                }
                childTransform.position += currentSoldierDirectionOfMovement / 100 * speed;
                currentSoldierPosition += currentSoldierDirectionOfMovement / 100 * speed;
                UpdateSoldierPosition(currentSoldierPosition, childID);
            }

            if (currentSoldierVelocity.y != 0) Debug.LogWarning("movement direction y should be 0");
        }
    }


    bool IsFacing(int soldierID, Vector3 direction) {
        //transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);

        if (transform.GetChild(soldierID).forward.normalized == direction.normalized) {
            return true;
        }
        return false;
    }


    void RotateTowards(int soldierID, Vector3 fromDirection, Vector3 toDirection) {
        Quaternion newRotation = Quaternion.LookRotation(Vector3.RotateTowards(fromDirection, toDirection, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
        transform.GetChild(soldierID).rotation = newRotation;

        SetFacingDirection(soldierID, newRotation * Vector3.forward);
    }

    #endregion

    #region Target positions
    Matrix4x4[] targetPositionsMatrixed;
    [SerializeField]
    Mesh capsuleMesh;
    [SerializeField]
    Material material;
    void RenderAllTargetPositions() {
        RenderParams renderParams = new(material);
        Graphics.RenderMeshInstanced(renderParams, capsuleMesh, 0, targetPositionsMatrixed); // <- figure ts out
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
    public float forwardsMagnitude;
    public float rightMagnitude;

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

    void AssignDirectionMagnitudes() {
        forwardsMagnitude = offsetPerRow.magnitude;
        rightMagnitude = offsetPerTroop.magnitude;
    }
    #endregion

    #region Soldier questions
    void InitializePositions() {
        BoundingBox = new();
        if (soldierInformation == null || transform.childCount == 0) {
            for (int i = 0; i < NumberOfSoldiers; i++) {
                //set current position
                SetPosition(i, transform.GetChild(i).position);

                //set target position
                soldierInformation[i][0, 0] = transform.GetChild(i).position.x;
                soldierInformation[i][1, 0] = transform.GetChild(i).position.y;
                soldierInformation[i][2, 0] = transform.GetChild(i).position.z;
                BoundingBox.Encapsulate(transform.GetChild(i * 2).transform.position);
            }

        }
    }

    void AssignPositionInListIndexes() {
        for (int i = 0; i < NumberOfSoldiers; i++) {
            soldierInformation[i][7] = i;
        }
    }

    public int NumberOfKills {
        get { return numberOfKills; }
        set {
            numberOfKills = value;
            StatTracker.instance.AddKill(this);
        }
    }
    int numberOfKills;
    public void SoldierDeath(int indexInList) {

        int removingSoldierIndex = (int)soldierInformation[indexInList][7];
        //remove from the grid system
        CustomGrid.instance.RemoveSoldier((unitNumber, removingSoldierIndex));

        //remove from highlighted target positions (in the camera)
        if (selected)
            CameraScript.instance.RemoveSoldier(this);

        //Removal from lists and others in script

        BoundingBox.RemovePoint(indexInList);
        TargetPositionBoundingBox.RemovePoint(indexInList);
        for (int i = indexInList; i < NumberOfSoldiers; i++) {
            soldierInformation[i][7]--;
        }

        if (!IsFacingOpponent()) {
            TurnToFaceOpponent();
        }
        if (!isAssigningPositions) {
            Debug.Log("Soldier death");
            NewUnitFormation(previousStartingPoint, offsetPerTroop, offsetPerRow, CurrentWidth);
        }//if (targetSoldierPositions != null && targetSoldierPositions.Count != 0)
        //    targetSoldierPositions.RemoveAt(indexInList);
    }

    public void UpdateSoldierPosition(Vector3 position, int soldierID) {
        CustomGrid.instance.UpdateSoldierPosition((unitNumber, soldierID));
        SetPosition(soldierID, position);
        BoundingBox.ChangePoint(soldierID, position);
        OverallAI.instance.UpdateUnitPosition(BoundingBox.Center, AIIndex, playersUnit);
    }



    /// <summary>
    /// Checks if there is a soldier from this unit in a given position
    /// </summary>
    /// <param name="position"> the position that is being checked </param>
    /// <returns> returns true if there is a soldier in the given position </returns>
    public bool SoldierInPosition(Vector3 position, int soldierIndex, out Vector3 soldierRelativeDirection) {
        // unit id then soldier id
        (int, int)[] nearbySoldiers = CustomGrid.instance.RetrieveNearbySoldiers(position);
        soldierRelativeDirection = Vector3.zero;
        if (nearbySoldiers == null) return false;
        Vector3 excludedPosition = GetPosition(soldierIndex);
        Vector3 enemyPosition;
        for (int i = 0; i < nearbySoldiers.Length; i++) {
            enemyPosition = AssignUnitNumber.instance.GetPositionOfSoldier(nearbySoldiers[i].Item1, nearbySoldiers[i].Item2);
            if (excludedPosition == enemyPosition) continue;
            Vector3 directionAndDistanceBetweenSoldiers = enemyPosition - position;
            float magnitude = directionAndDistanceBetweenSoldiers.sqrMagnitude;
            if (magnitude < offsetDistance) {
                if (AssignUnitNumber.instance.GetUnit(nearbySoldiers[i].Item1).playersUnit != playersUnit && DoNotEngageTimer > 0) {
                    if (!InCombat)
                        CollidedWithOpponent(nearbySoldiers[i]);
                    else if (!currentlyFighting.Contains(AssignUnitNumber.instance.GetUnit(nearbySoldiers[i].Item1)))
                        EngageInCombat(AssignUnitNumber.instance.GetUnit(nearbySoldiers[i].Item1));

                    if (!(soldierInformation[soldierIndex][15] == -1)) {
                        SetOpponent(soldierIndex, enemyPosition, nearbySoldiers[i].Item1, nearbySoldiers[i].Item2);
                        Unit opposingUnit = AssignUnitNumber.instance.GetUnit(nearbySoldiers[i].Item1);
                        if (opposingUnit.GetOpponentPosition(nearbySoldiers[i].Item2).x == float.PositiveInfinity) {
                            AssignUnitNumber.instance.GetUnit(nearbySoldiers[i].Item1).SetOpponent(nearbySoldiers[i].Item2, position, unitNumber, soldierIndex);
                        }
                        else {
                            SetOpponentToNone(soldierIndex);
                        }
                    }
                }
                soldierRelativeDirection = directionAndDistanceBetweenSoldiers;
                return false;
            }
        }
        return true;
    }

    //List<Soldier> GetSoldiersInPosition(Vector3 position, int excludeIndex) {
    //    List<Soldier> returningList = new();
    //    Vector3 excludedPosition = soldierPositions[ChildIndexToListIndex(excludeIndex)];
    //    foreach (Vector3 childPosition in soldierPositions) {
    //        if (childPosition == excludedPosition) {
    //            continue;
    //        }
    //        Vector3 offset = childPosition - position;
    //        if (Vector3.SqrMagnitude(offset) < offsetDistance) {
    //            returningList.Add(childSoldiers[soldierPositions.IndexOf(childPosition)]);
    //        }
    //    }
    //    return returningList;
    //}
    /// <summary>
    /// Sets the position so the unit knows where all of the soldiers in a unit are
    /// </summary>
    /// <param name="unitIndexInChildren"> Use transform.getindex for this </param>
    /// <param name="newPosition"> This is the position that the unit is attempting to get to </param>
    /// <returns> this returns whether or not you can set the position to that position based off of the other soldiers in the area </returns>
    //public bool SetNewPositionOfSoldier(int soldierID, Vector3 newPosition) {
    //    for (int i = 0; i < NumberOfSoldiers; i++) {
    //        if (i == unitIndexInArrays) continue;
    //        Soldier current = childSoldiers[i];
    //        if (Vector3.Magnitude(current.transform.position - childSoldiers[unitIndexInArrays].transform.position) < offsetDistance) {
    //            return false;
    //        }
    //        current.transform.rotation = Quaternion.LookRotation(-offsetPerRow, Vector3.up);
    //    }
    //    return true;
    //}


    //int ChildIndexToListIndex(int indexInList) {
    //    try {
    //        return childSoldiers.IndexOf(transform.GetChild(indexInList).GetComponent<Soldier>());
    //    }
    //    catch (System.Exception e) {
    //        Debug.Log("sibling index: " + indexInList);
    //        Debug.Log(transform.GetChild(indexInList).gameObject.name);
    //        Debug.Log(transform.GetChild(indexInList).GetComponent<Soldier>());
    //        throw e;
    //    }
    //}
    #endregion

    #region combat
    #region combatStatistics
    [Tooltip("value between 0 and 50")]
    public int attack = 25;
    [Tooltip("value between 0 and 10")]
    public int defense = 5;
    [Tooltip("value between 0 and 10")]
    public int armour = 5;
    #endregion

    public void Defeated(Unit unit) {
        if (!currentlyFighting.Contains(unit)) return;
        currentlyFighting.Remove(unit);
        //CustomGrid.instance.RemoveUnit(unit);
        //if (currentlyFighting.Count == 0) BreakCombat(unit);
    }

    bool IsFacingOpponent() {
        return QuadrantOfPoint(currentlyFighting[0].targetPositionBoundingBox.Center) == "front";
    }
    public void MovedByPlayer() {
        BreakCombat();
        doNotEngageTimerStart = Time.time + timeWithoutCombatCollision;
    }
    public Vector3 previousStartingPoint;
    [SerializeField]
    float timeWithoutCombatCollision = 5;
    float doNotEngageTimerStart;
    float DoNotEngageTimer {
        get { return Time.time - doNotEngageTimerStart; }
    }
    void CollidedWithOpponent((int, int) soldier) {
        Unit collidedUnit = AssignUnitNumber.instance.GetUnit(soldier.Item1);
        string quadrant = collidedUnit.QuadrantOfPoint(CenterPoint);
        Vector3 localUnitStartPosition;
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

        Debug.Log("Collided With Opponent");
        NewUnitFormation(localUnitStartPosition, offsetPerTroop, offsetPerRow, currentWidth);

        EngageInCombat(collidedUnit);
    }

    public bool InCombat {
        get { return !Array.TrueForAll(soldierInformation, x => x[11] == -1); }
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
        Vector3 startingPoint = currentlyFighting[0].previousStartingPoint + currentlyFighting[0].offsetPerTroop * currentWidth;

        Debug.Log("Turn to face opponent");
        if (Vector3.SqrMagnitude(CenterPoint - currentlyFighting[0].CenterPoint) < 36)
            NewUnitFormation(startingPoint + offsetPerRow, offsetPerTroop, offsetPerRow, currentWidth);
        else
            NewUnitFormation(startingPoint, plannedOffsetPerTroop, plannedOffsetPerRow, currentWidth);
    }

    void NewUnitFormation(Vector3 startPosition, Vector3 offsetPerTroop, Vector3 offsetPerRow, int newWidth) {
        Debug.Log(startPosition + ", " + offsetPerTroop + ", " + offsetPerRow + ", " + newWidth);
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

        previousStartingPoint = startPosition;

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
            StartCoroutine(WaitForUpdatePosition(listOfPositions));
        }
    }
    IEnumerator WaitForUpdatePosition(List<Vector3> list) {
        while (isAssigningPositions) {
            yield return null;
        }
        StartCoroutine(UpdatePosition(list));
    }
    bool isAssigningPositions;
    IEnumerator UpdatePosition(List<Vector3> listOfPositions) {
        isAssigningPositions = true;
        SetNewTargetSolderPositions(listOfPositions);
        List<Vector3> oldSoldierPositions = new();
        List<Vector3> oldTargetPositions = new();
        List<int> soldierIndex = new(); ;

        foreach (Matrix4x4 soldier in soldierInformation) {
            oldSoldierPositions.Add(new Vector3(soldier[4], soldier[5], soldier[6]));
            oldTargetPositions.Add(new Vector3(soldier[0], 0, soldier[1]));

        }

        int count = 0;
        for (int h = 0; h < NumberOfSoldiers; h++) {
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
            if (oldTargetPositions[indexOfOldPosition] != null) {
                SetTargetPosition(soldierIndex[indexOfOldPosition], listOfPositions[indexOfNewPosition]);
            }
            oldTargetPositions.RemoveAt(indexOfOldPosition);
            oldSoldierPositions.RemoveAt(indexOfOldPosition);
            listOfPositions.RemoveAt(indexOfNewPosition);
            soldierIndex.RemoveAt(indexOfOldPosition);
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

    public Vector3 offsetPerTroop;
    public Vector3 offsetPerRow;

    public void InstantArrangeByWidth(int widthCount) {
        currentWidth = widthCount;
        int currentWidthIndex = 0;
        int currentRowIndex = 0;
        Vector3 FirstPosition = (Vector3.forward + Vector3.right) / 2;
        for (int i = 0; i < NumberOfSoldiers; i++) {
            Vector3 positionOfThisSoldier = FirstPosition + offsetPerRow * currentRowIndex + offsetPerTroop * currentWidthIndex;

            //Debug.Log(positionOfThisSoldier); 
            Debug.Log(soldierInformation[i]);
            InstantSetPosition(i, positionOfThisSoldier);


            currentWidthIndex++;
            if (currentWidthIndex == widthCount) {
                currentWidthIndex = 0;
                currentRowIndex++;
            }
        }
    }
    public void SetSoldierCount(int quantity) {
        if (CustomSettings.instance == null) {
            CustomSettings.AssignInstance();
        }
        if (soldierInformation == null || soldierInformation.Length != CustomSettings.instance.unitSize) {
            Debug.Log("resetting soldierInformation");
            soldierInformation = null;
            try {
                Debug.Log(NumberOfSoldiers);
            }
            catch {
                soldierInformation = new Matrix4x4[0];
            }
        }

        if (soldierInformation == null || NumberOfSoldiers == 0 && soldierInformation.Length > 0) {
            Debug.Log(NumberOfSoldiers);
        }

        //increase up to size
        if (NumberOfSoldiers < quantity) {
            Debug.Log("adding soldiers");
            for (int i = soldierInformation.Length - 1; i > NumberOfSoldiers; i--) {
                RemoveSoldier(i);
            }
            for (int i = NumberOfSoldiers; i < quantity; i++) {
                AddSoldierAndTargetPosition();
                Debug.Log(soldierInformation[NumberOfSoldiers]);
            }
        }
        //shrink to size
        else if (NumberOfSoldiers >= quantity) {
            for (int i = soldierInformation.Length - 1; i > NumberOfSoldiers; i--) {
                RemoveSoldier(i);
            }
            for (int i = NumberOfSoldiers - 1; i >= quantity; i--) {
                DestroyImmediate(transform.GetChild(i).gameObject);
                RemoveSoldier(i);
            }
        }
        else {
            Debug.LogWarning("tried to set unit count to what it already was? no change but change registered?");
        }

        InstantArrangeByWidth(quantity / 5);
    }

    [SerializeField]
    GameObject serializedItem;
    public static GameObject staticItem;

    void AddSoldierAndTargetPosition() {
        int childCount = transform.childCount;

        GameObject addedSoldier = Instantiate(serializedItem);
        addedSoldier.transform.parent = transform;
        addedSoldier.transform.name = "Soldier " + childCount;
        int length = transform.childCount;
        soldierInformation = new Matrix4x4[transform.childCount + 1];

        SetTargetPosition(childCount - 1, Vector3.zero);
        soldierInformation[childCount - 1][3] = unitNumber;
        SetPosition(childCount - 1, Vector3.zero);
        soldierInformation[childCount - 1][7] = childCount - 1;
        SetFacingDirection(childCount - 1, -offsetPerRow);
        SetOpponent(childCount, Vector3.positiveInfinity, -1, -1);

    }
    #endregion

    public void DebugArrayNumber() {
        Debug.Log(transform.childCount);
    }

    [SerializeField, HideInInspector]
    int a;
    public void DebugSoldierInfo(int soldierIndesx) {
        Debug.Log(soldierInformation[soldierIndesx]);
    }

    #region Unity editor
#if UNITY_EDITOR
    [SerializeField]
    bool drawPlayerArrow;
    private void OnDrawGizmosSelected() {
        //CustomGrid.instance.DisplayUnitCheckingSquares(this);
        if (Application.isPlaying)
            BoundingBox.DisplayBox();
        DisplayDiagonal();

        if (currentlyFighting != null && currentlyFighting.Count() != 0) {
            GLFunctions.GLshapes.DrawArrow(CenterPoint, currentlyFighting[0].targetPositionBoundingBox.Center, Color.purple);
        }
    }

    void DisplayDiagonal() {
        GLFunctions.GLshapes.DrawArrow(CenterPoint, CenterPoint + Rotate90Degrees(GetForwardRightDiagonal()), Color.black);
        Vector3 rotatedDiagonal = new(GetForwardRightDiagonal().z, GetForwardRightDiagonal().y, GetForwardRightDiagonal().x);
        Debug.DrawLine(CenterPoint - Rotate90Degrees(rotatedDiagonal), CenterPoint + Rotate90Degrees(rotatedDiagonal));
        GLFunctions.GLshapes.DrawArrow(CenterPoint, CenterPoint + Rotate90Degrees(GetForwardLeftDiagonal()), Color.black, 1);
        rotatedDiagonal = new(GetForwardLeftDiagonal().z, GetForwardLeftDiagonal().y, GetForwardLeftDiagonal().x);
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
