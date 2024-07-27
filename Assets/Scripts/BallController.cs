using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BallController : MonoBehaviour
{
    public SimulationManager simulationManager;
    public AudioClip collisionSound;
    public AudioClip throwSound;
    private Rigidbody ballRb;
    private Hand previousHand = Hand.None;
    private AudioSource audioSource;
    private bool isBallAscending;
    private bool isBallStoppedAtPeak;
    private Vector3 velocityAtPeak;
    private bool ballCollidedWithEnvironment;

    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        IsPeaking();
    }

    private void IsPeaking()
    {
        if (ballRb.isKinematic || ballCollidedWithEnvironment) return;
        if (ballRb.velocity.y > 0)
        {
            isBallAscending = true;
        }
        else if (isBallAscending && ballRb.velocity.y <= 0)
        {
            isBallAscending = false;
            if (simulationManager.GetShouldBallStopAtThePeak() // if this is a tutorial step where this should happen
                && simulationManager.GetBallsThrownSincePreviousFlash() <= (simulationManager.GetCurrentNumberOfBalls() - 1) // last ball of the flash shouldn't stop at the peak
                && simulationManager.GetPreviouslyThrownBallId() == transform.GetInstanceID().ToString()) // we could also check some condition to not stop a ball that bounced from the ground etc.
            {
                StopBallAtThePeak();
            }
        }
    }

    private void StopBallAtThePeak()
    {
        isBallStoppedAtPeak = true;
        velocityAtPeak = ballRb.velocity;
        ballRb.velocity = Vector3.zero;
        ballRb.useGravity = false;
    }


    public void ReleaseBallFromThePeak()
    {
        if (!isBallStoppedAtPeak) return;
        ballRb.useGravity = true;
        ballRb.velocity = velocityAtPeak;
    }

    public Hand GetPreviousHand()
    {
        return previousHand;
    }

    public void SetPreviousHand(Hand hand)
    {
        previousHand = hand;
    }

    public void AdjustVelocityOnRelease()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(AdjustVelocityOnReleaseCoroutine());
        }
    }

    private IEnumerator AdjustVelocityOnReleaseCoroutine()
    {
        yield return new WaitForFixedUpdate();
        float rootOfCurrentSpeedMultiplier = simulationManager.GetRootOfCurrentSpeedMultiplier();
        // ballRb.velocity = new Vector3(.2f, 5, 0) * rootOfCurrentSpeedMultiplier;
        ballRb.velocity = new Vector3(ballRb.velocity.x, ballRb.velocity.y, 0) * rootOfCurrentSpeedMultiplier;
    }

    private void OnCollisionEnter(Collision other)
    {
        audioSource.PlayOneShot(collisionSound);
        if (!other.gameObject.CompareTag("Ball"))
        {
            ballCollidedWithEnvironment = true;
            previousHand = Hand.None;
        }
    }

    public void SetIsBallStoppedAtPeak(bool value)
    {
        isBallStoppedAtPeak = value;
    }

    public void PlayThrowSound()
    {
        audioSource.PlayOneShot(throwSound);
    }

    public void Spawn(Vector3 initialPosition)
    {
        transform.SetPositionAndRotation(initialPosition, Quaternion.identity);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballCollidedWithEnvironment = false;
        ballRb.useGravity = true;
        isBallStoppedAtPeak = false;
        isBallAscending = false;
        gameObject.SetActive(true);
    }

    public void OnBallGrabbed()
    {
        ballCollidedWithEnvironment = false;
        ballRb.useGravity = true;
        isBallAscending = false;
        isBallStoppedAtPeak = false;
        audioSource.PlayOneShot(collisionSound);
    }
}
