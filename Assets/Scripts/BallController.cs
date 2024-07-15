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
    // private Transform ballTr;
    // private bool isBallAscending = false;
    // private float peakHeight;
    // private float ballHeightOffsetY = 0.1f;
    private Hand previousHand = Hand.None;
    private float gravityAdjustmentForce = 1;
    private AudioSource audioSource;


    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        // ballTr = transform;
        // headset = Camera.main.transform;
    }

    // private void Update()
    // {
    //     CheckPeakHeight();
    // }

    // void CheckPeakHeight()
    // {
    //     if (ballRb.velocity.y > 0)
    //     {
    //         isBallAscending = true;
    //     }
    //     else if (isBallAscending && ballRb.velocity.y <= 0)
    //     {
    //         peakHeight = ballTr.position.y;
    //         isBallAscending = false;
    //         CheckHeightSuccess(peakHeight);
    //     }
    // }

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
}
