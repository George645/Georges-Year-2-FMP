using UnityEngine;

public class TargetPosition : MonoBehaviour {
    public Soldier thisSoldier;


    #region Set position
    public void NewPosition(Vector3 position) {
        transform.position = new Vector3(transform.parent.InverseTransformPoint(position).x, transform.parent.TransformPoint(Vector3.zero).y, transform.parent.InverseTransformPoint(position).z); //for some reason, we are having to clamp the y component of this.
        thisSoldier.SetTarget(new Vector3(position.x, transform.position.y, position.z) + Vector3.up);
    }
    #endregion

    #region Setup
    private void OnEnable() {
        transform.localScale = new Vector3(1, 0.1f, 1);
    }
    public void InstantSetPosition(Vector3 position) {
        transform.localPosition = position;
        thisSoldier.transform.position = transform.position + Vector3.up;
        thisSoldier.targetPosition = transform.position + Vector3.up;
        transform.parent.GetComponent<Unit>().SetNewPositionOfSoldier(transform.GetSiblingIndex(), transform.position + Vector3.up);
    }
    #endregion
}
