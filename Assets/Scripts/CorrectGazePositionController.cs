using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectGazePositionController : MonoBehaviour
{
    public Transform headset;

    private void Update()
    {
        transform.position = headset.position;

        Quaternion headsetRotation = headset.rotation;

        transform.rotation = Quaternion.Euler(-25, headsetRotation.eulerAngles.y, headsetRotation.eulerAngles.z);
    }
}
