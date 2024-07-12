using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAnimationController : MonoBehaviour
{
    public Transform throwingPointLeft;
    public Transform throwingPointRight;
    public Transform catchingPointLeft;
    public Transform catchingPointRight;
    public Rigidbody[] ballsRigidbodies;
    public float timeOfTransitionToThrowingPosition;
    public float upwardForce; // 3.5
    public float insideForce; // 0.43
    private int currentBallIndex = 0;
    private bool didPreviousBallReachPeak = true;
    public float gravityCounter = 5;

    void Start()
    {
        foreach (Rigidbody ballRb in ballsRigidbodies)
        {
            ballRb.useGravity = false;
        }
        ballsRigidbodies[0].transform.position = catchingPointRight.position;
        ballsRigidbodies[1].transform.position = catchingPointLeft.position;
        ballsRigidbodies[2].transform.position = catchingPointRight.position;
        ShowThreeBallsTutorial();
    }

    void FixedUpdate()
    {
        ApplyCustomGravity();
    }

    private void ApplyCustomGravity()
    {
        foreach (Rigidbody ballRb in ballsRigidbodies)
        {
            if (ballRb.useGravity)
            {
                ballRb.AddForce(gravityCounter * Vector3.up, ForceMode.Acceleration);
            }
        }
    }

    public void ShowThreeBallsTutorial()
    {
        StartCoroutine(JugglingLoop());
    }

    private void ThrowBallFromRightHand(Rigidbody ballRb)
    {
        ballRb.useGravity = true;
        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(Vector3.up * upwardForce + Vector3.left * insideForce, ForceMode.Impulse);
    }

    private void ThrowBallFromLeftHand(Rigidbody ballRb)
    {
        ballRb.useGravity = true;
        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(Vector3.up * upwardForce + Vector3.right * insideForce, ForceMode.Impulse);
    }

    private IEnumerator SmoothlyMoveBallToThrowingPositionAndThrow(Rigidbody ballRb)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = ballRb.transform.position;
        Vector3 targetPosition = ballRb.transform.position.x > 0 ? throwingPointRight.position : throwingPointLeft.position;

        while (elapsedTime < timeOfTransitionToThrowingPosition)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeOfTransitionToThrowingPosition);
            ballRb.transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            yield return null;
        }

        ballRb.transform.position = targetPosition;

        if (ballRb.transform.position.x > 0)
        {
            ThrowBallFromRightHand(ballRb);
        }
        else
        {
            ThrowBallFromLeftHand(ballRb);
        }
    }

    private IEnumerator JugglingLoop()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (didPreviousBallReachPeak)
            {
                Rigidbody currentBallRb = ballsRigidbodies[currentBallIndex];
                StartCoroutine(SmoothlyMoveBallToThrowingPositionAndThrow(currentBallRb));
                didPreviousBallReachPeak = false;
                currentBallIndex = (currentBallIndex + 1) % ballsRigidbodies.Length;
                StartCoroutine(WaitForBallToReachPeak(currentBallRb));
                StartCoroutine(WaitForBallToReachHandHeight(currentBallRb));
            }
            yield return null;
        }
    }
    private IEnumerator WaitForBallToReachHandHeight(Rigidbody ballRb)
    {
        while (ballRb.transform.position.y >= throwingPointLeft.position.y)
        {
            yield return null;
        }
        Debug.Log("ball y: " + ballRb.transform.position.y);
        Debug.Log("throwing point y: " + throwingPointLeft.position.y);
        ballRb.useGravity = false;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.transform.position = ballRb.transform.localPosition.x > 0 ? catchingPointRight.position : catchingPointLeft.position;
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
