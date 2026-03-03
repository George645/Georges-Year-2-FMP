using System.Collections.Generic;
using UnityEngine;

public class Soldierattack : MonoBehaviour
{
    SphereCollider attackRange;
    EnemyScript target;
    [SerializeField] List<Collider> inRange = new();
    Vector3 closest;
    float damage = 10f;
    float counter = 0;
    public bool foundEnemy = false;
    void Start() {
        attackRange = GetComponent<SphereCollider>();
        attackRange.isTrigger = true;
        counter = Random.value * 3;
    }
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<EnemyScript>() != null) {
            target = other.GetComponent<EnemyScript>();
            attackRange.enabled = false;
        }
    }
    void TheForLoop() {
        foreach (Collider enemy in inRange) {
            if (enemy == null) {
                inRange.Remove(enemy);
                TheForLoop();
                break;
            }
            closest = new Vector3(transform.position.x + 100, 19, transform.position.z + 100);
            if ((enemy.transform.position - transform.position).magnitude < (closest - transform.position).magnitude) {
                closest = enemy.transform.position;
                target = enemy.GetComponent<EnemyScript>();
            }

        }
    }
    private void OnTriggerStay(Collider other) {
        if (other.GetComponent<EnemyScript>() != null) {
            if (inRange.Contains(other)) {
                TheForLoop();
            }
            else {
                inRange.Add(other);
            }
        }
    }
    private void FixedUpdate() {
        if (target != null && (target.transform.position - transform.position).magnitude < attackRange.radius) {
            var lookPos = target.transform.position - transform.parent.position;
            lookPos = new Vector3(-lookPos.x, 0, -lookPos.z);
            var rotation = Quaternion.LookRotation(lookPos);
            transform.parent.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);
            if (counter <= 0) {
                target.GetComponent<EnemyScript>().TakeDamage(damage);
                counter = 3;
            }
            else {
                counter -= Time.deltaTime;
            }
            foundEnemy = true;
        }
        else {
            var lookPos = transform.parent.parent.parent.GetChild(0).GetComponent<LineScript>().previousLineColumnDirection;
            lookPos = new Vector3(-lookPos.x, 0, -lookPos.z);
            var rotation = Quaternion.LookRotation(lookPos);
            transform.parent.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);
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
            foundEnemy = false;
        }
    }
}
