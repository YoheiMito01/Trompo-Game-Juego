using TMPro;
using UnityEngine;

public class MapSelector : MonoBehaviour
{
    public GameObject[] mapPrefabs;

    public string[] mapNames;

    [TextArea]
    public string[] descriptions;

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

        if (Photon.Pun.PhotonNetwork.IsMasterClient)
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
        currentIndex++;

        if (currentIndex >= mapPrefabs.Length)
            currentIndex = 0;

        MostrarMapa();
    }

    public void Previous()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = mapPrefabs.Length - 1;

        MostrarMapa();
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
}
