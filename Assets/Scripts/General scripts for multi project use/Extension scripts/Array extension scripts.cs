using System;
using UnityEngine;

public static class ArrayExtensionScripts {
    /// <summary>
    /// Removes a variable from an array - highly inefficient, use sparingly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="index">The index you want removed</param>
    /// <returns></returns>
    public static T[] RemoveAt<T>(this T[] source, int index) {
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }
}
