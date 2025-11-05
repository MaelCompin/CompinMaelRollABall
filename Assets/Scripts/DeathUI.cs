using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeathUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject deathPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public Button retryButton;

    private void Start()
    {
        deathPanel.SetActive(false);
        retryButton.onClick.AddListener(OnRetry);
    }

    public void ShowDeathScreen(float score, float highScore)
    {
        deathPanel.SetActive(true);
        scoreText.text = $"Score : {score:0}";
        highScoreText.text = $"Meilleur score : {highScore:0}";
    }

    private void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}