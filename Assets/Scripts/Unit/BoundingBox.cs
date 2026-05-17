using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoundingBox { //Potentially make this a monobehaviour script one day
    List<Vector3> pointsInsideBox;
    public Vector3 upperBound;
    public Vector3 lowerBound;
    public Vector3 size {
        get { return upperBound - lowerBound; }
    }
    public Vector3 extents {
        get { return size / 2; }
    }
    public Vector3 Center {
        get { return center; }
    }
    Vector3 center;
    public BoundingBox() {
        pointsInsideBox = new();
    }
    public BoundingBox(Vector3 point) {
        pointsInsideBox = new();
        pointsInsideBox.Add(point);
        upperBound = point;
        lowerBound = point;
    }
    public void Encapsulate(Vector3 point) {
        center *= pointsInsideBox.Count;
        center += point;
        checkIfSizeChanged(point);
        pointsInsideBox.Add(point);
        center /= pointsInsideBox.Count;
    }
    public void ChangePoint(int index, Vector3 newPos) {
        center *= pointsInsideBox.Count();
        center -= pointsInsideBox[index];
        if (pointsInsideBox[index].x == lowerBound.x)
            lowerBound.x = newPos.x;
        pointsInsideBox[index] = newPos;
        checkIfSizeChanged(newPos);
        center += newPos;
        center /= pointsInsideBox.Count();
    }

    public void LogInfo() {
        Debug.Log("Upper bound: " + upperBound + ", lower bound: " + lowerBound + ", list size " + pointsInsideBox.Count());
    }
    void checkIfSizeChanged(Vector3 point) {
        if (upperBound == Vector3.zero && lowerBound == Vector3.zero) {
            upperBound = point;
            lowerBound = point;
            return;
        }
        if (point.x > upperBound.x)
            upperBound.x = point.x;
        if (point.y > upperBound.y)
            upperBound.y = point.y;
        if (point.z > upperBound.z)
            upperBound.z = point.z;
        if (point.x < lowerBound.x)
            lowerBound.x = point.x;
        if (point.y < lowerBound.y)
            lowerBound.y = point.y;
        if (point.z < lowerBound.z)
            lowerBound.z = point.z;
    }
    public void DisplayBox() {
        Gizmos.color = UnityEngine.Color.green;
        Gizmos.DrawWireCube(center, size);
        //Debug.Log(center + ", " + size);
        //foreach (Vector3 point in pointsInsideBox) {
        //    Debug.Log(point);
        //}
    }
    public void RemovePoint(int index) {
        center *= pointsInsideBox.Count();
        center -= pointsInsideBox[index];
        pointsInsideBox.RemoveAt(index);
        center /= pointsInsideBox.Count();
    }
}
