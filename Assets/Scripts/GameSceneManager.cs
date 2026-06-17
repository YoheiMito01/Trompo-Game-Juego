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
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        int topIndex =
            (int)PhotonNetwork.LocalPlayer.CustomProperties["TopIndex"];

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
