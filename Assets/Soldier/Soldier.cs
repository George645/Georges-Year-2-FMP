using Unity.VisualScripting;
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
    public void SetTarget(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
        moving = true;
    }
    private void FixedUpdate() {
        if (!moving) return;
        if (targetPosition != transform.position) {
            directionOfMovement = (targetPosition - transform.position).normalized;

            if (transform.forward != directionOfMovement) {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
                return;
            }

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
