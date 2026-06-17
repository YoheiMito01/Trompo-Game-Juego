using Photon.Pun;          // Libreria principal de Photon PUN
using Photon.Realtime;     // Permite acceder a la informacion de los jugadores
using UnityEngine;         // Libreria principal de Unity

// Clase encargada de generar el trompo correspondiente
// cuando comienza la partida
public class GameSceneManager : MonoBehaviour
{
    // Punto de aparicion del primer jugador
    public Transform spawn1;

    // Punto de aparicion del segundo jugador
    public Transform spawn2;

    // Lista con los nombres de los prefabs registrados en Photon
    public string[] prefabNames;

    // Se ejecuta automaticamente al cargar la escena
    void Start()
    {
        // Genera el trompo del jugador local
        SpawnPlayer();
    }

    // Crea el trompo correspondiente al jugador
    void SpawnPlayer()
    {
        // Obtiene el indice del trompo seleccionado anteriormente
        // desde las propiedades personalizadas del jugador
        int topIndex =
            (int)PhotonNetwork.LocalPlayer.CustomProperties["TopIndex"];

        // Variable que almacenara el punto de aparicion
        Transform spawn;

        // Si el numero del jugador es 1 utilizara el primer spawn
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            spawn = spawn1;
        }
        else
        {
            // Todos los demas jugadores utilizaran el segundo spawn
            spawn = spawn2;
        }

        // Crea el trompo en la red utilizando Photon
        // Todos los jugadores conectados podran verlo
        PhotonNetwork.Instantiate(
            prefabNames[topIndex],
            spawn.position,
            spawn.rotation
        );
    }
}