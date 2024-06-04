using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SimulationManager : MonoBehaviour
{
    private float speed;
    public Slider slider;
    public float originalVelocityThrow;
    public XRGrabInteractable[] balls;
    public TextMeshProUGUI speedText;
    private Vector3[] ballsOrigins;

    void Start()
    {
        ballsOrigins = new Vector3[balls.Length];
        for (int i = 0; i < balls.Length; i++)
        {
            ballsOrigins[i] = balls[i].transform.position;
        }
    }

    public void ResetBalls()
    {
        for (int i = 0; i < balls.Length; i++)
        {
            balls[i].transform.position = ballsOrigins[i];
            Rigidbody rb = balls[i].GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void UpdateSpeed()
    {
        speed = slider.value / 10f;
        speedText.text = "Speed: " + speed.ToString();
        Time.timeScale = speed;
    }

    private void Update()
    {
        foreach (XRGrabInteractable ball in balls)
        {
            if (ball.isSelected)
            {
                ball.throwVelocityScale = originalVelocityThrow * speed;
            }
        }
    }
}
