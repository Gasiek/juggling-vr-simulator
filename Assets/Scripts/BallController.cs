using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BallController : MonoBehaviour
{
    public AudioClip collisionSound;
    public AudioClip throwSound;
    // public SimulatorEvent ballThrownTooLowEvent;
    // private Transform headset;
    private Rigidbody ballRb;
    private Transform ballTr;
    // private float ballHeightOffsetY = 0.1f;
    private Hand previousHand = Hand.None;
    private float gravityAdjustmentForce = 1;
    private AudioSource audioSource;
    // private object peakHeight;
    private bool shouldStopAtThePeak;
    private bool isBallAscending;
    private Vector3 velocityAtPeak;

    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        ballTr = transform;
        // headset = Camera.main.transform;
    }

    private void Update()
    {
        IsPeaking();
    }

    private void IsPeaking()
    {
        if (ballRb.velocity.y > 0)
        {
            isBallAscending = true;
        }
        else if (isBallAscending && ballRb.velocity.y <= 0)
        {
            // peakHeight = ballTr.position.y;
            isBallAscending = false;
            if (shouldStopAtThePeak) // we could also check some condition to not stop a ball that bounced from the ground etc.
            {
                StopBallAtThePeak();
            }
        }
    }

    private void StopBallAtThePeak()
    {
        velocityAtPeak = ballRb.velocity;
        ballRb.velocity = Vector3.zero;
        ballRb.useGravity = false;
    }


    public void ReleaseBallFromThePeak()
    {
        ballRb.useGravity = true;
        ballRb.velocity = velocityAtPeak;
    }

    // void CheckHeightSuccess(float height)
    // {
    //     float minHeight = headset.position.y + ballHeightOffsetY;

    //     if (height < minHeight)
    //     {
    //         ballThrownTooLowEvent.Raise();
    //     }
    // }

    void FixedUpdate()
    {
        if (Time.timeScale != 1.0f)
        {
            ballRb.AddForce(Vector3.down * gravityAdjustmentForce, ForceMode.Acceleration);
        }
    }

    public Hand GetPreviousHand()
    {
        return previousHand;
    }

    public void SetPreviousHand(Hand hand)
    {
        previousHand = hand;
    }

    public void UpdateGravityAdjustmentForce()
    {
        gravityAdjustmentForce = (1.0f / Time.timeScale - 1.0f) * Physics.gravity.magnitude;
    }

    private void OnCollisionEnter(Collision other)
    {
        audioSource.PlayOneShot(collisionSound);
    }

    public void PlayThrowSound()
    {
        audioSource.PlayOneShot(throwSound);
    }

    public void SetShouldStopAtThePeak(bool newValue)
    {
        shouldStopAtThePeak = newValue;
    }
}
