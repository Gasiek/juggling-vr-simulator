using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CatchCounter : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    public TextMeshProUGUI currentScoreText;
    private int currentScore = 0;

    private void Start()
    {
        currentScore = 0;
        currentScoreText.text = currentScore.ToString() + " / " + simulationManager.GetBallsToProgress();
    }

    public void UpdateCurrentScore()
    {
        if (simulationManager.IsBallGrounded())
        {
            return;
        }
        currentScore++;
        currentScoreText.text = currentScore.ToString() + " / " + simulationManager.GetBallsToProgress();
    }

    public void ResetCurrentScore()
    {
        currentScore = 0;
        currentScoreText.text = currentScore.ToString() + " / " + simulationManager.GetBallsToProgress();
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}
