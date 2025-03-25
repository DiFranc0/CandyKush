using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Classe para o menu principal
public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;


    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
