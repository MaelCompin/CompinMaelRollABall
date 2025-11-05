using UnityEngine;
using UnityEngine.UI;
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
    public Image fadePanel;

    [Header("Stats")]
    public int runScore = 0;
    public int highScore = 0;
    public bool isPlaying = false;
    public float speedMultiplier = 1f;
    public float scoreMultiplier = 1f;
    public int loopCount = 0;

    [Header("Caméra")]
    public Camera mainCamera;
    public Vector3 menuCameraPosition = new Vector3(-13f, 7.5f, -7.5f);
    public Vector3 menuCameraRotation = new Vector3(19f, 40f, 0.2f);
    public float cameraTransitionDuration = 1.5f;
    public float celebrationRotationSpeed = 30f;
    public float celebrationDuration = 2f;

    [Header("Player")]
    public Vector3 playerStartPosition = new Vector3(-5.19f, 0.9f, 0.36f);

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;
    public float endLevelFadeDuration = 1f;

    [Header("Difficulty Scaling")]
    public float speedIncrease = 1.1f;
    public float scoreIncrease = 1.5f;

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

        if (ball != null)
        {
            playerStartPosition = ball.transform.position;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.position = menuCameraPosition;
            mainCamera.transform.rotation = Quaternion.Euler(menuCameraRotation);
        }

        if (menuUI != null) menuUI.SetActive(true);
        if (scoreUI != null) scoreUI.SetActive(false);
        if (retryUI != null)
        {
            retryUI.SetActive(false);
            Debug.Log("🎮 GameUI désactivé au démarrage");
        }

        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
        }

        if (ball != null) ball.FreezeBall();
        if (camFollow != null) camFollow.enabled = false;

        UpdateScoreUI();
    }

    public void PlayGame()
    {
        speedMultiplier = 1f;
        scoreMultiplier = 1f;
        loopCount = 0;
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        isPlaying = true;
        runScore = 0;

        if (menuUI != null)
            menuUI.SetActive(false);

        if (scoreUI != null)
            scoreUI.SetActive(true);

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

    public void RetryGame()
    {
        speedMultiplier = 1f;
        scoreMultiplier = 1f;
        loopCount = 0;
        StartCoroutine(RetryGameWithFade());
    }

    private IEnumerator RetryGameWithFade()
    {
        Debug.Log("🔁 Retry button pressed - Starting fade out...");
        
        yield return StartCoroutine(FadeToBlack());
        
        runScore = 0;
        
        if (retryUI != null)
        {
            retryUI.SetActive(false);
            
            CanvasGroup canvasGroup = retryUI.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            Transform deathPanel = retryUI.transform.Find("DeathPanel");
            if (deathPanel != null)
            {
                deathPanel.gameObject.SetActive(false);
            }
        }

        if (ball != null)
        {
            ball.transform.position = playerStartPosition;
            ball.ResetBall();
            
            BallScore ballScore = ball.GetComponent<BallScore>();
            if (ballScore != null)
            {
                ballScore.ResetScore();
            }
        }

        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.ResetAllCrystals();
        }

        if (camFollow != null && ball != null)
        {
            camFollow.target = ball.transform;
            camFollow.enabled = false;
            
            if (mainCamera != null)
            {
                Vector3 targetPos = ball.transform.position + ball.transform.TransformDirection(new Vector3(0f, 4f, -6f));
                mainCamera.transform.position = targetPos;
                mainCamera.transform.rotation = Quaternion.LookRotation(ball.transform.position - targetPos);
            }
            
            camFollow.enabled = true;
        }

        yield return new WaitForSeconds(0.1f);
        
        isPlaying = true;

        if (scoreUI != null)
            scoreUI.SetActive(true);

        if (ball != null)
            ball.UnfreezeBall();

        UpdateScoreUI();
        
        yield return StartCoroutine(FadeFromBlack());
        
        Debug.Log("✅ Game restarted!");
    }

    private IEnumerator FadeToBlack()
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        c.a = 1f;
        fadePanel.color = c;
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        c.a = 0f;
        fadePanel.color = c;
    }

    private IEnumerator SlowFadeToBlack()
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < endLevelFadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / endLevelFadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        c.a = 1f;
        fadePanel.color = c;
    }

    private IEnumerator SlowFadeFromBlack()
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < endLevelFadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / endLevelFadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        c.a = 0f;
        fadePanel.color = c;
    }

    public void AddScore(int amount)
    {
        int finalAmount = Mathf.RoundToInt(amount * scoreMultiplier);
        runScore += finalAmount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = runScore.ToString();
            scoreText.fontSize = 60f;
            scoreText.color = Color.white;
        }
    }

    public void OnPlayerDeath()
    {
        Debug.Log($"💀 OnPlayerDeath called! retryUI null? {retryUI == null}");
        
        isPlaying = false;

        if (scoreUI != null)
            scoreUI.SetActive(false);

        if (retryUI != null)
        {
            Debug.Log($"🎮 Activating GameUI... Current state: {retryUI.activeSelf}");
            retryUI.SetActive(true);
            
            CanvasGroup canvasGroup = retryUI.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                Debug.Log("✅ CanvasGroup alpha set to 1");
            }

            Transform deathPanel = retryUI.transform.Find("DeathPanel");
            if (deathPanel != null)
            {
                deathPanel.gameObject.SetActive(true);
                Debug.Log("✅ DeathPanel activated");
            }
            
            Debug.Log($"🎮 GameUI activated! New state: {retryUI.activeSelf}");
        }
        else
        {
            Debug.LogError("❌ retryUI is NULL! Assign GameUI in Inspector!");
        }

        if (retryScoreText != null)
        {
            retryScoreText.text = "Score : " + runScore;
            Debug.Log($"📊 Score text set to: {retryScoreText.text}");
        }
        else
        {
            Debug.LogError("❌ retryScoreText is NULL!");
        }

        if (runScore > highScore)
            highScore = runScore;

        if (highScoreText != null)
        {
            highScoreText.text = "Highscore : " + highScore;
            Debug.Log($"🏆 Highscore text set to: {highScoreText.text}");
        }
        else
        {
            Debug.LogError("❌ highScoreText is NULL!");
        }
    }

    public void OnLevelComplete()
    {
        StartCoroutine(LevelCompleteSequence());
    }

    private IEnumerator LevelCompleteSequence()
    {
        isPlaying = false;
        
        Debug.Log($"🎉 Level complete! Loop {loopCount + 1}");

        if (camFollow != null)
            camFollow.enabled = false;

        if (mainCamera != null && ball != null)
        {
            Vector3 cameraOffset = new Vector3(0f, 3f, -5f);
            Vector3 targetPos = ball.transform.position + cameraOffset;
            mainCamera.transform.position = targetPos;

            float elapsed = 0f;
            while (elapsed < celebrationDuration)
            {
                mainCamera.transform.RotateAround(ball.transform.position, Vector3.up, celebrationRotationSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        yield return StartCoroutine(SlowFadeToBlack());

        loopCount++;
        speedMultiplier *= speedIncrease;
        scoreMultiplier *= scoreIncrease;

        Debug.Log($"🚀 New speed multiplier: {speedMultiplier:F2}x, Score multiplier: {scoreMultiplier:F2}x");

        if (ball != null)
        {
            ball.transform.position = playerStartPosition;
            ball.ResetBall();
            ball.UpdateSpeed(speedMultiplier);
            
            BallScore ballScore = ball.GetComponent<BallScore>();
            if (ballScore != null)
            {
                ballScore.ResetScore();
            }
        }

        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.ResetAllCrystals();
        }

        if (camFollow != null && ball != null)
        {
            camFollow.target = ball.transform;
            camFollow.enabled = false;
            
            if (mainCamera != null)
            {
                Vector3 targetPos = ball.transform.position + ball.transform.TransformDirection(new Vector3(0f, 4f, -6f));
                mainCamera.transform.position = targetPos;
                mainCamera.transform.rotation = Quaternion.LookRotation(ball.transform.position - targetPos);
            }
            
            camFollow.enabled = true;
        }

        yield return new WaitForSeconds(0.2f);

        isPlaying = true;

        if (ball != null)
            ball.UnfreezeBall();

        UpdateScoreUI();

        yield return StartCoroutine(SlowFadeFromBlack());

        Debug.Log("✅ Next loop started!");
    }
}
