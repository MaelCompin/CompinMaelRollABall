using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Déplacement")]
    public float baseSpeed = 5f;
    private float forwardSpeed;
    public float jumpForce = 3.2f;
    public float airGravity = -4.5f;
    public float fallGravity = -9.81f;
    public float groundGravity = -9.81f;
    public float groundCheckDistance = 0.8f;
    public LayerMask groundLayer;
    public string deathZoneTag = "DeathZone";

    [Header("Death Detection")]
    public float deathHeight = -5f;

    [Header("Audio")]
    public AudioClip switchDirectionSound;
    [Range(0f, 1f)] public float switchDirectionVolume = 1f;
    
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    
    public AudioClip endJumpSound;
    [Range(0f, 1f)] public float endJumpVolume = 1f;

    private Rigidbody rb;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool isFalling = false;
    private bool canMove = false;
    private bool isDead = false;
    private bool goingForward = true;
    private bool jumping = false;
    private bool isFinished = false;

    private BallSquash ballSquash;
    private AudioSource audioSource;

    public void UpdateSpeed(float multiplier)
    {
        forwardSpeed = baseSpeed * multiplier;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.freezeRotation = true;

        ballSquash = GetComponent<BallSquash>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        transform.rotation = Quaternion.identity;

        forwardSpeed = baseSpeed * (GameManager.Instance?.speedMultiplier ?? 1f);

        Invoke(nameof(EnableMovement), 0.1f);
    }

    private void EnableMovement() => canMove = true;

    void Update()
    {
        if (!canMove || isDead || isFinished) return;

        if (transform.position.y < deathHeight)
        {
            Die();
            return;
        }

        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        if (isGrounded && jumping)
            jumping = false;

        if (isGrounded && !wasGrounded)
        {
            ballSquash?.Impact();
            PlaySound(endJumpSound, endJumpVolume);
        }

        if ((Input.GetMouseButtonDown(0) || IsDirectionKeyPressed()) && isGrounded)
        {
            goingForward = !goingForward;
            float targetY = goingForward ? 0f : 90f;
            transform.rotation = Quaternion.Euler(0f, targetY, 0f);
            PlaySound(switchDirectionSound, switchDirectionVolume);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            DoJump();

        bool hasGroundBelow = CheckGrounded();
        isFalling = !hasGroundBelow && !isGrounded;

        if (isGrounded && rb.linearVelocity.magnitude < 0.05f)
        {
            rb.AddForce(-transform.forward * 3f, ForceMode.Impulse);
        }
    }

    private bool IsDirectionKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
               Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
               Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.C) ||
               Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F) ||
               Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.I) ||
               Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.L) ||
               Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.O) ||
               Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.R) ||
               Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.U) ||
               Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.X) ||
               Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z);
    }

    void FixedUpdate()
    {
        if (!canMove || isDead || isFinished) return;

        Vector3 forwardVel = transform.forward * forwardSpeed;
        rb.linearVelocity = new Vector3(forwardVel.x, rb.linearVelocity.y, forwardVel.z);

        float gravityForce = groundGravity;

        if (!isGrounded)
        {
            gravityForce = (jumping && rb.linearVelocity.y > 0) ? airGravity : fallGravity;
        }

        rb.AddForce(Vector3.up * gravityForce, ForceMode.Acceleration);
    }

    private void DoJump()
    {
        jumping = true;
        ballSquash?.Squash();
        PlaySound(jumpSound, jumpVolume);
        Invoke(nameof(PerformJump), 0.08f);
    }

    private void PerformJump()
    {
        ballSquash?.Stretch();

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private bool CheckGrounded()
    {
        float sphereRadius = 0.25f;
        return Physics.SphereCast(transform.position, sphereRadius, Vector3.down, out _, groundCheckDistance, groundLayer);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(deathZoneTag))
        {
            Die();
        }

        if (other.CompareTag("FinishZone"))
        {
            StopAtFinish();
        }
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        GameManager.Instance?.OnPlayerDeath();
    }

    public void ResetBall()
    {
        isDead = false;
        isFinished = false;
        canMove = false;
        jumping = false;
        goingForward = true;
        
        transform.rotation = Quaternion.identity;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
        
        Invoke(nameof(EnableMovement), 0.1f);
    }

    public void StopAtFinish()
    {
        if (isFinished) return;
        isFinished = true;
        StartCoroutine(SlowDownAndCelebrate());
    }

    private IEnumerator SlowDownAndCelebrate()
    {
        float startSpeed = forwardSpeed;
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            forwardSpeed = Mathf.Lerp(startSpeed, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        forwardSpeed = 0f;
        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(1f);
        GameManager.Instance?.OnLevelComplete();
    }

    public void FreezeBall()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        canMove = false;
    }

    public void UnfreezeBall()
    {
        rb.isKinematic = false;
        canMove = true;
    }
}
