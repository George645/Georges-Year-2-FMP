//using UnityEngine;

//public class SoldierLevelCombat {
//    /// <summary>
//    /// Defines a combat that takes place between an attacker and a defender
//    /// </summary>
//    /// <param name="aggressor"> This is the Soldier from the unit that started attacking. </param>
//    /// <param name="defender"> This is the defender from the unit that got attacked </param>
//    public SoldierLevelCombat((int, int) aggressor, (int, int) defender) {
//        this.aggressor = aggressor;
//        this.defender = defender;
//        if (!defenderUnit.SoldierIsEngagedInCombat(aggressor, this)) {
//            return;
//        }
//        if (!aggressorUnit.SoldierIsEngagedInCombat(defender, this)) {
//            defender.DisEngage();
//        }
//    }

//    (int, int) aggressor;
//    Unit aggressorUnit {
//        get { return AssignUnitNumber.instance.GetUnit(aggressor.Item1); }
//    }

//    (int, int) defender;
//    Unit defenderUnit {
//        get { return AssignUnitNumber.instance.GetUnit(defender.Item1); }
//    }

//    float lastDamage;
//    float delayBeforeDamage;
//    const float shortestTimeBetweenCombats = .5f;
//    const float longestTimeBetweenCombats = 3;
//    public void NewAggressor((int, int) soldier) {
//        if (defender == soldier) {
//            (aggressor, defender) = (defender, aggressor);
//        }
//    }

//    public void DeathOf((int, int) soldier) {
//        (int, int) victor = soldier == aggressor ? defender : aggressor;

//        victor.Won();
//        lastDamage = Time.time;
//        delayBeforeDamage = 1000000;
//    }

//    public void RunDamageNumbers() {
//        if (defender == null || aggressor == null)
//            (defender == null ? aggressor : defender).Won();
//        if (Time.time < lastDamage + delayBeforeDamage)
//            return;
        
//        LaunchAttack(aggressor);

//        lastDamage = Time.time;
//        delayBeforeDamage = Random.Range(shortestTimeBetweenCombats, longestTimeBetweenCombats);
//    }
//    void LaunchAttack((int, int) soldierAttacking) {
//        int defenseStat = soldierAttacking == aggressor ? defender.Defense : aggressor.Defense;
//        int randomValue = Random.Range(0, soldierAttacking.Attack + defenseStat);
//        if (randomValue < defenseStat + .5)
//            LaunchAttack(soldierAttacking == aggressor ? defender : aggressor);
//        else {
//            int attackStrength = randomValue - defenseStat;
//            (soldierAttacking == aggressor ? defender : aggressor).Damage(Mathf.Max(attackStrength, 0));
//        }
//    }
//}