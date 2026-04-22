using UnityEngine;

public class PanelEnabler : MonoBehaviour{
    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)) {
            transform.GetComponent<Canvas>().enabled = !transform.GetComponent<Canvas>().enabled;
            Time.timeScale = -Time.timeScale + 1;
        }
    }
}
