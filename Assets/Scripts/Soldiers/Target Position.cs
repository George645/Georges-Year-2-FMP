using UnityEngine;

public class TargetPosition : MonoBehaviour {
    public Soldier thisSoldier;


    #region Set position
    public void NewPosition(Vector3 position) {
        transform.position = new Vector3(position.x, 19, position.z); 
        thisSoldier.SetTarget(new Vector3(position.x, 20, position.z) + Vector3.up); // if height is implemented, this will need changing
    }
    #endregion

    #region toggleVisibility
    bool enabled = false;
    MeshRenderer meshRenderer;
    private void Start() {
        meshRenderer = transform.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }
    public void Enable() {
        if (enabled) return;
        meshRenderer.enabled = true;
        enabled = true;
    }
    public void Disable() {
        if (!enabled) return;
        meshRenderer.enabled = false;
        enabled = false;
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
