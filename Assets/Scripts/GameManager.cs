using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    private List<RoomInfo> rooms = new List<RoomInfo>();
    [Header("UI")]
    public TMP_InputField inputName;
    public TMP_Text textIndicator;
    public GameObject btnCreateRoom;

    [Header("Lobby")]
    public GameObject panelLobby;
    public Transform contentPlayers;
    public GameObject playerPrefab;

    public GameObject panelStart;
    public GameObject panelCreateRoom;

    public TMP_Text salaName;
    public GameObject btnStart;
    public GameObject btnJoinRoom;
    public TMP_Text joinRoomText;

    private void Start()
    {
        btnJoinRoom.SetActive(false);
        btnCreateRoom.SetActive(false);
        panelLobby.SetActive(false);
        panelStart.SetActive(true);
        panelCreateRoom.SetActive(false);
        btnCreateRoom.SetActive(false);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // CONEXIÓN

    public void ConnectPhoton()
    {
        if (string.IsNullOrEmpty(inputName.text))
        {
            textIndicator.text = "Ingresa un nombre.";
            return;
        }

        PhotonNetwork.NickName = inputName.text;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        textIndicator.text =
            "Bienvenido " + PhotonNetwork.NickName;

        btnCreateRoom.SetActive(true);

        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        textIndicator.text = "Desconectado";

        Debug.Log(cause);
    }

    // SALA

    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions();

        options.MaxPlayers = 4;
        options.IsVisible = true;
        options.PublishUserId = true;

        PhotonNetwork.JoinOrCreateRoom(
            PhotonNetwork.NickName,
            options,
            TypedLobby.Default
        );
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Sala creada.");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Entraste a la sala");

        // Ocultar menú principal
        panelStart.SetActive(false);

        // Mostrar lobby
        panelCreateRoom.SetActive(true);

        // Nombre de la sala
        salaName.text = PhotonNetwork.CurrentRoom.Name;

        // Solo el host puede iniciar
        btnStart.SetActive(PhotonNetwork.IsMasterClient);

        // Actualizar jugadores
        ActualizarListaJugadores();
    }

    // JUGADORES

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entró.");

        ActualizarListaJugadores();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " salió.");

        ActualizarListaJugadores();
    }

    // ACTUALIZAR UI

    void ActualizarListaJugadores()
    {
        // Eliminar los jugadores anteriores
        foreach (Transform hijo in contentPlayers)
        {
            Destroy(hijo.gameObject);
        }

        // Crear un item por cada jugador conectado
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject item = Instantiate(playerPrefab, contentPlayers);

            TMP_Text texto = item.GetComponentInChildren<TMP_Text>();

            texto.text = player.NickName;
        }
    }
    public void StartGame()
    {
        PhotonNetwork.LoadLevel("GameBase");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        rooms = roomList;

        if (rooms.Count > 0)
        {
            btnJoinRoom.SetActive(true);

            joinRoomText.text =
                "Unirse a la sala de " + rooms[0].Name;
        }
        else
        {
            btnJoinRoom.SetActive(false);
        }
    }
    public void JoinRoom()
    {
        if (rooms.Count > 0)
        {
            PhotonNetwork.JoinRoom(rooms[0].Name);
        }
    }
}