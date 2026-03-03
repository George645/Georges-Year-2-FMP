using TMPro;
using UnityEngine;

public class PanelScript : MonoBehaviour{
    Player player;
    RectTransform playerHealth;
    void Start(){
        player = FindAnyObjectByType<Player>();
        playerHealth = transform.GetChild(0).GetComponent<RectTransform>();
    }
    void FixedUpdate(){
        playerHealth.offsetMax = new Vector2(-950 + 400 * player.Health / player.MaxHealth, playerHealth.offsetMax.y);
        transform.GetChild(1).GetComponent<TMP_Text>().text = (int)player.Health + "/" + player.MaxHealth;
    }
}
