using UnityEngine;
using UnityEngine.UI;

public class PurpleCustomButtonScript : MonoBehaviour {
    public Unit Unit { get { return unit; } set { unit = value; } }
    Unit unit;

    public PurpleCustomButtonScript(Unit unit) {
        this.unit = unit;
    }

    public void OnButtonClicked() {
        if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            CameraScript.instance.DisableLastSelection();
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
