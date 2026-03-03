using UnityEngine;

public class Destroyonbuttonpress : MonoBehaviour{
    void Update(){
        if (Input.anyKeyDown) {
            Destroy(gameObject);
        }
    }
}