using UnityEngine;
using UnityEngine.SceneManagement;

public class AssignNumberOfUnits : MonoBehaviour {
    [SerializeField]
    bool PlayersArmy;
    [SerializeField, Range(1, 20)]
    public int defaultPlayerUnitCount;
    [SerializeField, Range(1, 20)]
    public int defaultEnemyUnitCount;
    void Awake() {
        //This can be redone at some point to make it work simpler
        int usedPlayerCount = defaultPlayerUnitCount;
        if (NumberOfUnits.PlayerQuantityOfUnits != 0) usedPlayerCount = NumberOfUnits.PlayerQuantityOfUnits;
        int usedEnemyCount = defaultEnemyUnitCount;
        if (NumberOfUnits.EnemyQuantityOfUnits != 0) usedEnemyCount = NumberOfUnits.EnemyQuantityOfUnits;

        int currentUsedUnitQuantity = PlayersArmy ? usedPlayerCount : usedEnemyCount;

        Vector3 startPosition = transform.GetChild(0).position - ((currentUsedUnitQuantity < 5) ? currentUsedUnitQuantity : 5) / 2 * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;

        transform.GetChild(0).position = startPosition;
        Unit unit = transform.GetChild(0).GetComponent<Unit>().GetComponent<Unit>();

        int width = 1;
        int depth = 0;

        for (int i = 1; i < currentUsedUnitQuantity; i++) {
            GameObject a = Instantiate(transform.GetChild(0).gameObject);
            a.transform.parent = transform;
            a.transform.position = startPosition + width * unit.CurrentWidth * unit.offsetPerTroop - depth * unit.NumberOfSoldiers / unit.CurrentWidth * unit.offsetPerRow * 3;
            a.GetComponent<Unit>().playersUnit = PlayersArmy;

            width++;
            if (width == 5) {
                width = 0;
                depth++;
            }
        }
    }
    [SerializeField]
    string battleFinished;
    private void Update() {
        if (transform.childCount == 0) {
            if (PlayersArmy)
                SceneManager.LoadScene(battleFinished);
        }
    }
}
