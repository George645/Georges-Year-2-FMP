using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemysAttack : MonoBehaviour{
    SphereCollider attackRange;
    PlayerAndSoldier target;
    EnemyScript thisEnemy;
    [SerializeField]List<Collider> inRange = new();
    Vector3 closest;
    float damage = 2f;
    float counter = 0;
    void Start(){
        attackRange = GetComponent<SphereCollider>();
        attackRange.isTrigger = true;
        counter = Random.value * 3;
        thisEnemy = this.transform.parent.GetComponent<EnemyScript>();
    }
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<PlayerAndSoldier>() != null) {
            target = other.GetComponent<PlayerAndSoldier>();
            attackRange.enabled = false;
            thisEnemy.GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
        }
    }
    void TheForLoop() {
        foreach (Collider friendly in inRange) {
            if (friendly == null) {
                inRange.Remove(friendly);
                TheForLoop();
                break;
            }
            closest = new Vector3(transform.position.x + 100, 19, transform.position.z + 100);
            if ((friendly.transform.position - transform.position).magnitude < (closest - transform.position).magnitude) {
                closest = friendly.transform.position;
                target = friendly.GetComponent<PlayerAndSoldier>();
                thisEnemy.GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
            }

        }
    }
    private void OnTriggerStay(Collider other) {
        if (other.GetComponent<PlayerAndSoldier>() != null) {
            if (inRange.Contains(other)) {
                TheForLoop() ;
            }
            else {
                inRange.Add(other);
            }
        }
    }
    private void FixedUpdate() {
        if (target != null && (target.transform.position - transform.position).magnitude < attackRange.radius) {
            if (counter <= 0) {
                Debug.Log("deal damage");
                if (target.GetComponent<SoldierScript>() != null) {
                    target.GetComponent<SoldierScript>().TakeDamage(damage);
                }
                else if (target.GetComponent<Player>() != null) {
                    target.GetComponent<Player>().TakeDamage(damage);
                }
                else if (target.GetComponent<SoldierSpawner>() != null) {
                    target.GetComponent<SoldierSpawner>().TakeDamage(damage);
                }
                counter = 3;
            }
            else {
                counter -= Time.deltaTime;
            }
        }
        else {
            if (counter == -1) {
                attackRange.enabled = true;
                counter = 1;
            }
            if (counter <= 0) {
                attackRange.enabled = true;
                counter = -1;
            }
            else {
                attackRange.enabled = false;
                counter -= Time.deltaTime;
            }
        }
    }
}