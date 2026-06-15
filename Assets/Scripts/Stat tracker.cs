using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatTracker : MonoBehaviour {
    public static StatTracker instance;
    int[] playerUnitKills;
    int[] opponentUnitKills;
    Unit[] playerUnitReferences;
    Unit[] opponentUnitReferences;
    public bool victory;

    public int playerUnitQuantity {
        get { return playerUnitReferences.Count(); }
    }
    public int opponentUnitQuantity {
        get { return opponentUnitReferences.Count(); }
    }

    public int Slkdfugheu {
        init { slkdfugheu = value; }
        get { return slkdfugheu; }
    }
    int slkdfugheu = 5;

    void Start() {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(gameObject);
        playerUnitReferences = FindObjectsByType<Unit>(FindObjectsSortMode.None).Where(x => x.playersUnit).ToArray();
        playerUnitKills = new int[playerUnitReferences.Length];
        opponentUnitReferences = FindObjectsByType<Unit>(FindObjectsSortMode.None).Where(x => !x.playersUnit).ToArray();
        opponentUnitKills = new int[opponentUnitReferences.Length];
    }

    public int GetKills(bool Allied, int index) {
        if (Allied)
            return playerUnitKills[index];
        else
            return opponentUnitKills[index];
    }

    public void EndBattle(bool victory) {
        this.victory = victory;
        SceneManager.LoadScene(resultsScene);
    }

    public void AddKill(Unit unit) {
        if (unit.playersUnit) {
            int editingIndex = playerUnitReferences.ToList().IndexOf(unit);
            playerUnitKills[editingIndex]++;
        }
        else if (!unit.playersUnit) {
            int editingIndex = opponentUnitReferences.ToList().IndexOf(unit);
            opponentUnitKills[editingIndex]++;
        }
    }
    [SerializeField]
    string mainBattleScene;
    [SerializeField]
    string resultsScene;
    void Update() {
        if (!(SceneManager.GetActiveScene().name == mainBattleScene || SceneManager.GetActiveScene().name == resultsScene)) {
            instance = null;
            Destroy(gameObject);
        }
    }
}
