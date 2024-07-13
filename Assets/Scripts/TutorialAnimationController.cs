using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAnimationController : MonoBehaviour
{
    public bool shouldStickToPlayer = false;
    public SimulationManager simulationManager;
    public Transform headset;
    // public Vector3 headsetOffset;
    public Transform throwingPointLeft;
    public Transform throwingPointRight;
    public Transform catchingPointLeft;
    public Transform catchingPointRight;
    public Material animatedBallMaterial;
    public List<Rigidbody> ballsRigidbodies;
    public float timeOfTransitionToThrowingPosition;
    public float upwardForce; // 3.5 // gravitycounter 6, force 2.3
    public float insideForce; // 0.43 // gravitycounter 6, force 0.255
    // public float gravityCounter;
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

    private void Start()
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

        // ShowOneBallTutorial();
        // ShowTwoBallsTutorial();
        ShowThreeBallsTutorial(newShouldLoop: true);
    }

    private void Update()
    {
        if (shouldStickToPlayer)
        {
            transform.position = headset.position;
            transform.rotation = headset.rotation;
        }
        else
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
    }

    // void FixedUpdate()
    // {
    //     ApplyCustomGravity();
    // }

    // private void ApplyCustomGravity()
    // {
    //     foreach (Rigidbody ballRb in currentBallsRigidbodies)
    //     {
    //         if (ballRb.useGravity)
    //         {
    //             ballRb.AddForce(gravityCounter * Vector3.up, ForceMode.Acceleration);
    //         }
    //     }
    // }

    public void HideTutorial()
    {
        StopAllCoroutines();
        StartCoroutine(SmoothlyFadeOut());
        foreach (Rigidbody ballRb in currentBallsRigidbodies)
        {
            ballRb.gameObject.SetActive(false);
        }
    }

    public void ShowTutorial()
    {
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
        currentBallsRigidbodies = new List<Rigidbody>(ballsRigidbodies.GetRange(0, ballCount));
        foreach (Rigidbody ballRb in currentBallsRigidbodies)
        {
            ballRb.gameObject.SetActive(true);
        }
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
                // StartCoroutine(WaitForBallToReachHandHeight(currentBallRb));
            }
            yield return null;
        }
    }

    private void ThrowBall(Rigidbody ballRb)
    {
        ballRb.useGravity = true;
        ballRb.velocity = Vector3.zero;
        Vector3 force;
        if (ballRb.transform.position.x > 0)
        {
            force = Vector3.up * upwardForce + Vector3.left * insideForce;
        }
        else
        {
            force = Vector3.up * upwardForce + Vector3.right * insideForce;
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

    private IEnumerator WaitForBallToReachHandHeight(Rigidbody ballRb)
    {
        while (Math.Round(ballRb.transform.localPosition.y, 2) >= throwingPointLeft.localPosition.y)
        {
            yield return null;
        }
        ballRb.useGravity = false;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        if (ballRb.transform.localPosition.x > 0)
        {
            ballRb.transform.position = catchingPointRight.position;
        }
        else
        {
            ballRb.transform.position = catchingPointLeft.position;
        }
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
            Debug.Log("Ball caught");
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
