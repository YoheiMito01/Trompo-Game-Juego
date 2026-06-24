using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviourPunCallbacks
{
    [Header("UI del Menú de Pausa")]
    public GameObject panelPausa;

    [Header("Nombres de Escenas")]
    public string nombreEscenaMenu = "MenuPrincipal";
    public string nombreEscenaLobby = "LobbyScene";

    private bool estaPausado = false;
    private string escenaDestino;

    private void Start()
    {
        // Nos aseguramos de que el menú empiece oculto al iniciar la partida
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
    }

    private void Update()
    {
        // Detecta si el jugador presiona la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausa();
        }
    }

    public void TogglePausa()
    {
        estaPausado = !estaPausado;
        panelPausa.SetActive(estaPausado);
    }

    // --- FUNCIONES PARA LOS BOTONES ---

    public void BotonVolverAlMenu()
    {
        // Asegúrate de actualizar el nombre de la escena si tu menú principal se llama diferente
        escenaDestino = "Menu Principal";

        // ¡EL FILTRO INTELIGENTE!
        // PhotonNetwork.InRoom nos dice si estamos dentro de una partida/sala activa.
        if (PhotonNetwork.InRoom)
        {
            // Si estamos jugando en una sala, salimos de ella de forma segura
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            // Si ya estamos en el MasterServer (como en la selección de trompos o lobby),
            // no hay necesidad de salir de ningún lado. Cargamos el menú directamente.
            UnityEngine.SceneManagement.SceneManager.LoadScene(escenaDestino);
        }
    }

    public void BotonVolverAlLobby()
    {
        escenaDestino = nombreEscenaLobby;

        // Le avisamos a Photon que nos salimos de la sala actual
        PhotonNetwork.LeaveRoom();
    }

    public void BotonCerrarJuego()
    {
        // Si estamos en una sala, nos salimos antes de cerrar por educación con el servidor
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }

    // --- CALLBACK AUTOMÁTICO DE PHOTON ---
    // Como LeaveRoom() tarda unos milisegundos en completarse por internet,
    // Photon ejecutará esta función AUTOMÁTICAMENTE cuando ya estemos desconectados con éxito.
    public override void OnLeftRoom()
    {
        // El seguro de vida: Si la escena está vacía, ignora la orden.
        if (!string.IsNullOrEmpty(escenaDestino))
        {
            SceneManager.LoadScene(escenaDestino);
        }
    }
}