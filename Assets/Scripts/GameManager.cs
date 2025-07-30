using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public GameObject player;


    public void TitleScreen()
    {
        Invoke("LoadTitleScreen", 5f);
    }

    public void LoadTitleScreen()
    {
        SceneManager.LoadScene(0);
    }
}
