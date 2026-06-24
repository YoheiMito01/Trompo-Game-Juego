using UnityEngine;                  // Libreria principal de Unity
using Photon.Pun;                  // Libreria de Photon PUN
using Photon.Realtime;             // Permite trabajar con salas y jugadores
using TMPro;                       // Libreria para textos e InputField
using System.Collections.Generic;  // Permite utilizar listas

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
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.InRoom)
        {
            panelStart.SetActive(false);
            btnJoinRoom.SetActive(false);
            btnCreateRoom.SetActive(false);
            panelLobby.SetActive(false);

            panelCreateRoom.SetActive(true);
            salaName.text = PhotonNetwork.CurrentRoom.Name;
            btnStart.SetActive(PhotonNetwork.IsMasterClient);

            ActualizarListaJugadores();
        }
        else
        {
            btnJoinRoom.SetActive(false);
            btnCreateRoom.SetActive(false);
            panelLobby.SetActive(false);
            panelStart.SetActive(true);
            panelCreateRoom.SetActive(false);
        }
    }

    // Conecta al jugador con Photon (CORREGIDO)
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
            // Flujo normal: Es la primera vez que abre el juego y conecta desde cero
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            // ¡EL ARREGLO AQUÍ! Si ya viene conectado de una partida anterior,
            // forzamos la activación del menú de inmediato sin esperar al servidor.
            textIndicator.text = "Bienvenido de vuelta " + PhotonNetwork.NickName;
            btnCreateRoom.SetActive(true);

            // Si por alguna razón se salió del lobby, lo volvemos a meter
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        textIndicator.text = "Bienvenido " + PhotonNetwork.NickName;
        btnCreateRoom.SetActive(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        textIndicator.text = "Desconectado: " + cause.ToString();
        btnCreateRoom.SetActive(false);
        btnJoinRoom.SetActive(false);
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Photon se está conectando al servidor. Espera un momento...");
            textIndicator.text = "Conectando al servidor, espera...";
            return;
        }

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

    public override void OnJoinedRoom()
    {
        panelStart.SetActive(false);
        panelCreateRoom.SetActive(true);
        salaName.text = PhotonNetwork.CurrentRoom.Name;
        btnStart.SetActive(PhotonNetwork.IsMasterClient);
        ActualizarListaJugadores();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ActualizarListaJugadores();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ActualizarListaJugadores();
    }

    void ActualizarListaJugadores()
    {
        foreach (Transform hijo in contentPlayers)
        {
            Destroy(hijo.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject item = Instantiate(playerPrefab, contentPlayers);
            TMP_Text texto = item.GetComponentInChildren<TMP_Text>();
            texto.text = player.NickName;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel("SelectTopScene");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        rooms = roomList;

        if (rooms.Count > 0)
        {
            btnJoinRoom.SetActive(true);
            joinRoomText.text = "Unirse a la sala de " + rooms[0].Name;
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