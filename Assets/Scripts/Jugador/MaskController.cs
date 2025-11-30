using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MaskController : MonoBehaviour
{
    public Transform headTransform; // Transform de la cabeza del jugador
    public float proximityDistance = 0.3f; // Distancia para detectar si la máscara está cerca de la cara
    public Vector3 maskOffset = new Vector3(0, 0, 0.1f); // Ajuste de posición de la máscara
    public Transform desiredPosition; // Posición donde debe ajustarse la máscara
    public float moveSpeed = 5f; // Velocidad de movimiento al colocarse

    private XRGrabInteractable grabInteractable;
    private Rigidbody maskRigidbody;
    private bool isMovingToFace = false;
    private bool isAttached = false; // Indica si la máscara está en la cara

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        maskRigidbody = GetComponent<Rigidbody>();

        // Suscribirse a los eventos
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void Update()
    {
        if (isMovingToFace)
        {
            // Mover suavemente la máscara hacia la cara
            transform.position = Vector3.MoveTowards(transform.position, desiredPosition.position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredPosition.rotation, moveSpeed * Time.deltaTime);

            // Si la máscara llega a la posición deseada, la fijamos
            if (Vector3.Distance(transform.position, desiredPosition.position) < 0.01f)
            {
                AttachToFace();
            }
        }
        //Debug.Log($"Distance: {Vector3.Distance(transform.position, headTransform.position)}");
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Si la máscara está en la cara, permitir soltarla nuevamente
        if (isAttached)
        {
            DetachFromFace();
        }

        // Restaurar la física para poder mover la máscara libremente
        isMovingToFace = false;
        maskRigidbody.isKinematic = false;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Verificar que no fue cancelado
        if (args.isCanceled) return;

        // Si la máscara está cerca de la cabeza, activar el movimiento hacia la cara
        if (Vector3.Distance(transform.position, headTransform.position) < proximityDistance)
        {
            Debug.Log("Attach");
            isMovingToFace = true;
        }
        else
        {
            Debug.Log("Detach1");
            DetachFromFace();
        }
    }

    private void AttachToFace()
    {
        isMovingToFace = false;
        isAttached = true;
        maskRigidbody.isKinematic = true; // Hacer la máscara cinemática para que no caiga
        transform.SetPositionAndRotation(desiredPosition.position, desiredPosition.rotation);
        transform.SetParent(desiredPosition.parent); // Anclar a la cabeza
    }

    private void DetachFromFace()
    {
        Debug.Log("Detach2");
        isAttached = false;
        transform.SetParent(null); // Desanclar la máscara de la cabeza
        maskRigidbody.isKinematic = false; // Restaurar la física
    }

    private void OnDestroy()
    {
        // Limpieza de eventos
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}