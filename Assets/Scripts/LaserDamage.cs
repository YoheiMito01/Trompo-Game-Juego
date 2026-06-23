using UnityEngine;

public class LaserDamage : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Cantidad de spin que quita por segundo mientras lo toca")]
    public float damagePerSecond = 300f;

    // OnTriggerStay se ejecuta TODOS los frames que un objeto esté tocando el láser
    private void OnTriggerStay(Collider other)
    {
        // Verificamos si lo que tocó el láser es un trompo
        TopController trompo = other.GetComponent<TopController>();

        if (trompo != null)
        {
            // Le quitamos spin basado en el tiempo para que sea suave y constante
            trompo.TakeLaserDamage(damagePerSecond * Time.fixedDeltaTime);
        }
    }
}
