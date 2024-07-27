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
    [SerializeField] private InputActionReference nextLevelAction;
    [SerializeField] private InputActionReference previousLevelAction;
    public Transform[] ballsOriginsOnTable;
    public TutorialStep[] tutorialSteps;
    [SerializeField] private TextMeshProUGUI numberOfBallsText;
    [SerializeField] private TextMeshProUGUI numberOfBallsHeldText;
    private int currentNumberOfBallsInGame = 1;
    private bool isBallGrounded = false;
    private bool shouldBallStopAtThePeak = false;
    private bool shouldTrackGaze = false;
    private float currentSpeedMultiplier = 1f;
    private float rootOfCurrentSpeedMultiplier = 1f;
    private int ballsToProgress = 10;
    private int currentlyHeldBalls = 0;
    private CatchCounter catchCounter;
    private int currentTutorialStep = -1;
    private Vector3 originalGravity;
    private int ballsThrownSincePreviousFlash = 0;
    private List<string> ballsIdQueue = new();

    private void OnEnable()
    {
        nextLevelAction.action.performed += _ => LoadNextTutorialStep();
        nextLevelAction.action.Enable();
        previousLevelAction.action.performed += _ => LoadPreviousTutorialStep();
        previousLevelAction.action.Enable();
    }

    private void OnDisable()
    {
        nextLevelAction.action.performed -= _ => LoadNextTutorialStep();
        nextLevelAction.action.Disable();
        previousLevelAction.action.performed -= _ => LoadPreviousTutorialStep();
        previousLevelAction.action.Disable();
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

    private void SetSpeedMultiplier(float value)
    {
        currentSpeedMultiplier = value;
        rootOfCurrentSpeedMultiplier = Mathf.Sqrt(currentSpeedMultiplier);
        speedText.text = rootOfCurrentSpeedMultiplier.ToString();
        Physics.gravity = new Vector3(0, originalGravity.y * currentSpeedMultiplier, 0);
    }

    public float GetRootOfCurrentSpeedMultiplier()
    {
        return rootOfCurrentSpeedMultiplier;
    }

    public void SpawnBallsOnTable()
    {
        StartCoroutine(DespawnAndSpawnBalls());
    }

    private IEnumerator DespawnAndSpawnBalls()
    {
        DeactivateAllBalls();
        yield return new WaitForSeconds(0.5f);
        ResetBallsIdQueue();
        ballsThrownSincePreviousFlash = 0;
        yield return new WaitForSeconds(0.5f);
        // yield return new WaitForEndOfFrame();
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
            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ball.gameObject.SetActive(false);
        }
    }

    public void CheckProgress() // TODO: This should happen in catch counter
    {
        if (currentlyHeldBalls == currentNumberOfBallsInGame)
        {
            ballsThrownSincePreviousFlash = 0;
            if (catchCounter.GetCurrentScore() >= ballsToProgress)
            {
                StartCoroutine(LevelPassedAfterDelay());
            }
        }
    }

    private IEnumerator LevelPassedAfterDelay()
    {
        yield return new WaitForSeconds(1);
        levelPassed.Raise();
    }

    public int GetCurrentNumberOfBalls()
    {
        return currentNumberOfBallsInGame;
    }

    public void OnBallGrabbedCorrectly()
    {
        catchCounter.UpdateCurrentScore();
    }

    public void OnBallGrabbed()
    {
        Debug.Log("Ball grabbed");
        currentlyHeldBalls++;
        numberOfBallsHeldText.text = currentlyHeldBalls.ToString();
        if (currentlyHeldBalls == currentNumberOfBallsInGame)
        {
            flashHappend.Raise();
        }
    }

    public void OnBallReleased()
    {
        Debug.Log("Ball released");
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
        if (currentTutorialStep == tutorialSteps.Length - 1)
        {
            return;
        }
        tutorialAnimationController.HideTutorial();
        currentTutorialStep++;
        shouldTrackGaze = tutorialSteps[currentTutorialStep].trackGaze;
        currentNumberOfBallsInGame = tutorialSteps[currentTutorialStep].numberOfBalls;
        shouldBallStopAtThePeak = tutorialSteps[currentTutorialStep].shouldBallStopAtThePeak;
        SetSpeedMultiplier(tutorialSteps[currentTutorialStep].speedMultiplier);
        if (tutorialSteps[currentTutorialStep].showTutorial)
        {
            tutorialAnimationController.ShowTutorial(isPOV: tutorialSteps[currentTutorialStep].isTutorialPOV);
        }
        SpawnBallsOnTable();
    }

    private void LoadPreviousTutorialStep()
    {
        if (currentTutorialStep == 0)
        {
            return;
        }
        tutorialAnimationController.HideTutorial();
        currentTutorialStep--;
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

    public Vector3 GetOriginalGravity()
    {
        return originalGravity;
    }

    public float GetCurrentSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }

    public int GetBallsToProgress()
    {
        return ballsToProgress;
    }

    public bool GetShouldTrackGaze()
    {
        return shouldTrackGaze;
    }
}
