using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    public static DeathMenu Instance;

    [Header("Composants UI")]
    [SerializeField] private CanvasGroup deathCanvas; // Le panneau transparent avec les boutons
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        Instance = this;
        deathCanvas.alpha = 0f;
        deathCanvas.gameObject.SetActive(false);

        retryButton.onClick.AddListener(RestartLevel);
    }

    public void ShowDeathMenu()
    {
        deathCanvas.gameObject.SetActive(true);
        StartCoroutine(FadeInCanvas());
    }

    private System.Collections.IEnumerator FadeInCanvas()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.5f;
            deathCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        deathCanvas.interactable = true;
        deathCanvas.blocksRaycasts = true;
    }

    private void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}