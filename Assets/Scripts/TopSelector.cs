using TMPro;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class TopSelector : MonoBehaviour
{
    public GameObject[] topPrefabs;

    public string[] topNames;

    [TextArea]
    public string[] descriptions;

    public Transform spawnPoint;

    public TMP_Text nameText;

    public TMP_Text descriptionText;

    private int currentIndex = 0;

    private GameObject currentTop;

    private void Start()
    {
        MostrarTop();
    }

    public void Siguiente()
    {
        currentIndex++;

        if (currentIndex >= topPrefabs.Length)
            currentIndex = 0;

        MostrarTop();
    }

    public void Anterior()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = topPrefabs.Length - 1;

        MostrarTop();
    }

    void MostrarTop()
    {
        if (currentTop != null)
            Destroy(currentTop);

        currentTop = Instantiate(
            topPrefabs[currentIndex],
            spawnPoint.position,
            Quaternion.identity
        );

        nameText.text = topNames[currentIndex];

        descriptionText.text = descriptions[currentIndex];
    }
    public void SeleccionarTop()
    {
        Hashtable props = new Hashtable();

        props["TopIndex"] = currentIndex;

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log("Seleccionaste el trompo: " + currentIndex);
    }
}
