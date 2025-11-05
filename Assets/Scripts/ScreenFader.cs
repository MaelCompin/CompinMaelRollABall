using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("Réglages du fondu")]
    [SerializeField] private Image fadeImage;       // L'image noire plein écran
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ garde le fader entre les scènes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadeImage != null)
        {
            // ✅ commence avec écran noir au tout début du jeu
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // ✅ fondu depuis le noir au lancement du jeu
        StartCoroutine(FadeOut());
    }

    // 🔹 Fondu vers le noir
    public IEnumerator FadeIn()
    {
        gameObject.SetActive(true); // s'assure que le canvas est visible
        float elapsed = 0f;

        Color c = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
    }

    // 🔹 Fondu depuis le noir vers transparent
    public IEnumerator FadeOut()
    {
        gameObject.SetActive(true);
        float elapsed = 0f;

        Color c = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
        gameObject.SetActive(false); // désactive après le fade pour libérer la vue
    }
}
