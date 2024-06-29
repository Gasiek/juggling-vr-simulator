using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CatchCounter : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highestScoreText;
    private int currentScore = 0;
    private int highestScore = 0;

    private void Start()
    {
        currentScore = 0;
        highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        currentScoreText.text = currentScore.ToString();
        highestScoreText.text = highestScore.ToString();
    }

    public void UpdateCurrentScore()
    {
        currentScore++;
        currentScoreText.text = currentScore.ToString();
        if (currentScore > highestScore)
        {
            UpdateHighestScore();
        }
    }

    private void UpdateHighestScore()
    {
        highestScore = currentScore;
        highestScoreText.text = highestScore.ToString();
        PlayerPrefs.SetInt("HighestScore", highestScore);
    }

    public void ResetCurrentScore()
    {
        currentScore = 0;
        currentScoreText.text = currentScore.ToString();
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}
