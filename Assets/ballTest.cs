using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballTest : MonoBehaviour
{
    public float gravityMultiplier = 1f;
    public float magicNumber = 1f;
    private Rigidbody ballRb;
    public float upwardForce = 2;
    void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(ThrowUp), 2, 4);
    }

    private void FixedUpdate()
    {
        ballRb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }

    public void ThrowUp()
    {
        StartCoroutine(ThrowUpCoorutine());
    }

    private IEnumerator ThrowUpCoorutine()
    {
        ballRb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        yield return new WaitForFixedUpdate();
        ballRb.velocity *= Mathf.Sqrt(gravityMultiplier);
    }
}
