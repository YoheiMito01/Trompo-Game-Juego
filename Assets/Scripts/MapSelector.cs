using TMPro;
using UnityEngine;
using Photon.Pun; // IMPORTANTE: Agregamos la librería de Photon

// Cambiamos MonoBehaviour por MonoBehaviourPun para poder usar RPCs
public class MapSelector : MonoBehaviourPun
{
    [Header("Datos de los Mapas")]
    public GameObject[] mapPrefabs;
    public string[] mapNames;
    [TextArea]
    public string[] descriptions;

    // NUEVO: Aquí pondrás el nombre EXACTO de la escena que corresponde a cada mapa
    public string[] sceneNames;

    [Header("Referencias UI")]
    public Transform spawnPoint;
    public TMP_Text mapNameText;
    public TMP_Text descriptionText;
    public GameObject selectButton;
    public TMP_Text waitingText;

    private int currentIndex;
    private GameObject currentMap;

    private void Start()
    {
        MostrarMapa();

        // Control de Interfaz
        if (PhotonNetwork.IsMasterClient)
        {
            selectButton.SetActive(true);
            waitingText.gameObject.SetActive(false);
        }
        else
        {
            selectButton.SetActive(false);
            waitingText.gameObject.SetActive(true);
        }
    }

    public void Next()
    {
        // Solo el Host puede cambiar de mapa
        if (!PhotonNetwork.IsMasterClient) return;

        currentIndex++;

        if (currentIndex >= mapPrefabs.Length)
            currentIndex = 0;

        MostrarMapa();

        // Le avisamos a los demás jugadores que actualicen su vista
        photonView.RPC("RPC_SincronizarMapa", RpcTarget.Others, currentIndex);
    }

    public void Previous()
    {
        // Solo el Host puede cambiar de mapa
        if (!PhotonNetwork.IsMasterClient) return;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = mapPrefabs.Length - 1;

        MostrarMapa();

        // Le avisamos a los demás jugadores que actualicen su vista
        photonView.RPC("RPC_SincronizarMapa", RpcTarget.Others, currentIndex);
    }

    void MostrarMapa()
    {
        if (currentMap != null)
            Destroy(currentMap);

        currentMap = Instantiate(
            mapPrefabs[currentIndex],
            spawnPoint.position,
            Quaternion.identity
        );

        mapNameText.text = mapNames[currentIndex];
        descriptionText.text = descriptions[currentIndex];
    }

    // Esta es la función que debes ponerle al botón "Seleccionar" en el Inspector
    public void ConfirmarSeleccion()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Carga la escena correspondiente al índice actual
            PhotonNetwork.LoadLevel(sceneNames[currentIndex]);
        }
    }

    // Esta función se ejecuta en las pantallas de los CLIENTES cuando el Host cambia de mapa
    [PunRPC]
    void RPC_SincronizarMapa(int nuevoIndice)
    {
        // Actualizamos el índice local del cliente y mostramos el mapa
        currentIndex = nuevoIndice;
        MostrarMapa();
    }
}