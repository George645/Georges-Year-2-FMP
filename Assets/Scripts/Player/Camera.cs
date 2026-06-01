using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    const int distanceFromEdgeOfScreenDivider = 20;
    Vector3 unitStartPosition, unitEndPosition;

    List<Unit> currentlySelectedUnits;
    List<List<GameObject>> currentlyManipulatedTargetPositions = new();

    [SerializeField]
    GameObject highlightedTargetPositionParent;

    public static CameraScript instance;
    public static List<GameObject> startingPositions;

    public Vector3 position;

    [SerializeField, Range(0, 200)]
    int backupSensitivity;
    int Sensitivity {
        get {
            return PlayerPrefs.GetInt("Sensitivity", backupSensitivity); //make a Sensitivity slider in the pause menu to handle this: make it a 0-100 slider
        }
        set { }
    }
    #region Unity functions
    private void Start() {
        position = transform.position;
        if (instance == null)
            DontDestroyOnLoad(this.gameObject);
        else
            Destroy(this.gameObject);

        instance = this;
        currentlySelectedUnits = new();
        DisableLastSelection();
    }
    void Update() {
        if (Time.timeScale == 0) return;
        if (Input.GetMouseButtonDown(0)) {
            if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                DisableLastSelection();
            CheckIfClickingOnUnit();
        }
        if (currentlySelectedUnits.Count != 0) {
            if (Input.GetMouseButtonDown(1)) {
                SetStartPosition();
                SetMovementLine();
            }
            if (Input.GetMouseButton(1)) {
                SetEndPosition();
                SetMovementLine();
            }
            if (Input.GetMouseButtonUp(1)) {
                SetEndPosition();
                SetMovementLine();
                SendPositionalDataToUnit();
            }
        }

        //Rotation
        if (CheckMousePositionForRotation() || RotationKeysPressed()) Rotate();

        //Movement
        if (CheckMousePositionForMovement() || MovementKeysPressed()) Movement();

        //vertical movement and rotation (scroll(
        AddOrSubtractScrollAmount();
        AddScrollingDelta();
    }
    #endregion

    #region moving units
    public void DisableLastSelection() {
        if (currentlySelectedUnits.Count != 0) {
            currentlySelectedUnits.ForEach(x => x.selected = false);
            currentlySelectedUnits = new();
        }
        currentlyManipulatedTargetPositions.ForEach(x => x.ForEach(y => y.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false));
        currentlyManipulatedTargetPositions = new();
    }
    void CheckIfClickingOnUnit() {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction * 10000, out RaycastHit hitInfo, 10000)) {
            if (hitInfo.collider.gameObject.name.Contains("Soldier") && hitInfo.collider.gameObject.GetComponent<Soldier>().unit.playersUnit) {
                Select(hitInfo.collider.transform.parent.GetComponent<Unit>());
            }
        }
    }

    public void Select(Unit unit) {
        if (currentlySelectedUnits.Contains(unit)) throw new NotImplementedException("Not implemented removing a unit from the lists");

        currentlySelectedUnits.Add(unit);
        currentlySelectedUnits[^1].selected = true;
        currentlyManipulatedTargetPositions.Add(GetPotentialPositionDiskForUnit(currentlySelectedUnits[^1]));
    }

    List<GameObject> GetPotentialPositionDiskForUnit(Unit unit) {
        List<GameObject> returningList = new();

        for (int i = 0; i < unit.NumberOfSoldiers; i++) {
            returningList.Add(highlightedTargetPositionParent.GetComponentsInChildren<MeshRenderer>().Where(x => x.allowOcclusionWhenDynamic == false).ToArray()[0].transform.gameObject); // <- This could be very ineffiecient
            returningList[^1].GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = true;
        }
        return returningList;
    }
    void SetStartPosition() {
        unitStartPosition = ScreenPointToGroundPoint(Input.mousePosition);
    }
    void SetEndPosition() {
        unitEndPosition = ScreenPointToGroundPoint(Input.mousePosition);
    }
    void SetMovementLine() {
        if (Vector3.SqrMagnitude(unitStartPosition - unitEndPosition) < currentlySelectedUnits.Sum(x => x.offsetPerTroop.magnitude) * 3 + currentlySelectedUnits.Sum(x => x.offsetPerTroop.magnitude) * 0.5f) { // <- This is going to be veeery inefficient
            foreach (List<GameObject> innerList in currentlyManipulatedTargetPositions.Where(x => x[^1].GetComponent<MeshRenderer>().enabled)) {
                ToggleMeshRenderers(false, innerList);
            }
            Vector3 oldCenter = new(currentlySelectedUnits.Sum(x => x.TargetPositionBoundingBox.Center.x) / currentlySelectedUnits.Count(), 21, currentlySelectedUnits.Sum(x => x.TargetPositionBoundingBox.Center.z) / currentlySelectedUnits.Count());
            Vector3 newCenter = unitStartPosition;
            Vector3 offset = newCenter - oldCenter;

            for (int i = 0; i < currentlyManipulatedTargetPositions.Count; i++) {
                List<GameObject> unitDisks = currentlyManipulatedTargetPositions[i];
                Vector3 oldMidPoint = currentlySelectedUnits[i].TargetPositionBoundingBox.Center;
                Vector3 relativeStartingPosition = -currentlySelectedUnits[i].offsetPerRow * (currentlySelectedUnits[i].NumberOfSoldiers / currentlySelectedUnits[i].CurrentWidth) / 2 - currentlySelectedUnits[i].offsetPerTroop * currentlySelectedUnits[i].CurrentWidth / 2;

                int currentWidth = 0;
                int currentRow = 0;
                for (int j = 0; j < unitDisks.Count; j++) {
                    unitDisks[i].transform.position = oldMidPoint + offset + relativeStartingPosition + currentWidth * currentlySelectedUnits[i].offsetPerTroop + currentRow * currentlySelectedUnits[i].offsetPerRow;
                    currentWidth++;
                    if (currentWidth == currentlySelectedUnits[i].CurrentWidth) {
                        currentWidth = 0;
                        currentRow++;
                    }
                }
            }
        }
        else {
            foreach (List<GameObject> innerList in currentlyManipulatedTargetPositions.Where(x => x[^1].GetComponent<MeshRenderer>().enabled == false)) {
                ToggleMeshRenderers(true, innerList);
            }

            Vector3 startingPosition = unitStartPosition;

            Vector3 distanceBetweenStartAndEnd = unitEndPosition - startingPosition;
            //Something needs to be added in here to make it so that the distance between start and end can't scale up infinitely
            Vector3 distanceBetweenStartAndEndWithoutInBetweenGap = distanceBetweenStartAndEnd - ((currentlySelectedUnits.Count - 1) * 5 * distanceBetweenStartAndEnd.normalized);
            Vector3 distancePerUnit = distanceBetweenStartAndEndWithoutInBetweenGap / currentlySelectedUnits.Count;
            int currentWidth = 0;
            int currentRow = 0;
            for (int index = 0; index < currentlySelectedUnits.Count; index++) {
                Vector3 soldierOffsetPerTroop = distanceBetweenStartAndEnd.normalized * currentlySelectedUnits[index].offsetPerTroop.magnitude;
                Vector3 soldierOffsetPerRow = new (soldierOffsetPerTroop.z, soldierOffsetPerTroop.y, -soldierOffsetPerTroop.x);
                currentlySelectedUnits[index].potentialOffsetPerRow = soldierOffsetPerRow;
                currentlySelectedUnits[index].potentialOffsetPerTroop = soldierOffsetPerTroop;

                currentWidth = 0;
                currentRow = 0;
                for (int j = 0; j < currentlyManipulatedTargetPositions[index].Count; j++) {
                    currentlyManipulatedTargetPositions[index][j].transform.position = startingPosition + (distancePerUnit + distanceBetweenStartAndEnd.normalized * 5) * index + currentWidth * soldierOffsetPerTroop + currentRow * soldierOffsetPerRow;
                    currentWidth++;
                    if ((currentWidth * soldierOffsetPerTroop).sqrMagnitude > (startingPosition - (startingPosition + distancePerUnit + distanceBetweenStartAndEnd.normalized / 2)).sqrMagnitude) {
                        currentlySelectedUnits[index].potentialNextWidth = currentWidth - 1;
                        currentRow++;
                        currentWidth = 0;
                    }
                }
            }
        }
    }
    void ToggleMeshRenderers(bool isEnabled, List<GameObject> meshRenderers) {
        for (int i = 0; i < meshRenderers.Count; i++) {
            meshRenderers[i].GetComponent<MeshRenderer>().enabled = isEnabled;
        }
    }
    void SendPositionalDataToUnit() {
        foreach (List<GameObject> innerList in currentlyManipulatedTargetPositions.Where(x => x[^1].GetComponent<MeshRenderer>().enabled)) {
            ToggleMeshRenderers(false, innerList);
        }
        for (int i = 0; i < currentlySelectedUnits.Count(); i++) {
            currentlySelectedUnits[i].ApplyPotentials();
            List<Vector3> ListOfPositions = new();
            foreach (GameObject thing in currentlyManipulatedTargetPositions[i]) {
                ListOfPositions.Add(thing.transform.position);
            }
            Debug.Log("positional data sent");
            currentlySelectedUnits[i].NewPositions(ListOfPositions);
            currentlySelectedUnits[i].MovedByPlayer();
        }
    }

    public void RemoveSoldier(Unit unit) {
        int indexOfUnit = currentlySelectedUnits.FindIndex(x => x == unit);
        currentlyManipulatedTargetPositions[indexOfUnit].RemoveAt(0);
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
        position += movement / 100;
    }
    #endregion

    #region scrolling

    float scrolledDelta = 1;
    [SerializeField]
    float scrolledPosition = 0;
    void AddOrSubtractScrollAmount() {
        scrolledDelta += Input.mouseScrollDelta.y;
    }

    const float minValue = 0;
    const float maxValue = 90;
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

    const float degreesToRadians = 0.01745328f;
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
        transform.SetLocalPositionAndRotation(Vector3.Lerp(priorPosition, multiplier * new Vector3(0, -Mathf.Sin(scrolledPosition * degreesToRadians) * positionYMultiplier + positionYAddition, -Mathf.Sin(scrolledPosition * degreesToRadians) * positionZMultiplier + positionZAddition), Math.Clamp(Time.time - time, 0, 1)), Quaternion.Lerp(priorRotation, new Quaternion(0, 180, Mathf.Sin(scrolledPosition * degreesToRadians) * rotationMultiplier, priorRotation.w), Math.Clamp((Time.time - time) / 50, 0, 1)));
    }
    #endregion
}