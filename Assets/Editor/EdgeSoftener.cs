using UnityEngine;
using UnityEditor;

public class EdgeSoftener : EditorWindow
{
    private GameObject selectedObject;
    private float edgeSize = 0.3f;
    private float edgeHeight = 0.1f;
    private bool hideMeshes = true;

    [MenuItem("Tools/Soft Edges Generator")]
    public static void ShowWindow()
    {
        GetWindow<EdgeSoftener>("Soft Edges");
    }

    void OnGUI()
    {
        GUILayout.Label("🧱 Générateur de bords adoucis", EditorStyles.boldLabel);

        if (Selection.activeGameObject != null)
            selectedObject = Selection.activeGameObject;

        selectedObject = (GameObject)EditorGUILayout.ObjectField("Route cible :", selectedObject, typeof(GameObject), true);
        edgeSize = EditorGUILayout.FloatField("Largeur du biseau :", edgeSize);
        edgeHeight = EditorGUILayout.FloatField("Hauteur du biseau :", edgeHeight);
        hideMeshes = EditorGUILayout.Toggle("Masquer les meshes :", hideMeshes);

        if (GUILayout.Button("➡️ Ajouter les bords adoucis"))
        {
            if (selectedObject == null)
            {
                Debug.LogError("❌ Aucune route sélectionnée !");
                return;
            }
            AddSoftEdges(selectedObject, edgeSize, edgeHeight, hideMeshes);
        }
    }

    private void AddSoftEdges(GameObject parent, float size, float height, bool hide)
    {
        // Supprime les anciens bords
        foreach (Transform child in parent.transform)
        {
            if (child.name.StartsWith("SoftEdge_"))
                DestroyImmediate(child.gameObject);
        }

        BoxCollider box = parent.GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogError("❌ L’objet sélectionné n’a pas de BoxCollider !");
            return;
        }

        Vector3 halfSize = box.size * 0.5f;

        // Ajoute les quatre bords
        CreateEdge(parent, new Vector3(0, -halfSize.y, halfSize.z), Vector3.back, "Avant", size, height, hide);
        CreateEdge(parent, new Vector3(0, -halfSize.y, -halfSize.z), Vector3.forward, "Arrière", size, height, hide);
        CreateEdge(parent, new Vector3(halfSize.x, -halfSize.y, 0), Vector3.left, "Droite", size, height, hide);
        CreateEdge(parent, new Vector3(-halfSize.x, -halfSize.y, 0), Vector3.right, "Gauche", size, height, hide);

        Debug.Log($"✅ Bords adoucis ajoutés à {parent.name}");
    }

    private void CreateEdge(GameObject parent, Vector3 localPos, Vector3 dir, string suffix, float size, float height, bool hide)
    {
        GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edge.name = "SoftEdge_" + suffix;
        edge.transform.SetParent(parent.transform, false);

        edge.transform.localPosition = localPos;
        edge.transform.localRotation = Quaternion.LookRotation(dir) * Quaternion.Euler(45f, 0f, 0f);
        edge.transform.localScale = new Vector3(1f, height, size);

        if (hide)
            edge.GetComponent<MeshRenderer>().enabled = false;

        var rb = edge.GetComponent<Rigidbody>();
        if (rb) DestroyImmediate(rb);

        edge.GetComponent<Collider>().isTrigger = false;
        edge.layer = parent.layer;
    }
}
