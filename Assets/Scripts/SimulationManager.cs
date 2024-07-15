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
    public GameObject ballPrefab;
    public TutorialAnimationController tutorialAnimationController;
    public TextMeshProUGUI speedText;
    public SimulatorEvent levelPassed;
    public Transform[] ballsOriginsOnTable;
    public TutorialStep[] tutorialSteps;
    public Transform ballsParent;
    public int currentNumberOfBallsInGame = 1;
    [SerializeField] private InputActionReference increaseSpeedAction;
    [SerializeField] private InputActionReference decreaseSpeedAction;
    [SerializeField] private TextMeshProUGUI numberOfBallsText;
    [SerializeField] private TextMeshProUGUI numberOfBallsHeldText;
    private bool isBallGrounded = false;
    private bool shouldBallStopAtThePeak = false;
    private float speedMultiplier = 1f;
    private List<GameObject> balls = new();
    private int ballsToProgress = 10;
    private int currentlyHeldBalls = 0;
    private CatchCounter catchCounter;
    private BallResetHandler ballResetHandler;
    private string previouslyThrownBallId;
    private int currentTutorialStep = -1;

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
        ballResetHandler = GetComponent<BallResetHandler>();
    }

    void Start()
    {
        // SpawnBallsOnTable();
        LoadNextTutorialStep();
    }

    private void OnIncreaseSpeed(InputAction.CallbackContext context)
    {
        if (speedMultiplier >= 1.5f)
        {
            return;
        }
        speedMultiplier = System.MathF.Round(speedMultiplier + 0.1f, 1);
        speedText.text = speedMultiplier.ToString();
        Time.timeScale = speedMultiplier;
    }

    private void SetSpeedMultiplier(float value)
    {
        speedMultiplier = value;
        speedText.text = speedMultiplier.ToString();
        Time.timeScale = speedMultiplier;
    }

    private void OnDecreaseSpeed(InputAction.CallbackContext context)
    {
        if (speedMultiplier <= 0.5f)
        {
            return;
        }
        speedMultiplier = System.MathF.Round(speedMultiplier - 0.1f, 1);
        speedText.text = speedMultiplier.ToString();
        Time.timeScale = speedMultiplier;
    }

    public void SpawnBallsOnTable()
    {
        if (currentNumberOfBallsInGame > ballsOriginsOnTable.Length)
        {
            currentNumberOfBallsInGame = ballsOriginsOnTable.Length;
        }
        StartCoroutine(DestroyAndSpawnBalls());
    }

    private IEnumerator DestroyAndSpawnBalls()
    {
        DestroyAllBalls();
        ballResetHandler.ClearBalls();
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < currentNumberOfBallsInGame; i++)
        {
            var ball = Instantiate(ballPrefab, ballsOriginsOnTable[i].position, Quaternion.identity, ballsParent);
            BallController ballController = ball.GetComponent<BallController>();
            ballController.UpdateGravityAdjustmentForce();
            ballController.SetShouldStopAtThePeak(shouldBallStopAtThePeak);
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            XRGrabInteractable ballGrabInteractable = ball.GetComponent<XRGrabInteractable>();
            ballRb.velocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballResetHandler.RegisterInstantiatedBall(ballGrabInteractable);
            balls.Add(ball);
        }
        numberOfBallsText.text = currentNumberOfBallsInGame.ToString();
    }

    private void DestroyAllBalls()
    {
        foreach (var ball in balls)
        {
            Destroy(ball);
        }
        balls.Clear();
    }

    // public void StartNextLevel()
    // {
    //     StartCoroutine(IncreaseNumberOfBalls());
    // }

    // private IEnumerator IncreaseNumberOfBalls()
    // {
    //     yield return new WaitForSeconds(1f); // here should be some info that a person finished the level
    //     currentNumberOfBallsInGame++;
    //     SpawnBallsOnTable();
    // }

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

    public int GetCurrentlyHeldBalls()
    {
        return currentlyHeldBalls;
    }

    public void SetPreviouslyThrownBallId(string id)
    {
        previouslyThrownBallId = id;
    }

    public string GetPreviouslyThrownBallId()
    {
        return previouslyThrownBallId;
    }
}
