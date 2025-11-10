using UnityEngine;
using System.Collections;

public class BallSquash : MonoBehaviour
{
    [Header("Références")]
    public Transform ballVisual;

    [Header("Paramètres d'animation")]
    public float squashScale = 0.8f;
    public float stretchScale = 1.2f;
    public float impactScale = 0.7f;
    public float animationSpeed = 10f;

    private Vector3 originalScale;
    private Coroutine currentRoutine;
    private Rigidbody rb;
    private float lastYVelocity;

    void Start()
    {
        if (ballVisual == null)
            ballVisual = transform;

        originalScale = ballVisual.localScale;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb != null)
        {
            lastYVelocity = rb.linearVelocity.y;
        }
    }

    public void Squash()
    {
        RestartRoutine(SquashRoutine());
    }

    public void Stretch()
    {
        RestartRoutine(StretchRoutine());
    }

    public void Impact()
    {
        float impactForce = Mathf.Abs(lastYVelocity);
        if (impactForce > 1f)
        {
            RestartRoutine(ImpactRoutine());
        }
    }

    private void RestartRoutine(IEnumerator routine)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(routine);
    }

    private IEnumerator SquashRoutine()
    {
        yield return AnimateTo(new Vector3(originalScale.x * 1.2f, originalScale.y * squashScale, originalScale.z * 1.2f));
    }

    private IEnumerator StretchRoutine()
    {
        yield return AnimateTo(new Vector3(originalScale.x * 0.8f, originalScale.y * stretchScale, originalScale.z * 0.8f));
    }

    private IEnumerator ImpactRoutine()
    {
        Vector3 target = new Vector3(originalScale.x * 1.3f, originalScale.y * impactScale, originalScale.z * 1.3f);

        while (Vector3.Distance(ballVisual.localScale, target) > 0.01f)
        {
            ballVisual.localScale = Vector3.Lerp(ballVisual.localScale, target, Time.deltaTime * (animationSpeed * 2));
            yield return null;
        }

        while (Vector3.Distance(ballVisual.localScale, originalScale) > 0.01f)
        {
            ballVisual.localScale = Vector3.Lerp(ballVisual.localScale, originalScale, Time.deltaTime * animationSpeed);
            yield return null;
        }

        ballVisual.localScale = originalScale;
        currentRoutine = null;
    }

    private IEnumerator AnimateTo(Vector3 targetScale)
    {
        while (Vector3.Distance(ballVisual.localScale, targetScale) > 0.01f)
        {
            ballVisual.localScale = Vector3.Lerp(ballVisual.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null;
        }

        while (Vector3.Distance(ballVisual.localScale, originalScale) > 0.01f)
        {
            ballVisual.localScale = Vector3.Lerp(ballVisual.localScale, originalScale, Time.deltaTime * animationSpeed);
            yield return null;
        }

        ballVisual.localScale = originalScale;
        currentRoutine = null;
    }
}
