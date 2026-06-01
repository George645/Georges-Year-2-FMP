using System.Collections;
using System.Xml.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Soldier : MonoBehaviour {
    public Unit unit;
    int SiblingIndex {
        get {
            return transform.GetSiblingIndex();
        }
    }
    Animator animator;
    SkinnedMeshRenderer meshRenderer;
    LODGroup lodGroup;

    #region Unity functions
    public void SetTarget(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
        moving = true;
    }
    private void Start() {
        lodGroup = GetComponent<LODGroup>();
        meshRenderer = transform.GetChild(2).GetComponent<SkinnedMeshRenderer>();
        rightDirection = transform.right;
        currentPosition = transform.position;
        facingDirection = transform.forward;
        animator = GetComponent<Animator>();
        targetPosition = transform.position;
    }
    private void FixedUpdate() {

        animator.enabled = meshRenderer.isVisible;
        if (Time.timeScale == 0) return;
        animator.SetBool("hitting", false);
        if (isFighting)
            RunCombatLoop();
        else
            Movement(targetPosition);
    }
    #endregion

    #region movement
    public bool moving = false;
    internal Vector3 targetPosition;
    Vector3 directionOfMovement;
    [SerializeField]
    int speed = 15;
    [SerializeField]
    int speedOfRotation = 4;
    public int customGridIndex = -1;
    public int unitIndex;

    Vector3 rightDirection;
    public Vector3 currentPosition;
    Vector3 facingDirection;
    [SerializeField]
    int reach = 5;

    public void Pushed(Vector3 inDirection) {
        transform.position += inDirection / 25 * speed;
        currentPosition += inDirection / 25 * speed;

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

        if (!moving) return;

        //sets the position to the destination if close enough
        if (Vector3.SqrMagnitude(currentPosition - targetPos) < .01f) {
            transform.position = targetPos;
            currentPosition = targetPos;
            unit.UpdateSoldierPosition(currentPosition, unitIndex, this);
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
            if (directionOfMovement.y != 0) Debug.Log(targetPos - currentPosition);
            if (!IsFacing(directionOfMovement)) {
                RotateTowards(directionOfMovement);
                return;
            }
            if (!unit.SoldierInPosition(currentPosition + directionOfMovement / 100 * speed, unitIndex, out Vector3 soldierInPosition) && !ignoreColliders) {
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
            unit.UpdateSoldierPosition(currentPosition, unitIndex, this);
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
    public int Attack => unit.attack;
    public int Defense => unit.defense;
    public int Armour => unit.armour;

    #endregion

    private void TheDestruction() {
        if (currentCombat != null)
            currentCombat.DeathOf(this);
        else
            Debug.Log("Not in combat");
        unit.SoldierDeath(unitIndex);
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        Destroy(transform.parent.GetChild(transform.GetSiblingIndex() + 1).gameObject);
        Destroy(gameObject);
    }

    int health = 100;
    public void Damage(int damageQuantity) {
        int scaledDamage = damageQuantity - damageQuantity * Armour / 100;
        health -= scaledDamage;
        if (health < 0) {
            TheDestruction();
        }
    }

    public void Won() {
        unit.NumberOfKills++;
        currentCombat = null;
        currentOpponent = null;
        isFighting = false;
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
        if ((currentOpponent.currentPosition - currentPosition).sqrMagnitude > reach * reach) {
            Movement(currentOpponent.currentPosition + (currentOpponent.currentPosition - currentPosition).normalized * 2);
            return;
        }
        if (!IsFacing(currentOpponent.currentPosition - currentPosition)) {
            RotateTowards(currentOpponent.currentPosition - currentPosition);
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
        Gizmos.DrawCube(currentPosition + Vector3.up * 2, Vector3.one * 3);
    }
#endif
    #endregion
}
