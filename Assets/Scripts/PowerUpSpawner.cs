using System.Collections;      // Libreria para utilizar corrutinas
using UnityEngine;             // Libreria principal de Unity
using Photon.Pun;              // Libreria para utilizar Photon PUN

// Clase encargada de generar automaticamente los boosts en la arena (Área Circular)
public class PowerUpSpawner : MonoBehaviourPun
{
    // Prefab del boost que sera generado (Debe estar en la carpeta Resources)
    [SerializeField] private GameObject powerUpPrefab;

    // Tiempo de espera entre cada intento de aparicion
    [SerializeField] private float spawnTime = 10f;

    // CAMBIO: Ahora usamos un Radio para definir el tamaño del círculo de la arena
    [SerializeField] private float arenaRadius = 10f;

    // Ajusta la altura exacta (Y) en la que quieres que flote el boost sobre el suelo
    [SerializeField] private float spawnHeightY = 0.5f;

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

    // CAMBIO: Dibuja un círculo verde en el editor de Unity para visualizar el área redonda
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 centro = transform.position;
        float pasos = 50f; // Qué tan definido se verá el círculo en el editor
        float perimetro = 2f * Mathf.PI;
        Vector3 puntoAnterior = centro + new Vector3(arenaRadius, 0f, 0f);

        for (int i = 1; i <= pasos; i++)
        {
            float angulo = (i / pasos) * perimetro;
            Vector3 puntoSiguiente = centro + new Vector3(Mathf.Cos(angulo) * arenaRadius, 0f, Mathf.Sin(angulo) * arenaRadius);

            // Forzamos la altura visual en los gizmos para que coincida con el suelo/spawn
            puntoAnterior.y = spawnHeightY;
            puntoSiguiente.y = spawnHeightY;

            Gizmos.DrawLine(puntoAnterior, puntoSiguiente);
            puntoAnterior = puntoSiguiente;
        }
    }

    // Corrutina que controla la aparicion de los boosts
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTime);

            if (currentPowerUp != null)
                continue;

            SpawnPowerUp();
        }
    }

    // CAMBIO: Genera un nuevo boost en una posicion aleatoria CIRCULAR
    void SpawnPowerUp()
    {
        // Genera un punto aleatorio bidimensional dentro de un círculo de radio 1
        Vector2 puntoEnCirculo = Random.insideUnitCircle * arenaRadius;

        // Convertimos ese punto 2D (X, Y) a coordenadas 3D del mundo (X, Z) usando la posición del spawner como centro
        Vector3 pos = transform.position + new Vector3(puntoEnCirculo.x, 0f, puntoEnCirculo.y);

        // Ajusta la altura configurada
        pos.y = spawnHeightY;

        // Crea el boost utilizando Photon para que aparezca sincronizado
        currentPowerUp = PhotonNetwork.Instantiate(
            powerUpPrefab.name,
            pos,
            Quaternion.identity
        );
    }

    public void PowerUpCollected()
    {
        currentPowerUp = null;
    }
}