using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalButtonSound : MonoBehaviour
{
    public static GlobalButtonSound Instance;

    [Header("Configuración de Sonido")]
    [Tooltip("Arrastra aquí el efecto de sonido global para los botones")]
    public AudioClip clickSound;

    private AudioSource audioSource;

    void Awake()
    {
        // Patrón Singleton: Evitamos clones si volvemos a cargar la primera escena
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Hace que el objeto no se destruya al cambiar de escena

            // Agregamos el componente AudioSource automáticamente por código
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // Le decimos a Unity que ejecute 'OnSceneLoaded' cada vez que termine de cargar una escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Esta función se dispara automáticamente al entrar a cualquier escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscamos TODOS los botones de la jerarquía (el 'true' incluye los que estén apagados/ocultos)
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button btn in allButtons)
        {
            // Removemos el evento primero para evitar que suene doble si se recarga la misma escena
            btn.onClick.RemoveListener(PlayClickSound);

            // Inyectamos la orden de reproducir el sonido en el evento OnClick del botón
            btn.onClick.AddListener(PlayClickSound);
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound); // Permite que varios sonidos se superpongan si haces clics muy rápidos
        }
    }

    void OnDestroy()
    {
        // Buena práctica: limpiar el evento de memoria si cerramos el juego
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
