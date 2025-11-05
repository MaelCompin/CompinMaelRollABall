using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Références UI")]
    public Canvas menuUI;        // Ton menu principal
    public Canvas scoreUI;       // Ton Canvas de score (facultatif)

    [Header("Références gameplay")]
    public Camera mainCamera;
    public Transform playerBall;
    public Vector3 cameraMenuPosition = new Vector3(-13f, 7.5f, -7.5f);
    public Vector3 cameraMenuRotation = new Vector3(19f, 40f, 0.2f);
    public float cameraTransitionTime = 2f;

    private BallController ballController;
    private CameraFollow cameraFollow;

    void Start()
    {
        // Cherche la boule automatiquement
        if (playerBall == null)
        {
            GameObject ballObj = GameObject.FindWithTag("Player");
            if (ballObj != null)
                playerBall = ballObj.transform;
        }

        if (playerBall != null)
        {
            ballController = playerBall.GetComponent<BallController>();
            if (ballController != null)
                Invoke(nameof(FreezeBallSafely), 0.1f);
        }

        // Désactive CameraFollow tant qu'on est dans le menu
        if (mainCamera != null)
        {
            cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow != null)
                cameraFollow.enabled = false;

            mainCamera.transform.position = cameraMenuPosition;
            mainCamera.transform.rotation = Quaternion.Euler(cameraMenuRotation);
        }

        // Masquer le score au démarrage
        if (scoreUI != null)
            scoreUI.enabled = false;
    }

    private void FreezeBallSafely()
    {
        ballController?.FreezeBall();
        Debug.Log("❄️ Boule gelée (Menu)");
    }

    public void PlayGame()
    {
        Debug.Log("▶️ Bouton 'Jouer' pressé !");
        StartCoroutine(TransitionToGame());
    }

    private IEnumerator TransitionToGame()
    {
        Debug.Log("🎬 Début de la transition caméra");

        // Cache le menu
        if (menuUI != null)
            menuUI.enabled = false;

        yield return new WaitForSeconds(0.2f);

        if (mainCamera == null || playerBall == null)
        {
            Debug.LogWarning("❌ Caméra ou PlayerBall manquant !");
            yield break;
        }

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 targetPos = playerBall.position + playerBall.TransformDirection(new Vector3(0f, 4f, -6f));
        Quaternion targetRot = Quaternion.LookRotation(playerBall.position - targetPos);

        float elapsed = 0f;
        while (elapsed < cameraTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / cameraTransitionTime);
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        Debug.Log("✅ Transition terminée, caméra + boule actives");

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        ballController?.UnfreezeBall();

        // Affiche le score
        if (scoreUI != null)
        {
            scoreUI.enabled = true;
            Debug.Log("🏁 Score affiché !");
        }
    }
}
