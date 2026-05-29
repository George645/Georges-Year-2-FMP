using UnityEngine;

public class TargetPosition : MonoBehaviour {
    public Soldier thisSoldier;


    #region Set position
    public void NewPosition(Vector3 position) {
        transform.position = new Vector3(position.x, 20, position.z); 
        thisSoldier.SetTarget(new Vector3(position.x, 22, position.z) + Vector3.up); // if height is implemented, this will need changing
    }
    #endregion

    #region toggleVisibility
    bool visible = false;
    MeshRenderer meshRenderer;
    private void Start() {
        meshRenderer = transform.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }
    public void Enable() {
        if (visible) return;
        meshRenderer.enabled = true;
        visible = true;
    }
    public void Disable() {
        if (!visible) return;
        meshRenderer.enabled = false;
        visible = false;
    }
    #endregion

    #region Setup
    private void OnEnable() {
        transform.localScale = new Vector3(1, 0.1f, 1);
    }
    public void InstantSetPosition(Vector3 position) {
        transform.localPosition = position;
        thisSoldier.transform.position = transform.position + Vector3.up * 2;
        thisSoldier.targetPosition = transform.position + Vector3.up * 2;
        transform.parent.GetComponent<Unit>().SetNewPositionOfSoldier(thisSoldier.unitIndex, transform.position + Vector3.up);
    }
    #endregion
}
