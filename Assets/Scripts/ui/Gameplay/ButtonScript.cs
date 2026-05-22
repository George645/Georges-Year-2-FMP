using UnityEngine;
using UnityEngine.UI;

public class PurpleCustomButtonScript : MonoBehaviour {
    public Unit Unit { get { return unit; } set { unit = value; } }
    Unit unit;

    public PurpleCustomButtonScript(Unit unit) {
        this.unit = unit;
    }

    private void OnMouseDown() {
        CameraScript.instance.Select(unit);
    }

    private void Update() {
        if (unit.selected) {
            GetComponent<Image>().color = Color.orange;
        }
        else {
            GetComponent<Image>().color = Color.white;
        }
    }
}
