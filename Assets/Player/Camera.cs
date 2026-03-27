using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    int distanceFromEdgeOfScreenDivider = 20;
    Vector3 unitStartPosition, unitEndPosition;
    Unit currentlySelected;
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

    private void Awake() {
        if (startingPositions == null) {
            startingPositions = new() { transform.gameObject };
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
                SendPositionalDataToUnit();
            }
            SetMovementLine();
        }
    }
    void FixedUpdate() {
        //Rotation
        if (CheckMousePositionForRotation() || RotationKeysPressed()) Rotate();

        //Movement
        if (CheckMousePositionForMovement() || MovementKeysPressed()) Movement();

        //vertical movement and rotation
        if (Scrolling()) LerpBetweenPoints(currentIndex - direction);
    }

    #region moving units
    List<GameObject> currentlyManipulatedTargetPositions = new();
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
                for (int i = 0; i < currentlySelected.NumberOfSoldiers; i++) {
                    currentlyManipulatedTargetPositions.Add(highlightedTargetPositionParent.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    void SetStartPosition() {
        unitStartPosition = ScreenPointToGroundPoint(Input.mousePosition);
    }
    void SetEndPosition() {
        unitEndPosition = ScreenPointToGroundPoint(Input.mousePosition);
    }
    void SetMovementLine() {
        if (Vector3.SqrMagnitude(unitStartPosition - unitEndPosition) < 4 /* 2 squared */) {
            if (highlightedTargetPositionParent.transform.GetChild(0).GetComponent<MeshRenderer>().enabled) {
                ToggleMeshRenderers(false);
            }
            Vector3 startingPosition = unitStartPosition - currentlySelected.offsetPerTroop * currentlySelected.CurrentWidth / 2;
            int currentWidth = 0;
            int currentRow = 0;
            for (int i = 0; i < currentlyManipulatedTargetPositions.Count; i++) {
                currentlyManipulatedTargetPositions[i].transform.position = startingPosition + currentWidth * currentlySelected.offsetPerTroop + currentRow * currentlySelected.OffsetPerRow;
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
            Vector3 startingPosition = unitStartPosition;
            Vector3 soldierOffsetPerTroop = (unitEndPosition - unitStartPosition).normalized * currentlySelected.offsetPerTroop.magnitude;
            Vector3 soldierOffsetPerRow = new Vector3(soldierOffsetPerTroop.z, soldierOffsetPerTroop.y, -soldierOffsetPerTroop.x);
            int currentWidth = 0;
            int currentRow = 0;
            for (int i = 0; i < currentlyManipulatedTargetPositions.Count; i++) {
                currentlyManipulatedTargetPositions[i].transform.position = startingPosition + currentWidth * soldierOffsetPerTroop + currentRow * soldierOffsetPerRow;
                currentWidth++;
                if ((currentWidth * soldierOffsetPerTroop).sqrMagnitude > (startingPosition - unitEndPosition).sqrMagnitude) {
                    currentRow++;
                    currentWidth = 0;
                }
            }
        }
    }
    void ToggleMeshRenderers(bool isEnabled) {
        for (int i = 0; i < currentlyManipulatedTargetPositions.Count; i++) {
            highlightedTargetPositionParent.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = isEnabled;
        }
    }
    void SendPositionalDataToUnit() {
        ToggleMeshRenderers(false);
        List<Vector3> ListOfPositions = new();
        foreach (GameObject thing in currentlyManipulatedTargetPositions) {
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

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount / 100, transform.eulerAngles.z);
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
        transform.parent.position += movement / 100;
    }
    #endregion

    int direction;
    bool Scrolling() {
        if (Input.mouseScrollDelta.y == 0) {
            return false;
        }
        Debug.Log(Input.mouseScrollDelta);
        direction = (int)Input.mouseScrollDelta.y;
        return true;
    }

    [SerializeField, HideInInspector]
    int positionsCount;
    [SerializeField, HideInInspector] // can't be above or below the positions count
    int currentIndex;
    [SerializeField]
    List<PositionAndRotation> positions;
    public void LerpBetweenPoints(int targetIndex) {
        StartCoroutine(LerpBetweenPointsCoroutine(targetIndex));
    }
    [SerializeField]
    int speed = 10;
    IEnumerator LerpBetweenPointsCoroutine(int targetIndex) {
        if (!(targetIndex >= positionsCount || targetIndex < 0)) {
            float time = Time.time;
            PositionAndRotation startingPosition = new(transform.localPosition, transform.localRotation);
            while (Time.time - time <= 10f / speed) {
                transform.localPosition = Vector3.Lerp(startingPosition.position, positions[targetIndex].position, (Time.time - time) / (10f / speed));
                transform.localRotation = Quaternion.Lerp(startingPosition.rotation, positions[targetIndex].rotation, (Time.time - time) / (10f / speed));
                yield return null;
            }
            SetCameraPosition(targetIndex);
            currentIndex = targetIndex;
            yield return null;
        }
    }

    public void SetCameraPosition(int index) {
        transform.localPosition = positions[index].position;
        transform.localRotation = positions[index].rotation;
    }
#if UNITY_EDITOR
    public void SetElementAtIndexToPosition(int index) {
        positions[index] = new PositionAndRotation(transform.localPosition, transform.localRotation);
    }
#endif
}