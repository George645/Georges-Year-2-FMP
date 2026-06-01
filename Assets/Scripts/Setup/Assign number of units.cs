using UnityEngine;
using UnityEngine.SceneManagement;

public class AssignNumberOfUnits : MonoBehaviour {
    [SerializeField]
    bool PlayersArmy;
    [Range(1, 20)]
    public int defaultPlayerUnitCount;
    [Range(1, 20)]
    public int defaultEnemyUnitCount;
    void Awake() {
        //This can be redone at some point to make it work simpler
        int usedPlayerCount = defaultPlayerUnitCount;
        if (NumberOfUnits.PlayerQuantityOfUnits != 0) usedPlayerCount = NumberOfUnits.PlayerQuantityOfUnits;
        int usedEnemyCount = defaultEnemyUnitCount;
        if (NumberOfUnits.EnemyQuantityOfUnits != 0) usedEnemyCount = NumberOfUnits.EnemyQuantityOfUnits;

        int currentUsedUnitQuantity = PlayersArmy ? usedPlayerCount : usedEnemyCount;

        CustomSettings.AssignInstance();
        Vector3 startPosition = transform.GetChild(0).position - ((currentUsedUnitQuantity < 5 * 180 / CustomSettings.instance.unitSize) ? currentUsedUnitQuantity : 5 * 180 / CustomSettings.instance.unitSize / 2) / 2 * transform.GetChild(0).GetComponent<Unit>().CurrentWidth * transform.GetChild(0).GetComponent<Unit>().offsetPerTroop;

        transform.GetChild(0).position = startPosition;
        Unit unit = transform.GetChild(0).GetComponent<Unit>().GetComponent<Unit>();
        if (CustomSettings.instance == null)
            CustomSettings.AssignInstance();
        unit.SetSoldierCount(CustomSettings.instance.unitSize);

        int width = 1;
        int depth = 0;

        for (int i = 1; i < currentUsedUnitQuantity; i++) {
            GameObject a = Instantiate(transform.GetChild(0).gameObject);
            a.transform.parent = transform;
            a.transform.position = startPosition + width * unit.CurrentWidth * unit.offsetPerTroop - depth * unit.NumberOfSoldiers / unit.CurrentWidth * unit.offsetPerRow * 3;
            a.GetComponent<Unit>().playersUnit = PlayersArmy;
            a.GetComponent<Unit>().SetSoldierCount(CustomSettings.instance.unitSize);

            width++;
            if (width >= 5 * 180 / CustomSettings.instance.unitSize) {
                width = 0;
                depth++;
            }
        }
    }
    private void Update() {
        if (transform.childCount == 0) {
            if (PlayersArmy)
                StatTracker.instance.EndBattle(false);
            else
                StatTracker.instance.EndBattle(true);
        }
    }
}
