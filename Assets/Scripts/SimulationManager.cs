using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[Serializable]
public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class SimulationManager : MonoBehaviour
{
    public BallController[] initialBalls;
    public GazeInteractorController gazeInteractorController;
    public TutorialAnimationController tutorialAnimationController;
    public ControllerTutorialManager controllerTutorialManager;
    public AudioSource backgroundAudioSource;
    // I need a reference to an action like button press to trigger the event
    public InputActionReference nextLevelAction;
    public InputActionReference previousLevelAction;
    public TextMeshProUGUI speedText;
    public SimulatorEvent levelPassed;
    public SimulatorEvent flashHappend;
    public Material fadeToBlackMaterial;
    public Transform[] ballsOriginsOnTable;
    public TutorialStep[] tutorialSteps;
    public ControllerTutorialManager sideResetTutorial;
    [SerializeField] private TextMeshProUGUI numberOfBallsText;
    [SerializeField] private TextMeshProUGUI numberOfBallsHeldText;
    [SerializeField] private AudioClip throwFromCenterAudioClip;
    [SerializeField] private AudioClip handsLowerAudioClip;
    private Difficulty currentDifficulty = Difficulty.Easy;
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
        nextLevelAction.action.performed += _ => LoadNextLevel();
        previousLevelAction.action.performed += _ => LoadPreviousLevel();
    }

    private void OnDisable()
    {
        nextLevelAction.action.performed -= _ => LoadNextLevel();
        previousLevelAction.action.performed -= _ => LoadPreviousLevel();
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

    public int GetCurrentNumberOfBalls()
    {
        return currentNumberOfBallsInGame;
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
        // catchCounter.ResetCurrentScore();
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
            audioTutorialIsPlaying = true;
            if (audioTutorialCountdownCoroutine != null)
            {
                StopCoroutine(audioTutorialCountdownCoroutine);
            }
            audioTutorialCountdownCoroutine = StartCoroutine(CountdownToFinishAudioTutorial(tutorialSteps[currentTutorialStep].moveToNextStepAfterSeconds));
            if (tutorialSteps[currentTutorialStep].showGrabBallsTutorial) controllerTutorialManager.ShowGrabTutorial();
            else if (tutorialSteps[currentTutorialStep].showResetBallsTutorial) controllerTutorialManager.ShowBallResetTutorial();
        }
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
        }
    }

    private void LoadPreviousTutorialStep()
    {
        audioTutorialIsPlaying = false;
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
        // catchCounter.ResetCurrentScore();
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
            audioTutorialIsPlaying = true;
            if (audioTutorialCountdownCoroutine != null)
            {
                StopCoroutine(audioTutorialCountdownCoroutine);
            }
            audioTutorialCountdownCoroutine = StartCoroutine(CountdownToFinishAudioTutorial(tutorialSteps[currentTutorialStep].moveToNextStepAfterSeconds));
            if (tutorialSteps[currentTutorialStep].showGrabBallsTutorial) controllerTutorialManager.ShowGrabTutorial();
            else if (tutorialSteps[currentTutorialStep].showResetBallsTutorial) controllerTutorialManager.ShowBallResetTutorial();
        }
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
        }
    }

    public void SetWaitingForCorrectGazeToFalseAndCheckProgress()
    {
        if (waitingForCorrectGaze)
        {
            waitingForCorrectGaze = false;
        }
    }

    public void SetDifficultyToEasy()
    {
        currentDifficulty = Difficulty.Easy;
    }

    public void SetDifficultyToMedium()
    {
        currentDifficulty = Difficulty.Medium;
    }

    public void SetDifficultyToHard()
    {
        currentDifficulty = Difficulty.Hard;
    }

    public Difficulty GetDifficulty()
    {
        return currentDifficulty;
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        LoadNextTutorialStep();
    }

    public void LoadPreviousLevel()
    {
        LoadPreviousTutorialStep();
    }

    public void PlayAudioTutorialThrowFromCenter()
    {
    }

    public void PlayAudioTutorialHandsLower()
    {
        if (tutorialSteps[currentTutorialStep].name[0] != '0')
        {
        }
    }
}
