using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Transform headset;
    public SimulatorEvent ballThrownTooLowEvent;
    private Rigidbody ballRb;
    private Transform ballTr;
    private bool isBallAscending = false;
    private float peakHeight;
    private float ballHeightOffsetY = 0.1f;

    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
        ballTr = transform;
    }

    private void Update()
    {
        CheckPeakHeight();
    }

    void CheckPeakHeight()
    {
        if (ballRb.velocity.y > 0)
        {
            isBallAscending = true;
        }
        else if (isBallAscending && ballRb.velocity.y <= 0)
        {
            peakHeight = ballTr.position.y;
            isBallAscending = false;
            CheckHeightSuccess(peakHeight);
        }
    }

    void CheckHeightSuccess(float height)
    {
        float minHeight = headset.position.y - ballHeightOffsetY;

        if (height < minHeight)
        {
            ballThrownTooLowEvent.Raise();
        }
    }
}
