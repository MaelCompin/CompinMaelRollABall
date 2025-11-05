using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject menuUI;
    public GameObject scoreUI;
    public TextMeshProUGUI scoreText;
    public GameObject retryUI;
    public TextMeshProUGUI retryScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Stats")]
    public int runScore = 0;
    public int highScore = 0;
    public bool isPlaying = false;
    public float speedMultiplier = 1f;

    [Header("Caméra")]
    public Camera mainCamera;
    public Vector3 menuCameraPosition = new Vector3(-13f, 7.5f, -7.5f);
    public Vector3 menuCameraRotation = new Vector3(19f, 40f, 0.2f);
    public float cameraTransitionDuration = 1.5f;

    private BallController ball;
    private CameraFollow camFollow;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ball = FindAnyObjectByType<BallController>();
        camFollow = FindAnyObjectByType<CameraFollow>();
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            mainCamera.transform.position = menuCameraPosition;
            mainCamera.transform.rotation = Quaternion.Euler(menuCameraRotation);
        }

        if (menuUI != null) menuUI.SetActive(true);
        if (scoreUI != null) scoreUI.SetActive(true);
        if (retryUI != null) retryUI.SetActive(false);

        if (ball != null) ball.FreezeBall();
        if (camFollow != null) camFollow.enabled = false;

        UpdateScoreUI();
    }

    public void PlayGame()
    {
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        isPlaying = true;
        runScore = 0;

        if (menuUI != null)
            menuUI.SetActive(false);

        if (mainCamera != null && ball != null)
        {
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            Vector3 endPos = ball.transform.position + ball.transform.TransformDirection(new Vector3(0f, 4f, -6f));
            Quaternion endRot = Quaternion.LookRotation(ball.transform.position - endPos);

            float t = 0f;
            while (t < cameraTransitionDuration)
            {
                mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t / cameraTransitionDuration);
                mainCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t / cameraTransitionDuration);
                t += Time.deltaTime;
                yield return null;
            }

            mainCamera.transform.position = endPos;
            mainCamera.transform.rotation = endRot;
        }

        if (camFollow != null && ball != null)
        {
            camFollow.target = ball.transform;
            camFollow.enabled = true;
        }

        if (ball != null)
            ball.UnfreezeBall();

        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        runScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = runScore.ToString();
            scoreText.fontSize = 150f;
            scoreText.color = Color.white;
        }
    }

    public void OnPlayerDeath()
    {
        isPlaying = false;

        if (retryUI != null)
        {
            retryUI.SetActive(true);
        }

        if (retryScoreText != null)
            retryScoreText.text = "Score : " + runScore;

        if (runScore > highScore)
            highScore = runScore;

        if (highScoreText != null)
            highScoreText.text = "Highscore : " + highScore;
    }

    public void OnLevelComplete()
    {
        isPlaying = false;

        if (retryUI != null)
            retryUI.SetActive(true);

        if (retryScoreText != null)
            retryScoreText.text = "Score : " + runScore;

        if (runScore > highScore)
            highScore = runScore;

        if (highScoreText != null)
            highScoreText.text = "Highscore : " + highScore;
    }
}
