using UnityEngine;
using Photon.Pun;
using TMPro; // Por si quieres mostrar un texto con el tiempo restante

public class TutorialManager : MonoBehaviourPun
{
    [Header("Configuración del Tutorial")]
    [Tooltip("Tiempo en segundos que los jugadores estarán aquí")]
    public float tiempoTutorial = 10f;

    [Tooltip("Opcional: Texto para mostrar el tiempo restante")]
    public TMP_Text textoTemporizador;

    private float timer;
    private bool cargandoArena = false;

    void Start()
    {
        timer = tiempoTutorial;

        // Aquí debes instanciar el trompo del jugador para que pueda moverse y practicar
        // (Asegúrate de tener un Transform o unas coordenadas para que aparezcan)
        // Ejemplo:
        // PhotonNetwork.Instantiate("NombreDeTuPrefabDeTrompo", new Vector3(0, 1, 0), Quaternion.identity);
    }

    void Update()
    {
        // Actualizamos el texto visual para todos los jugadores
        if (textoTemporizador != null)
        {
            textoTemporizador.text = "Cargando arena en: " + Mathf.Ceil(timer).ToString();
        }

        // SOLO EL HOST lleva el control del cambio de escena real
        if (!PhotonNetwork.IsMasterClient || cargandoArena) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            cargandoArena = true;
            CargarArenaFinal();
        }
    }

    void CargarArenaFinal()
    {
        // Rescatamos el nombre del mapa que el Host guardó en la escena anterior
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MapaElegido", out object nombreMapa))
        {
            PhotonNetwork.LoadLevel((string)nombreMapa);
        }
        else
        {
            Debug.LogError("Hubo un error al recuperar el mapa. Volviendo al menú.");
            PhotonNetwork.LoadLevel("MenuOnline");
        }
    }
}