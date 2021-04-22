using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public bool gameActive = false;
    [SerializeField] TextMeshProUGUI objectiveText;
    [SerializeField] TextMeshProUGUI controlsText;
    [SerializeField] Button startButton;
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] Button restartButton;
    [SerializeField] TextMeshProUGUI VictoryText;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        //starts the game and removes the menu text
        gameActive = true;
        objectiveText.gameObject.SetActive(false);
        controlsText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        //stops the running of game objects and adds game over text and button for restart on the screen
        gameActive = false;
        restartButton.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);
    }
    public void RestartGame()
    {
        //reloads the game by reloading the active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Victory()
    {
        //stops the game and shows victory text
        gameActive = false;
        VictoryText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);

    }
}
