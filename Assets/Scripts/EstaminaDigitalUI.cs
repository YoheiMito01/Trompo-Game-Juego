using UnityEngine;
using TMPro; // Importante para controlar TextMeshPro
using Photon.Pun;

public class EstaminaDigitalUI : MonoBehaviour
{
    [Header("Referencias Visuales")]
    [SerializeField] private TMP_Text textoEstamina; // Arrastra aquí tu objeto TextMeshPro
    [SerializeField] private string textoPrefijo = "SPIN: "; // Por si quieres que diga "SPIN: 1000"

    private TopController miTrompo;

    void Update()
    {
        // 1. Si aún no tenemos nuestro trompo, lo buscamos en la escena
        if (miTrompo == null)
        {
            TopController[] todosLosTrompos = FindObjectsOfType<TopController>();
            foreach (var trompo in todosLosTrompos)
            {
                // Solo nos interesa el trompo que nos pertenece a NOSOTROS
                if (trompo.photonView.IsMine)
                {
                    miTrompo = trompo;
                    break;
                }
            }
        }
        else
        {
            // 2. Si ya encontramos nuestro trompo, actualizamos los números en pantalla
            // Usamos Mathf.CeilToInt para que muestre números enteros (ej: 950 en vez de 950.342)
            int estaminaEntera = Mathf.CeilToInt(miTrompo.currentSpin);

            // Mostramos el texto en el formato que quieras
            textoEstamina.text = textoPrefijo + estaminaEntera.ToString();

            // Opcional: Cambiar el color del texto si le queda poca estamina
            if (estaminaEntera < 200)
            {
                textoEstamina.color = Color.red; // Alerta roja
            }
            else
            {
                textoEstamina.color = Color.white; // Color normal armonioso
            }
        }
    }
}