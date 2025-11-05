using UnityEngine;

public class RetryButton : MonoBehaviour
{
    public void OnRetryButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryGame();
        }
    }
}