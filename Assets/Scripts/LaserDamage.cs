using UnityEngine;

public class LaserDamage : MonoBehaviour
{
    [Tooltip("Pérdida de giro por segundo mientras se toca el láser")]
    public float danoPorSegundo = 250f;

    private void OnTriggerStay(Collider other)
    {
        // Si el collider pertenece a un trompo
        TopController top = other.GetComponent<TopController>();

        if (top != null)
        {
            // Le indicamos al trompo que tome daño constante por el láser
            top.TakeLaserDamage(danoPorSegundo * Time.fixedDeltaTime);
        }
    }
}
