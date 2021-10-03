using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    private bool GameStarted = false;
    void Update()
    {
        if (GameStarted)
            return;
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameStarted = true;
            SceneManager.LoadScene("Scenes/GameScene");
        }
    }
}
