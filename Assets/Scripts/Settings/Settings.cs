using System;
using UnityEngine;

public class CustomSettings : MonoBehaviour {
    public static CustomSettings instance;
    public int unitSize;


    void Start() {
        if (instance == null || instance == this)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        InitializeVariables();
    }

    public static void AssignInstance() {
        if (instance == null)
            instance = FindFirstObjectByType<CustomSettings>();
        instance.InitializeVariables();
    }

    void InitializeVariables() {
        unitSize = ((PlayerPrefs.GetInt("unit Size", 180)));

    }

    public void ChangeUnitSize(int newSize) {
        newSize = newSize switch {
            0 => 60,
            1 => 90,
            2 => 120,
            3 => 180,
            _ => throw new IndexOutOfRangeException("int parsed was " + newSize + " when there was only 4 options"),
        };
        unitSize = newSize;
        PlayerPrefs.SetInt("unit Size", newSize);
    }
}
