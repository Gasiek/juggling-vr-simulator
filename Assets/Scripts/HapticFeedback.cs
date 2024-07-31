using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class HapticFeedback : MonoBehaviour
{
    public Hand hand;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (other.GetComponent<Rigidbody>().velocity.y >= 0)
                return;
            if (hand == Hand.Left)
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).SendHapticImpulse(0, 0.25f, 0.15f);
            else
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).SendHapticImpulse(0, 0.25f, 0.15f);
        }
    }
}
