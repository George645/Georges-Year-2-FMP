using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatTracker : MonoBehaviour {
    public static StatTracker instance;
    int[] unitKills;
    Unit[] unitReferences;
    void Start() {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(gameObject);
        unitReferences = FindObjectsByType<Unit>(FindObjectsSortMode.None).ToArray();
        unitKills = new int[unitReferences.Length];
    }

    public void AddKill(Unit unit) {
        int editingIndex = unitReferences.ToList().IndexOf(unit);
        unitKills[editingIndex]++;
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
