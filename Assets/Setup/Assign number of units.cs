using UnityEngine;

public class Assignnumberofunits : MonoBehaviour {
    [SerializeField]
    bool PlayersArmy;
    [SerializeField, Range(1, 20)]
    int defaultPlayerUnitCount;
    [SerializeField, Range(1, 20)]
    int defaultEnemyUnitCount;
    void Awake() {
        //This can be redone at some point to make it work simpler
        int usedPlayerCount = defaultPlayerUnitCount;
        if (NumberOfUnits.PlayerQuantityOfUnits != 0) usedPlayerCount = NumberOfUnits.PlayerQuantityOfUnits;
        int usedEnemyCount = defaultEnemyUnitCount;
        if (NumberOfUnits.EnemyQuantityOfUnits != 0) usedEnemyCount = NumberOfUnits.EnemyQuantityOfUnits;

        int currentUsedUnitQuantity = PlayersArmy ? usedPlayerCount : usedEnemyCount;

        Vector3 startPosition = transform.GetChild(0).position - currentUsedUnitQuantity / 2 * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;
        
        transform.GetChild(0).position = startPosition;

        if (PlayersArmy) {
            for (int i = 1; i < currentUsedUnitQuantity; i++) {
                GameObject a = Instantiate(transform.GetChild(0).gameObject);
                a.transform.parent = transform;
                a.transform.position = startPosition + i * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;
            }
        }
        else {
            for (int i = 1; i < currentUsedUnitQuantity; i++) {
                GameObject a = Instantiate(transform.GetChild(0).gameObject);
                a.transform.parent = transform;
                a.transform.position = startPosition + i * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;
            }
        }
    }
}
