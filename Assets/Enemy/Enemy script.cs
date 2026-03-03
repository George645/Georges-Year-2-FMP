using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour{
    public CapsuleCollider chasing = null;
    [SerializeField] float health = 100;
    float healingFactor = 0.1f;
    float healingCooldown = 0f;
    int maxHealth;
    public Vector3 target;
    NavMeshAgent agent;
    bool canExitLoop = true;
    public static List<PlayerAndSoldier> list;
    void Start() {
        do {
            transform.position = new Vector3(Random.value * 1000, 20f, Random.value * 1000);
            canExitLoop = true;
            foreach (GameObject location in Player.startingPositions) {
                canExitLoop = ((location.transform.position - transform.position).sqrMagnitude < 10000);
            }
        } while (canExitLoop);
        agent = GetComponent<NavMeshAgent>();
        if (list == null || list.Count == 0) {
            list = new(FindObjectsByType<PlayerAndSoldier>(FindObjectsSortMode.None));
        }
        chasing = list[(int)Random.Range(0, list.Count)].GetComponent<CapsuleCollider>();
        maxHealth = (int)health;
    }
    void Death() {
        if (health <= 0) {
            do {
                transform.position = new Vector3(Random.value * 1000, 20f, Random.value * 1000);
                canExitLoop = true;
                foreach (GameObject location in Player.startingPositions) {
                    canExitLoop = ((location.transform.position - transform.position).sqrMagnitude < 10000);
                }
            } while (canExitLoop);
            health = 100;
        }
    }
    public void TakeDamage(float damage) {
        health -= damage;
        healingCooldown = 10f;
    }
    void Update() {
        if (chasing != null) {
            try {
                target = chasing.transform.position;
            }
            catch { Debug.Log("error in line 36 enemyscript"); }
        }
        else {
            target = this.transform.position;
            if (list == null || list.Count == 0) {
                list = new(FindObjectsByType<PlayerAndSoldier>(FindObjectsSortMode.None));
            }
            chasing = list[(int)Random.Range(0, list.Count)].GetComponent<CapsuleCollider>();
        }
        Death();
    }
    void FixedUpdate() {
        list = new();
        healingCooldown -= Time.deltaTime;
        if (health < maxHealth && healingCooldown < 0f) {
            health += healingFactor;
        }
        else if (health > maxHealth) {
            health = maxHealth;
        }
        if (agent.destination != target) {
            agent.SetDestination(target);
        }
    }
}
