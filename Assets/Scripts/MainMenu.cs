using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    [Tooltip("Arrastra aquí el panel de los créditos desde la jerarquía")]
    public GameObject panelCreditos;

    public void Iniciar()
    {
        SceneManager.LoadScene("MenuOnline");
    }

    // Activa el panel de créditos
    public void MostrarCreditos()
    {
        if (panelCreditos != null)
        {
            panelCreditos.SetActive(true);
        }
    }

    // Desactiva el panel de créditos
    public void OcultarCreditos()
    {
        if (panelCreditos != null)
        {
            panelCreditos.SetActive(false);
        }
    }

    public void Sair()
    {
        // Nota: Application.Quit solo funciona en la build final del juego, no en el editor de Unity
        Application.Quit();
    }
}
