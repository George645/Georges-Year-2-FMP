using Unity.Play.Publisher.Editor;
using UnityEngine;

public class SoldierLevelCombat {
    /// <summary>
    /// Defines a combat that takes place between an attacker and a defender
    /// </summary>
    /// <param name="aggressor"> This is the Soldier from the unit that started attacking. </param>
    /// <param name="defender"> This is the defender from the unit that got attacked </param>
    public SoldierLevelCombat(Soldier aggressor, Soldier defender) {
        this.aggressor = aggressor;
        this.defender = defender;
        if (!defender.EngageInCombat(aggressor, this)) {
            return;
        }
        if (!aggressor.EngageInCombat(defender, this)) {
            defender.DisEngage();
        }
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

    public void DeathOf(Soldier soldier) {
        Soldier victor = soldier == aggressor ? defender : aggressor;
        Debug.Log("victor informed " + victor + ", loser defeater " + soldier);
        victor.Won();
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
        Debug.Log("dealt damage " + attackStrength + " vy " + aggressor + " to " + defender);
        (soldierAttacking == aggressor ? defender : aggressor).Damage(Mathf.Max(attackStrength, 0));
    }
}