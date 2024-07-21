using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimulationManager : MonoBehaviour
{
    public BallController[] initialBalls;
    public TutorialAnimationController tutorialAnimationController;
    public TextMeshProUGUI speedText;
    public SimulatorEvent levelPassed;
    public SimulatorEvent flashHappend;
    public Transform[] ballsOriginsOnTable;
    public TutorialStep[] tutorialSteps;
    public HeadsetPositionResetter headsetPositionResetter;
    [SerializeField] private InputActionReference increaseSpeedAction;
    [SerializeField] private InputActionReference decreaseSpeedAction;
    [SerializeField] private TextMeshProUGUI numberOfBallsText;
    [SerializeField] private TextMeshProUGUI numberOfBallsHeldText;
    private int currentNumberOfBallsInGame = 1;
    private bool isBallGrounded = false;
    private bool shouldBallStopAtThePeak = false;
    private float speedMultiplier = 1f;
    private int ballsToProgress = 10;
    private int currentlyHeldBalls = 0;
    private CatchCounter catchCounter;
    private int currentTutorialStep = -1;
    private Vector3 originalGravity;
    private int ballsThrownSincePreviousFlash = 0;
    private List<string> ballsIdQueue = new();

    private void OnEnable()
    {
        increaseSpeedAction.action.performed += OnIncreaseSpeed;
        decreaseSpeedAction.action.performed += OnDecreaseSpeed;

        increaseSpeedAction.action.Enable();
        decreaseSpeedAction.action.Enable();
    }

    private void OnDisable()
    {
        increaseSpeedAction.action.performed -= OnIncreaseSpeed;
        decreaseSpeedAction.action.performed -= OnDecreaseSpeed;

        increaseSpeedAction.action.Disable();
        decreaseSpeedAction.action.Disable();
    }


    private void Awake()
    {
        catchCounter = GetComponent<CatchCounter>();
    }

    void Start()
    {
        originalGravity = Physics.gravity;
        LoadNextTutorialStep();
    }

    private void OnIncreaseSpeed(InputAction.CallbackContext context)
    {
        if (speedMultiplier >= 1.5f)
        {
            return;
        }
        speedMultiplier = MathF.Round(speedMultiplier + 0.1f, 1);
        speedText.text = speedMultiplier.ToString();
        Physics.gravity = new Vector3(0, originalGravity.y * speedMultiplier, 0);
    }

    private void SetSpeedMultiplier(float value)
    {
        speedMultiplier = value;
        speedText.text = speedMultiplier.ToString();
        Physics.gravity = new Vector3(0, originalGravity.y * speedMultiplier, 0);
    }

    private void OnDecreaseSpeed(InputAction.CallbackContext context)
    {
        if (speedMultiplier <= 0.5f)
        {
            return;
        }
        speedMultiplier = MathF.Round(speedMultiplier - 0.1f, 1);
        speedText.text = speedMultiplier.ToString();
        Physics.gravity = new Vector3(0, originalGravity.y * speedMultiplier, 0);
    }

    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }

    public void SpawnBallsOnTable()
    {
        StartCoroutine(DespawnAndSpawnBalls());
    }

    private IEnumerator DespawnAndSpawnBalls()
    {
        DeactivateAllBalls();
        ResetBallsIdQueue();
        ballsThrownSincePreviousFlash = 0;
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < currentNumberOfBallsInGame; i++)
        {
            initialBalls[i].Spawn(ballsOriginsOnTable[i].position);
        }
        numberOfBallsText.text = currentNumberOfBallsInGame.ToString();
    }

    private void DeactivateAllBalls()
    {
        foreach (var ball in initialBalls)
        {
            ball.gameObject.SetActive(false);
        }
    }

    private void CheckProgress() // TODO: This should happen in catch counter
    {
        if (currentlyHeldBalls == currentNumberOfBallsInGame)
        {
            ballsThrownSincePreviousFlash = 0;
            if (catchCounter.GetCurrentScore() >= ballsToProgress)
            {
                levelPassed.Raise();
            }
        }
    }

    public int GetCurrentNumberOfBalls()
    {
        return currentNumberOfBallsInGame;
    }

    public void OnBallGrabbedCorrectly()
    {
        catchCounter.UpdateCurrentScore();
        CheckProgress();
    }

    public void OnBallGrabbed()
    {
        currentlyHeldBalls++;
        numberOfBallsHeldText.text = currentlyHeldBalls.ToString();
        if (currentlyHeldBalls == currentNumberOfBallsInGame)
        {
            flashHappend.Raise();
        }
    }

    public void OnBallReleased()
    {
        currentlyHeldBalls--;
        ballsThrownSincePreviousFlash++;
        numberOfBallsHeldText.text = currentlyHeldBalls.ToString();
    }

    public void RegisterReleasedBall(string releasedBallId)
    {
        ballsIdQueue.RemoveAll(id => id == releasedBallId);
        ballsIdQueue.Add(releasedBallId);
    }

    public void ResetAlreadyThrownBallsCounter()
    {
        ballsThrownSincePreviousFlash = 0;
    }

    public void LoadNextTutorialStep()
    {
        tutorialAnimationController.HideTutorial();
        currentTutorialStep++;
        currentNumberOfBallsInGame = tutorialSteps[currentTutorialStep].numberOfBalls;
        shouldBallStopAtThePeak = tutorialSteps[currentTutorialStep].shouldBallStopAtThePeak;
        SetSpeedMultiplier(tutorialSteps[currentTutorialStep].speedMultiplier);
        if (tutorialSteps[currentTutorialStep].showTutorial)
        {
            tutorialAnimationController.ShowTutorial(isPOV: tutorialSteps[currentTutorialStep].isTutorialPOV);
        }
        SpawnBallsOnTable();
    }

    public void SetBallGrounded(bool value)
    {
        isBallGrounded = value;
    }

    public bool IsBallGrounded()
    {
        return isBallGrounded;
    }

    public string GetPreviouslyThrownBallId()
    {
        return ballsIdQueue.Count > 0 ? ballsIdQueue[^1] : "";
    }

    public bool GetShouldBallStopAtThePeak()
    {
        return shouldBallStopAtThePeak;
    }

    public int GetBallsThrownSincePreviousFlash()
    {
        return ballsThrownSincePreviousFlash;
    }

    public void ResetBallsIdQueue()
    {
        ballsIdQueue.Clear();
    }
}
