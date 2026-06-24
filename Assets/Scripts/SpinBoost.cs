using UnityEngine;
using Photon.Pun;

// Clase encargada de controlar el funcionamiento del boost de energia
[RequireComponent(typeof(AudioSource))] // <-- Asegura que Unity le ponga un AudioSource al objeto
public class SpinBoost : MonoBehaviourPun
{
    [Header("Configuración de Boost")]
    [SerializeField] private float spinBonus = 300f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Efectos de Sonido")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip disappearSound;

    private AudioSource audioSource;
    private PowerUpSpawner spawner;
    private bool collected = false;
    private Collider col;
    private Renderer[] renderers;

    void Awake()
    {
        // Obtenemos las referencias necesarias
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        spawner = FindObjectOfType<PowerUpSpawner>();

        // 1. SONIDO DE APARICIÓN: Se reproduce para todos apenas el objeto se crea en su pantalla
        if (spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        // Solo el Host programa el tiempo de vida
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke(nameof(InitiateTimeout), lifeTime);
        }
    }

    // --- LÓGICA DE TIEMPO AGOTADO (NADIE LO TOMÓ) ---
    void InitiateTimeout()
    {
        if (collected) return;

        // El Host le avisa a TODOS los jugadores que el tiempo se acabó
        photonView.RPC(nameof(RpcTimeout), RpcTarget.All);
    }

    [PunRPC]
    void RpcTimeout()
    {
        if (collected) return;
        collected = true;

        // 2. SONIDO DE DESAPARICIÓN: Nadie lo agarró
        if (disappearSound != null)
        {
            audioSource.PlayOneShot(disappearSound);
        }

        StartDestructionSequence(disappearSound);
    }

    // --- LÓGICA DE RECOLECCIÓN (ALGUIEN LO TOMÓ) ---
    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        TopController top = other.GetComponent<TopController>();
        if (top == null) return;

        PhotonView topView = top.GetComponent<PhotonView>();
        if (topView == null || !topView.IsMine) return;

        // Le damos la energía al dueño localmente al instante
        top.AddSpin(spinBonus);

        // Avisamos a TODOS que este boost fue recogido
        photonView.RPC(nameof(RpcCollected), RpcTarget.All);
    }

    [PunRPC]
    void RpcCollected()
    {
        if (collected) return;
        collected = true;

        // 3. SONIDO DE RECOLECCIÓN: Alguien lo agarró
        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        StartDestructionSequence(collectSound);
    }

    // --- SECUENCIA DE DESTRUCCIÓN FANTASMA ---
    void StartDestructionSequence(AudioClip clipPlayed)
    {
        // Apagamos colisiones y gráficos para que parezca que desapareció al instante
        if (col != null) col.enabled = false;
        foreach (Renderer r in renderers) r.enabled = false;

        // Solo el Host se encarga de la destrucción real en red y de avisarle al Spawner
        if (PhotonNetwork.IsMasterClient)
        {
            if (spawner != null) spawner.PowerUpCollected();

            // Calculamos cuánto dura el sonido para esperar antes de destruir el objeto
            float delay = (clipPlayed != null) ? clipPlayed.length : 0.1f;
            Invoke(nameof(NetworkDestroy), delay);
        }
    }

    void NetworkDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}