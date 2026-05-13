using GLFunctions;
using UnityEngine;

public class GLnumbersdebugger : MonoBehaviour {
    private void OnDrawGizmos() {
        NumbersTester();
        ArrowTest();
    }
    void NumbersTester() {
        GLNumbers.DisplayNumber(0, new Vector3(0, 0, 18));
        GLNumbers.DisplayNumber(1, new Vector3(0, 0, 15));
        GLNumbers.DisplayNumber(2, new Vector3(0, 0, 12));
        GLNumbers.DisplayNumber(3, new Vector3(0, 0, 9));
        GLNumbers.DisplayNumber(4, new Vector3(0, 0, 6));
        GLNumbers.DisplayNumber(5, new Vector3(0, 0, 3));
        GLNumbers.DisplayNumber(6, new Vector3(0, 0, 0));
        GLNumbers.DisplayNumber(7, new Vector3(0, 0, -3));
        GLNumbers.DisplayNumber(8, new Vector3(0, 0, -6));
        GLNumbers.DisplayNumber(9, new Vector3(0, 0, -9));
        GLNumbers.DisplayNumber(0123456789, new Vector3(0, 0, -12));
        GLNumbers.DisplayNumber(0123456789, new Vector3(0, 0, -15), null, null, .5f);
        GLNumbers.DisplayNumber(0123456789, new Vector3(0, 0, -21), null, null, 2);
    }
    [SerializeField]
    Vector3 arrowStartPosition;
    [SerializeField]
    Vector3 arrowEndPosition;
    [SerializeField]
    Color arrowColor;
    [SerializeField]
    int ArrowHeadSize = 1;
    void ArrowTest() {
        GLshapes.DrawArrow(arrowStartPosition, arrowEndPosition, arrowColor, ArrowHeadSize);
    }
}
