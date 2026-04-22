using System;
using UnityEngine;

[Serializable]
public class PositionAndRotation {
    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public Quaternion rotation;
    public PositionAndRotation(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }
}
