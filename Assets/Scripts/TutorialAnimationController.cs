using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAnimationController : MonoBehaviour
{
    public Transform throwingPointLeft;
    public Transform throwingPointRight;
    public Transform catchingPointLeft;
    public Transform catchingPointRight;
    public List<Rigidbody> ballsRigidbodies;
    public float timeOfTransitionToThrowingPosition;
    public float upwardForce; // 3.5
    public float insideForce; // 0.43
    public float gravityCounter;
    public float delayAfterThrowingAllBalls;
    private List<Rigidbody> currentBallsRigidbodies = new();
    private int currentBallIndex = 0;
    private int ballsToThrow;
    private bool didPreviousBallReachPeak = true;
    private bool shouldLoop = false;

    void Start()
    {
        foreach (Rigidbody ballRb in ballsRigidbodies)
        {
            ballRb.useGravity = false;
            ballRb.gameObject.SetActive(false);
        }

        ballsRigidbodies[0].transform.position = catchingPointRight.position;
        ballsRigidbodies[1].transform.position = catchingPointLeft.position;
        ballsRigidbodies[2].transform.position = catchingPointRight.position;

        ShowOneBallTutorial();
        // ShowTwoBallsTutorial();
        // ShowThreeBallsTutorial(newShouldLoop: false);
    }

    void FixedUpdate()
    {
        ApplyCustomGravity();
    }

    private void ApplyCustomGravity()
    {
        foreach (Rigidbody ballRb in currentBallsRigidbodies)
        {
            if (ballRb.useGravity)
            {
                ballRb.AddForce(gravityCounter * Vector3.up, ForceMode.Acceleration);
            }
        }
    }

    public void ShowThreeBallsTutorial(bool newShouldLoop = false)
    {
        shouldLoop = newShouldLoop;
        InitializeTutorial(3);
        StartCoroutine(JugglingLoop());
    }

    public void ShowTwoBallsTutorial()
    {
        InitializeTutorial(2);
        StartCoroutine(JugglingLoop());
    }

    public void ShowOneBallTutorial()
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
                StartCoroutine(WaitForBallToReachHandHeight(currentBallRb));
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
        while (ballRb.transform.position.y >= throwingPointLeft.position.y)
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
        while (ballRb.velocity.y > 0 || ballRb.transform.position.y < throwingPointLeft.position.y)
        {
            yield return null;
        }
        didPreviousBallReachPeak = true;
    }
}
