using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MenuController: MonoBehaviour
{
    [Header("Levels To Load")]
    public string GameLevel;
    private string levelToLoad;

    public void AcessLevel()
    {
        SceneManager.LoadScene(GameLevel);
    }


       

}
