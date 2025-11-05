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

        // ✅ Orientation neutre — la boule va dans l'axe de la route dès le départ
        transform.rotation = Quaternion.identity;

        forwardSpeed = baseSpeed * (GameManager.Instance?.speedMultiplier ?? 1f);

        Invoke(nameof(EnableMovement), 0.1f);
    }

    private void EnableMovement() => canMove = true;

    void Update()
    {
        if (!canMove || isDead || isFinished) return;

        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        if (isGrounded && jumping)
            jumping = false;

        if (isGrounded && !wasGrounded && rb.linearVelocity.y < -1f)
            ballSquash?.Impact();

        // Rotation à 90° entre X et Z UNIQUEMENT quand la boule est sur la route
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.RightArrow)) && isGrounded)
        {
            goingForward = !goingForward;
            float targetY = goingForward ? 0f : 90f;
            transform.rotation = Quaternion.Euler(0f, targetY, 0f);
        }

        // Saut
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            DoJump();

        bool hasGroundBelow = CheckGrounded();
        isFalling = !hasGroundBelow && !isGrounded;

        // Petit boost si bloquée sur un bord
        if (isGrounded && rb.linearVelocity.magnitude < 0.05f)
        {
            rb.AddForce(-transform.forward * 3f, ForceMode.Impulse);
        }
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
        float sphereRadius = 0.35f;
        return Physics.SphereCast(transform.position, sphereRadius, Vector3.down, out _, groundCheckDistance, groundLayer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(deathZoneTag))
        {
            if (!isDead)
            {
                isDead = true;
                rb.linearVelocity = Vector3.zero;
                GameManager.Instance?.OnPlayerDeath();
            }
        }

        if (other.CompareTag("FinishZone"))
        {
            StopAtFinish();
        }
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
