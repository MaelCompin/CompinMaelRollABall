using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance;

    private GameObject[] allCrystals;

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

        GameObject[] crystalObjects = GameObject.FindGameObjectsWithTag("Crystal");
        allCrystals = new GameObject[crystalObjects.Length];
        for (int i = 0; i < crystalObjects.Length; i++)
        {
            allCrystals[i] = crystalObjects[i];
        }
    }

    public void ResetAllCrystals()
    {
        foreach (GameObject crystal in allCrystals)
        {
            if (crystal != null)
            {
                crystal.SetActive(true);
            }
        }
    }
}