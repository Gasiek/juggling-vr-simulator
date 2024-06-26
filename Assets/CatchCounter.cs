using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CatchCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highestScoreText;
    private int currentScore = 0;
    private int highestScore = 0;

    private void Start()
    {
        currentScoreText.text = currentScore.ToString();
        highestScoreText.text = PlayerPrefs.GetInt("HighestScore", 0).ToString();
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
}
