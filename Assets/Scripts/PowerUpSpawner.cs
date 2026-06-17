using System.Collections;      // Libreria para utilizar corrutinas
using UnityEngine;             // Libreria principal de Unity
using Photon.Pun;              // Libreria para utilizar Photon PUN

// Clase encargada de generar automaticamente los boosts en la arena
public class PowerUpSpawner : MonoBehaviourPun
{
    // Prefab del boost que sera generado
    [SerializeField] private GameObject powerUpPrefab;

    // Tiempo de espera entre cada intento de aparicion
    [SerializeField] private float spawnTime = 10f;

    // Tamaño del area donde pueden aparecer los boosts
    [SerializeField] private Vector2 arenaSize = new Vector2(20, 20);

    // Referencia al boost que actualmente existe en la escena
    private GameObject currentPowerUp;

    // Se ejecuta automaticamente al iniciar la escena
    private void Start()
    {
        // Solo el Host tiene permitido generar boosts
        // Esto evita que aparezcan varios boosts diferentes en cada cliente
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Inicia la corrutina encargada de crear los boosts periodicamente
        StartCoroutine(SpawnRoutine());
    }

    // Dibuja un cuadro verde en el editor de Unity
    // para visualizar el area de aparicion de los boosts
    private void OnDrawGizmos()
    {
        // Define el color del dibujo
        Gizmos.color = Color.green;

        // Dibuja un cubo transparente que representa el area de spawn
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(arenaSize.x, 0.1f, arenaSize.y)
        );
    }

    // Corrutina que controla la aparicion de los boosts
    IEnumerator SpawnRoutine()
    {
        // Se ejecuta continuamente mientras exista el objeto
        while (true)
        {
            // Espera el tiempo configurado antes de continuar
            yield return new WaitForSeconds(spawnTime);

            // Si ya existe un boost en la escena espera al siguiente ciclo
            if (currentPowerUp != null)
                continue;

            // Si no existe ninguno crea uno nuevo
            SpawnPowerUp();
        }
    }

    // Genera un nuevo boost en una posicion aleatoria
    void SpawnPowerUp()
    {
        // Comienza utilizando la posicion del objeto spawner
        Vector3 pos = transform.position;

        // Genera una posicion aleatoria dentro del ancho permitido
        pos.x += Random.Range(-arenaSize.x / 2f, arenaSize.x / 2f);

        // Genera una posicion aleatoria dentro del largo permitido
        pos.z += Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f);

        // Ajusta la altura para que el boost quede sobre el suelo
        pos.y = 0.5f;

        // Crea el boost utilizando Photon para que aparezca
        // sincronizado en todos los jugadores conectados
        currentPowerUp = PhotonNetwork.Instantiate(
            powerUpPrefab.name,
            pos,
            Quaternion.identity
        );
    }

    // Se llama cuando un jugador recoge el boost
    // o cuando este desaparece por tiempo
    public void PowerUpCollected()
    {
        // Elimina la referencia al boost actual
        // permitiendo que pueda generarse uno nuevo
        currentPowerUp = null;
    }
}