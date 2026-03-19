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
        Movement();
    }
    bool canMove;
    void Movement() {
        if (!moving) return;
        if (targetPosition != transform.position) {
            directionOfMovement = (targetPosition - transform.position).normalized;

            if (transform.forward != directionOfMovement) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
                return;
            }
            if (!unit.SetNewPositionOfSoldier(SiblingIndex, transform.position + directionOfMovement / 100 * speed)) {
                if (unit.SetNewPositionOfSoldier(SiblingIndex, directionOfMovement + transform.right / 200 * speed)) {
                    directionOfMovement += transform.right / 2;
                }
                else if (unit.SetNewPositionOfSoldier(SiblingIndex, directionOfMovement - transform.right / 200 * speed)) {
                    directionOfMovement -= transform.right / 2;
                }
                else {
                    unit.Push(SiblingIndex, directionOfMovement);
                    directionOfMovement = Vector3.zero;
                }
            }
            directionOfMovement.Normalize();
            transform.position += directionOfMovement / 100 * speed;
        }

        if (Vector3.SqrMagnitude(transform.position - targetPosition) < .01f) {
            transform.position = targetPosition;
            if (transform.forward != -unit.OffsetPerRow.normalized) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, -unit.OffsetPerRow.normalized, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
            }
            else moving = false;
        }


        if (directionOfMovement.y != 0) Debug.LogWarning("movement direction y should be 0");
    }
    #endregion
}
