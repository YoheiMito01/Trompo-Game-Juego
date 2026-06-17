using UnityEngine;                  // Libreria principal de Unity
using Photon.Pun;                  // Libreria de Photon PUN
using Photon.Realtime;             // Permite trabajar con salas y jugadores
using TMPro;                       // Libreria para textos e InputField
using System.Collections.Generic;  // Permite utilizar listas

// Clase principal que controla la conexion y el lobby online
public class GameManager : MonoBehaviourPunCallbacks
{
    // Lista donde se almacenan las salas encontradas
    private List<RoomInfo> rooms = new List<RoomInfo>();

    [Header("UI")]

    // Campo donde el jugador escribe su nickname
    public TMP_InputField inputName;

    // Texto que muestra mensajes al jugador
    public TMP_Text textIndicator;

    // Boton para crear una sala
    public GameObject btnCreateRoom;

    [Header("Lobby")]

    // Panel del lobby
    public GameObject panelLobby;

    // Contenedor donde apareceran los nombres de los jugadores
    public Transform contentPlayers;

    // Prefab utilizado para mostrar cada jugador en la lista
    public GameObject playerPrefab;

    // Panel inicial del juego
    public GameObject panelStart;

    // Panel que aparece una vez creada la sala
    public GameObject panelCreateRoom;

    // Texto que muestra el nombre de la sala
    public TMP_Text salaName;

    // Boton para iniciar la partida
    public GameObject btnStart;

    // Boton para unirse a una sala existente
    public GameObject btnJoinRoom;

    // Texto que indica a que sala se unira el jugador
    public TMP_Text joinRoomText;

    // Se ejecuta automaticamente al iniciar la escena
    private void Start()
    {
        // Oculta todos los elementos que aun no deben verse
        btnJoinRoom.SetActive(false);
        btnCreateRoom.SetActive(false);
        panelLobby.SetActive(false);

        // Muestra el menu principal
        panelStart.SetActive(true);

        // Oculta el lobby hasta crear o unirse a una sala
        panelCreateRoom.SetActive(false);

        // Hace que cuando el Host cambie de escena,
        // todos los jugadores cambien automaticamente
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Conecta al jugador con Photon
    public void ConnectPhoton()
    {
        // Comprueba que el usuario escribio un nombre
        if (string.IsNullOrEmpty(inputName.text))
        {
            textIndicator.text = "Ingresa un nombre.";
            return;
        }

        // Guarda el nickname del jugador
        PhotonNetwork.NickName = inputName.text;

        // Si aun no esta conectado inicia la conexion
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // Se ejecuta automaticamente cuando Photon conecta correctamente
    public override void OnConnectedToMaster()
    {
        // Muestra un mensaje de bienvenida
        textIndicator.text =
            "Bienvenido " + PhotonNetwork.NickName;

        // Activa el boton para crear una sala
        btnCreateRoom.SetActive(true);

        // Entra al lobby para buscar salas disponibles
        PhotonNetwork.JoinLobby();
    }

    // Se ejecuta cuando se pierde la conexion
    public override void OnDisconnected(DisconnectCause cause)
    {
        textIndicator.text = "Desconectado";
    }

    // Crea una nueva sala o entra si ya existe una con ese nombre
    public void CreateRoom()
    {
        // Configuracion de la sala
        RoomOptions options = new RoomOptions();

        // Cantidad maxima de jugadores
        options.MaxPlayers = 4;

        // Permite que otros jugadores la vean
        options.IsVisible = true;

        // Publica el ID de los usuarios
        options.PublishUserId = true;

        // Crea o entra a la sala
        PhotonNetwork.JoinOrCreateRoom(
            PhotonNetwork.NickName,
            options,
            TypedLobby.Default
        );
    }

    // Se ejecuta al entrar correctamente a una sala
    public override void OnJoinedRoom()
    {
        // Oculta el menu principal
        panelStart.SetActive(false);

        // Muestra el lobby de la sala
        panelCreateRoom.SetActive(true);

        // Escribe el nombre de la sala
        salaName.text = PhotonNetwork.CurrentRoom.Name;

        // Solo el Host puede iniciar la partida
        btnStart.SetActive(PhotonNetwork.IsMasterClient);

        // Actualiza la lista de jugadores
        ActualizarListaJugadores();
    }

    // Se ejecuta cuando un nuevo jugador entra a la sala
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ActualizarListaJugadores();
    }

    // Se ejecuta cuando un jugador abandona la sala
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ActualizarListaJugadores();
    }

    // Actualiza la lista visual de jugadores conectados
    void ActualizarListaJugadores()
    {
        // Elimina la lista anterior
        foreach (Transform hijo in contentPlayers)
        {
            Destroy(hijo.gameObject);
        }

        // Recorre todos los jugadores conectados
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Crea un nuevo elemento en la lista
            GameObject item = Instantiate(playerPrefab, contentPlayers);

            // Obtiene el texto del prefab
            TMP_Text texto = item.GetComponentInChildren<TMP_Text>();

            // Escribe el nickname del jugador
            texto.text = player.NickName;
        }
    }

    // Inicia la partida cargando la siguiente escena
    public void StartGame()
    {
        PhotonNetwork.LoadLevel("SelectTopScene");
    }

    // Se ejecuta cuando Photon actualiza la lista de salas disponibles
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Guarda la lista recibida
        rooms = roomList;

        // Si existe al menos una sala
        if (rooms.Count > 0)
        {
            // Muestra el boton para unirse
            btnJoinRoom.SetActive(true);

            // Indica a que sala se unira el jugador
            joinRoomText.text =
                "Unirse a la sala de " + rooms[0].Name;
        }
        else
        {
            // Si no existen salas oculta el boton
            btnJoinRoom.SetActive(false);
        }
    }

    // Permite unirse a la primera sala encontrada
    public void JoinRoom()
    {
        // Comprueba que exista al menos una sala
        if (rooms.Count > 0)
        {
            PhotonNetwork.JoinRoom(rooms[0].Name);
        }
    }
}