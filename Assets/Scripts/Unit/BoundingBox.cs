using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BoundingBox { //Potentially make this a monobehaviour script one day
    List<Vector3> pointsInsideBox;
    public Vector3 UpperBound {
        get { return new Vector3(Mathf.Max(pointsInsideBox.Select(x => x.x).ToArray()), 20, Mathf.Max(pointsInsideBox.Select(x => x.z).ToArray())); }
    }
    public Vector3 LowerBound {
        get { return new Vector3(Mathf.Min(pointsInsideBox.Select(x => x.x).ToArray()), 20, Mathf.Min(pointsInsideBox.Select(x => x.z).ToArray())); }

    }
    public Vector3 Size {
        get { return UpperBound - LowerBound; }
    }
    public Vector3 Extents {
        get { return Size / 2; }
    }
    public Vector3 Center {
        get { return center; }
    }
    Vector3 center;
    public BoundingBox() {
        pointsInsideBox = new();
    }
    public BoundingBox(Vector3 point) {
        pointsInsideBox = new() {
            point
        };
    }
    public void Encapsulate(Vector3 point) {
        center *= pointsInsideBox.Count;
        center += point;
        pointsInsideBox.Add(point);
        center /= pointsInsideBox.Count;
    }
    public void ChangePoint(int index, Vector3 newPos) {
        center *= pointsInsideBox.Count();
        center -= pointsInsideBox[index];
        pointsInsideBox[index] = newPos;
        center += newPos;
        center /= pointsInsideBox.Count();
    }

    public void LogInfo() {
        Debug.Log("Upper bound: " + UpperBound + ", lower bound: " + LowerBound + ", list Size " + pointsInsideBox.Count());
    }
    
    public void DisplayBox() {
        Gizmos.color = UnityEngine.Color.green;
        Gizmos.DrawWireCube(center, Size);
        //Debug.Log(center + ", " + Size);
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
