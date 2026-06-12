using System.Threading;
using UnityEngine;

public class AssignUnitNumber : MonoBehaviour {
    public static AssignUnitNumber instance;
    Unit[] units;
    void Start() {
        AssignInstance();
        AssignUnitNumbers();
    }

    public void AssignInstance() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void AssignUnitNumbers() {
        if (units == null || units.Length == 0)
            return;
        units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        int count = 0;
        foreach (Unit unit in units) {
            unit.unitNumber = count;
            count++;
        }
    }

    public Unit GetUnit(int unitNumber) {
        return units[unitNumber];
    }

    /// <summary>
    /// <para>[0, 0] == [0] = target position x       </para>
    /// <para>[1, 0] == [1] = target position z       </para>
    /// <para>[2, 0] == [2] = custom grid index       </para>
    /// <para>[3. 0] == [3] = unit number             </para>
    /// <para>[0, 1] == [4] = current position x      </para>
    /// <para>[1, 1] == [5] = current position y      </para>
    /// <para>[2, 1] == [6] = current position z      </para>
    /// <para>[3. 1] == [7] = soldier number          </para>
    /// <para>[0, 2] == [8] =  current velocity x     </para>
    /// <para>[1, 2] == [9] =  current velocity y     </para>
    /// <para>[2, 2] == [10] = current velocity z     </para>
    /// <para>[3. 2] == [11] = opponent unit number   </para>
    /// <para>[0, 3] == [12] = opponent position x    </para>
    /// <para>[1, 3] == [13] = opponent position y    </para>
    /// <para>[2, 3] == [14] = opponent position z    </para>
    /// <para>[3. 3] == [15] = opponent soldier number</para>
    /// </summary>
    public Matrix4x4 GetSoldier(int unitNumber, int soldierNumber) {
        return GetUnit(unitNumber).GetSoldier(soldierNumber);
    }

    public Vector3 GetPositionOfSoldier(int unitNumber, int soldierNumber) {
        return GetUnit(unitNumber).GetPosition(soldierNumber);
    }
}
