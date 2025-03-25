using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Classe para gerenciar o UI
public class UIManager : MonoBehaviour
{
    public Board gameBoard;
    public GameObject winScreen;
    public Button restartButton;
    public Button menuButton;
    public ProgressBar progressBar;
    public TMP_Text totalScoreText;
     

    private void Start()
    {
    
    }
    
    private void Update()
    {
        if (progressBar.IsComplete())
        {
            winScreen.SetActive(true);
            totalScoreText.text = $"Total Score: {gameBoard.score}";
        }
    }

    public void RestartGame()
    {
        winScreen.SetActive(false);
        gameBoard.RestartGame();
    }

    public void GoToMenu()
    {
        // Implementar navega��o para o menu principal
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    public void AddProgress(float amount)
    {
        progressBar.AddProgress(amount);
    }
}
