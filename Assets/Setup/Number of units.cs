using TMPro;
using UnityEngine;

public class NumberOfUnits : MonoBehaviour {
    [SerializeField]
    internal GameObject playerNumber;
    [SerializeField]
    internal GameObject enemyNumber;
    static GameObject PlayerNumber;
    static GameObject EnemyNumber;
    public static int PlayerQuantityOfUnits;
    public static int EnemyQuantityOfUnits;
    private void Start() {
        DontDestroyOnLoad(this);
        PlayerNumber = playerNumber;
        EnemyNumber = enemyNumber;
    }

    public static void ChangeEnemyNumber(float amount) {
        EnemyNumber.GetComponent<TMP_Text>().text = amount.ToString();
        EnemyQuantityOfUnits = (int)amount;
    }

    public static void ChangePlayerNumber(float amount) {
        PlayerNumber.GetComponent<TMP_Text>().text = amount.ToString();
        PlayerQuantityOfUnits = (int)amount;
    }
}
