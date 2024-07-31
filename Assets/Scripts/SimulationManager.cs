using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimulationManager : MonoBehaviour
{
    public BallController[] initialBalls;
    public GazeInteractorController gazeInteractorController;
    public TutorialAnimationController tutorialAnimationController;
    public ControllerTutorialManager controllerTutorialManager;
    public AudioSource backgroundAudioSource;
    public TextMeshProUGUI speedText;
    public SimulatorEvent levelPassed;
    public SimulatorEvent flashHappend;
    public Material fadeToBlackMaterial;
    [SerializeField] private InputActionReference nextLevelAction;
    [SerializeField] private InputActionReference previousLevelAction;
    public Transform[] ballsOriginsOnTable;
    public TutorialStep[] tutorialSteps;
    public TutorialAudioPlayer tutorialAudioPlayer;
    public ControllerTutorialManager sideResetTutorial;
    [SerializeField] private TextMeshProUGUI numberOfBallsText;
    [SerializeField] private TextMeshProUGUI numberOfBallsHeldText;
    private int currentNumberOfBallsInGame = 1;
    private bool isBallGrounded = false;
    private bool shouldBallStopAtThePeak = false;
    private bool shouldTrackGaze = false;
    private float currentSpeedMultiplier = 1f;
    private float rootOfCurrentSpeedMultiplier = 1f;
    private int ballsToProgress = 0;
    private int currentlyHeldBalls = 0;
    private CatchCounter catchCounter;
    private int currentTutorialStep = -1;
    private Vector3 originalGravity;
    private int ballsThrownSincePreviousFlash = 0;
    private List<string> ballsIdQueue = new();
    private bool audioTutorialIsPlaying;
    private Coroutine audioTutorialCountdownCoroutine;
    private int delayAfterTutorialAudio = 2;
    private bool waitingForBallResetEvent = false;
    private bool waitingForCorrectGaze = false;

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
            if (catchCounter.GetCurrentScore() >= ballsToProgress && !audioTutorialIsPlaying)
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
        Debug.Log("Ball grabbed correctly");
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
        audioTutorialIsPlaying = false;
        if (currentTutorialStep == tutorialSteps.Length - 1)
        {
            return;
        }
        controllerTutorialManager.HideAllTutorials();
        tutorialAnimationController.HideTutorial();
        currentTutorialStep++;
        if (tutorialSteps[currentTutorialStep].name[0] != '0')
        {
            sideResetTutorial.ShowInfiniteBallResetTutorial();
        }
        else
        {
            sideResetTutorial.HideInfiniteBallResetTutorial();
        }
        shouldTrackGaze = tutorialSteps[currentTutorialStep].trackGaze;
        currentNumberOfBallsInGame = tutorialSteps[currentTutorialStep].numberOfBalls;
        shouldBallStopAtThePeak = tutorialSteps[currentTutorialStep].shouldBallStopAtThePeak;
        ballsToProgress = tutorialSteps[currentTutorialStep].ballsToProgress;
        catchCounter.ResetCurrentScore();
        SetSpeedMultiplier(tutorialSteps[currentTutorialStep].speedMultiplier);
        waitingForBallResetEvent = tutorialSteps[currentTutorialStep].waitsForABallResetEvent;
        waitingForCorrectGaze = tutorialSteps[currentTutorialStep].waitsForCorrectGaze;
        if (tutorialSteps[currentTutorialStep].waitsForCorrectGaze)
        {
            StartCoroutine(DelayedFirstGazeCheck());
        }
        if (tutorialSteps[currentTutorialStep].showTutorial)
        {
            tutorialAnimationController.ShowTutorial(isPOV: tutorialSteps[currentTutorialStep].isTutorialPOV);
        }
        if (tutorialSteps[currentTutorialStep].audioGuideTrack != null)
        {
            tutorialAudioPlayer.PlayAudioClip(tutorialSteps[currentTutorialStep].audioGuideTrack);
            audioTutorialIsPlaying = true;
            if (audioTutorialCountdownCoroutine != null)
            {
                StopCoroutine(audioTutorialCountdownCoroutine);
            }
            audioTutorialCountdownCoroutine = StartCoroutine(CountdownToFinishAudioTutorial(tutorialSteps[currentTutorialStep].moveToNextStepAfterSeconds));
            if (tutorialSteps[currentTutorialStep].showGrabBallsTutorial) controllerTutorialManager.ShowGrabTutorial();
            else if (tutorialSteps[currentTutorialStep].showResetBallsTutorial) controllerTutorialManager.ShowBallResetTutorial();
        }
        SpawnBallsOnTable();
    }

    private IEnumerator DelayedFirstGazeCheck()
    {
        yield return new WaitForSeconds(11.5f);
        fadeToBlackMaterial.DOFade(1, 0.5f);
        yield return new WaitForSeconds(0.5f);
        gazeInteractorController.enabled = true;
    }

    private IEnumerator CountdownToFinishAudioTutorial(int numberOfSeconds)
    {
        yield return new WaitForSeconds(numberOfSeconds);
        backgroundAudioSource.DOFade(.5f, 1);
        yield return new WaitForSeconds(delayAfterTutorialAudio);
        audioTutorialIsPlaying = false;
        if (ballsToProgress == 0 && !waitingForBallResetEvent && !waitingForCorrectGaze)
        {
            StartCoroutine(LevelPassedAfterDelay());
        }
    }

    private void LoadPreviousTutorialStep()
    {
        if (currentTutorialStep == 0)
        {
            return;
        }
        controllerTutorialManager.HideAllTutorials();
        tutorialAnimationController.HideTutorial();
        currentTutorialStep--;
        if (tutorialSteps[currentTutorialStep].name[0] != '0')
        {
            sideResetTutorial.ShowInfiniteBallResetTutorial();
        }
        else
        {
            sideResetTutorial.HideInfiniteBallResetTutorial();
        }
        shouldTrackGaze = tutorialSteps[currentTutorialStep].trackGaze;
        currentNumberOfBallsInGame = tutorialSteps[currentTutorialStep].numberOfBalls;
        shouldBallStopAtThePeak = tutorialSteps[currentTutorialStep].shouldBallStopAtThePeak;
        ballsToProgress = tutorialSteps[currentTutorialStep].ballsToProgress;
        catchCounter.ResetCurrentScore();
        SetSpeedMultiplier(tutorialSteps[currentTutorialStep].speedMultiplier);
        waitingForBallResetEvent = tutorialSteps[currentTutorialStep].waitsForABallResetEvent;
        waitingForCorrectGaze = tutorialSteps[currentTutorialStep].waitsForCorrectGaze;
        if (tutorialSteps[currentTutorialStep].waitsForCorrectGaze)
        {
            StartCoroutine(DelayedFirstGazeCheck());
        }
        if (tutorialSteps[currentTutorialStep].showTutorial)
        {
            tutorialAnimationController.ShowTutorial(isPOV: tutorialSteps[currentTutorialStep].isTutorialPOV);
        }
        if (tutorialSteps[currentTutorialStep].audioGuideTrack != null)
        {
            tutorialAudioPlayer.PlayAudioClip(tutorialSteps[currentTutorialStep].audioGuideTrack);
            audioTutorialIsPlaying = true;
            if (audioTutorialCountdownCoroutine != null)
            {
                StopCoroutine(audioTutorialCountdownCoroutine);
            }
            audioTutorialCountdownCoroutine = StartCoroutine(CountdownToFinishAudioTutorial(tutorialSteps[currentTutorialStep].moveToNextStepAfterSeconds));
            if (tutorialSteps[currentTutorialStep].showGrabBallsTutorial) controllerTutorialManager.ShowGrabTutorial();
            else if (tutorialSteps[currentTutorialStep].showResetBallsTutorial) controllerTutorialManager.ShowBallResetTutorial();
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

    public void SetWaitingForBallResetEventToFalseAndCheckProgress()
    {
        if (waitingForBallResetEvent)
        {
            waitingForBallResetEvent = false;
            CheckProgress();
        }
    }

    public void SetWaitingForCorrectGazeToFalseAndCheckProgress()
    {
        if (waitingForCorrectGaze)
        {
            waitingForCorrectGaze = false;
            CheckProgress();
        }
    }
}
