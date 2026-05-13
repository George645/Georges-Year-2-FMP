using UnityEngine;

namespace GLFunctions {
    public class GLshapes {
        public static void DrawArrow(Vector3 startPos, Vector3 endPos, Color? colour = null, int arrowHeadSize = 1) {
            Color arrowColour = colour == null ? Color.black : (Color)colour;
            arrowColour.a = 1;
            Gizmos.color = arrowColour;
            Gizmos.DrawLine(startPos, endPos);
            Vector3 lineDirection = (endPos - startPos).normalized;
            Vector3 newLineDirection = new Vector3(-0.707f * lineDirection.x + 0.707f * lineDirection.z, -0.707f * lineDirection.y, -0.707f * lineDirection.x - 0.707f * lineDirection.z);
            Gizmos.DrawLine(endPos, endPos + newLineDirection * arrowHeadSize);
            Vector3 lineDirection2 = (endPos - startPos).normalized;
            Vector3 newLineDirection2 = new Vector3(-0.707f * lineDirection2.x - 0.707f * lineDirection2.z, -0.707f * lineDirection2.y, 0.707f * lineDirection2.x - 0.707f * lineDirection2.z);
            Gizmos.DrawLine(endPos, endPos + newLineDirection2 * arrowHeadSize);
        }
    }
}
