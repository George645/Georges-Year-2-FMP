using UnityEngine;

public class Highlightedtargetpositions : MonoBehaviour {
    [SerializeField]
    GameObject instanciatingObject;
    public void CreateHighlightedPosition() {
        Instantiate(instanciatingObject).transform.parent = transform;
        transform.GetChild(transform.childCount - 1).name = transform.GetChild(transform.childCount - 1).name + (transform.childCount);
        transform.GetChild(transform.childCount - 1).GetComponent<MeshRenderer>().enabled = false;
    }
    public void RemoveHighlightedPosition() {
        Destroy(transform.GetChild(0).gameObject);
    }
}
