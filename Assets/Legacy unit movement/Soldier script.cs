using System;
using UnityEngine;

public class SoldierScript : MonoBehaviour { 
    SoldierMarker sm;
    Soldierattack soldierAttack;
    Vector3 movement;
    public Vector3 endPosition;
    public bool moving = true;
    bool start = true;
    [SerializeField]float health = 100;
    int maxHealth;
    float healingCooldown = 0f;
    float healingFactor = 0.2f;
    Animator anim;
    public enum SoldierState {
        idle = 0,
        moving = 1,
        attacking = 2
    }
    public SoldierState soldierstate = SoldierState.idle;
    private void Start(){
        soldierAttack = transform.GetChild(2).GetComponent<Soldierattack>();
        anim = transform.GetChild(0).GetComponent<Animator>();
        maxHealth = (int)health;
        sm = this.transform.parent.GetChild(1).GetComponent<SoldierMarker>();
        endPosition = sm.transform.position;
    }
    private void Death() {
        if (health < 0) {
            this.transform.parent.parent.GetChild(0).GetComponent<LineScript>().RemoveSoldier(this.transform.parent.GetComponent<SoldierMarker>());
            Destroy(this.transform.parent.gameObject);
        }
    }
    public void TakeDamage(float damage) {
        health -= damage;
        healingCooldown = 10;
    }
    private void Update() {
        Death();
        //need something to run after start but before everything else, so used another variable that only lets it run once
        if (start) {
            transform.position = sm.transform.position;
            endPosition = sm.transform.position;
            start = false;
        }
        if (moving) {
            endPosition = sm.transform.position;
            endPosition.x -= 0.2f;
            endPosition.z -= 0.2f;
            soldierstate = SoldierState.moving;
        }
        else if (soldierAttack.foundEnemy) {
            soldierstate = SoldierState.attacking;
        }
        else {
            soldierstate = SoldierState.idle;
        }
        anim.SetInteger("Which is it", (int)soldierstate);
    }
    void FixedUpdate(){
        healingCooldown -= Time.deltaTime;
        if (health < maxHealth && healingCooldown < 0f) {
            health += healingFactor;
        }
        else if (health > maxHealth) {
            health = maxHealth;
        }
        this.transform.position = new Vector3(transform.position.x, 19f, transform.position.z);
        if (Math.Abs(endPosition.x - transform.position.x) > 0 || Math.Abs(endPosition.y - transform.position.y) > 0) {
            movement = new Vector3(endPosition.x - this.transform.position.x, 0f, endPosition.z - this.transform.position.z);
            if (movement.magnitude > 0.1) {
                movement = movement.normalized /10;
            }
            movement *= Time.deltaTime * 60;
            transform.position += movement;
        }
    }
}