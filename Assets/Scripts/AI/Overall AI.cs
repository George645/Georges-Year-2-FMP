using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

enum State {
    consolidating = 0,
    travelling = 1,
    defending = 2,
    attacking = 3
}

public class OverallAI : MonoBehaviour {
    public static OverallAI instance;
    Unit[] AIUnits;
    BoundingBox AIUnitsBoundingBox;

    Unit[] playersUnits;
    BoundingBox playersUnitsBoundingBox;

    [SerializeField]
    State currentState = State.consolidating;

    #region Unity functions
    void Start() {
        if (instance == null) {
            instance = this;
        }

        AIUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None).Where(x => !x.playersUnit).ToArray();
        AIUnitsBoundingBox = new();
        int i = 0;
        foreach (Unit unit in AIUnits) {
            unit.AIIndex = i;
            AIUnitsBoundingBox.Encapsulate(unit.CenterPoint);
            i++;
        }
        playersUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None).Where(x => x.playersUnit).ToArray();
        playersUnitsBoundingBox = new();
        int j = 0;
        foreach (Unit unit in playersUnits) {
            unit.AIIndex = j;
            playersUnitsBoundingBox.Encapsulate(unit.CenterPoint);
            j++;
        }
        DecideOption();
        StartCoroutine(IsInCorrectState());
        StartCoroutine(OncePer5SecondsUpdate());
    }

    IEnumerator OncePer5SecondsUpdate() {
        yield return new WaitForSeconds(1);
        while (true) {
            if (currentState == State.consolidating) {
                if (!newDefense)
                    ReenableDefenseVariables();
                if (hasEngagedEnemy)
                    ReenableAttackingVariables();
                Consolidate();
            }
            else if (currentState == State.defending) {
                if (hasEngagedEnemy)
                    ReenableAttackingVariables();
                if (!DefenseFormed && newDefense)
                    FormDefense();
                else
                    HoldAndReinforceDefense();
            }
            else if (currentState == State.travelling) {
                Travel();
                if (!newDefense)
                    ReenableDefenseVariables();
                if (hasEngagedEnemy)
                    ReenableAttackingVariables();
            }
            else if (currentState == State.attacking) {
                if (!newDefense)
                    ReenableDefenseVariables();
                if (!hasEngagedEnemy)
                    EngageEnemy();
                else
                    SupportEngagedTroops();
            }
            else {
                throw new InvalidEnumArgumentException("Not found enum arguament " + currentState);
            }
            yield return new WaitForSeconds(5);
        }
    }
    #endregion


    #region general
    IEnumerator IsInCorrectState() {
        while (true) {
            yield return new WaitForSeconds(30);
            DecideOption();
        }
    }
    public void UpdateUnitPosition(Vector3 position, int listIndex, bool PlayersTeam) {
        (PlayersTeam ? playersUnitsBoundingBox : AIUnitsBoundingBox).ChangePoint(listIndex, position);
    }

    void AssignUnitPosition(Vector3 startPosition, Vector3 offsetPerTroop, Vector3 offsetPerRow, int width, Unit unit) {
        Vector3[] targetPositions = new Vector3[unit.NumberOfSoldiers];

        int widthCount = 0;
        int depthCount = 0;
        for (int i = 0; i < unit.NumberOfSoldiers; i++) {
            targetPositions[i] = startPosition + widthCount * offsetPerTroop + depthCount * offsetPerRow;
            widthCount++;
            if (widthCount >= width) {
                widthCount = 0;
                depthCount++;
            }
        }

        unit.NewPositions(targetPositions.ToList());
    }

    Vector3 ApproximateFrontLeftPoint(Unit unit) {
        return unit.CenterPoint - (unit.offsetPerRow * unit.NumberOfSoldiers / unit.CurrentWidth + unit.offsetPerTroop * unit.CurrentWidth) / 2;
    }

    void DecideOption() {
        if (AIUnitsBoundingBox.Size.sqrMagnitude > (3 * AIUnits[0].rightMagnitude * AIUnits[0].CurrentWidth * AIUnits.Count()) * (3 * AIUnits[0].rightMagnitude * AIUnits[0].CurrentWidth * AIUnits.Count())) {
            currentState = State.consolidating;
            return;
        }
        if (playersUnits.Count() > AIUnits.Count()) {
            currentState = State.defending;
            return;
        }
        if ((AIUnitsBoundingBox.Center - playersUnitsBoundingBox.Center).sqrMagnitude > 50000) {
            currentState = State.travelling;
            return;
        }
        currentState = State.attacking;
    }

    void FormLine(Unit[] units, Vector3 startPosition, Vector3 endPosition) {
        Vector3 distanceBetweenStartAndEnd = endPosition - startPosition;
        //Something needs to be added in here to make it so that the distance between start and end can't scale up infinitely
        Vector3 distanceBetweenStartAndEndWithoutInBetweenGap = distanceBetweenStartAndEnd - (units.Count() - 1) * 5 * distanceBetweenStartAndEnd.normalized;
        Vector3 distancePerUnit = distanceBetweenStartAndEndWithoutInBetweenGap / units.Count();
        int currentWidth;
        int currentRow;
        for (int index = 0; index < units.Count(); index++) {
            if (units[index].previousStartingPoint == startPosition + (distancePerUnit + distanceBetweenStartAndEnd.normalized * 5) * index) continue;
            Vector3 soldierOffsetPerTroop = distanceBetweenStartAndEnd.normalized * units[index].offsetPerTroop.magnitude;
            Vector3 soldierOffsetPerRow = new(soldierOffsetPerTroop.z, soldierOffsetPerTroop.y, -soldierOffsetPerTroop.x);
            units[index].potentialOffsetPerRow = soldierOffsetPerRow;
            units[index].potentialOffsetPerTroop = soldierOffsetPerTroop;

            currentWidth = 0;
            currentRow = 0;
            Vector3[] arrayOfNewPositions = new Vector3[units[index].NumberOfSoldiers];
            for (int j = 0; j < units[index].NumberOfSoldiers; j++) {
                arrayOfNewPositions[j] = startPosition + (distancePerUnit + distanceBetweenStartAndEnd.normalized * 5) * index + currentWidth * soldierOffsetPerTroop + currentRow * soldierOffsetPerRow;
                currentWidth++;
                if ((currentWidth * soldierOffsetPerTroop).sqrMagnitude > (startPosition - (startPosition + distancePerUnit + distanceBetweenStartAndEnd.normalized / 2)).sqrMagnitude) {
                    units[index].potentialNextWidth = currentWidth - 1;
                    currentRow++;
                    currentWidth = 0;
                }
            }
            units[index].NewPositions(arrayOfNewPositions.ToList());
        }
    }

    #endregion


    #region consolidating
    void Consolidate() {
        int widthOfLines = 5; // base this off of the number of soldiers to have it be like when there is under 7, there is only one line, and when there is more than 7, there are two lines with the front one having more units
        Vector3 center = AIUnitsBoundingBox.Center;
        Vector3 normalizedDirectionAggressing = (playersUnitsBoundingBox.Center - AIUnitsBoundingBox.Center).normalized;
        Vector3 normalizedUnitRows = new(-normalizedDirectionAggressing.z, 0, normalizedDirectionAggressing.x);

        for (int i = 0; i < AIUnits.Count(); i += widthOfLines) {
            Vector3 startPosition = center + ((AIUnits.Count() - i) / 5 + 1) * AIUnits[0].forwardsMagnitude * AIUnits[0].NumberOfSoldiers / AIUnits[0].CurrentWidth * normalizedDirectionAggressing * 3 - ((AIUnits.Count() - i > 5) ? 5 : AIUnits.Count() - i) * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedUnitRows / 2;
            Vector3 endPosition = center + ((AIUnits.Count() - i) / 5 + 1) * AIUnits[0].forwardsMagnitude * AIUnits[0].NumberOfSoldiers / AIUnits[0].CurrentWidth * normalizedDirectionAggressing * 3 + ((AIUnits.Count() - i > 5) ? 5 : AIUnits.Count() - i) * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedUnitRows / 2;
            FormLine(AIUnits[i..((i + 5) < AIUnits.Count() ? i + widthOfLines : AIUnits.Count())], startPosition, endPosition);
        }
    }

    #endregion

    #region moving
    void Travel() {
        int widthOfLines = 5; // base this off of the number of soldiers to have it be like when there is under 7, there is only one line, and when there is more than 7, there are two lines with the front one having more units
        Vector3 normalizedDirectionAggressing = (playersUnitsBoundingBox.Center - AIUnitsBoundingBox.Center).normalized;
        Vector3 center = playersUnitsBoundingBox.Center - (((AIUnits.Count() / 5) + 3) * AIUnits[0].forwardsMagnitude * AIUnits[0].NumberOfSoldiers * normalizedDirectionAggressing / AIUnits[0].CurrentWidth * 2);
        Vector3 normalizedUnitRows = new(-normalizedDirectionAggressing.z, 0, normalizedDirectionAggressing.x);


        for (int i = 0; i < AIUnits.Count(); i += widthOfLines) {
            Vector3 startPosition = center + ((AIUnits.Count() - i) / 5 + 1) * AIUnits[0].forwardsMagnitude * AIUnits[0].NumberOfSoldiers / AIUnits[0].CurrentWidth * normalizedDirectionAggressing * 3 - ((AIUnits.Count() - i > 5) ? 5 : AIUnits.Count() - i) * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedUnitRows / 2;
            Vector3 endPosition = center + ((AIUnits.Count() - i) / 5 + 1) * AIUnits[0].forwardsMagnitude * AIUnits[0].NumberOfSoldiers / AIUnits[0].CurrentWidth * normalizedDirectionAggressing * 3 + ((AIUnits.Count() - i > 5) ? 5 : AIUnits.Count() - i) * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedUnitRows / 2;
            FormLine(AIUnits[i..((i + widthOfLines) < AIUnits.Count() ? i + widthOfLines : (AIUnits.Count()))], startPosition, endPosition);
        }
    }

    #endregion

    #region defending
    bool DefenseFormed {
        get {
            try {
                for (int i = 0; i < positionsInDefendingLine.Count(); i++) {
                    if ((positionsInDefendingLine[i] - unitsInDefendingLine[i].TargetPositionBoundingBox.Center).sqrMagnitude > 1000) {
                        return false;
                    }
                }
            }
            catch {
                return false;
            }
            return true;
        }
    }
    bool newDefense = true;
    Vector3[] positionsInDefendingLine;
    Unit[] unitsInDefendingLine;
    Unit[] notInDefendingLine;
    BoundingBox defendingLine;
    void ReenableDefenseVariables() {
        newDefense = true;
    }
    void FormDefense() {
        newDefense = false;
        Vector3 centerOfFrontLine = playersUnitsBoundingBox.Center + ((AIUnitsBoundingBox.Center - playersUnitsBoundingBox.Center).sqrMagnitude > ((AIUnitsBoundingBox.Center - playersUnitsBoundingBox.Center).normalized * 50).sqrMagnitude ? AIUnitsBoundingBox.Center - playersUnitsBoundingBox.Center : (AIUnitsBoundingBox.Center - playersUnitsBoundingBox.Center).normalized * 50);
        Vector3 normalizedDirectionOfPlayersUnits = (centerOfFrontLine - playersUnitsBoundingBox.Center).normalized;
        relativisticForwards = normalizedDirectionOfPlayersUnits;
        Vector3 normalizedDirectionOfTroopOffset = new(-normalizedDirectionOfPlayersUnits.z, 0, normalizedDirectionOfPlayersUnits.x);
        FormLine(AIUnits[..(int)(AIUnits.Count() * 0.75f)], centerOfFrontLine - (AIUnits.Count() * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedDirectionOfTroopOffset / 2), centerOfFrontLine + (AIUnits.Count() * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedDirectionOfTroopOffset / 2));

        Vector3 startPosition = centerOfFrontLine - (AIUnits.Count() * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedDirectionOfTroopOffset / 2);
        Vector3 endPosition = centerOfFrontLine + (AIUnits.Count() * AIUnits[0].CurrentWidth * AIUnits[0].rightMagnitude * normalizedDirectionOfTroopOffset / 2);
        Vector3 distanceBetweenStartAndEnd = endPosition - startPosition;
        //Something needs to be added in here to make it so that the distance between start and end can't scale up infinitely
        Vector3 distanceBetweenStartAndEndWithoutInBetweenGap = distanceBetweenStartAndEnd - (AIUnits[..(int)(AIUnits.Count() * 0.75f)].Count() - 1) * 5 * distanceBetweenStartAndEnd.normalized;
        Vector3 distancePerUnit = distanceBetweenStartAndEndWithoutInBetweenGap / AIUnits[..(int)(AIUnits.Count() * 0.75f)].Count();
        positionsInDefendingLine = new Vector3[(int)(AIUnits.Count() * 0.75f) + 1];
        unitsInDefendingLine = new Unit[(int)(AIUnits.Count() * 0.75f) + 1];
        notInDefendingLine = new Unit[AIUnits.Count() - ((int)(AIUnits.Count() * 0.75f) + 1)];
        defendingLine = new();

        for (int i = 0; i <= AIUnits.Count() * 0.75; i++) {
            positionsInDefendingLine[i] = startPosition + (distancePerUnit + distanceBetweenStartAndEnd.normalized * 5) * i + AIUnits[i].offsetPerTroop * AIUnits[i].CurrentWidth / 2 + AIUnits[i].offsetPerRow * AIUnits[i].NumberOfSoldiers / AIUnits[i].CurrentWidth / 2;
            defendingLine.Encapsulate(positionsInDefendingLine[i]);
            unitsInDefendingLine[i] = AIUnits[i];
        }
        for (int i = (int)(AIUnits.Count() * 0.75); i < AIUnits.Count(); i++) {
            try {
                notInDefendingLine[i - (int)(AIUnits.Count() * 0.75) - 1] = AIUnits[i];
            }
            catch (System.Exception e){
                Debug.Log(i + ", " + (AIUnits.Count() * 0.75f));
                Debug.Log(AIUnits.Length);
                Debug.Log(AIUnits[i]);
                Debug.Log(notInDefendingLine.Length);
                Debug.Log(notInDefendingLine[i - (int)(AIUnits.Count() * 0.75) - 1]);
                throw e;
            }
        }

        Vector3 centerOfBackLine = centerOfFrontLine + (AIUnits[^1].forwardsMagnitude * AIUnits[^1].NumberOfSoldiers * normalizedDirectionOfPlayersUnits / AIUnits[^1].CurrentWidth * 3);
        FormLine(AIUnits[(int)(AIUnits.Count() * 0.75f)..], centerOfBackLine - ((AIUnits.Count() - (int)(AIUnits.Count() * 0.75f)) * AIUnits[^1].CurrentWidth * AIUnits[^1].rightMagnitude * normalizedDirectionOfTroopOffset / 2), centerOfBackLine + ((AIUnits.Count() - (int)(AIUnits.Count() * 0.75f)) * AIUnits[^1].CurrentWidth * AIUnits[0].rightMagnitude * normalizedDirectionOfTroopOffset / 2));

        currentlyManeuveringIntoIndexesPosition = new();
        currentlyManeuveringUnit = new();
        playersUnitsBehindAILines = new();
        unitDealingWithUnitBehindLines = new();
    }

    Vector3 relativisticForwards;

    List<int> currentlyManeuveringIntoIndexesPosition;
    List<Unit> currentlyManeuveringUnit;

    List<Unit> playersUnitsBehindAILines;
    List<Unit> unitDealingWithUnitBehindLines;

    void HoldAndReinforceDefense() {
        newDefense = false;
        if (notInDefendingLine.Where(x => x != null).Count() == 0) return;

        for (int i = 0; i < unitsInDefendingLine.Length; i++) {
            if (currentlyManeuveringIntoIndexesPosition.Contains(i)) {
                if ((currentlyManeuveringUnit[currentlyManeuveringIntoIndexesPosition.IndexOf(i)].CenterPoint - positionsInDefendingLine[i]).sqrMagnitude > 1000)
                    continue;
                else {
                    unitsInDefendingLine[i] = currentlyManeuveringUnit[currentlyManeuveringIntoIndexesPosition.IndexOf(i)];
                    currentlyManeuveringIntoIndexesPosition.RemoveAt(currentlyManeuveringIntoIndexesPosition.IndexOf(i));
                    currentlyManeuveringIntoIndexesPosition.Remove(i);
                }
            }
            Unit unit = unitsInDefendingLine[i];
            if (unit == null || (unit.CenterPoint - positionsInDefendingLine[i]).sqrMagnitude > 1000) {
                Unit newUnit = notInDefendingLine[0];
                Unit copyingUnit = (unit == null ? newUnit : unitsInDefendingLine[i]);
                notInDefendingLine.RemoveAt(0);
                AssignUnitPosition(positionsInDefendingLine[i] - newUnit.offsetPerRow * newUnit.NumberOfSoldiers / copyingUnit.CurrentWidth / 2 - newUnit.offsetPerTroop * copyingUnit.CurrentWidth / 2, newUnit.offsetPerTroop, newUnit.offsetPerRow, copyingUnit.CurrentWidth, newUnit);
                currentlyManeuveringIntoIndexesPosition.Add(i);
                currentlyManeuveringUnit.Add(newUnit);
            }
        }
        for (int i = 0; i < playersUnits.Count(); i++) {
            if (Vector3.Dot(relativisticForwards, defendingLine.Center - playersUnits[i].CenterPoint) < 0) {
                if (!playersUnitsBehindAILines.Contains(playersUnits[i])) {
                    unitDealingWithUnitBehindLines.Add(notInDefendingLine[0]);
                    notInDefendingLine.RemoveAt(0);
                    playersUnitsBehindAILines.Add(playersUnits[i]);
                }
            }
        }
        for (int i = 0; i < unitDealingWithUnitBehindLines.Count(); i++) {
            if (unitDealingWithUnitBehindLines[i] == null && !(notInDefendingLine.Count() == 0)) {
                unitDealingWithUnitBehindLines[i] = notInDefendingLine[0];
                notInDefendingLine.RemoveAt(0);
            }
            if (!unitDealingWithUnitBehindLines[i].InCombat) {
                Vector3 facingDirection = (playersUnitsBehindAILines[i].CenterPoint - unitDealingWithUnitBehindLines[i].CenterPoint).normalized;
                AssignUnitPosition(ApproximateFrontLeftPoint(playersUnitsBehindAILines[i]) + playersUnitsBehindAILines[i].offsetPerTroop * playersUnitsBehindAILines[i].CurrentWidth, new Vector3(facingDirection.z, 0, -facingDirection.x), -facingDirection, unitDealingWithUnitBehindLines[i].CurrentWidth, unitDealingWithUnitBehindLines[i]);
            }
        }
    }
    #endregion


    #region attacking

    bool hasEngagedEnemy = false;

    void ReenableAttackingVariables() {
        hasEngagedEnemy = false;
    }
    List<Unit> playersUnitsThatAreEngaged;
    void EngageEnemy() {
        playersUnitsThatAreEngaged ??= new();
        hasEngagedEnemy = true;
        playersUnitsThatAreEngaged.AddRange(from Unit unit in AIUnits
                                            let movingToUnit = playersUnits[(new System.Random()).Next(0, playersUnits.Count() - 1)]
                                            select movingToUnit);
    }
    void SupportEngagedTroops() {
        for (int i = 0; i < AIUnits.Length; i++) {
            Unit unit = AIUnits[i];
            if (!unit.InCombat) {
                try {
                    Vector3 facingDirection = (playersUnitsThatAreEngaged[i].CenterPoint - unit.CenterPoint).normalized;
                    AssignUnitPosition(playersUnitsThatAreEngaged[i].CenterPoint, new Vector3(-facingDirection.z, 0, facingDirection.x) * playersUnitsThatAreEngaged[i].rightMagnitude, -facingDirection * playersUnitsThatAreEngaged[i].forwardsMagnitude, unit.CurrentWidth, unit);
                }
                catch (System.Exception e) {
                    Debug.Log(i);
                    throw e;
                }
            }
        }
    }
    #endregion

    private void OnDrawGizmosSelected() {
        AIUnitsBoundingBox.DisplayBox();
        playersUnitsBoundingBox.DisplayBox();
    }
}
