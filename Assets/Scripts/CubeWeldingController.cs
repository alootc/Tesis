using System.Collections.Generic;
using UnityEngine;


public class CubeWeldingController : MonoBehaviour
{
    public bool hasAcid = false; // Indica si el cubo ya tiene ácido aplicado
    public bool isWelded = false; // Indica si el cubo ya está soldado

    public GameObject weldingParticlePrefab; // Prefab de partículas de soldadura
    public Material weldingMaterial; // Material para el cordón de soldadura

    private List<Vector3> weldPoints = new List<Vector3>(); // Puntos del cordón de soldadura
    private LineRenderer weldLine; // Línea que representa el cordón

    private void Start()
    {
        // Crear el LineRenderer para el cordón de soldadura
        weldLine = gameObject.AddComponent<LineRenderer>();
        weldLine.startWidth = 0.05f;
        weldLine.endWidth = 0.05f;
        weldLine.material = weldingMaterial;
        weldLine.positionCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si la pistola de soldadura toca el cubo y si el cubo tiene ácido aplicado
        if (!isWelded && hasAcid && other.CompareTag("WeldingGun"))
        {
            Debug.Log("Pistola de soldadura tocó el cubo con ácido.");

            // Buscar otros cubos cercanos con ácido aplicado
            Collider[] nearbyCubes = Physics.OverlapSphere(transform.position, 0.5f);
            foreach (Collider cube in nearbyCubes)
            {
                if (cube.CompareTag("Metal") && cube.gameObject != this.gameObject)
                {
                    CubeWeldingController otherCubeController = cube.GetComponent<CubeWeldingController>();
                    if (otherCubeController != null && otherCubeController.hasAcid && !otherCubeController.isWelded)
                    {
                        Debug.Log("Cubo cercano con ácido encontrado.");

                        // Alinear el cubo con el otro
                        cube.transform.position = transform.position + (cube.transform.position - transform.position).normalized * 0.5f;

                        // Hacer el cubo hijo del otro para soldarlo
                        cube.transform.SetParent(transform);

                        // Desactivar física para que no se separen
                        Rigidbody rb = cube.GetComponent<Rigidbody>();
                        if (rb != null) rb.isKinematic = true;

                        Rigidbody rbThis = GetComponent<Rigidbody>();
                        if (rbThis != null) rbThis.isKinematic = true;

                        // Marcar ambos como soldados
                        isWelded = true;
                        otherCubeController.isWelded = true;

                        // Desactivar la interacción
                        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                        if (grabInteractable != null) grabInteractable.enabled = false;

                        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable otherGrabInteractable = cube.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                        if (otherGrabInteractable != null) otherGrabInteractable.enabled = false;

                        // Agregar los puntos del cordón de soldadura
                        weldPoints.Add(transform.position);
                        weldPoints.Add(cube.transform.position);

                        // Dibujar el cordón de soldadura con LineRenderer
                        weldLine.positionCount = weldPoints.Count;
                        weldLine.SetPositions(weldPoints.ToArray());

                        // Crear efecto de partículas de soldadura
                        if (weldingParticlePrefab != null)
                        {
                            ParticleSystem weldingParticles = Instantiate(weldingParticlePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
                            if (weldingParticles != null)
                            {
                                weldingParticles.Play();
                            }
                            else
                            {
                                Debug.LogError("El prefab de partículas de soldadura no tiene un componente ParticleSystem.");
                            }
                        }
                        else
                        {
                            Debug.LogError("El prefab de partículas de soldadura no está asignado.");
                        }

                        Debug.Log("Cubos soldados con cordón de soldadura.");
                        break;
                    }
                }
            }
        }
    }

    // Método para marcar que el cubo tiene ácido aplicado
    public void ApplyAcid()
    {
        hasAcid = true;
        // Debug.Log("Ácido aplicado al cubo.");
    }
}