using UnityEngine;

public class Assignnumberofunits : MonoBehaviour {
    [SerializeField]
    bool PlayersArmy;
    void Start() {
        if (NumberOfUnits.PlayerQuantityOfUnits == 0) return;
        Vector3 startPosition = transform.GetChild(0).position - NumberOfUnits.EnemyQuantityOfUnits / 2 * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;
        transform.GetChild(0).position = startPosition;
        if (PlayersArmy) {
            for (int i = 1; i < NumberOfUnits.PlayerQuantityOfUnits; i++) {
                GameObject a = Instantiate(transform.GetChild(0).gameObject);
                a.transform.parent = transform;
                a.transform.position = startPosition + i * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;
            }
        }
        else {
            for (int i = 1; i < NumberOfUnits.EnemyQuantityOfUnits; i++) {
                GameObject a = Instantiate(transform.GetChild(0).gameObject);
                a.transform.parent = transform;
                a.transform.position = startPosition + i * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;
            }
        }
    }

    void Update() {

    }
}
