using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviourPunCallbacks
{
    public static MatchManager Instance; // Esto nos permite llamarlo desde el trompo fácilmente

    [Header("Interfaz Final")]
    public GameObject panelFinal;
    public TMP_Text textoResultado;

    [Header("Botones (Solo el Salir lo ven todos)")]
    public GameObject botonRepetir;
    public GameObject botonLobby;
    public GameObject botonSalir;

    [Header("Nombres de Escenas")]
    public string nombreEscenaMenu = "MenuPrincipal";
    public string nombreEscenaLobby = "LobbyScene";

    private int jugadoresVivos = 0;
    private bool partidaTerminada = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        panelFinal.SetActive(false);

        // Control de botones: Los clientes solo pueden salir. El Host decide a dónde va la sala.
        if (!PhotonNetwork.IsMasterClient)
        {
            botonRepetir.SetActive(false);
            botonLobby.SetActive(false);
        }
    }

    // El TopController llamará a esto al nacer
    public void RegistrarTrompo()
    {
        photonView.RPC("RPC_SumarJugador", RpcTarget.All);
    }

    [PunRPC]
    void RPC_SumarJugador()
    {
        jugadoresVivos++;
    }

    // El TopController llamará a esto cuando su giro sea 0
    public void ReportarDerrota(int idJugadorPerdedor)
    {
        if (partidaTerminada) return;
        photonView.RPC("RPC_ProcesarEliminacion", RpcTarget.All, idJugadorPerdedor);
    }

    [PunRPC]
    void RPC_ProcesarEliminacion(int idJugadorPerdedor)
    {
        jugadoresVivos--;

        // 1. ¿Soy yo el que perdió? Muestro Derrota.
        if (PhotonNetwork.LocalPlayer.ActorNumber == idJugadorPerdedor)
        {
            MostrarPantalla(false);
        }
        // 2. ¿Queda solo 1 vivo y NO fui yo el que perdió? Muestro Victoria.
        else if (jugadoresVivos <= 1)
        {
            MostrarPantalla(true);
            partidaTerminada = true; // Evitamos que se ejecute dos veces
        }
    }

    void MostrarPantalla(bool esVictoria)
    {
        panelFinal.SetActive(true);
        textoResultado.text = esVictoria ? "¡VICTORIA!" : "DERROTA";
        textoResultado.color = esVictoria ? Color.green : Color.red;
    }

    // --- FUNCIONES PARA LOS BOTONES (Asígnalas en el Inspector) ---

    public void BotonSalir()
    {
        PhotonNetwork.LeaveRoom(); // Desconecta de la sala
        SceneManager.LoadScene(nombreEscenaMenu); // Te manda al menú offline
    }

    public void BotonRepetir()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // PhotonNetwork.LoadLevel recarga la escena actual para todos sincronizadamente
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }

    public void BotonVolverAlLobby()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Carga la escena del Lobby para toda la sala
            PhotonNetwork.LoadLevel(nombreEscenaLobby);
        }
    }
}
