using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class ButtonScript : MonoBehaviour{
    public void ResumeGame() {
        Time.timeScale = 1;
        transform.parent.parent.GetComponent<Canvas>().enabled = false;
    }
    public void LeaveGame() {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
        try {
            Destroy(FindAnyObjectByType<CameraScript>());
        }
        catch { }
    }
    public void ChangeScene(string sceneName) {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }
    public void QuitGame() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}