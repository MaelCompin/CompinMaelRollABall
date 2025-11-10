using UnityEngine;
using System.Collections.Generic;

public class LevelColorManager : MonoBehaviour
{
    public static LevelColorManager Instance;

    [Header("Références")]
    public GameObject playerBall;
    public Camera mainCamera;
    public LayerMask roadLayer;

    [Header("Intensité des couleurs")]
    [Range(0f, 1f)]
    public float roadDarkenFactor = 0.7f;
    [Range(0f, 1f)]
    public float ballDarkenFactor = 0.5f;
    [Range(0f, 1f)]
    public float backgroundDarkenFactor = 0.3f;

    [Header("Exclusions")]
    public string[] excludedTags = new string[] { "Crystal" };

    private Color currentBaseColor;
    private List<Material> roadMaterials = new List<Material>();
    private List<Material> environmentMaterials = new List<Material>();
    private Material ballMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        CacheAllMaterials();
        ApplyRandomColors();
    }

    private void CacheAllMaterials()
    {
        roadMaterials.Clear();
        environmentMaterials.Clear();

        MeshRenderer[] allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

        foreach (MeshRenderer renderer in allRenderers)
        {
            if (renderer.gameObject == playerBall)
            {
                if (renderer.material != null)
                {
                    ballMaterial = renderer.material;
                }
                continue;
            }

            if (IsExcluded(renderer.gameObject))
            {
                continue;
            }

            bool isRoad = IsInLayerMask(renderer.gameObject, roadLayer);

            foreach (Material mat in renderer.materials)
            {
                if (mat == null) continue;

                if (isRoad)
                {
                    if (!roadMaterials.Contains(mat))
                    {
                        roadMaterials.Add(mat);
                    }
                }
                else
                {
                    if (!environmentMaterials.Contains(mat))
                    {
                        environmentMaterials.Add(mat);
                    }
                }
            }
        }
    }

    public void ApplyRandomColors()
    {
        currentBaseColor = GenerateRandomColor();
        
        Color roadColor = currentBaseColor * roadDarkenFactor;
        Color ballColor = currentBaseColor * ballDarkenFactor;
        Color backgroundColor = currentBaseColor * backgroundDarkenFactor;

        foreach (Material mat in environmentMaterials)
        {
            if (mat != null)
            {
                mat.color = currentBaseColor;
            }
        }

        foreach (Material mat in roadMaterials)
        {
            if (mat != null)
            {
                mat.color = roadColor;
            }
        }

        if (ballMaterial != null)
        {
            ballMaterial.color = ballColor;
        }

        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = backgroundColor;
        }
    }

    private Color GenerateRandomColor()
    {
        float hue = Random.Range(0f, 1f);
        float saturation = Random.Range(0.7f, 1f);
        float value = Random.Range(0.6f, 0.9f);
        
        return Color.HSVToRGB(hue, saturation, value);
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((1 << obj.layer) & layerMask) != 0;
    }

    private bool IsExcluded(GameObject obj)
    {
        foreach (string tag in excludedTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    private string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
}
