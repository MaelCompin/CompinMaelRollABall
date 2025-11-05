using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cible à suivre")]
    public Transform target;

    [Header("Position de suivi")]
    public Vector3 offset = new Vector3(0f, 4f, -6f);
    public float followSpeed = 5f;
    public float rotationSmooth = 5f;

    [Header("Regard")]
    public bool lookAtTarget = true;

    void LateUpdate()
    {
        if (!target) return;

        // Position toujours alignée avec la rotation de la boule
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Déplacement fluide
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Regarde la boule
        if (lookAtTarget)
        {
            Quaternion targetRot = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSmooth * Time.deltaTime);
        }
    }
}