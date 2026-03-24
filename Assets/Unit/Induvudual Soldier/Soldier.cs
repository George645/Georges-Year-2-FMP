using System.Collections;
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
    public void SetTarget(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
        moving = true;
    }
    private void FixedUpdate() {
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
        if (Vector3.SqrMagnitude(transform.position - targetPos) < .01f) {
            transform.position = targetPos;
            if (!RotateTowards(-unit.OffsetPerRow.normalized)) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, -unit.OffsetPerRow.normalized, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
                return;
            }
            else {
                if (transform.position != targetPosition) {
                    Movement(targetPosition);
                }
                moving = false;
            }
        }

        //moves towards the destination if possible, if not, tries to move around the unit in front
        if (targetPos != transform.position) {
            directionOfMovement = (targetPos - transform.position).normalized;

            if (!RotateTowards(directionOfMovement)) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
                return;
            }
            if (!unit.SoldierInPosition(transform.position + directionOfMovement / 100 * speed, SiblingIndex, out Vector3 soldierInPosition, directionOfMovement) && !ignoreColliders) {
                if (Vector3.Dot(transform.right, soldierInPosition) < 0) {
                    if (rightStuck) {
                        StartCoroutine(nameof(TemporarilyIgnoreColliders));
                    }
                    leftStuck = true;
                    directionOfMovement = transform.right;
                }
                else if (Vector3.Dot(transform.right, soldierInPosition) > 0) {
                    rightStuck = true;
                    directionOfMovement = -transform.right;
                }
                else {
                    //unit.Push(SiblingIndex, transform.position + directionOfMovement);
                    //directionOfMovement += transform.right * (2*Mathf.RoundToInt(Random.Range(0, 1) - 1));
                    directionOfMovement = Vector3.zero;
                }
            }
            else {
                leftStuck = false;
                rightStuck = false;
            }
            transform.position += directionOfMovement / 100 * speed;
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
}
