using TMPro;                          // Libreria para utilizar textos de TextMeshPro
using UnityEngine;                    // Libreria principal de Unity
using Photon.Pun;                     // Libreria de Photon PUN
using Photon.Realtime;                // Permite acceder a la informacion de los jugadores
using ExitGames.Client.Photon;        // Permite trabajar con Hashtables de Photon

// Clase encargada de mostrar y seleccionar un trompo
public class TopSelector : MonoBehaviourPunCallbacks
{
    // Lista de prefabs de los trompos disponibles
    public GameObject[] topPrefabs;

    // Nombre correspondiente a cada trompo
    public string[] topNames;

    // Descripcion de cada trompo
    [TextArea]
    public string[] descriptions;

    // Lugar donde aparecera el modelo del trompo seleccionado
    public Transform spawnPoint;

    // Texto donde se mostrara el nombre del trompo
    public TMP_Text nameText;

    // Texto donde se mostrara la descripcion del trompo
    public TMP_Text descriptionText;

    // Indice del trompo actualmente seleccionado
    private int currentIndex = 0;

    // Referencia al modelo que esta siendo mostrado
    private GameObject currentTop;

    // Boton para confirmar la seleccion
    public GameObject botonSeleccionar;

    // Variable que evita seleccionar mas de una vez
    private bool selected = false;

    // Se ejecuta automaticamente al iniciar la escena
    private void Start()
    {
        // Muestra el primer trompo disponible
        MostrarTop();
    }

    // Cambia al siguiente trompo de la lista
    public void Siguiente()
    {
        // Aumenta el indice
        currentIndex++;

        // Si llega al final vuelve al primero
        if (currentIndex >= topPrefabs.Length)
            currentIndex = 0;

        // Actualiza la vista
        MostrarTop();
    }

    // Cambia al trompo anterior
    public void Anterior()
    {
        // Disminuye el indice
        currentIndex--;

        // Si pasa del primero vuelve al ultimo
        if (currentIndex < 0)
            currentIndex = topPrefabs.Length - 1;

        // Actualiza la vista
        MostrarTop();
    }

    // Muestra el trompo seleccionado en pantalla
    void MostrarTop()
    {
        // Si habia un modelo anterior lo destruye
        if (currentTop != null)
            Destroy(currentTop);

        // Instancia el nuevo modelo en el punto de visualizacion
        currentTop = Instantiate(
            topPrefabs[currentIndex],
            spawnPoint.position,
            Quaternion.identity
        );

        // Actualiza el nombre del trompo
        nameText.text = topNames[currentIndex];

        // Actualiza la descripcion del trompo
        descriptionText.text = descriptions[currentIndex];
    }

    // Se ejecuta cuando el jugador confirma su seleccion
    public void SeleccionarTop()
    {
        // Evita que el jugador seleccione varias veces
        if (selected)
            return;

        selected = true;

        // Crea una tabla donde se almacenaran propiedades personalizadas
        Hashtable props = new Hashtable();

        // Guarda el indice del trompo seleccionado
        props["TopIndex"] = currentIndex;

        // Envia esa informacion a Photon para que quede asociada al jugador
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // Comprueba si todos los jugadores ya seleccionaron un trompo
        ComprobarInicio();
    }

    // Comprueba si todos los jugadores ya hicieron su seleccion
    void ComprobarInicio()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("TopIndex"))
                return;
        }

        // En lugar de cargar "GameBase", cargamos la escena de mapas
        PhotonNetwork.LoadLevel("SelectMapScene");
    }

    // Se ejecuta automaticamente cuando un jugador cambia una propiedad
    public override void OnPlayerPropertiesUpdate(
        Player targetPlayer,
        ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Comprueba si la propiedad modificada fue el trompo seleccionado
        if (changedProps.ContainsKey("TopIndex"))
        {
            // Cada vez que un jugador selecciona un trompo,
            // el Host verifica si todos ya terminaron
            ComprobarInicio();
        }
    }
}
