using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreResetter : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private SimulatorEvent ballHitGroundEvent;
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            simulationManager.SetBallGrounded(true);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            simulationManager.SetBallGrounded(false);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            ballHitGroundEvent.Raise();
        }
    }
}
