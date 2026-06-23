using UnityEngine;
using Photon.Pun;

public class ZonaDeCaida : MonoBehaviour
{
    // Usamos OnTriggerEnter para que el trompo atraviese el plano como un fantasma
    // en lugar de rebotar contra él como si fuera el suelo normal.
    private void OnTriggerEnter(Collider other)
    {
        // Revisamos si el objeto que acaba de caer es un trompo
        TopController trompo = other.GetComponent<TopController>();

        // 1. Verificamos que sí sea un trompo.
        // 2. Verificamos que NOSOTROS seamos los dueños de este trompo por la red.
        if (trompo != null && trompo.photonView.IsMine)
        {
            // Reducimos el giro a cero al instante.
            // NOTA: Si tu variable de giro se llama diferente (ej: "spin", "energiaActual"), 
            // cámbiala en esta línea para que coincida con tu TopController.
            trompo.currentSpin = 0f;

            // Opcional: Si en tu TopController tienes una función específica que 
            // hace que el trompo explote o se declare derrotado, puedes llamarla aquí.
            // Ejemplo: trompo.PerderPartida();
        }
    }
}