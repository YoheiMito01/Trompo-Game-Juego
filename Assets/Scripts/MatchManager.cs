using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviourPunCallbacks
{
    public static MatchManager Instance;

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

    private string escenaDestino = ""; // Inicializamos vacío para evitar errores
    private bool solicitandoSalida = false; // Control de seguridad

    private void Awake()
    {
        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        panelFinal.SetActive(false);
        partidaTerminada = false;

        if (PhotonNetwork.InRoom)
        {
            jugadoresVivos = PhotonNetwork.CurrentRoom.PlayerCount;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            botonRepetir.SetActive(false);
            botonLobby.SetActive(false);
        }
    }

    public void RegistrarTrompo() { }

    public void ReportarDerrota(int idJugadorPerdedor)
    {
        if (partidaTerminada) return;
        photonView.RPC("RPC_ProcesarEliminacion", RpcTarget.All, idJugadorPerdedor);
    }

    [PunRPC]
    void RPC_ProcesarEliminacion(int idJugadorPerdedor)
    {
        jugadoresVivos--;

        if (PhotonNetwork.LocalPlayer.ActorNumber == idJugadorPerdedor)
        {
            MostrarPantalla(false);
        }
        else if (jugadoresVivos <= 1)
        {
            MostrarPantalla(true);
            partidaTerminada = true;
        }
    }

    void MostrarPantalla(bool esVictoria)
    {
        panelFinal.SetActive(true);
        textoResultado.text = esVictoria ? "¡VICTORIA!" : "DERROTA";
        textoResultado.color = esVictoria ? Color.green : Color.red;
    }

    // --- FUNCIONES PARA LOS BOTONES ---

    public void BotonSalir()
    {
        solicitandoSalida = true; // Confirmamos que este script dio la orden
        escenaDestino = nombreEscenaMenu;

        // Desconexión total. Así al volver al Menú, Photon se reconectará desde cero limpio.
        PhotonNetwork.Disconnect();
    }

    public void BotonRepetir()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // En lugar de cargar la escena de inmediato, le pedimos a todos 
            // que limpien su basura de red primero.
            photonView.RPC("RPC_LimpiarYReiniciar", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_LimpiarYReiniciar()
    {
        // 1. Cada jugador elimina del servidor de Photon todo lo que haya creado (sus trompos)
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

        // 2. Apagamos el panel final
        panelFinal.SetActive(false);

        // 3. Ahora que el servidor está limpio, el Host da la orden de recargar la escena
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }

    [PunRPC]
    void RPC_ForzarReinicio()
    {
        // 1. Apagamos el panel de victoria/derrota para el cliente inmediatamente
        panelFinal.SetActive(false);

        // 2. Ejecutamos la carga del nivel
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }

    public void BotonVolverAlLobby()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(nombreEscenaLobby);
        }
    }

    // --- CALLBACKS DE SEGURIDAD ---

    public override void OnLeftRoom()
    {
        // Solo cargamos la escena si ESTE script la pidió y no está vacía
        if (solicitandoSalida && !string.IsNullOrEmpty(escenaDestino))
        {
            SceneManager.LoadScene(escenaDestino);
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        // Si usamos BotonSalir, se activará esto en lugar de OnLeftRoom
        if (solicitandoSalida && !string.IsNullOrEmpty(escenaDestino))
        {
            SceneManager.LoadScene(escenaDestino);
        }
    }
}