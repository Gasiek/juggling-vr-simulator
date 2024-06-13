using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRenderer : MonoBehaviour
{
    [Header("Display control")]
    [SerializeField]
    [Range(10, 100)]
    private int LinePoints = 25;
    [SerializeField]
    [Range(0.1f, 0.25f)]
    private float TimeBetweenPoints = 0.1f;

    private Rigidbody rb;
    private LineRenderer lineRenderer;

    private Vector3 previousBallPosition;
    private Vector3 ballVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    void Start()
    {
        previousBallPosition = rb.position;
    }

    void Update()
    {
        // ballVelocity = (rb.position - previousBallPosition) / Time.deltaTime;
        // previousBallPosition = rb.position;
        if (rb.velocity.y > 0)
        {
            DrawProjection(rb.velocity);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void DrawProjection(Vector3 initialVelocity)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;
        Vector3 startPosition = transform.position;
        // Vector3 startVelocity = rb.velocity;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float t = TimeBetweenPoints; t < LinePoints; t += TimeBetweenPoints)
        {
            i++;
            float x = startPosition.x + initialVelocity.x * t;
            float y = startPosition.y + initialVelocity.y * t + 0.5f * Physics.gravity.y * t * t;
            float z = startPosition.z + initialVelocity.z * t;
            lineRenderer.SetPosition(i, new Vector3(x, y, z));
        }
    }
}
