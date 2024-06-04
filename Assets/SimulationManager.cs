using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SimulationManager : MonoBehaviour
{
    private float speed;
    public Slider slider;
    public float originalVelocityThrow;
    public XRGrabInteractable ball1;
    public XRGrabInteractable ball2;
    public XRGrabInteractable ball3;
    public TextMeshProUGUI speedText;
    private Vector3 origin1;
    private Vector3 origin2;
    private Vector3 origin3;

    void Start()
    {
        origin1 = ball1.transform.position;
        origin2 = ball2.transform.position;
        origin3 = ball3.transform.position;
    }

    public void ResetBalls()
    {
        ball1.transform.position = origin1;
        ball2.transform.position = origin2;
        ball3.transform.position = origin3;
    }

    public void UpdateSpeed()
    {
        speed = slider.value / 10f;
        speedText.text = "Speed: " + speed.ToString();
        Time.timeScale = speed;
    }

    private void Update()
    {
        ball1.throwVelocityScale = originalVelocityThrow * speed;
        ball2.throwVelocityScale = originalVelocityThrow * speed;
        ball3.throwVelocityScale = originalVelocityThrow * speed;
    }
}
