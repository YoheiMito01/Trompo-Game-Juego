using System.Collections;  // ¡IMPORTANTE! Necesario para usar Corrutinas (IEnumerator)
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public Transform spawn1;
    public Transform spawn2;
    public string[] prefabNames;

    void Start()
    {
        // En lugar de llamar a la función de golpe, iniciamos la corrutina
        StartCoroutine(SpawnPlayerConRetraso());
    }

    IEnumerator SpawnPlayerConRetraso()
    {
        // 1. Le damos medio segundo a Photon para que el Cliente termine de sincronizar la escena
        yield return new WaitForSeconds(0.5f);

        // 2. Seguro de vida: Esperamos hasta que Photon confirme que el jugador 
        // tiene guardado su "TopIndex" antes de intentar leerlo.
        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("TopIndex"))
        {
            yield return null; // Espera al siguiente frame y vuelve a preguntar
        }

        // 3. Ahora es 100% seguro leer la propiedad y crear el trompo
        int topIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["TopIndex"];

        Transform spawn;

        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            spawn = spawn1;
        }
        else
        {
            spawn = spawn2;
        }

        PhotonNetwork.Instantiate(
            prefabNames[topIndex],
            spawn.position,
            spawn.rotation
        );
    }
}