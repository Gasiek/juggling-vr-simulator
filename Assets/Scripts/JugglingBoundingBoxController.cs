using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugglingBoundingBoxController : MonoBehaviour
{
    public Transform headset;  // The VR headset transform
    // public AudioSource failureSound;  // Sound to play on failure
    public Color failureColor;  // Color to set on failure
    public Color initialColor;
    public SimulatorEvent ballThrownTooHighEvent;  // Event to raise when the ball goes out of bounds
    public Renderer boxRenderer;
    public bool shouldTriggerEvents = false;

    public float boxHeight = 0.4f;  // Height of the box in meters
    public float boxDepth = 0.4f;  // Height of the box in meters
    public float boxOffsetY = 0.4f;  // Height of the box in meters
    public float boxOffsetZ = 0.4f;  // Height of the box in meters
    public float smoothTime = 0.3f;  // Time to smooth the movement

    private GameObject boundingBox;  // The semi-transparent box
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        boundingBox = gameObject;
        if (shouldTriggerEvents)
        {
            initialColor = boxRenderer.material.color;
        }

        SetBoxPositionAndSize();
    }

    private void Update()
    {
        SmoothUpdateBoxPosition();
    }

    private void SetBoxPositionAndSize()
    {
        // boundingBox.transform.localScale = new Vector3(boundingBox.transform.localScale.x, boxHeight, boxDepth);
        boundingBox.transform.position = new Vector3(headset.position.x, headset.position.y + boxOffsetY, headset.position.z + boxOffsetZ);
    }

    void SmoothUpdateBoxPosition()
    {
        Vector3 targetPosition = new Vector3(headset.position.x, headset.position.y + boxOffsetY, headset.position.z + boxOffsetZ);
        boundingBox.transform.position = Vector3.SmoothDamp(boundingBox.transform.position, targetPosition, ref velocity, smoothTime);
    }

    private void OnTriggerExit(Collider other)
    {
        if (shouldTriggerEvents && other.CompareTag("Ball"))
        {
            ballThrownTooHighEvent.Raise();
        }
    }
    public void IndicateFailure()
    {
        // failureSound.Play();
        boxRenderer.material.color = failureColor;
        Invoke(nameof(ResetBoxColor), 1.0f);
    }

    private void ResetBoxColor()
    {
        boxRenderer.material.color = initialColor;
    }
}
