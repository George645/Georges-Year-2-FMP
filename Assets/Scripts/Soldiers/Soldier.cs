using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Soldier : MonoBehaviour {
    public Unit unit;
    int siblingIndex = -1;
    int SiblingIndex {
        get {
            if (siblingIndex != -1) {
                return siblingIndex;
            }
            else {
                siblingIndex = transform.GetSiblingIndex();
                return siblingIndex;
            }
        }
    }
    Animator animator;
    #region Unity functions
    public void SetTarget(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
        moving = true;
    }
    private void Start() {
        rightDirection = transform.right;
        currentPosition = transform.position;
        facingDirection = transform.forward;
        animator = GetComponent<Animator>();
    }
    private void Update() {
        if (isFighting)
            RunCombatLoop();
        else
            Movement(targetPosition);
            Movement(targetPosition);
    }
    //Technically a battle function, but also unity function
    private void OnDestroy() {
        if (currentCombat != null)
            currentCombat.DeathOf(this);
        unit.SoldierDeath(transform.GetSiblingIndex());
    }
    #endregion

    #region movement
    bool moving = false;
    internal Vector3 targetPosition;
    Vector3 directionOfMovement;
    [SerializeField]
    int speed = 15;
    [SerializeField]
    int speedOfRotation = 4;
    public int indexInArrays = -1;

    Vector3 rightDirection;
    Vector3 currentPosition;
    Vector3 facingDirection;
    [SerializeField]
    int reach = 5;

    public void Pushed(Vector3 inDirection) {
        transform.position += inDirection / 25 * speed;
        if (moving == false) {
            moving = true;
        }
        else {
            moving = false;
            StartCoroutine(nameof(MoveAfterDelay));
        }
        //moving = true;
    }
    IEnumerator MoveAfterDelay() {
        yield return new WaitForSeconds(0.1f);
        moving = true;
    }
    bool rightStuck = false;
    bool leftStuck = false;
    void Movement(Vector3 targetPos) {
        animator.SetBool("hitting", false);

        if (!moving) return;

        //sets the position to the destination if close enough
        if (Vector3.SqrMagnitude(currentPosition - targetPos) < .01f) {
            transform.position = targetPos;
            currentPosition = targetPos;
            unit.UpdateSoldierPosition(currentPosition, siblingIndex, this);
            if (!IsFacing(-unit.offsetPerRow.normalized)) {
                RotateTowards(-unit.offsetPerRow.normalized);
                return;
            }
            else {
                moving = false;
            }
        }

        //moves towards the destination if possible, if not, tries to move around the unit in front
        if (targetPos != currentPosition) {
            directionOfMovement = (targetPos - currentPosition).normalized;

            if (!IsFacing(directionOfMovement)) {
                RotateTowards(directionOfMovement);
                return;
            }
            if (!unit.SoldierInPosition(currentPosition + directionOfMovement / 100 * speed, SiblingIndex, out Vector3 soldierInPosition) && !ignoreColliders) {
                float dotProduct = Vector3.Dot(rightDirection, soldierInPosition);
                if (dotProduct < 0) {
                    if (rightStuck) {
                        StartCoroutine(nameof(TemporarilyIgnoreColliders));
                    }
                    leftStuck = true;
                    directionOfMovement = rightDirection;
                }
                else if (dotProduct > 0) {
                    if (leftStuck) {
                        StartCoroutine(nameof(TemporarilyIgnoreColliders));
                    }
                    rightStuck = true;
                    directionOfMovement = -rightDirection;
                }
                else {
                    directionOfMovement = Vector3.zero;
                }
            }
            else {
                leftStuck = false;
                rightStuck = false;
            }
            transform.position += directionOfMovement / 100 * speed;
            currentPosition += directionOfMovement / 100 * speed;
            unit.UpdateSoldierPosition(currentPosition, siblingIndex, this);
        }

        if (directionOfMovement.y != 0) Debug.LogWarning("movement direction y should be 0");
    }
    bool ignoreColliders;
    IEnumerator TemporarilyIgnoreColliders() {
        ignoreColliders = true;
        yield return new WaitForSeconds(5);
        ignoreColliders = false;
    }
    /// <summary>
    /// turns towards a direction
    /// </summary>
    /// <param name="direction">The direction to end facing</param>
    /// <returns>Whether or not the soldier is facing the direction</returns>
    bool IsFacing(Vector3 direction) {
        //transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionOfMovement, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
        if (transform.forward == direction.normalized) {
            return true;
        }
        return false;
    }
    void RotateTowards(Vector3 direction) {
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(facingDirection, direction, speedOfRotation * 0.01f, speedOfRotation * 0.01f), Vector3.up);
        rightDirection = transform.right;
        facingDirection = transform.forward;
    }
    #endregion

    #region Combat

    #region Combat stats
    public int attack => unit.attack;
    public int defense => unit.defense;
    public int armour => unit.armour;

    #endregion

    int health = 100;
    public void Damage(int damageQuantity) {
        int scaledDamage = damageQuantity - damageQuantity * armour / 100;
        Debug.Log(scaledDamage);
        health -= scaledDamage;
        if (health < 0) {
            Destroy(gameObject);
        }
    }

    public void Won() {
        unit.numberOfKills++;
        currentCombat = null;
        currentOpponent = null;
        isFighting = false;
        Debug.Log(currentCombat + ", " + currentOpponent + ", " + isFighting + ", " + name);
    }

    SoldierLevelCombat currentCombat;
    public bool isFighting = false;
    Soldier currentOpponent = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentOpponent"></param>
    /// <param name="currentCombat"></param>
    /// <returns> returns true if can be engaged in combat </returns>
    public bool EngageInCombat(Soldier currentOpponent, SoldierLevelCombat currentCombat) {
        if (this.currentOpponent != null) return false;

        this.currentCombat = currentCombat;
        this.currentOpponent = currentOpponent;
        isFighting = true;
        return true;
    }
    public void DisEngage() {
        currentCombat = null;
        currentOpponent = null;
        isFighting = false;
    }
    void RunCombatLoop() {
        animator.SetBool("hitting", false);
        try {
            if ((currentOpponent.transform.position - transform.position).sqrMagnitude > reach * reach) {
                Movement(currentOpponent.transform.position + (currentOpponent.transform.position - transform.position).normalized * 2);
                return;
            }
        }
        catch (System.Exception e){
            Debug.Log(currentCombat + ", " + currentOpponent + ", " + isFighting + ", " + name);
            throw e;
        }
        if (!IsFacing(currentOpponent.transform.position - transform.position)) {
            RotateTowards(currentOpponent.transform.position - transform.position);
            return;
        }
        animator.SetBool("hitting", true);
        currentCombat.RunDamageNumbers();

    }
    #endregion

    #region Draw debugs
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (Selection.activeGameObject != gameObject) return;
        CustomGrid.instance.DisplaySoldierCheckingSquares(this);
        CustomGrid.instance.ColourSoldiersExcluding(this);
        DrawCubeAroundThis(Color.darkRed);
    }
    public void DrawCubeAroundThis(Color color) {
        color.a = 0.3f;
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position + Vector3.up * 2, Vector3.one * 3);
    }
#endif
    #endregion
}
