using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAnimationController : MonoBehaviour
{
    public bool shouldStickToPlayer = false;
    public SimulationManager simulationManager;
    public Transform headset;
    public Transform animatedLeftHand;
    public Transform animatedRightHand;
    public Transform handThrowingPointLeft;
    public Transform handThrowingPointRight;
    public Transform handCatchingPointLeft;
    public Transform handCatchingPointRight;
    public Transform throwingPointLeft;
    public Transform throwingPointRight;
    public Transform catchingPointLeft;
    public Transform catchingPointRight;
    public Material animatedHandMaterial;
    public Material animatedBallMaterial;
    public List<Rigidbody> ballsRigidbodies;
    public float timeOfTransitionToThrowingPosition;
    public float upwardForce; // 3.5 // gravitycounter 6, force 2.3
    public float insideForce; // 0.43 // gravitycounter 6, force 0.255
    public float delayAfterThrowingAllBalls;
    private List<Rigidbody> currentBallsRigidbodies = new();
    private Vector3 originalPosition;
    private int currentBallIndex = 0;
    private int ballsToThrow;
    private bool didPreviousBallReachPeak = true;
    private bool shouldLoop = false;
    private float originalAlpha = 1;
    private float fadeDuration = 0.3f;
    private bool tutorialIsPlaying;

    private void Awake()
    {
        animatedBallMaterial.color = new Color(animatedBallMaterial.color.r, animatedBallMaterial.color.g, animatedBallMaterial.color.b, 0);
        animatedHandMaterial.color = new Color(animatedHandMaterial.color.r, animatedHandMaterial.color.g, animatedHandMaterial.color.b, 0);
        originalPosition = transform.position;

        foreach (Rigidbody ballRb in ballsRigidbodies)
        {
            ballRb.useGravity = false;
            ballRb.gameObject.SetActive(false);
        }

        animatedLeftHand.gameObject.SetActive(false);
        animatedRightHand.gameObject.SetActive(false);

        ballsRigidbodies[0].transform.position = catchingPointLeft.position;
        ballsRigidbodies[1].transform.position = catchingPointRight.position;
        ballsRigidbodies[2].transform.position = catchingPointLeft.position;
    }

    // private void Start()
    // {
    //     ShowTutorial(true);
    // }

    private void Update()
    {
        if (shouldStickToPlayer)
        {
            transform.position = headset.position;
        }
        else
        {
            transform.position = originalPosition;
        }
    }

    public void HideTutorial()
    {
        if (!tutorialIsPlaying) return;
        tutorialIsPlaying = false;
        StopAllCoroutines();
        StartCoroutine(SmoothlyFadeOut());
        foreach (Rigidbody ballRb in currentBallsRigidbodies)
        {
            ballRb.gameObject.SetActive(false);
        }
        animatedLeftHand.gameObject.SetActive(false);
        animatedRightHand.gameObject.SetActive(false);
    }

    public void ShowTutorial(bool isPOV)
    {
        if (tutorialIsPlaying) return;
        tutorialIsPlaying = true;
        shouldStickToPlayer = isPOV;
        switch (simulationManager.GetCurrentNumberOfBalls())
        {
            case 1:
                ShowOneBallTutorial();
                break;
            case 2:
                ShowTwoBallsTutorial();
                break;
            case 3:
                ShowThreeBallsTutorial();
                break;
            default:
                ShowThreeBallsTutorial();
                break;
        }
    }

    private void ShowThreeBallsTutorial(bool newShouldLoop = false)
    {
        shouldLoop = newShouldLoop;
        InitializeTutorial(3);
        StartCoroutine(JugglingLoop());
    }

    private void ShowTwoBallsTutorial()
    {
        InitializeTutorial(2);
        StartCoroutine(JugglingLoop());
    }

    private void ShowOneBallTutorial()
    {
        InitializeTutorial(1);
        StartCoroutine(JugglingLoop());
    }

    private void InitializeTutorial(int ballCount)
    {
        didPreviousBallReachPeak = true;
        currentBallIndex = 0;

        currentBallsRigidbodies = new List<Rigidbody>(ballsRigidbodies.GetRange(0, ballCount));
        foreach (Rigidbody ballRb in currentBallsRigidbodies)
        {
            ballRb.useGravity = false;
            ballRb.velocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.gameObject.SetActive(true);
        }
        animatedLeftHand.gameObject.SetActive(true);
        animatedRightHand.gameObject.SetActive(true);
        ballsRigidbodies[0].transform.position = catchingPointLeft.position;
        ballsRigidbodies[1].transform.position = catchingPointRight.position;
        ballsRigidbodies[2].transform.position = catchingPointLeft.position;
        animatedLeftHand.position = handCatchingPointLeft.position;
        animatedRightHand.position = handCatchingPointRight.position;
        StartCoroutine(SmoothlyFadeIn());
        ballsToThrow = ballCount;
    }

    private IEnumerator JugglingLoop()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            if (didPreviousBallReachPeak)
            {
                if (ballsToThrow == 0)
                {
                    yield return new WaitForSeconds(delayAfterThrowingAllBalls);
                    ballsToThrow = currentBallsRigidbodies.Count;
                }
                Rigidbody currentBallRb = currentBallsRigidbodies[currentBallIndex];
                StartCoroutine(SmoothlyMoveBallToThrowingPositionAndThrow(currentBallRb));
                StartCoroutine(SmoothlyMoveHandToThrowingPositionAndBack(currentBallRb.transform.position.x > 0 ? animatedLeftHand : animatedRightHand));
                didPreviousBallReachPeak = false;
                if (!shouldLoop)
                {
                    ballsToThrow--;
                }
                currentBallIndex = (currentBallIndex + 1) % currentBallsRigidbodies.Count;
                StartCoroutine(WaitForBallToReachPeak(currentBallRb));
            }
            yield return null;
        }
    }

    private void ThrowBall(Rigidbody ballRb)
    {
        ballRb.useGravity = true;
        ballRb.velocity = Vector3.zero;
        Vector3 force;
        float rootOfCurrentSpeedMultiplier = simulationManager.GetRootOfCurrentSpeedMultiplier();
        if (ballRb.transform.position.x > 0)
        {
            force = rootOfCurrentSpeedMultiplier * (upwardForce * Vector3.up + Vector3.left * insideForce);
        }
        else
        {
            force = rootOfCurrentSpeedMultiplier * (upwardForce * Vector3.up + Vector3.right * insideForce);
        }

        ballRb.AddForce(force, ForceMode.Impulse);
    }

    private IEnumerator SmoothlyMoveHandToThrowingPositionAndBack(Transform animatedHand)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = animatedHand.position;
        Vector3 targetPosition;
        if (animatedHand.position.x > 0)
        {
            targetPosition = handThrowingPointLeft.position;
        }
        else
        {
            targetPosition = handThrowingPointRight.position;
        }

        while (elapsedTime < timeOfTransitionToThrowingPosition)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeOfTransitionToThrowingPosition);
            animatedHand.position = Vector3.Lerp(startingPosition, targetPosition, t);
            yield return null;
        }

        animatedHand.position = targetPosition;
        yield return new WaitForSeconds(0.1f);
        elapsedTime = 0;
        while (elapsedTime < timeOfTransitionToThrowingPosition)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeOfTransitionToThrowingPosition);
            animatedHand.position = Vector3.Lerp(targetPosition, startingPosition, t);
            yield return null;
        }
        animatedHand.position = startingPosition;
    }
    private IEnumerator SmoothlyMoveBallToThrowingPositionAndThrow(Rigidbody ballRb)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = ballRb.transform.position;
        Vector3 targetPosition;
        if (ballRb.transform.position.x > 0)
        {
            targetPosition = throwingPointLeft.position;
        }
        else
        {
            targetPosition = throwingPointRight.position;
        }

        while (elapsedTime < timeOfTransitionToThrowingPosition)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeOfTransitionToThrowingPosition);
            ballRb.transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            yield return null;
        }

        ballRb.transform.position = targetPosition;
        ThrowBall(ballRb);
    }

    private IEnumerator WaitForBallToReachPeak(Rigidbody ballRb)
    {
        yield return new WaitForSeconds(timeOfTransitionToThrowingPosition + 0.1f);
        while (ballRb.velocity.y > 0 || ballRb.transform.localPosition.y < throwingPointLeft.localPosition.y)
        {
            yield return null;
        }
        didPreviousBallReachPeak = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AnimatedBall"))
        {
            Rigidbody ballRb = other.gameObject.GetComponent<Rigidbody>();
            if (!ballRb.useGravity) return;
            ballRb.useGravity = false;
            if (ballRb.velocity.x > 0)
            {
                ballRb.velocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
                ballRb.transform.position = catchingPointLeft.position;
            }
            else
            {
                ballRb.velocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
                ballRb.transform.position = catchingPointRight.position;
            }
        }
    }

    private IEnumerator SmoothlyFadeIn()
    {
        float elapsedTime = 0;
        Color startColor = animatedBallMaterial.color;
        Color handStartColor = animatedHandMaterial.color;
        Color targetColor = new(startColor.r, startColor.g, startColor.b, originalAlpha);
        Color handTargetColor = new(handStartColor.r, handStartColor.g, handStartColor.b, originalAlpha);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color newColor = Color.Lerp(startColor, targetColor, t);
            Color newHandColor = Color.Lerp(handStartColor, handTargetColor, t);
            animatedBallMaterial.color = newColor;
            animatedHandMaterial.color = newHandColor;
            yield return null;
        }

        animatedBallMaterial.color = targetColor;
        animatedHandMaterial.color = handTargetColor;
    }

    private IEnumerator SmoothlyFadeOut()
    {
        float elapsedTime = 0;
        Color startColor = animatedBallMaterial.color;
        Color handStartColor = animatedHandMaterial.color;
        Color targetColor = new(startColor.r, startColor.g, startColor.b, 0);
        Color handTargetColor = new(handStartColor.r, handStartColor.g, handStartColor.b, 0);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color newColor = Color.Lerp(startColor, targetColor, t);
            Color newHandColor = Color.Lerp(handStartColor, handTargetColor, t);
            animatedBallMaterial.color = newColor;
            animatedHandMaterial.color = newHandColor;
            yield return null;
        }

        animatedBallMaterial.color = targetColor;
        animatedHandMaterial.color = handTargetColor;
    }
}
