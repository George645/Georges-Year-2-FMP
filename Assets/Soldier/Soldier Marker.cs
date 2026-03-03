using UnityEngine;

public class SoldierMarker : MonoBehaviour
{
    public SoldierScript stm;
    public LineScript ls;
    public bool marking;
    void Awake(){
        stm = transform.parent.GetChild(0).GetComponent<SoldierScript>();
        ls = transform.parent.parent.GetChild(0).GetComponent<LineScript>();
        ls.AddMarker(this);
    }
    void Update(){
        stm.moving = !marking;
        if (Input.GetKeyDown(KeyCode.Z)) {
            GetComponent<MeshRenderer>().enabled = true;
        }
        else if (Input.GetKey(KeyCode.Z)) {
            GetComponent<MeshRenderer>().enabled = true;
        }
        else if (Input.GetKeyUp(KeyCode.Z)) {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
