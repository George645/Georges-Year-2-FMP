using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    int distanceFromEdgeOfScreenDivider = 20;
    Vector3 unitStartPosition, unitEndPosition;
    Unit currentlySelected;
    List<Unit> currentlySelectedList;
    [SerializeField]
    GameObject highlightedTargetPositionParent;

    public static CameraScript instance;
    public static List<GameObject> startingPositions;

    [SerializeField, Range(0, 200)]
    int backupSensitivity;
    int Sensitivity {
        get {
            return PlayerPrefs.GetInt("Sensitivity", backupSensitivity); //make a Sensitivity slider in the pause menu to handle this: make it a 0-100 slider
        }
        set { }
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
            DisableLastSelection();
            CheckIfClickingOnUnit();
        }
        if (currentlySelected != null) {
            if (Input.GetMouseButtonDown(1)) {
                SetStartPosition();
            }
            if (Input.GetMouseButton(1)) {
                SetEndPosition();
            }
            if (Input.GetMouseButtonUp(1)) {
                SetEndPosition();
            }
            if (currentlySelectedList.Count >= 1) {
                for (int i = 0; i < currentlySelectedList.Count; i++) {
                    SetMovementLine(i);
                    Debug.Log("movement Line set: " + i);
                }
            }
            else {
                SetMovementLine(-1);
                Debug.Log("movement Line set: 0");
            }
        }
        AddOrSubtractScrollAmount();
        //Rotation
        if (CheckMousePositionForRotation() || RotationKeysPressed()) Rotate();

        //Movement
        if (CheckMousePositionForMovement() || MovementKeysPressed()) Movement();

        //vertical movement and rotation
        AddScrollingDelta();
    }

    #region moving units
    List<List<GameObject>> currentlyManipulatedTargetPositions = new();
    void DisableLastSelection() {
        if (currentlySelected != null) {
            currentlySelected.selected = false;
            currentlySelected = null;
        }
        currentlyManipulatedTargetPositions = new();
    }
    void CheckIfClickingOnUnit() {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction * 10000, out RaycastHit hitInfo, 10000)) {
            if (hitInfo.collider.gameObject.name.Contains("Soldier")) {
                currentlySelected = hitInfo.collider.transform.parent.GetComponent<Unit>();
                currentlySelected.selected = true;
                if (Input.GetKey(KeyCode.LeftShift)) {
                    if (currentlySelectedList.Count == 0) {
                        currentlySelectedList.Add(currentlySelected);
                    }
                }
                else {
                    currentlySelectedList = new();
                }
                currentlyManipulatedTargetPositions.Add(new());
                Debug.Log(currentlyManipulatedTargetPositions.Count);
                Debug.Log(currentlyManipulatedTargetPositions[currentlyManipulatedTargetPositions.Count - 1]);
                for (int i = 0; i < currentlySelected.NumberOfSoldiers; i++) {
                    currentlyManipulatedTargetPositions[currentlyManipulatedTargetPositions.Count - 1].Add(highlightedTargetPositionParent.transform.GetChild(i).gameObject);
                }
                if (currentlySelectedList.Count >= 1) {
                    currentlySelected = null;
                }
            }
        }
    }

    void SetStartPosition() {
        unitStartPosition = ScreenPointToGroundPoint(Input.mousePosition);
    }
    void SetEndPosition() {
        Vector3 groundPointOfEndPosition = ScreenPointToGroundPoint(Input.mousePosition);
        if ((groundPointOfEndPosition - unitStartPosition).sqrMagnitude < currentlySelected.offsetPerTroop.sqrMagnitude * 3)
            unitEndPosition = unitStartPosition;
        else if ((groundPointOfEndPosition - unitStartPosition).sqrMagnitude > currentlySelected.offsetPerTroop.sqrMagnitude * (currentlySelected.NumberOfSoldiers / (currentlySelected.NumberOfSoldiers > 120 ? 4 : 3)) * (currentlySelected.NumberOfSoldiers / 3))
            unitEndPosition = unitStartPosition + (groundPointOfEndPosition - unitStartPosition).normalized * currentlySelected.offsetPerTroop.magnitude * (currentlySelected.NumberOfSoldiers / (currentlySelected.NumberOfSoldiers > 120 ? 4 : 3) - 0.1f);
        else
            unitEndPosition = groundPointOfEndPosition;
    }
    void SetMovementLine(int Count) {
        Vector3 localUnitStartPosition;
        Vector3 localUnitEndPosition;
        List<GameObject> localCurrentlyManipulatedPositions;
        if (Count >= 0) {
            localCurrentlyManipulatedPositions = currentlyManipulatedTargetPositions[Count];
            localUnitStartPosition = unitStartPosition + unitEndPosition * Count / currentlySelectedList.Count;
            localUnitEndPosition = unitEndPosition - unitStartPosition * Count / currentlySelectedList.Count;
            currentlySelected = currentlySelectedList[Count];
            throw new Exception("This doens't quite work yet");
        }
        else {
            localCurrentlyManipulatedPositions = currentlyManipulatedTargetPositions[0];
            localUnitStartPosition = unitStartPosition;
            localUnitEndPosition = unitEndPosition;
        }
        if (Vector3.SqrMagnitude(localUnitStartPosition - localUnitEndPosition) == 0) {
            if (highlightedTargetPositionParent.transform.GetChild(0).GetComponent<MeshRenderer>().enabled) {
                ToggleMeshRenderers(false);
            }
            Vector3 startingPosition = localUnitStartPosition - currentlySelected.offsetPerTroop * currentlySelected.CurrentWidth / 2;
            int currentWidth = 0;
            int currentRow = 0;
            for (int i = 0; i < localCurrentlyManipulatedPositions.Count; i++) {
                localCurrentlyManipulatedPositions[i].transform.position = startingPosition + currentWidth * currentlySelected.offsetPerTroop + currentRow * currentlySelected.offsetPerRow;
                currentWidth++;
                if (currentWidth == currentlySelected.CurrentWidth) {
                    currentRow++;
                    currentWidth = 0;
                }
            }
        }
        else {
            if (!highlightedTargetPositionParent.transform.GetChild(0).GetComponent<MeshRenderer>().enabled) {
                ToggleMeshRenderers(true);
            }
            Vector3 startingPosition = localUnitStartPosition;
            Vector3 soldierOffsetPerTroop = (localUnitEndPosition - localUnitStartPosition).normalized * currentlySelected.offsetPerTroop.magnitude;
            Vector3 soldierOffsetPerRow = new Vector3(soldierOffsetPerTroop.z, soldierOffsetPerTroop.y, -soldierOffsetPerTroop.x);
            currentlySelected.offsetPerTroop = soldierOffsetPerTroop;
            currentlySelected.offsetPerRow = soldierOffsetPerRow;
            int currentWidth = 0;
            int currentRow = 0;
            for (int i = 0; i < localCurrentlyManipulatedPositions.Count; i++) {
                localCurrentlyManipulatedPositions[i].transform.position = startingPosition + currentWidth * soldierOffsetPerTroop + currentRow * soldierOffsetPerRow;
                currentWidth++;
                if ((currentWidth * soldierOffsetPerTroop).sqrMagnitude > (startingPosition - localUnitEndPosition).sqrMagnitude) {
                    currentRow++;
                    currentWidth = 0;
                }
            }
        }
        if (Input.GetMouseButtonUp(1)) {
            if (Count >= 0) {
                SendPositionalDataToUnit(Count);
            }
            else {
                SendPositionalDataToUnit(0);
            }
        }
    }
    void ToggleMeshRenderers(bool isEnabled) {
        int count = 0;
        for (int i = 0; i < currentlyManipulatedTargetPositions.Count - 1; i++) {
            for (int j = 0; j < currentlyManipulatedTargetPositions[i].Count - 1;) {
                highlightedTargetPositionParent.transform.GetChild(count).GetComponent<MeshRenderer>().enabled = isEnabled;
                count++;
            }
        }
    }
    void SendPositionalDataToUnit(int index) {
        ToggleMeshRenderers(false);
        List<Vector3> ListOfPositions = new();
        foreach (GameObject thing in currentlyManipulatedTargetPositions[index]) {
            ListOfPositions.Add(thing.transform.position);
        }
        Debug.Log("positional data sent");
        currentlySelected.NewPositions(ListOfPositions);
    }
    /// <summary>
    /// returns zero if there is no collision with the floor
    /// </summary>
    /// <param name="screenPoint"> the point on the screen that you wish the ray to come from </param>
    /// <returns></returns>
    Vector3 ScreenPointToGroundPoint(Vector3 screenPoint) {
        Ray raycast = Camera.main.ScreenPointToRay(screenPoint);
        LayerMask groundMask = 1 << LayerMask.NameToLayer("Ground");
        if (Physics.Raycast(raycast.origin, raycast.direction * 1000, out RaycastHit hitInfo, 1000, groundMask.value)) {
            return hitInfo.point;
        }
        return Vector3.zero;
    }
    #endregion

    #region rotation
    bool CheckMousePositionForRotation() {
        if (Input.mousePosition.x > Screen.width || Input.mousePosition.x < 0 || Input.mousePosition.y > Screen.height || Input.mousePosition.y < 0) return false;
        if (Input.mousePosition.x < Screen.width / distanceFromEdgeOfScreenDivider || Input.mousePosition.x > Screen.width - Screen.width / distanceFromEdgeOfScreenDivider) return true;
        return false;
    }
    bool RotationKeysPressed() {
        return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E);
    }
    void Rotate() {
        float rotationAmount = 0;
        if (Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width / distanceFromEdgeOfScreenDivider)
            rotationAmount -= 1 * Sensitivity;
        if (Input.GetKey(KeyCode.Q))
            rotationAmount -= 1 * Sensitivity;
        if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > Screen.width - Screen.width / distanceFromEdgeOfScreenDivider)
            rotationAmount += 1 * Sensitivity;
        if (Input.GetKey(KeyCode.E))
            rotationAmount += 1 * Sensitivity;

        transform.parent.eulerAngles = new Vector3(transform.parent.eulerAngles.x, transform.parent.eulerAngles.y + rotationAmount / 100, transform.parent.eulerAngles.z);
    }
    #endregion

    #region Movement
    bool MovementKeysPressed() {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) return true;
        return false;
    }
    bool CheckMousePositionForMovement() {
        if (Input.mousePosition.x > Screen.width || Input.mousePosition.x < 0 || Input.mousePosition.y > Screen.height || Input.mousePosition.y < 0) return false;
        if (Input.mousePosition.y < Screen.height / distanceFromEdgeOfScreenDivider || Input.mousePosition.y > Screen.height - Screen.height / distanceFromEdgeOfScreenDivider) return true;
        return false;
    }
    void Movement() {
        Vector3 movement = Vector3.zero;
        Vector3 forwards = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;

        //vertical movement
        if (Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height / distanceFromEdgeOfScreenDivider) movement -= forwards * Sensitivity;
        if (Input.GetKey(KeyCode.S)) movement -= forwards * Sensitivity;
        if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - Screen.height / distanceFromEdgeOfScreenDivider) movement += forwards * Sensitivity;
        if (Input.GetKey(KeyCode.W)) movement += forwards * Sensitivity;

        //horizontal movement
        if (Input.GetKey(KeyCode.A)) movement -= right * Sensitivity;
        if (Input.GetKey(KeyCode.D)) movement += right * Sensitivity;

        //speed boost
        if (Input.GetKey(KeyCode.LeftShift)) movement *= 3;

        transform.parent.position += movement / 100;
    }
    #endregion

    #region scrolling

    float scrolledDelta = 1;
    [SerializeField]
    float scrolledPosition = 0;
    void AddOrSubtractScrollAmount() {
        scrolledDelta += Input.mouseScrollDelta.y;
    }

    float minValue = 0;
    float maxValue = 90;
    [SerializeField]
    float positionZMultiplier = 25;
    [SerializeField]
    float positionZAddition = 10;
    [SerializeField]
    float positionYMultiplier = 20;
    [SerializeField]
    float positionYAddition = 5;
    [SerializeField]
    float rotationMultiplier = 50;
    [SerializeField]
    float multiplier = 3;

    float degreesToRadians = 0.01745328f;
    float time = 0;
    Vector3 priorPosition;
    Quaternion priorRotation;

    [SerializeField]
    float a;
    [SerializeField]
    float b;
    [SerializeField]
    float c;
    public void AddScrollingDelta() {
        if (scrolledDelta != 0) {
            priorPosition = transform.localPosition;
            priorRotation = transform.localRotation;
            time = Time.time;
        }
        scrolledPosition = Math.Clamp(scrolledPosition + scrolledDelta * PlayerPrefs.GetInt("Sensitivity", 50) * 0.2f, -maxValue, minValue);
        scrolledDelta = 0;
        transform.localPosition = Vector3.Lerp(priorPosition, multiplier * new Vector3(0, -Mathf.Sin(scrolledPosition * degreesToRadians) * positionYMultiplier + positionYAddition, -Mathf.Sin(scrolledPosition * degreesToRadians) * positionZMultiplier + positionZAddition), Math.Clamp(Time.time - time, 0, 1));
        transform.localRotation = Quaternion.Lerp(priorRotation, new Quaternion(0, 180, Mathf.Sin(scrolledPosition * degreesToRadians) * rotationMultiplier, priorRotation.w), Math.Clamp((Time.time - time) / 50, 0, 1));

    }
    #endregion
}