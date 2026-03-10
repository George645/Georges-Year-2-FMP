using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class CameraScript : MonoBehaviour {
    double rotation;
    double startZ = -10f;
    double startX = 0f;
    int distanceFromEdgeOfScreenDivider = 50;
    Vector3 startMousePosition;
    LineScript ol;
    LineRenderer lr;
    Vector3 mousePosStart, mousePosEnd;
    int unitWidth, mask, previousUnitWidth = 10;

    public static CameraScript instance;
    public static List<GameObject> startingPositions;

    int sensitivity {
        get {
            return PlayerPrefs.GetInt("Sensitivity", 50); //make a sensitivity slider in the pause menu to handle this: make it a 0-100 slider
        }
    }

    private void Awake() {
        if (startingPositions == null) {
            startingPositions = new();
            startingPositions.Add(transform.gameObject);
            foreach (LineRenderer line in FindObjectsByType<LineRenderer>(FindObjectsSortMode.None)) {
                startingPositions.Add(line.gameObject);
            }
        }
    }
    private void Start() {
        if (instance == null) {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else {
            Destroy(this.gameObject);
        }
    }
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            CheckIfClickingOnUnit();
        }
        if (ol != null) {
            GetUnitWidth();
            mask = LayerMask.GetMask("Default");
            if (Input.GetMouseButtonDown(1)) {
                RaycastHit hit;
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000f, mask);
                mousePosStart = hit.point;
                ShowMarkers(true);
            }
            else if (Input.GetMouseButton(1)) {
                RaycastHit hit;
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000f, mask);
                mousePosEnd = hit.point;
                ol.unitWidth = unitWidth;
            }
            else if (Input.GetMouseButtonUp(1)) {
                RaycastHit hit;
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000f, mask);
                ol.unitWidth = unitWidth;
                mousePosEnd = hit.point;
                previousUnitWidth = unitWidth;
                ShowMarkers(false);
            }
        }
    }
    void FixedUpdate() {
        //Rotation
        if (CheckMousePositionForRotation() || RotationKeysPressed()) Rotate();

        //Movement
        if (CheckMousePositionForMovement() || MovementKeysPressed()) Movement();
    }
    
    #region rotation
    bool CheckMousePositionForRotation() {
        if (Input.mousePosition.x < Screen.width / distanceFromEdgeOfScreenDivider || Input.mousePosition.x > Screen.width - Screen.width / distanceFromEdgeOfScreenDivider) {
            return true;
        }
        return false;
    }
    bool RotationKeysPressed() {
        return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E);
    }
    void Rotate() {
        float rotationAmount = 0;
        if (Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width / distanceFromEdgeOfScreenDivider)
            rotationAmount -= 1 * sensitivity;
        if (Input.GetKey(KeyCode.Q))
            rotationAmount -= 1 * sensitivity;
        if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > Screen.width - Screen.width / distanceFromEdgeOfScreenDivider)
            rotationAmount += 1 * sensitivity;
        if (Input.GetKey(KeyCode.E))
            rotationAmount += 1 * sensitivity;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount / 100, transform.eulerAngles.z);
    }
    #endregion

    #region Movement
    bool MovementKeysPressed() {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) return true;
        return false;
    }
    bool CheckMousePositionForMovement() {
        if (Input.mousePosition.y < Screen.height / distanceFromEdgeOfScreenDivider || Input.mousePosition.y > Screen.height - Screen.height / distanceFromEdgeOfScreenDivider) return true;
        return false;
    }
    void Movement() {
        Vector3 movement = Vector3.zero;
        Vector3 forwards = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;

        //vertical movement
        if (Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height / distanceFromEdgeOfScreenDivider) movement -= forwards * sensitivity;
        if (Input.GetKey(KeyCode.S)) movement -= forwards * sensitivity;
        if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - Screen.height / distanceFromEdgeOfScreenDivider) movement += forwards * sensitivity;
        if (Input.GetKey(KeyCode.W)) movement += forwards * sensitivity;

        //horizontal movement
        if (Input.GetKey(KeyCode.A)) movement -= right * sensitivity;
        if (Input.GetKey(KeyCode.D)) movement += right * sensitivity;
        transform.position += movement / 100;
    }
    #endregion

    #region moving units
    private void CheckIfClickingOnUnit() {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000, LayerMask.GetMask("SoldierLayer")) && hit.transform.name.Contains("Soldier")) {
            if (ol != null) {
                ol.selected = false;
            }
            ol = hit.transform.parent.parent.GetChild(0).GetComponent<LineScript>();
            ol.selected = true;
            previousUnitWidth = ol.unitWidth;
            lr = hit.transform.parent.parent.GetChild(0).GetComponent<LineRenderer>();
        }
        else {
            if (ol != null) {
                SetMarkingFalse();
                ol.selected = false;
                ol = null;
                lr = null;
            }
        }
    }
    void SetMarkingFalse() {

        foreach (SoldierMarker sm in ol.soldierMarkerList) {
            if (sm == null) {
                ol.soldierMarkerList.Remove(sm);
                SetMarkingFalse();
                break;
            }
            sm.marking = false;
            sm.transform.position = new Vector3(sm.stm.endPosition.x, 19, sm.stm.endPosition.z);
        }
    }
    private void GetUnitWidth() {
        unitWidth = (int)Math.Round(Vector3.Distance(mousePosStart, mousePosEnd), 1);
        if (unitWidth < 3) {
            unitWidth = previousUnitWidth;
        }
        else {
            try {
                unitWidth = Math.Clamp(unitWidth, 3, ol.soldierMarkerList.Count / 3);
            }
            catch {
                unitWidth = 1;
            }
        }
    }
    private void ShowMarkers(bool setTo) {
        foreach (SoldierMarker soldierMarker in ol.soldierMarkerList) {
            if (soldierMarker == null) {
                ol.soldierMarkerList.Remove(soldierMarker);
                ShowMarkers(setTo);
                break;
            }
            soldierMarker.GetComponent<MeshRenderer>().enabled = setTo;
            soldierMarker.marking = setTo;
        }
    }
    #endregion
}
