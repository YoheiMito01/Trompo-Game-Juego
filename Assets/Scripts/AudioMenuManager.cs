using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para detectar los cambios de escena

public class AudioMenuManager : MonoBehaviour
{
    // Asegura que solo exista un administrador de música a la vez (Singleton)
    public static AudioMenuManager Instance { get; private set; }

    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Nombres de tus Escenas de Pelea")]
    [Tooltip("Escribe exactamente el nombre de tus escenas de juego donde NO debe sonar esta música")]
    [SerializeField] private string nombreArena1 = "ArenaCubos";
    [SerializeField] private string nombreArena2 = "ArenaNormal";

    private void Awake()
    {
        // Sistema de seguridad: Si ya existe un reproductor de música, destruye el nuevo para no duplicar el sonido
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ¡LA MAGIA! Esto hace que el objeto sobreviva entre cambios de escena de menús
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Le decimos a Unity que nos avise cada vez que cambie de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Esta función corre automáticamente CADA VEZ que se carga cualquier escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si entramos a cualquiera de las arenas de combate...
        if (scene.name == nombreArena1 || scene.name == nombreArena2)
        {
            // Pausamos o destruimos la música del menú para que no se mezcle con la de pelea
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        else
        {
            // Si volvemos al menú principal desde una partida, la música vuelve a sonar
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
