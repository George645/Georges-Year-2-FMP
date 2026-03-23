using System.Collections;
using UnityEngine;

public class Soldier : MonoBehaviour {
    public Unit unit;

    #region movement
    bool moving = false;
    internal Vector3 targetPosition;
    Vector3 directionOfMovement;
    [SerializeField]
    int speed = 10;
    [SerializeField]
    int speedOfRotation = 3;
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
        Movement(targetPosition, false);
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
    bool canMove;
    void Movement(Vector3 targetPos, bool ignoreRotation) {
        if (!moving) return;
        if (targetPos != transform.position) {
            directionOfMovement = (targetPos - transform.position).normalized;

            if (!ignoreRotation && !RotateTowards(directionOfMovement)) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
                return;
            }
            //if (!(Vector3.SqrMagnitude(transform.position - targetPos) < 10)) {
            //    if (!unit.SoldierInPosition(transform.position + directionOfMovement / 100 * speed, SiblingIndex, out Vector3 soldierInPosition, directionOfMovement)) {
            //        if (Vector3.Dot(transform.right, soldierInPosition) < 0) {
            //            directionOfMovement = transform.right;
            //        }
            //        else if (Vector3.Dot(transform.right, soldierInPosition) > 0) {
            //            directionOfMovement = -transform.right;
            //        }
            //        else {
            //            //unit.Push(SiblingIndex, transform.position + directionOfMovement);
            //            //directionOfMovement += transform.right * (2*Mathf.RoundToInt(Random.Range(0, 1) - 1));
            //            directionOfMovement = Vector3.zero;
            //        }
            //    }
            //}
            //else {
            //    Debug.Log("hi");
            //}
            transform.position += directionOfMovement / 100 * speed;
        }

        if (Vector3.SqrMagnitude(transform.position - targetPos) < .01f) {
            transform.position = targetPos;
            if (!ignoreRotation && !RotateTowards(-unit.OffsetPerRow.normalized)) {
                return;
            }
            else {
                if (transform.position != targetPosition) {
                    Movement(targetPosition, false);
                }
                moving = false;
            }
        }
        if (directionOfMovement.y != 0) Debug.LogWarning("movement direction y should be 0");
    }
    /// <summary>
    /// turns towards a direction
    /// </summary>
    /// <param name="direction">The direction to end facing</param>
    /// <returns>Whether or not the soldier is facing the direction</returns>
    bool RotateTowards(Vector3 direction) {
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
        if (transform.forward == direction) {
            return true;
        }
        return false;
    }
    #endregion
}
