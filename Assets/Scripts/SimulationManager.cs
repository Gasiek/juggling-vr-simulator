using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SimulationManager : MonoBehaviour
{
    public BallController[] initialBalls;
    public TutorialAnimationController tutorialAnimationController;
    public TextMeshProUGUI speedText;
    public SimulatorEvent levelPassed;
    public Transform[] ballsOriginsOnTable;
    public TutorialStep[] tutorialSteps;
    [SerializeField] private InputActionReference increaseSpeedAction;
    [SerializeField] private InputActionReference decreaseSpeedAction;
    [SerializeField] private TextMeshProUGUI numberOfBallsText;
    [SerializeField] private TextMeshProUGUI numberOfBallsHeldText;
    private int currentNumberOfBallsInGame = 1;
    private bool isBallGrounded = false;
    private bool shouldBallStopAtThePeak = false;
    private float speedMultiplier = 1f;
    private List<GameObject> balls = new();
    private int ballsToProgress = 10;
    private int currentlyHeldBalls = 0;
    private CatchCounter catchCounter;
    private string previouslyThrownBallId;
    private int currentTutorialStep = -1;
    private Vector3 originalGravity;

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
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < currentNumberOfBallsInGame; i++)
        {
            initialBalls[i].Spawn(ballsOriginsOnTable[i].position);
            balls.Add(initialBalls[i].gameObject);
        }
        numberOfBallsText.text = currentNumberOfBallsInGame.ToString();
    }

    public bool HasNextBallBeenThrown(string currentBallId)
    {
        return balls[-1].transform.GetInstanceID().ToString() == currentBallId;
    }

    private void DeactivateAllBalls()
    {
        foreach (var ball in initialBalls)
        {
            ball.gameObject.SetActive(false);
        }
        balls.Clear();
    }

    private void CheckProgress() // TODO: This should happen in catch counter
    {
        if (currentlyHeldBalls == currentNumberOfBallsInGame)
        {
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
    }

    public void OnBallReleased()
    {
        currentlyHeldBalls--;
        numberOfBallsHeldText.text = currentlyHeldBalls.ToString();
        MoveBallToTheEndOfList();
    }

    private void MoveBallToTheEndOfList()
    {
        balls.Add(balls[0]);
        balls.RemoveAt(0);
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

    public void SetPreviouslyThrownBallId(string id)
    {
        previouslyThrownBallId = id;
    }

    public string GetPreviouslyThrownBallId()
    {
        return previouslyThrownBallId;
    }

    public bool GetShouldBallStopAtThePeak()
    {
        return shouldBallStopAtThePeak;
    }
}
