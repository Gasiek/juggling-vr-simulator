using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BallController : MonoBehaviour
{
    public SimulationManager simulationManager;
    public Transform leftHand;
    public Transform rightHand;
    public Transform headset;
    public AudioClip collisionSound;
    public AudioClip throwSound;
    private Rigidbody ballRb;
    private Hand previousHand = Hand.None;
    private AudioSource audioSource;
    private bool isBallAscending;
    private bool isBallStoppedAtPeak;
    private Vector3 velocityAtPeak;
    private bool ballCollidedWithEnvironment;
    private int numberOfBallToBallCollisions;
    private int numberOfBallToBallCollisionsForTutorial = 3;
    private bool isGrabbed;

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

    public void AdjustVelocityOnRelease(Hand hand)
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(AdjustVelocityOnReleaseCoroutine(hand));
        }
    }

    private IEnumerator AdjustVelocityOnReleaseCoroutine(Hand releaseHand)
    {
        yield return new WaitForFixedUpdate();
        if (ballRb.velocity.y < 1) yield break;
        float rootOfCurrentSpeedMultiplier = simulationManager.GetRootOfCurrentSpeedMultiplier();
        ballRb.useGravity = true;
        switch (simulationManager.GetDifficulty())
        {
            case Difficulty.Easy:
                ballRb.velocity = CalculatePerfectVelocity(releaseHand == Hand.Left ? rightHand : leftHand);
                break;
            case Difficulty.Medium:
                ballRb.velocity = new Vector3(ballRb.velocity.x, ballRb.velocity.y, 0) * rootOfCurrentSpeedMultiplier;
                break;
            case Difficulty.Hard:
                ballRb.velocity = new Vector3(ballRb.velocity.x, ballRb.velocity.y, ballRb.velocity.z) * rootOfCurrentSpeedMultiplier;
                break;
        }
    }

    private Vector3 CalculatePerfectVelocity(Transform handDestination)
    {
        float peakHeight = headset.position.y - transform.position.y + 0.4f;
        Vector3 ballPosition = transform.position;
        Vector3 handPosition = handDestination.position;
#if UNITY_EDITOR
        peakHeight = 1.0f;
#endif

        float gravity = -Physics.gravity.y;

        // Calculate horizontal distance and direction
        Vector3 horizontalDirection = new Vector3(handPosition.x - ballPosition.x, 0, handPosition.z - ballPosition.z);
        float horizontalDistance = horizontalDirection.magnitude;
        horizontalDirection.Normalize();

        // Calculate vertical displacement
        float verticalDisplacement = handPosition.y - ballPosition.y;

        // Time to reach peak height
        float timeToPeak = Mathf.Sqrt(2 * peakHeight / gravity);

        // Total time of flight (up and down)
        float totalTime = timeToPeak + Mathf.Sqrt(2 * (peakHeight - verticalDisplacement) / gravity);

        // Initial velocities
        float initialHorizontalVelocity = horizontalDistance / totalTime;
        float initialVerticalVelocity = gravity * timeToPeak;

        // Calculate initial velocity vector
        Vector3 initialVelocity = horizontalDirection * initialHorizontalVelocity + Vector3.up * initialVerticalVelocity;
        return initialVelocity;
    }
    private void OnCollisionEnter(Collision other)
    {
        audioSource.PlayOneShot(collisionSound);
        if (other.gameObject.CompareTag("Environment"))
        {
            ballCollidedWithEnvironment = true;
            previousHand = Hand.None;
        }
        else if (
            other.gameObject.CompareTag("Ball") &&
            !isGrabbed &&
            !other.gameObject.GetComponent<BallController>().IsGrabbed() &&
            transform.position.y > 1.2f)
        {
            numberOfBallToBallCollisions++;
            if (numberOfBallToBallCollisions > numberOfBallToBallCollisionsForTutorial)
            {
                numberOfBallToBallCollisions = 0;
                simulationManager.PlayAudioTutorialThrowFromCenter();
            }
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
        ballCollidedWithEnvironment = false;
        isBallStoppedAtPeak = false;
        isBallAscending = false;
        gameObject.SetActive(true);
        ballRb.useGravity = true;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        isGrabbed = false;
    }

    public void OnBallGrabbed()
    {
        ballCollidedWithEnvironment = false;
        ballRb.useGravity = true;
        isBallAscending = false;
        isBallStoppedAtPeak = false;
        audioSource.PlayOneShot(collisionSound);
        isGrabbed = true;
    }

    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    public void SetIsGrabbed(bool value)
    {
        isGrabbed = value;
    }
}
