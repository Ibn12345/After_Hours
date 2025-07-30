using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{   
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private int count;
    private float movementX;
    private float movementY;
    public float speed = 0;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public GameObject instructionText;
    public TextMeshProUGUI speedText;

    private float smoothedSpeed = 0f;
    private float speedVelocity = 0f;
    public float speedSmoothTime = 0.2f;

    private float targetVolume = 0f;
    public float volumeFadeSpeed = 2f;

    public Image redFlashImage;
    public TextMeshProUGUI gameOverText;
    public float flashDuration = 0.5f;

    [Header("Skateboarding Sound")]
    public AudioClip skateboardingSoundClip;
    public float soundPlaySpeedThreshold = 1.0f;

    public GameManager gameManager;

    public Camera firstPersonCamera;
    public GameObject flashlightObject;
    private bool isFlashlightOn = false;
    public float mouseSensitivity = 2.0f;

    public float standingHeight = 2.0f;
    public float standingCameraY = 0.7f;

    public LayerMask groundLayer;
    public float groundCheckDistance = 0.5f;
    public float groundAlignSpeed = 10.0f;
    public float hoverHeight = 0.1f;
    public float flatGroundAngleThreshold = 5.0f;

    private float cameraPitch = 0.0f;
    private float targetHeight;
    private float targetCameraY;
    private AudioSource audioSource;

    [Header("Death Settings")]
    public AudioClip deathSound;

    public GameObject enemy;

    [SerializeField] private Animator doorsOpen = null;
    public GameObject doors;
    public GameObject endScreenPanel;

    [Header("Gravity Boost")]
    public float heightThreshold = 10f;
    public float extraGravity = 20f;

    [Header("Air Tilt")]
    public float airTiltSpeed = 2f;
    private bool isGrounded = false;

    private Vector3 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        count = 0;
        SetCountText();
        winTextObject.SetActive(false);
        instructionText.SetActive(false);
        enemy.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        redFlashImage.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetHeight = standingHeight;
        targetCameraY = standingCameraY;

        if (redFlashImage == null)
            redFlashImage = GameObject.Find("RedFlash").GetComponent<Image>();

        if (gameOverText == null)
            gameOverText = GameObject.Find("GameOverText").GetComponent<TextMeshProUGUI>();

        if (firstPersonCamera != null)
        {
            firstPersonCamera.transform.localPosition = new Vector3(0, standingCameraY, 0);
            firstPersonCamera.transform.localRotation = Quaternion.identity;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("PlayerController requires an AudioSource component!");
            enabled = false;
            return;
        }

        audioSource.clip = skateboardingSoundClip;
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 movement = transform.right * movementX + transform.forward * movementY;
        rb.AddForce(movement * speed);

        if (transform.position.y > heightThreshold)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, Color.red);

        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            float angleToUp = Vector3.Angle(hit.normal, Vector3.up);
            Quaternion targetRotation = angleToUp < flatGroundAngleThreshold
                ? Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0)
                : Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * groundAlignSpeed);

            Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y + hoverHeight, transform.position.z);
            rb.position = Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * groundAlignSpeed);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(15f, transform.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * airTiltSpeed);
        }
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * mouseX * mouseSensitivity);
        cameraPitch -= mouseY * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        if (firstPersonCamera != null)
        {
            firstPersonCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }

        Vector3 horizontalDisplacement = transform.position - lastPosition;
        horizontalDisplacement.y = 0f;
        float rawSpeed = horizontalDisplacement.magnitude / Time.deltaTime;
        smoothedSpeed = Mathf.SmoothDamp(smoothedSpeed, rawSpeed, ref speedVelocity, speedSmoothTime);
        speedText.text = "Speed: " + smoothedSpeed.ToString("F2");

        targetVolume = smoothedSpeed > soundPlaySpeedThreshold ? 1f : 0f;

        if (audioSource != null)
        {
            if (!audioSource.isPlaying && targetVolume > 0f)
            {
                audioSource.loop = true;
                audioSource.Play();
            }

            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, volumeFadeSpeed * Time.deltaTime);

            if (audioSource.volume <= 0f && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        lastPosition = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }

        if (other.gameObject.CompareTag("TriggerItem1"))
        {
            instructionText.GetComponent<TextMeshProUGUI>().text = "Find your way into the park...";
            instructionText.SetActive(true);
        }

        if (other.gameObject.CompareTag("Trigger2"))
        {
            instructionText.GetComponent<TextMeshProUGUI>().text = "Find keys to escape";
            enemy.SetActive(true);
        }

        if (other.gameObject.CompareTag("Triggeritem3"))
        {
            if (endScreenPanel != null)
            {
                endScreenPanel.SetActive(true);
            }

            rb.linearVelocity = Vector3.zero;
            movementX = 0f;
            movementY = 0f;

            if (audioSource != null)
            {
                audioSource.Stop();
            }

            StartCoroutine(EndSequence());
        }
    }

    IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(3f);
        gameManager.LoadTitleScreen();
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void OnFlashlightToggle()
    {
        if (flashlightObject != null)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightObject.SetActive(isFlashlightOn);
        }
    }

    private bool CanStandUp()
    {
        if (capsuleCollider == null) return true;

        Vector3 origin = transform.position + Vector3.up * capsuleCollider.radius;
        float distance = standingHeight - 1.0f;

        return !Physics.SphereCast(origin, capsuleCollider.radius * 0.9f, Vector3.up, out RaycastHit hit, distance);
    }

    void SetCountText()
    {
        countText.text = "Keys: " + count + "/5";
        enemy = GameObject.FindGameObjectWithTag("Enemy");

        if (count >= 5)
        {
            winTextObject.SetActive(true);
            if (enemy != null)
            {
                Destroy(enemy, 2f);
                doorsOpen.Play("DoorsLift", 0, 6f);
                instructionText.GetComponent<TextMeshProUGUI>().text = "Enter The Basement";
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("SkatePark"))
        {
            speed = 20;
        }
        else
        {
            speed = 12;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(HandlePlayerDeath());
        }
    }

    IEnumerator HandlePlayerDeath()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        float elapsed = 0f;
        redFlashImage.color = new Color(1f, 0f, 0f, 0f);
        redFlashImage.gameObject.SetActive(true);

        while (elapsed < flashDuration)
        {
            float alpha = Mathf.PingPong(elapsed * 4f, 1f);
            redFlashImage.color = new Color(1f, 0f, 0f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        redFlashImage.color = new Color(1f, 0f, 0f, 1f);
        gameOverText.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return new WaitForSeconds(2f);
        gameManager.LoadTitleScreen();
    }
}