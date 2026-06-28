using UnityEngine;

public static class Vectorextensionscript {
    /// <summary>
    /// rotates a given vector about the positive y axis.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 RightVector(this Vector3 direction) {
        return new Vector3(-direction.z, direction.y, direction.x);
    }
    //public static Vector3 RightVector(this Vector3 direction, Vector3 up) {
    //    return 
    //}
    // maybe come back to this at some point, but it is too complicated at the moment
}
