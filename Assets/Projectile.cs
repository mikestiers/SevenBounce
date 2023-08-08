using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Projectile : MonoBehaviour
{
    public Slider velocitySlider;
    public Text velocitySliderText;
    public Slider angleSlider;
    public Text angleSliderText;
    public Text highScoreText;
    public Text bounceCountText;
    public int highScore = 0;
    public float initialVelocity = 10f; // Initial velocity of the projectile
    public float launchAngle = 45f; // Launch angle of the projectile in degrees
    public float gravity = 9.8f; // Gravity value
    public float bounciness = 0.8f; // Bounciness factor (friction)
    public float currentSpeed; // Current speed of the projectile for use when velocity changes based on collision

    private Vector3 initialPosition;
    private float timeElapsed;
    private bool isLaunched;
    private bool isBouncing;
    private Vector3 bounceVelocity;

    private int bounceCount = 0;
    private int previousBounceCount = 0;

    private void LaunchProjectile()
    {
        ResetProjectile();
        bounceCount = 0;
        // Convert the launch angle from degrees to radians
        //float launchAngleRad = angleSlider.value * Mathf.Deg2Rad;

        // Set the initial time elapsed to 0
        timeElapsed = 0f;

        // Set the projectile as launched
        isLaunched = true;
        Debug.Log("Launch Velocity: " + currentSpeed);
    }

    private void UpdateProjectilePosition()
    {
        // Calculate the current horizontal position
        float currentX = currentSpeed * Mathf.Cos(launchAngle * Mathf.Deg2Rad) * timeElapsed;

        // Calculate the current vertical position
        float currentY = (currentSpeed * Mathf.Sin(launchAngle * Mathf.Deg2Rad) * timeElapsed) -
                        (0.5f * gravity * timeElapsed * timeElapsed);

        // Set the new position of the projectile
        transform.position = initialPosition + new Vector3(currentX, currentY, 0f);

        // Update the time elapsed
        timeElapsed += Time.deltaTime;
    }

    void UpdateBouncingProjectilePosition()
    {
        // Update the position based on the bounce velocity
        transform.position += bounceVelocity * Time.deltaTime;

        // Apply gravity to the bounce velocity
        bounceVelocity.y -= gravity * Time.deltaTime;
    }

    private Vector3 CalculateBounceVelocity(Vector3 surfaceNormal)
    {
        bounceCount += 1;
        HudManager.singleton.bounceCount.text = "Bounce Count: " + bounceCount;
        if (bounceCount >= highScore && bounceCount == previousBounceCount + 1)
        {
            highScore = bounceCount;
            HudManager.singleton.highScore.text = "High Score: " + highScore;
        }
        if (bounceCount == previousBounceCount + 2)
        {
            Debug.Log("Too many bounces");
            HudManager.singleton.gameOverText.text = "Too many bounces! R to try again";
            ResetProjectile();
        }
            
        currentSpeed *= 0.8f;
        // Calculate the current velocity
        Vector3 incomingVelocity = new Vector3(
            currentSpeed * Mathf.Cos(launchAngle * Mathf.Deg2Rad),
            currentSpeed * Mathf.Sin(launchAngle * Mathf.Deg2Rad) - gravity * timeElapsed,
            0f);

        // Calculate the reflection
        Vector3 reflection = incomingVelocity - 2 * (Vector3.Dot(incomingVelocity, surfaceNormal)) * surfaceNormal;

        // Apply the bounciness factor with no energy loss
        Vector3 bounce = reflection * bounciness;
        Debug.Log("Bounce Count: " + bounceCount);
        Debug.Log("Current Speed: " + currentSpeed);
        Debug.Log("Time Elapsed: " + timeElapsed);
        Debug.Log("Initial Velocity: " + initialVelocity);
        Debug.Log("Incoming Velocity: " + incomingVelocity);
        Debug.Log("Reflection: " + reflection);
        Debug.Log("Bounce Vector: " + bounce);
        return bounce;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Ground hit");
            if (bounceCount == previousBounceCount)
            {
                Debug.Log("Not enough bounces");
                HudManager.singleton.gameOverText.text = "Not enough bounces! R to try again";
            }
            ResetProjectile();
            //isLaunched = true; // so it can reset the bouncing calculations before setting isBouncing = true
            //if (isLaunched)
            //{
            //    isLaunched = false;
            //    isBouncing = true;
            //    Vector3 surfaceNormal = collision.contacts[0].normal;
            //    Debug.Log("surfaceNormal " + surfaceNormal);

            //    // Calculate the new bounce velocity each frame
            //    bounceVelocity = CalculateBounceVelocity(surfaceNormal);
            //}
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall hit");
            isLaunched = true; // so it can reset the bouncing calculations before setting isBouncing = true
            if (isLaunched)
            {
                isLaunched = false;
                isBouncing = true;
                Vector3 surfaceNormal = collision.contacts[0].normal;
                Debug.Log("surfaceNormal " + surfaceNormal);

                // Calculate the new bounce velocity each frame
                bounceVelocity = CalculateBounceVelocity(surfaceNormal);
            }
        }
    }

    public void ResetProjectile()
    {
        // Reset position
        transform.position = initialPosition;

        //
        velocitySlider.value = initialVelocity;
        currentSpeed = velocitySlider.value;
        angleSlider.value = launchAngle;
        //

        // Reset other properties
        isLaunched = false;
        isBouncing = false;
        timeElapsed = 0f;
        currentSpeed = velocitySlider.value;
        bounceVelocity = Vector3.zero;
        if (bounceCount == previousBounceCount + 2)
        {
            previousBounceCount = 0;
        }
        else
            previousBounceCount = bounceCount;
        HudManager.singleton.bounceCount.text = "Bounce Count: " + bounceCount;
        HudManager.singleton.previousBounceCount.text = "Previous Bounce Count: " + previousBounceCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        ResetProjectile();
    }

    // Update is called once per frame
    void Update()
    {

        velocitySliderText.text = "Velocity: " + velocitySlider.value.ToString();
        angleSliderText.text = "Angle: " + angleSlider.value.ToString();
        // Adjust angle manually
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            launchAngle += 1f;
            angleSlider.value = launchAngle;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            launchAngle -= 1f;
            angleSlider.value = launchAngle;
        }
        // Reset the projectile
        if (Input.GetKeyDown(KeyCode.R))
        {
            HudManager.singleton.gameOverText.text = "";
            bounceCount = 0;
            previousBounceCount = 0;
            ResetProjectile();
        }
        // Adjust velocity manually
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            initialVelocity += 1f;
            velocitySlider.value = initialVelocity;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            initialVelocity -= 1f;
            velocitySlider.value = initialVelocity;
        }

        // Launch the projectile
        if (Input.GetKeyDown(KeyCode.Space) && !isLaunched)
        {
            LaunchProjectile();
        }

        // Update the projectile's position if it is launched
        if (isLaunched)
        {
            UpdateProjectilePosition();
        }
        if (isBouncing)
        {
            UpdateBouncingProjectilePosition();
        }
    }
}
