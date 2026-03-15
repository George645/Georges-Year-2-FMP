using UnityEngine;
using System.Collections.Generic;
using System;

public class LineScript : MonoBehaviour{
    public List<SoldierMarker> soldierMarkerList = new();
    public bool selected = false;
    public int unitWidth = 10;
    int rowNumber = 0, columnNumber = 0, previousUnitWidth;
    LineRenderer lr;
    public Vector3 previousLineColumnDirection, previousLineRowDirection;
    Vector3 lineStartPosition, lineRowDirection, movement, lineColumnDirection;

    void Start() {
        lr = GetComponent<LineRenderer>();
        unitWidth = (int)Math.Round(Vector3.Distance(lr.GetPosition(0), lr.GetPosition(1)), 1);
        unitWidth = Math.Clamp(unitWidth, (int)Math.Sqrt(soldierMarkerList.Count), soldierMarkerList.Count / 3);
        lineRowDirection = lr.GetPosition(1) - lr.GetPosition(0); // Using these, we can multiply the position in the row (i) by the 
        lr.SetPosition(0, transform.parent.transform.position - lineRowDirection/2);
        lr.SetPosition(1, transform.parent.transform.position + lineRowDirection/2);
        lineStartPosition = lr.GetPosition(0); //between this line and the lines below it, we get where the line starts (this line), then it finds the direction of the rows (the next line) then it gets the direction of the columns. 
        lineColumnDirection = new Vector3(lineRowDirection.z * 1, lineRowDirection.y * 1, lineRowDirection.x * -1);
        foreach (SoldierMarker soldierMarker in soldierMarkerList) {
            soldierMarker.marking = true;
            Math.DivRem(columnNumber, 2, out int ColumnNumberDivRemainder);
            movement = new Vector3(lineStartPosition.x + lineRowDirection.normalized.x * rowNumber + lineColumnDirection.normalized.x * columnNumber + lineRowDirection.normalized.x * 0.5f * ColumnNumberDivRemainder, 19.1f, lineStartPosition.z + lineRowDirection.normalized.z * rowNumber + lineColumnDirection.normalized.z * columnNumber + lineRowDirection.normalized.z * ColumnNumberDivRemainder * 0.5f);
            soldierMarker.transform.position = movement;
            rowNumber++;
            if (rowNumber > unitWidth - 1) {
                columnNumber++;
                rowNumber = 0;
            }
        }
        previousLineColumnDirection = lineColumnDirection;
        previousLineRowDirection = lineRowDirection;
        previousUnitWidth = unitWidth;
    }
    public void AddMarker(SoldierMarker soldier) {
        soldierMarkerList.Add(soldier);
    }
    public void RemoveSoldier(SoldierMarker soldier) {
        if (soldierMarkerList.Contains(soldier)) {
            soldierMarkerList.Remove(soldier);
        }
        else {
            Debug.Log(soldier + " was not found in unit " + transform.parent.name);
        }
    }
    private void Update() {
        if(soldierMarkerList.Count == 0) {
            Destroy(this.transform.parent.gameObject);
        }
        lineStartPosition = lr.GetPosition(0);
        lineRowDirection = lr.GetPosition(1) - lr.GetPosition(0);
        if ((int)Math.Round(Vector3.Distance(lr.GetPosition(0), lr.GetPosition(1)), 1) < soldierMarkerList.Count / 30 + 2) {
            lineStartPosition = lr.GetPosition(0) - previousLineRowDirection / 2;
            lineColumnDirection = previousLineColumnDirection;
            lineRowDirection = previousLineRowDirection;
            unitWidth = previousUnitWidth;
        }
        if (Input.GetMouseButtonUp(1) && selected) {
            previousLineColumnDirection = lineColumnDirection;
            previousLineRowDirection = lineRowDirection;
            previousUnitWidth = unitWidth;
        }
    }
    void ForLoop() {
        foreach (SoldierMarker soldierMarker in soldierMarkerList) {
            if (soldierMarker == null) {
                soldierMarkerList.Remove(soldierMarker);
                ForLoop();
                break;
            }
            else {
                soldierMarker.marking = true;
                Math.DivRem(columnNumber, 2, out int ColumnNumberDivRemainder);
                movement = new Vector3(lineStartPosition.x + lineRowDirection.normalized.x * rowNumber + lineColumnDirection.normalized.x * columnNumber + lineRowDirection.normalized.x * 0.5f * ColumnNumberDivRemainder, 19.1f, lineStartPosition.z + lineRowDirection.normalized.z * rowNumber + lineColumnDirection.normalized.z * columnNumber + lineRowDirection.normalized.z * ColumnNumberDivRemainder * 0.5f);
                soldierMarker.transform.position = movement;
                rowNumber++;
                if (rowNumber > unitWidth - 1) {
                    columnNumber++;
                    rowNumber = 0;
                }
            }
        }
    }
    void FixedUpdate() {
        rowNumber = 0;
        columnNumber = 0;
        lineColumnDirection = new Vector3(lineRowDirection.z * 1, lineRowDirection.y * 1, lineRowDirection.x * -1);
        if (Input.GetMouseButton(1) && selected) {
            ForLoop();
        }
        if (Input.GetMouseButtonUp(1) && selected) {
            foreach (SoldierMarker soldierMarker in soldierMarkerList) {
                Math.DivRem(columnNumber, 2, out int ColumnNumberDivRemainder);
                movement = new Vector3(lineStartPosition.x + lineRowDirection.normalized.x * rowNumber + lineColumnDirection.normalized.x * columnNumber + lineRowDirection.normalized.x * 0.5f * ColumnNumberDivRemainder, 19.1f, lineStartPosition.z + lineRowDirection.normalized.z * rowNumber + lineColumnDirection.normalized.z * columnNumber + lineRowDirection.normalized.z * ColumnNumberDivRemainder * 0.5f);
                soldierMarker.transform.position = movement;
                rowNumber++;
                if (rowNumber > unitWidth - 1) {
                    columnNumber++;
                    rowNumber = 0;
                }
            }
            previousLineColumnDirection = lineColumnDirection;
            previousLineRowDirection = lineRowDirection;
        }
    }
}
