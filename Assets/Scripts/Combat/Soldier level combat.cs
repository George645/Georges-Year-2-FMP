using UnityEngine;
using System;

public class SoldierLevelCombat {
    /// <summary>
    /// Defines a combat that takes place between an attacker and a defender
    /// </summary>
    /// <param name="aggressor"> This is the Soldier from the unit that started attacking. </param>
    /// <param name="defender"> This is the defender from the unit that got attacked </param>
    public SoldierLevelCombat(Soldier aggressor, Soldier defender) {
        this.aggressor = aggressor;
        this.defender = defender;
        throw new Exception("Make it so this sends a reference to both units");
    }
    Soldier aggressor;
    Soldier defender;
    float lastDamage;
    float delayBeforeDamage;
    const float shortestTimeBetweenCombats = .5f;
    const float longestTimeBetweenCombats = 10;
    public void NewAggressor(Soldier soldier) {
        if (defender == soldier) {
            Soldier tempSoldier = defender;
            defender = aggressor;
            aggressor = tempSoldier;
        }
    }
    public void RunDamageNumbers() {
        if (Time.time < lastDamage + delayBeforeDamage)
            return;

        LaunchAttack(aggressor);
        
        lastDamage = Time.time;
        delayBeforeDamage = Random.Range(shortestTimeBetweenCombats, longestTimeBetweenCombats);
    }
    void LaunchAttack(Soldier soldierAttacking) {
        int defenseStat = soldierAttacking == aggressor ? defender.defense : aggressor.defense;
        int randomValue = Random.Range(0, soldierAttacking.attack + defenseStat);
        if (randomValue < defenseStat + .5)
            LaunchAttack(soldierAttacking == aggressor ? defender : aggressor);
        int attackStrength = randomValue - defenseStat;
        (soldierAttacking == aggressor ? defender : aggressor).Damage(attackStrength);
    }
}