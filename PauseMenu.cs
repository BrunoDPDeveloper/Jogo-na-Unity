using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;

    void Awake()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        // Garante que o cursor está escondido no início, mesmo antes do Start() do PlayerController
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerHealth.isDead || MachinePrinterGunsBuy.isBuyScreenOpen || ComputerTerminal.isTerminalOpen)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();

            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        if (GameOverManager.Instance != null && GameOverManager.Instance.gameOverPanel.activeInHierarchy)
        {
            // Se o game over panel estiver ativo, desative-o
            GameOverManager.Instance.gameOverPanel.SetActive(false);
        }
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        // --- CÓDIGO PARA ESCONDER O MOUSE ---
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}

