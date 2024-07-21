using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAnimationController : MonoBehaviour
{
    public bool shouldStickToPlayer = false;
    public SimulationManager simulationManager;
    public Transform headset;
    public Transform throwingPointLeft;
    public Transform throwingPointRight;
    public Transform catchingPointLeft;
    public Transform catchingPointRight;
    public Material animatedBallMaterial;
    public List<Rigidbody> ballsRigidbodies;
    public float timeOfTransitionToThrowingPosition;
    public float upwardForce; // 3.5 // gravitycounter 6, force 2.3
    public float insideForce; // 0.43 // gravitycounter 6, force 0.255
    public float delayAfterThrowingAllBalls;
    private List<Rigidbody> currentBallsRigidbodies = new();
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private int currentBallIndex = 0;
    private int ballsToThrow;
    private bool didPreviousBallReachPeak = true;
    private bool shouldLoop = false;
    private float originalAlpha = 0.3f;
    private float fadeDuration = 0.3f;

    private void Awake()
    {
        animatedBallMaterial.color = new Color(animatedBallMaterial.color.r, animatedBallMaterial.color.g, animatedBallMaterial.color.b, 0);
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        foreach (Rigidbody ballRb in ballsRigidbodies)
        {
            ballRb.useGravity = false;
            ballRb.gameObject.SetActive(false);
        }

        ballsRigidbodies[0].transform.position = catchingPointRight.position;
        ballsRigidbodies[1].transform.position = catchingPointLeft.position;
        ballsRigidbodies[2].transform.position = catchingPointRight.position;
    }

    private void Update()
    {
        if (shouldStickToPlayer)
        {
            Vector3 headsetEulerAngles = headset.rotation.eulerAngles;

            Vector3 currentEulerAngles = transform.rotation.eulerAngles;
            headsetEulerAngles.x = currentEulerAngles.x;

            transform.SetPositionAndRotation(headset.position, Quaternion.Euler(headsetEulerAngles));
        }
        else
        {
            transform.SetPositionAndRotation(originalPosition, originalRotation);
        }
    }

    public void HideTutorial()
    {
        StopAllCoroutines();
        StartCoroutine(SmoothlyFadeOut());
        foreach (Rigidbody ballRb in currentBallsRigidbodies)
        {
            ballRb.gameObject.SetActive(false);
        }
    }

    public void ShowTutorial(bool isPOV)
    {
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
            ballRb.gameObject.SetActive(true);
        }
        ballsRigidbodies[0].transform.position = catchingPointRight.position;
        ballsRigidbodies[1].transform.position = catchingPointLeft.position;
        ballsRigidbodies[2].transform.position = catchingPointRight.position;
        StartCoroutine(SmoothlyFadeIn());
        ballsToThrow = ballCount;
    }

    private IEnumerator JugglingLoop()
    {
        yield return new WaitForSeconds(1);
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
        float currentSpeedMultiplier = simulationManager.GetCurrentSpeedMultiplier();
        if (ballRb.transform.position.x > 0)
        {
            force = currentSpeedMultiplier * (upwardForce * Vector3.up + Vector3.left * insideForce);
        }
        else
        {
            force = currentSpeedMultiplier * (upwardForce * Vector3.up + Vector3.right * insideForce);
        }

        ballRb.AddForce(force, ForceMode.Impulse);
    }

    private IEnumerator SmoothlyMoveBallToThrowingPositionAndThrow(Rigidbody ballRb)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = ballRb.transform.position;
        Vector3 targetPosition;
        if (ballRb.transform.position.x > 0)
        {
            targetPosition = throwingPointRight.position;
        }
        else
        {
            targetPosition = throwingPointLeft.position;
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
                ballRb.transform.position = catchingPointRight.position;
            }
            else
            {
                ballRb.velocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
                ballRb.transform.position = catchingPointLeft.position;
            }
        }
    }

    private IEnumerator SmoothlyFadeIn()
    {
        float elapsedTime = 0;
        Color startColor = animatedBallMaterial.color;
        Color targetColor = new(startColor.r, startColor.g, startColor.b, originalAlpha);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color newColor = Color.Lerp(startColor, targetColor, t);
            animatedBallMaterial.color = newColor;
            yield return null;
        }

        animatedBallMaterial.color = targetColor;
    }

    private IEnumerator SmoothlyFadeOut()
    {
        float elapsedTime = 0;
        Color startColor = animatedBallMaterial.color;
        Color targetColor = new(startColor.r, startColor.g, startColor.b, 0);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color newColor = Color.Lerp(startColor, targetColor, t);
            animatedBallMaterial.color = newColor;
            yield return null;
        }

        animatedBallMaterial.color = targetColor;
    }
}
