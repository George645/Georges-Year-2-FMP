using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Soldier : MonoBehaviour {
    public Unit unit;

    #region movement
    bool moving = false;
    internal Vector3 targetPosition;
    Vector3 directionOfMovement;
    [SerializeField]
    int speed = 15;
    [SerializeField]
    int speedOfRotation = 4;
    int siblingIndex = -1;
    public int indexInArrays = -1;
    int SiblingIndex {
        get {
            if (siblingIndex != -1) {
                return siblingIndex;
            }
            else {
                siblingIndex = transform.GetSiblingIndex();
                return siblingIndex;
            }
        }
    }
    Vector3 rightDirection;
    Vector3 currentPosition;
    Vector3 facingDirection;
    public void SetTarget(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
        moving = true;
    }
    private void Start() {
        rightDirection = transform.right;
        currentPosition = transform.position;
        facingDirection = transform.forward;
    }
    private void Update() {
        Movement(targetPosition);
        
    }
    public void Pushed(Vector3 inDirection) {
        transform.position += inDirection / 25 * speed;
        if (moving == false) {
            moving = true;
        }
        else {
            moving = false;
            StartCoroutine(nameof(MoveAfterDelay));
        }
        //moving = true;
    }
    IEnumerator MoveAfterDelay() {
        yield return new WaitForSeconds(0.1f);
        moving = true;
    }
    bool rightStuck = false;
    bool leftStuck = false;
    void Movement(Vector3 targetPos) {

        if (!moving) return;

        //sets the position to the destination if close enough
        if (Vector3.SqrMagnitude(currentPosition - targetPos) < .01f) {
            transform.position = targetPos;
            currentPosition = targetPos;
            unit.UpdateSoldierPosition(currentPosition, siblingIndex, this);
            if (!RotateTowards(-unit.OffsetPerRow.normalized)) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(facingDirection, -unit.OffsetPerRow.normalized, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
                rightDirection = transform.right;
                facingDirection = transform.forward;
                return;
            }
            else {
                moving = false;
            }
        }

        //moves towards the destination if possible, if not, tries to move around the unit in front
        if (targetPos != currentPosition) {
            directionOfMovement = (targetPos - currentPosition).normalized;

            if (!RotateTowards(directionOfMovement)) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(facingDirection, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
                rightDirection = transform.right;
                facingDirection = transform.forward;
                return;
            }
            if (!unit.SoldierInPosition(currentPosition + directionOfMovement / 100 * speed, SiblingIndex, out Vector3 soldierInPosition) && !ignoreColliders) {
                float dotProduct = Vector3.Dot(rightDirection, soldierInPosition);
                if (dotProduct < 0) {
                    if (rightStuck) {
                        StartCoroutine(nameof(TemporarilyIgnoreColliders));
                    }
                    leftStuck = true;
                    directionOfMovement = rightDirection;
                }
                else if (dotProduct > 0) {
                    if (leftStuck) {
                        StartCoroutine(nameof(TemporarilyIgnoreColliders));
                    }
                    rightStuck = true;
                    directionOfMovement = -rightDirection;
                }
                else {
                    directionOfMovement = Vector3.zero;
                }
            }
            else {
                leftStuck = false;
                rightStuck = false;
            }
            transform.position += directionOfMovement / 100 * speed;
            currentPosition += directionOfMovement / 100 * speed;
            unit.UpdateSoldierPosition(currentPosition, siblingIndex, this);
        }

        if (directionOfMovement.y != 0) Debug.LogWarning("movement direction y should be 0");
    }
    bool ignoreColliders;
    IEnumerator TemporarilyIgnoreColliders() {
        ignoreColliders = true;
        yield return new WaitForSeconds(5);
        ignoreColliders = false;
    }
    /// <summary>
    /// turns towards a direction
    /// </summary>
    /// <param name="direction">The direction to end facing</param>
    /// <returns>Whether or not the soldier is facing the direction</returns>
    bool RotateTowards(Vector3 direction) {
        //transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
        if (transform.forward == direction) {
            return true;
        }
        return false;
    }
    #endregion

    #region Draw debugs
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (Selection.activeGameObject != gameObject) return;
        CustomGrid.instance.DisplaySoldierCheckingSquares(this);
        CustomGrid.instance.ColourSoldiersExcluding(this);
        DrawCapsuleAroundThis(Color.darkRed);
    }
    public void DrawCapsuleAroundThis(Color color) {
        color.a = 0.3f;
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position + Vector3.up * 2, Vector3.one * 3);
    }
#endif
    #endregion
}
