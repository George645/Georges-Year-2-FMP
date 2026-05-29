using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomGrid : MonoBehaviour {
    [SerializeField]
    int totalSizeOfMap = 3000;
    int borderCoordinateOfMap { get { return totalSizeOfMap / 2; } }

    #region Unity functions
    private void OnValidate() {

        SetInstance();
    }
    private void Awake() {
        MakeArrays();
    }
    void MakeArrays() {
        unitReferences = new Unit[] { };
        unitSquareIndex = new int[] { };
        soldierReferences = new Soldier[] { };
        soldierSquareIndex = new int[] { };
    }
    private void Start() {
        StartCoroutine(nameof(WaitOneFrame));
    }
    IEnumerator WaitOneFrame() {
        yield return null;
        SetInstance();
        CreateUnitList();
        CreateSoldierList();
    }
    #endregion

    #region Set instance
    public static CustomGrid instance;
    public void SetInstance() {
        if (instance == null || instance == this)
            instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    #region Unit

    #region variables
    [SerializeField]
    int unitGridWidthCount = 10;
    int unitGridSquareWidth {
        get { return totalSizeOfMap / unitGridWidthCount; }
    }
    [SerializeField]
    bool displayUnitGrid;
    [SerializeField]
    Unit[] unitReferences; // 1 variable for the unit, one number for the box it is in - split this into two arrays at some point
    [SerializeField]
    int[] unitSquareIndex;

    #endregion

    public void UpdateUnitPosition(Unit unit) {
        int index = Array.FindIndex(unitReferences, x => x == unit);
        unitSquareIndex[index] = UnitSpaceToArrayIndex(WorldSpaceToUnitSpace(unit.CenterPoint));
        UnitSort(index);
    }
    void CreateUnitList() {
        Unit[] tempList = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        unitReferences = new Unit[tempList.Length];
        unitSquareIndex = new int[tempList.Length];
        for (int i = 0; i < tempList.Length; i++) {
            unitReferences[i] = tempList[i];
            unitSquareIndex[i] = UnitSpaceToArrayIndex(WorldSpaceToUnitSpace(tempList[i].transform.position));
        }
        UnitSort();
    }

    public Unit[] RetrieveNearbyUnits(Vector3 position) {
        int arrayPositionOfPosition = UnitSpaceToArrayIndex(WorldSpaceToSoldierSpace(position));
        int[] neighbourPositionsToCheck = UnitNeighbours(arrayPositionOfPosition);
        List<Unit> unitsNearby = RetrieveUnitsInSquare(arrayPositionOfPosition).ToList();
        for (int i = 0; i < neighbourPositionsToCheck.Length; i++) {
            unitsNearby.AddRange(RetrieveUnitsInSquare(neighbourPositionsToCheck[i]));
        }
        return unitsNearby.ToArray();
    }
    public Unit[] RetrieveUnitsInSquare(int squareIndex) {
        int indexOfSquareIndex = UnitBinarySearchForSquare(squareIndex);
        int firstIndexOfSquareIndex = indexOfSquareIndex;
        int lastIndexOfSquareIndex = indexOfSquareIndex;
        while (unitSquareIndex[indexOfSquareIndex - 1] == squareIndex)
            firstIndexOfSquareIndex--;
        while (unitSquareIndex[indexOfSquareIndex + 1] == squareIndex)
            lastIndexOfSquareIndex++;
        Unit[] returningArray = new Unit[lastIndexOfSquareIndex - firstIndexOfSquareIndex];
        for (int i = firstIndexOfSquareIndex; i <= lastIndexOfSquareIndex; i++) {
            returningArray[i - firstIndexOfSquareIndex] = unitReferences[firstIndexOfSquareIndex];
        }
        return returningArray;
    }

    int UnitBinarySearchForSquare(int squareIndex) {
        int[] tempCheckingArrayIndexes = unitSquareIndex; // <- this is so gonna store a reference and not a copy
        int indexOfSquareIndex = -1;
        int addingToIndex = 0;
        while (indexOfSquareIndex == -1) {
            if (tempCheckingArrayIndexes.Length == 1) {
                if (tempCheckingArrayIndexes[0] < squareIndex) {
                    return addingToIndex;
                }
                else if (tempCheckingArrayIndexes[0] > squareIndex) {
                    return addingToIndex;
                }
            }
            if (tempCheckingArrayIndexes[tempCheckingArrayIndexes.Length / 2] == squareIndex) {
                indexOfSquareIndex = tempCheckingArrayIndexes.Length / 2 + addingToIndex;
            }
            else if (tempCheckingArrayIndexes[tempCheckingArrayIndexes.Length / 2] > squareIndex) {
                tempCheckingArrayIndexes = tempCheckingArrayIndexes[..(tempCheckingArrayIndexes.Length / 2)];
                continue;
            }
            else {
                addingToIndex += tempCheckingArrayIndexes.Length / 2;
                tempCheckingArrayIndexes = tempCheckingArrayIndexes[(tempCheckingArrayIndexes.Length / 2)..];
                continue;
            }
        }
        return indexOfSquareIndex;
    }

    int[] UnitNeighbours(int index) {
        List<int> returningArray = new();
        (int, int) unitSpace = ArrayIndexToUnitSpace(index);
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == j && j == 0) continue;
                try {
                    returningArray.Add(UnitSpaceToArrayIndex((unitSpace.Item1 + i, unitSpace.Item2 + j)));
                }
                catch (Exception e) {
                    if (e.Message.Contains("out of range")) {
                        continue;
                    }
                    else {
                        Debug.Log(e.Message);
                        throw e;
                    }
                }
            }
        }
        returningArray = returningArray.Where(x => x != 0).ToList();
        return returningArray.ToArray();
    }

    #region Unit sort
    Unit[] duplicateUnitReferenceArray;
    int[] duplicateUnitSquareIndexArray;
    void UnitSort() {
        duplicateUnitSquareIndexArray = new int[unitSquareIndex.Length];
        duplicateUnitReferenceArray = new Unit[unitReferences.Length];

        Array.Copy(unitSquareIndex, duplicateUnitSquareIndexArray, unitSquareIndex.Length);
        Array.Copy(unitReferences, duplicateUnitReferenceArray, unitSquareIndex.Length);

        UnitSort(0, unitSquareIndex.Length - 1, ref unitReferences, ref duplicateUnitReferenceArray, ref unitSquareIndex, ref duplicateUnitSquareIndexArray);
    }

    void UnitSort(int begin, int end, ref Unit[] unitArray1, ref Unit[] unitArray2, ref int[] intArray1, ref int[] intArray2) {
        if (end - begin <= 1) return;

        int middle = (end + begin) / 2;

        UnitSort(begin, middle, ref unitArray2, ref unitArray1, ref intArray2, ref intArray1);
        UnitSort(middle, end, ref unitArray2, ref unitArray1, ref intArray2, ref intArray1);

        UnitMerge(begin, end, ref unitArray2, ref unitArray1, ref intArray2, ref intArray1);
    }

    void UnitMerge(int begin, int end, ref Unit[] unitArray1, ref Unit[] unitArray2, ref int[] intArray1, ref int[] intArray2) {
        int middleConst = (begin + end) / 2;
        int middle = middleConst;

        for (int i = begin; i < end; i++) {
            if (begin < middleConst && (middle >= end || intArray1[begin] <= intArray1[middle])) {
                unitArray2[i] = unitArray1[begin];
                intArray2[i] = intArray1[begin];
                begin++;
            }
            else {
                unitArray2[i] = unitArray1[middle];
                intArray2[i] = intArray1[middle];
                middle++;
            }
        }
        for (int i = begin; i < end; i++) {
            unitArray1[i] = unitArray2[i];
            intArray1[i] = intArray1[i];
        }
    }

    void UnitSort(int startingIndexInSquareIndexArray) {
        int squareIndex = unitSquareIndex[startingIndexInSquareIndexArray];
        Unit unit = unitReferences[startingIndexInSquareIndexArray];
        if (startingIndexInSquareIndexArray != 0)
            unitSquareIndex[startingIndexInSquareIndexArray] = unitSquareIndex[startingIndexInSquareIndexArray - 1];
        else
            unitSquareIndex[0] = unitSquareIndex[0] - 1;
        int arrayPositionOfCorrectPosition = UnitBinarySearchForSquare(squareIndex);
        if (startingIndexInSquareIndexArray < arrayPositionOfCorrectPosition) {
            for (int i = startingIndexInSquareIndexArray; i < arrayPositionOfCorrectPosition; i++) {
                unitSquareIndex[i] = unitSquareIndex[i + 1];
                unitReferences[i] = unitReferences[i + 1];
            }
        }
        else if (startingIndexInSquareIndexArray > arrayPositionOfCorrectPosition) {
            if (unitSquareIndex[0] < squareIndex)
                arrayPositionOfCorrectPosition += 1;
            for (int i = startingIndexInSquareIndexArray; i > arrayPositionOfCorrectPosition; i--) {
                unitSquareIndex[i] = unitSquareIndex[i - 1];
                unitReferences[i] = unitReferences[i - 1];
            }
        }
        unitSquareIndex[arrayPositionOfCorrectPosition] = squareIndex;
        unitReferences[arrayPositionOfCorrectPosition] = unit;
    }
    #endregion

    #region Conversions
    Vector3 UnitSpaceToWorldSpace((int, int) unitSpace) {
        return new Vector3(unitSpace.Item1 * unitGridSquareWidth - borderCoordinateOfMap + offsetForMinorMisalignment, 21, unitSpace.Item2 * unitGridSquareWidth - borderCoordinateOfMap + offsetForMinorMisalignment);
    }
    (int, int) WorldSpaceToUnitSpace(Vector3 worldSpace) {// convert to a coordinate system with the negative negative corner being 0, 0
        if (worldSpace.x < -borderCoordinateOfMap + offsetForMinorMisalignment || worldSpace.x > borderCoordinateOfMap + offsetForMinorMisalignment || worldSpace.z < -borderCoordinateOfMap + offsetForMinorMisalignment || worldSpace.z > borderCoordinateOfMap + offsetForMinorMisalignment) throw new Exception("Position out of bounds exception: position must be withing positive and negative" + borderCoordinateOfMap);
        return (((int)worldSpace.x + borderCoordinateOfMap - offsetForMinorMisalignment) / unitGridSquareWidth, ((int)worldSpace.z + borderCoordinateOfMap - offsetForMinorMisalignment) / unitGridSquareWidth);
    }
    int UnitSpaceToArrayIndex((int, int) unitSpace) {
        if (unitSpace.Item1 < 0 || unitSpace.Item1 >= unitGridWidthCount || unitSpace.Item2 < 0 || unitSpace.Item2 >= unitGridWidthCount) throw new Exception("Position out of range, the positions must be ints in the range 0, " + unitGridWidthCount * unitGridWidthCount);
        return unitSpace.Item2 * unitGridWidthCount + unitSpace.Item1;
    }
    (int, int) ArrayIndexToUnitSpace(int index) {
        if (index < 0 || index > unitGridWidthCount * unitGridWidthCount) throw new Exception("Array startingIndexInSquareIndexArray must be between 0 and " + unitGridWidthCount * unitGridWidthCount);
        return (index % unitGridWidthCount, index / unitGridWidthCount);
    }

    #endregion

    #region Draw debug squares
    Color salmon = new Color(0.9803922f, 0.5019608f, 0.4470589f, 0.1f);
    public void DisplayUnitCheckingSquares(Unit unit) {// <- make a variable to check if you would like this debugginng on or not
        if (!displayUnitGrid) return;
        if (unitReferences == null || unitSquareIndex == null || unitReferences.Length == 0 || unitSquareIndex.Length == 0) CreateUnitList();
        UpdateUnitPosition(unit);
        int index = Array.FindIndex(unitReferences, x => x == unit); // <- make a check when this is null, remake the unitgrid list

        FillInUnitSquare(unitSquareIndex[index], new Color(1, 0, 0, .1f));
        int[] indexes = UnitNeighbours(unitSquareIndex[index]);
        foreach (int index2 in indexes) {
            try {
                FillInUnitSquare(index2, salmon);
            }
            catch { }
        }
    }
    void FillInUnitSquare(int index, Color color) { // <- make a variable to check if you would like this debugginng on or not
        (int, int) corner1 = ArrayIndexToUnitSpace(index);
        (int, int) corner2 = (corner1.Item1 + 1, corner1.Item2 + 1);
        Vector3 corner1Vector = UnitSpaceToWorldSpace(corner1);
        Vector3 corner2Vector = UnitSpaceToWorldSpace(corner2);
        Vector3 midpoint = (corner1Vector + corner2Vector) / 2;
        //Debug.Log(corner1Vector + ", " + corner2Vector + ", " + midpoint);
        Gizmos.color = color;
        Gizmos.DrawCube(midpoint, new Vector3(corner2Vector.x - corner1Vector.x, 0, corner2Vector.z - corner1Vector.z));
    }
    #endregion

    #endregion

    #region Soldier

    #region Soldier variables
    [SerializeField]
    int soldierGridWidthCount = 10;
    [SerializeField]
    bool displaySoldierGrid;
    [SerializeField]
    int[] soldierSquareIndex;
    [SerializeField]
    Soldier[] soldierReferences;
    int soldierGridSquareWidth {
        get { return totalSizeOfMap / (soldierGridWidthCount * unitGridWidthCount); } // < one of these has to be unit
    }

    #endregion

    public void UpdateSoldierPosition(Soldier soldier) {
        if (!Application.isPlaying && soldierSquareIndex.Length == 0) {
            MakeArrays();
            CreateSoldierList();
            CreateUnitList();
        }
        int index = soldier.customGridIndex;
        soldierSquareIndex[index] = SoldierSpaceToArrayIndex(WorldSpaceToSoldierSpace(soldier.currentPosition));
        SoldierSort(index);
    }
    void CreateSoldierList() {
        Soldier[] tempList = FindObjectsByType<Soldier>(FindObjectsSortMode.None);
        soldierReferences = new Soldier[tempList.Length];
        soldierSquareIndex = new int[tempList.Length];
        for (int i = 0; i < tempList.Length; i++) {
            soldierReferences[i] = tempList[i];
            soldierReferences[i].customGridIndex = i;
            soldierSquareIndex[i] = SoldierSpaceToArrayIndex(WorldSpaceToSoldierSpace(tempList[i].transform.position));
        }
        SoldierSort();
    }
    public Soldier[] RetrieveNearbySoldiers(Vector3 position) {
        int arrayPositionOfPosition = SoldierSpaceToArrayIndex(WorldSpaceToSoldierSpace(position));
        int[] neighbourPositionsToCheck = SoldierNeighbours(arrayPositionOfPosition);
        Soldier[] soldiersNearby = RetrieveSoldiersInSquare(arrayPositionOfPosition);
        for (int i = 0; i < neighbourPositionsToCheck.Length; i++) {
            soldiersNearby = RetrieveSoldiersInSquare(neighbourPositionsToCheck[i], soldiersNearby);
        }
        //for (int i = 0; i < soldiersNearby.Where(x => x == null).Count(); i++) {
        //    Debug.Log(soldiersNearby.Where(x => x == null).ToArray()[i] + ", " + neighbourPositionsToCheck[i]);
        //    soldiersNearby = soldiersNearby.RemoveAt(soldiersNearby.ToList().IndexOf(soldiersNearby.Where(x => x == null).ToArray()[i]));
        //}


        return soldiersNearby;
    }
    int SoldierBinarySearchForSquare(int squareIndex) { // get sub array is very slow in this, see if you can speed it up.
        int indexOfSquareIndex = -1;
        int addingToIndex = 0;
        int count = 0;
        int startingIndexChecking = 0;
        int endingIndexChecking = soldierSquareIndex.Length - 1;
        int middleIndexChecking = soldierSquareIndex.Length / 2 - 1;
        while (indexOfSquareIndex == -1) {
            count++;
            if (count > 200) throw new Exception("Too deep in the while loop");
            if (endingIndexChecking == startingIndexChecking) {
                if (soldierSquareIndex[endingIndexChecking] < squareIndex) {
                    return addingToIndex;
                }
                else if (soldierSquareIndex[endingIndexChecking] > squareIndex) {
                    if (addingToIndex - 1 == -1) addingToIndex = 1;
                    return addingToIndex - 1;
                }
            }
            if (soldierSquareIndex[middleIndexChecking] == squareIndex) {
                indexOfSquareIndex = addingToIndex + (middleIndexChecking - startingIndexChecking);
            }
            else if (soldierSquareIndex[middleIndexChecking] > squareIndex) {
                endingIndexChecking = middleIndexChecking;
                middleIndexChecking = (startingIndexChecking + endingIndexChecking) / 2;
                continue;
            }
            else {
                addingToIndex += middleIndexChecking - startingIndexChecking + 1;
                startingIndexChecking = middleIndexChecking + 1;
                middleIndexChecking = (startingIndexChecking + endingIndexChecking) / 2;
                continue;
            }
        }
        return indexOfSquareIndex;
    }
    int[] SoldierNeighbours(int index) {
        int[] returningArray = new int[8];
        int count = 0;
        (int, int) soldierSpace = ArrayIndexToSoldierSpace(index);
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (index + 3 * (i + 1) + j + 1 > soldierGridWidthCount * soldierGridWidthCount * unitGridWidthCount * unitGridWidthCount) continue;

                if (i == 0 && j == 0) {
                    count++; continue;
                }
                try {
                    returningArray[3 * (i + 1) + j + 1 - count] = SoldierSpaceToArrayIndex((soldierSpace.Item1 + i, soldierSpace.Item2 + j));
                }
                catch (Exception e) {
                    if (e.Message.Contains("out of range")) {
                        continue;
                    }
                    else {
                        Debug.Log(e.Message);
                        throw e;
                    }
                }
            }
        }
        //returningArray = returningArray.Where(x => x != 0).ToArray();
        return returningArray;
    }

    public Soldier[] RetrieveSoldiersInSquare(int squareIndex, Soldier[] priorSoldierArray) {
        int indexOfSquareIndex = SoldierBinarySearchForSquare(squareIndex);
        if (soldierSquareIndex[indexOfSquareIndex] != squareIndex) {
            return priorSoldierArray;
        }

        int firstIndexOfSquareIndex = indexOfSquareIndex;
        int lastIndexOfSquareIndex = indexOfSquareIndex;
        int priorSoldierArrayLength = priorSoldierArray == null ? 0 : priorSoldierArray.Length;
        while (firstIndexOfSquareIndex - 1 >= 0 && soldierSquareIndex[firstIndexOfSquareIndex - 1] == squareIndex)
            firstIndexOfSquareIndex--;
        while (lastIndexOfSquareIndex + 1 < soldierSquareIndex.Length && soldierSquareIndex[lastIndexOfSquareIndex + 1] == squareIndex)
            lastIndexOfSquareIndex++; //yan was here
        Soldier[] returningArray = new Soldier[priorSoldierArrayLength + lastIndexOfSquareIndex - firstIndexOfSquareIndex + 1];
        for (int i = 0; i < priorSoldierArrayLength; i++) {
            returningArray[i] = priorSoldierArray[i];
        }
        for (int i = firstIndexOfSquareIndex; i <= lastIndexOfSquareIndex; i++) {
            returningArray[i - firstIndexOfSquareIndex + priorSoldierArrayLength] = soldierReferences[i];
        }
        return returningArray;
    }
    public Soldier[] RetrieveSoldiersInSquare(int squareIndex) {
        int indexOfSquareIndex = SoldierBinarySearchForSquare(squareIndex);
        if (soldierSquareIndex[indexOfSquareIndex] != squareIndex) return null;

        int firstIndexOfSquareIndex = indexOfSquareIndex;
        int lastIndexOfSquareIndex = indexOfSquareIndex;
        while (firstIndexOfSquareIndex - 1 >= 0 && soldierSquareIndex[firstIndexOfSquareIndex - 1] == squareIndex)
            firstIndexOfSquareIndex--;
        while (lastIndexOfSquareIndex + 1 < soldierSquareIndex.Length && soldierSquareIndex[lastIndexOfSquareIndex + 1] == squareIndex)
            lastIndexOfSquareIndex++;
        Soldier[] returningArray = new Soldier[lastIndexOfSquareIndex - firstIndexOfSquareIndex + 1];
        for (int i = firstIndexOfSquareIndex; i <= lastIndexOfSquareIndex; i++) {
            returningArray[i - firstIndexOfSquareIndex] = soldierReferences[i];
        }
        return returningArray.Where(x => x != null).ToArray();
    }

    public void RemoveSoldier(Soldier soldier) {
        int indexOfSoldier = soldier.customGridIndex;
        int square = soldierSquareIndex[indexOfSoldier];
        soldierSquareIndex = soldierSquareIndex.RemoveAt(indexOfSoldier); 
        soldierReferences = soldierReferences.RemoveAt(indexOfSoldier);
        duplicateSoldierSquareIndexArray = duplicateSoldierSquareIndexArray.RemoveAt(indexOfSoldier);
        duplicateSoldierReferenceArray = duplicateSoldierReferenceArray.RemoveAt(indexOfSoldier);
        foreach (Soldier soldier1 in soldierReferences.Where(soldier => soldier.customGridIndex > indexOfSoldier)) {
            soldier1.customGridIndex -= 1;
        }
    }

    #region Soldier sort
    Soldier[] duplicateSoldierReferenceArray;
    int[] duplicateSoldierSquareIndexArray;
    void SoldierSort() {
        duplicateSoldierSquareIndexArray = new int[soldierSquareIndex.Length];
        duplicateSoldierReferenceArray = new Soldier[soldierReferences.Length];

        Array.Copy(soldierSquareIndex, duplicateSoldierSquareIndexArray, soldierSquareIndex.Length);
        Array.Copy(soldierReferences, duplicateSoldierReferenceArray, soldierSquareIndex.Length);

        SoldierSort(0, soldierSquareIndex.Length, ref soldierReferences, ref duplicateSoldierReferenceArray, ref soldierSquareIndex, ref duplicateSoldierSquareIndexArray);
    }
    void SoldierSort(int begin, int end, ref Soldier[] soldierArray1, ref Soldier[] soldierArray2, ref int[] intArray1, ref int[] intArray2) {
        if (end - begin <= 1) return;

        int middle = (end + begin) / 2;

        SoldierSort(begin, middle, ref soldierArray2, ref soldierArray1, ref intArray2, ref intArray1);
        SoldierSort(middle, end, ref soldierArray2, ref soldierArray1, ref intArray2, ref intArray1);

        SoldierMerge(begin, end, ref soldierArray2, ref soldierArray1, ref intArray2, ref intArray1);
    }

    void SoldierMerge(int begin, int end, ref Soldier[] soldierArray1, ref Soldier[] soldierArray2, ref int[] intArray1, ref int[] intArray2) {
        int middleConst = (begin + end) / 2;
        int middle = middleConst;

        for (int i = begin; i < end; i++) {
            //Debug.Log(intArray1[begin] + ", " + intArray1[middle] + ", " + (begin < middleConst && (middle >= end || intArray1[begin] <= intArray1[middle])));
            if (begin < middleConst && (middle >= end || intArray1[begin] <= intArray1[middle])) {
                soldierArray2[i] = soldierArray1[begin];
                soldierArray2[i].customGridIndex = i;
                intArray2[i] = intArray1[begin];
                begin++;
            }
            else {
                soldierArray2[i] = soldierArray1[middle];
                soldierArray2[i].customGridIndex = i;
                intArray2[i] = intArray1[middle];
                middle++;
            }
        }
    }

    void SoldierSort(int startingIndexInSquareIndexArray) {

        int squareIndex = soldierSquareIndex[startingIndexInSquareIndexArray];
        Soldier soldier = soldierReferences[startingIndexInSquareIndexArray];
        try {
            soldierSquareIndex[startingIndexInSquareIndexArray] = soldierSquareIndex.Length - 1 <= (startingIndexInSquareIndexArray) ? soldierSquareIndex[startingIndexInSquareIndexArray - 1] : soldierSquareIndex[startingIndexInSquareIndexArray + 1];
        }
        catch (Exception e) {
            Debug.Log(startingIndexInSquareIndexArray);
            Debug.Log(soldierSquareIndex.Length);
            Debug.Log(soldierSquareIndex.Length <= (startingIndexInSquareIndexArray));
            throw e;
        }
        int arrayPositionOfCorrectPosition = SoldierBinarySearchForSquare(squareIndex);


        if (startingIndexInSquareIndexArray < arrayPositionOfCorrectPosition)
            for (int i = startingIndexInSquareIndexArray; i < arrayPositionOfCorrectPosition; i++) {
                soldierSquareIndex[i] = soldierSquareIndex[i + 1];
                soldierReferences[i] = soldierReferences[i + 1];
                soldierReferences[i].customGridIndex = i;
            }
        else if (startingIndexInSquareIndexArray > arrayPositionOfCorrectPosition) {
            if (soldierSquareIndex[0] < squareIndex)
                arrayPositionOfCorrectPosition += 1;
            for (int i = startingIndexInSquareIndexArray; i > arrayPositionOfCorrectPosition; i--) {
                soldierSquareIndex[i] = soldierSquareIndex[i - 1];
                soldierReferences[i] = soldierReferences[i - 1];
                soldierReferences[i].customGridIndex = i;
            }
        }
        soldierSquareIndex[arrayPositionOfCorrectPosition] = squareIndex;
        soldierReferences[arrayPositionOfCorrectPosition] = soldier;
        soldierReferences[arrayPositionOfCorrectPosition].customGridIndex = arrayPositionOfCorrectPosition;
        
    }
    #endregion

    #region Conversions
    public void LogSoldierInfo(Vector3 position, int arrayPosition, Soldier name) {
        int actualPosition = SoldierBinarySearchForSquare(WorldSpaceToArrayIndex(position));
        Debug.Log("Supposed position: " + arrayPosition + ", actual position " + actualPosition);
        Debug.Log("Supposed soldier: " + name.gameObject.name + ", actual soldier in that position: " + soldierReferences[arrayPosition]);
        Debug.Log("Supposed neighbours: " + soldierReferences[arrayPosition - 1] + ", and index higher: " + soldierReferences[arrayPosition + 1]);
        Debug.Log("actual neighbours: " + soldierReferences[actualPosition - 1] + ", and index higher: " + soldierReferences[actualPosition + 1]);
    }
    int WorldSpaceToArrayIndex(Vector3 worldSpace) {
        return SoldierSpaceToArrayIndex(WorldSpaceToSoldierSpace(worldSpace));
    }

    Vector3 SoldierSpaceToWorldSpace((int, int) soldierSpace) {
        return new Vector3(soldierSpace.Item1 * soldierGridSquareWidth - borderCoordinateOfMap + offsetForMinorMisalignment, 21, soldierSpace.Item2 * soldierGridSquareWidth - borderCoordinateOfMap + offsetForMinorMisalignment);
    }
    (int, int) WorldSpaceToSoldierSpace(Vector3 worldSpace) {// convert to a coordinate system with the negative negative corner being 0, 0
        if (worldSpace.x < -borderCoordinateOfMap + offsetForMinorMisalignment || worldSpace.x > borderCoordinateOfMap + offsetForMinorMisalignment || worldSpace.z < -borderCoordinateOfMap + offsetForMinorMisalignment || worldSpace.z > borderCoordinateOfMap + offsetForMinorMisalignment) throw new Exception("Position out of bounds exception: position must be withing positive and negative" + borderCoordinateOfMap);
        return (((int)worldSpace.x + borderCoordinateOfMap - offsetForMinorMisalignment) / soldierGridSquareWidth, ((int)worldSpace.z + borderCoordinateOfMap - offsetForMinorMisalignment) / soldierGridSquareWidth);
    }
    int SoldierSpaceToArrayIndex((int, int) soldierSpace) {
        if (soldierSpace.Item1 < 0 || soldierSpace.Item1 > (soldierGridWidthCount * unitGridWidthCount) || soldierSpace.Item2 < 0 || soldierSpace.Item2 > (soldierGridWidthCount * unitGridWidthCount)) throw new Exception("Position out of range, the positions must be ints in the range 0, " + (soldierGridWidthCount * unitGridWidthCount));
        return soldierSpace.Item2 * (soldierGridWidthCount * unitGridWidthCount) + soldierSpace.Item1;
    }
    (int, int) ArrayIndexToSoldierSpace(int index) {
        if (index < 0 || index > (soldierGridWidthCount * unitGridWidthCount) * (soldierGridWidthCount * unitGridWidthCount)) throw new Exception("Array startingIndexInSquareIndexArray must be between 0 and " + soldierGridWidthCount * soldierGridWidthCount);
        return (index % (soldierGridWidthCount * unitGridWidthCount), index / (soldierGridWidthCount * unitGridWidthCount));
    }
    #endregion

    #region DrawDebugSquares

    Color lightBlue = new Color(0.333324f, 0.4673822f, 0.9019608f, 0.1f);
    public void DisplaySoldierCheckingSquares(Soldier soldier) {
        if (!displaySoldierGrid) return;
        if (soldierReferences == null || soldierSquareIndex == null || soldierReferences.Length == 0 || soldierSquareIndex.Length == 0) CreateSoldierList();
        UpdateSoldierPosition(soldier);

        int index = soldier.customGridIndex;
        FillInSoldierSquare(soldierSquareIndex[index], new Color(0, 0, 1, .2f));

        int[] neighbouringSquares = SoldierNeighbours(soldierSquareIndex[index]);
        foreach (int squareReferenceNumber in neighbouringSquares) {
            FillInSoldierSquare(squareReferenceNumber, lightBlue);
        }
    }
    void FillInSoldierSquare(int index, Color color) { // <- make a variable to check if you would like this debugginng on or not
        (int, int) corner1 = ArrayIndexToSoldierSpace(index);
        (int, int) corner2 = (corner1.Item1 + 1, corner1.Item2 + 1);
        Vector3 corner1Vector = SoldierSpaceToWorldSpace(corner1);
        Vector3 corner2Vector = SoldierSpaceToWorldSpace(corner2);
        Vector3 midpoint = (corner1Vector + corner2Vector) / 2;
        GLFunctions.GLNumbers.DisplayNumber(index, midpoint, null, null, 0.5f);
        Gizmos.color = color;
        Gizmos.DrawCube(midpoint, new Vector3(corner2Vector.x - corner1Vector.x, 0, corner2Vector.z - corner1Vector.z));
    }
    #endregion

    #endregion

    const int offsetForMinorMisalignment = 500;

#if UNITY_EDITOR

    #region Grid gizmo
    [SerializeField]
    Material lineMaterial;
    const int yHeight = 21;
    public void DisplayGrids() {
        if (displayUnitGrid) {
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            for (int i = 0; i <= unitGridWidthCount; i++) {
                //lines along one side
                GL.Vertex3(-borderCoordinateOfMap + offsetForMinorMisalignment, yHeight, -borderCoordinateOfMap + i * unitGridSquareWidth + offsetForMinorMisalignment);
                GL.Vertex3(borderCoordinateOfMap + offsetForMinorMisalignment, yHeight, -borderCoordinateOfMap + i * unitGridSquareWidth + offsetForMinorMisalignment);

                //lines along the other
                GL.Vertex3(-borderCoordinateOfMap + i * unitGridSquareWidth + offsetForMinorMisalignment, yHeight, -borderCoordinateOfMap + offsetForMinorMisalignment);
                GL.Vertex3(-borderCoordinateOfMap + i * unitGridSquareWidth + offsetForMinorMisalignment, yHeight, borderCoordinateOfMap + offsetForMinorMisalignment);
            }
            GL.End();
            GL.PopMatrix();
        }
        if (displaySoldierGrid) {

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            for (int i = 0; i <= soldierGridWidthCount * unitGridWidthCount; i++) {
                //lines along one side
                GL.Vertex3(-borderCoordinateOfMap + offsetForMinorMisalignment, yHeight, -borderCoordinateOfMap + i * soldierGridSquareWidth + offsetForMinorMisalignment);
                GL.Vertex3(borderCoordinateOfMap + offsetForMinorMisalignment, yHeight, -borderCoordinateOfMap + i * soldierGridSquareWidth + offsetForMinorMisalignment);

                //lines along the other
                GL.Vertex3(-borderCoordinateOfMap + i * soldierGridSquareWidth + offsetForMinorMisalignment, yHeight, -borderCoordinateOfMap + offsetForMinorMisalignment);
                GL.Vertex3(-borderCoordinateOfMap + i * soldierGridSquareWidth + offsetForMinorMisalignment, yHeight, borderCoordinateOfMap + offsetForMinorMisalignment);
            }
            GL.End();
            GL.PopMatrix();
        }
    }
    #endregion

    public void ColourSoldiersExcluding(Soldier soldier) {
        Soldier[] SoldiersNearby = new Soldier[] { };
        SoldiersNearby = RetrieveNearbySoldiers(soldier.transform.position);
        foreach (Soldier soldier1 in SoldiersNearby) {
            if (soldier1 == soldier) continue;
            soldier1.DrawCubeAroundThis(Color.softRed);
        }
    }

    private void OnDrawGizmos() {
        DisplayGrids();
    }
#endif
}
