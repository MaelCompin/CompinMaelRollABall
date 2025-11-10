using UnityEngine;

public class BallScore : MonoBehaviour
{
    [Header("Score automatique")]
    public float distanceStep = 5f;
    private float nextStep = 5f;
    private Vector3 startPos;

    [Header("Audio")]
    public AudioClip pickCrystalSound;
    [Range(0f, 1f)] public float pickCrystalVolume = 1f;
    
    private AudioSource audioSource;

    void Start()
    {
        startPos = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;

        float distance = Vector3.Distance(startPos, transform.position);

        if (distance >= nextStep)
        {
            int steps = Mathf.FloorToInt(distance / distanceStep);
            nextStep = (steps + 1) * distanceStep;

            GameManager.Instance.AddScore(1);
        }
    }

    public void ResetScore()
    {
        startPos = transform.position;
        nextStep = distanceStep;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crystal"))
        {
            CrystalPickup crystal = other.GetComponent<CrystalPickup>();
            if (crystal != null)
            {
                GameManager.Instance.AddScore(crystal.scoreValue);
                
                if (pickCrystalSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(pickCrystalSound, pickCrystalVolume);
                }
                
                other.gameObject.SetActive(false);
            }
        }
    }
}