using UnityEngine;
using Photon.Pun;

public class HazardKnockback : MonoBehaviour
{
    [Header("Configuración de Empuje")]
    [Tooltip("La fuerza/velocidad directa que se le sumará al trompo (independiente de su masa)")]
    public float fuerzaEmpuje = 15f; // Ahora un valor entre 10 y 30 debería ser más que suficiente

    private void OnCollisionEnter(Collision collision)
    {
        TopController trompo = collision.gameObject.GetComponent<TopController>();

        if (trompo != null && trompo.photonView.IsMine)
        {
            Rigidbody rb = trompo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Calculamos la dirección del empuje (desde el centro del cubo hacia el trompo)
                Vector3 direccionEmpuje = collision.transform.position - transform.position;

                // Ignoramos la altura (Y) para que el empuje sea siempre horizontal
                direccionEmpuje.y = 0;

                // CAMBIO: Usamos VelocityChange para ignorar la masa del Rigidbody
                rb.AddForce(direccionEmpuje.normalized * fuerzaEmpuje, ForceMode.VelocityChange);
            }
        }
    }
}