using TMPro;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class Results : MonoBehaviour {
    void Start() {
        transform.GetChild(0).GetComponent<TMP_Text>().text = StatTracker.instance.victory ? "Victory" : "Defeat";

        transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = "Size: " + (StatTracker.instance.playerUnitQuantity * CustomSettings.instance.unitSize);
        transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = "Size: " + (StatTracker.instance.opponentUnitQuantity * CustomSettings.instance.unitSize);

        GameObject alliedArmy = transform.GetChild(1).gameObject;
        GameObject opposingArmy = transform.GetChild(2).gameObject;

        for (int i = 0; i < StatTracker.instance.playerUnitQuantity; i++) {
            alliedArmy.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = "" + StatTracker.instance.GetKills(true, i);
        }
        for (int i = StatTracker.instance.playerUnitQuantity; i < alliedArmy.transform.childCount; i++) {
            alliedArmy.transform.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < StatTracker.instance.playerUnitQuantity; i++) {
            alliedArmy.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = "" + StatTracker.instance.GetKills(false, i);
        }
        for (int i = StatTracker.instance.opponentUnitQuantity; i < opposingArmy.transform.childCount; i++) {
            opposingArmy.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
