using System.Collections;
using UnityEngine;
using Photon.Pun;

public class NetworkStadiumManager : MonoBehaviourPun
{
    [Header("Referencias de Prefabs (NO necesitan PhotonView)")]
    public GameObject prefabAviso; // Un plano o círculo transparente en el suelo
    public GameObject prefabCubo;  // El cubo gigante que caerá

    [Header("Área del Estadio")]
    public float radioEstadio = 15f;
    public float alturaSuelo = 0.5f;

    [Header("Tiempos y Alturas")]
    public float tiempoDeAviso = 2f;    // Cuánto tiempo se muestra la marca antes de caer
    public float alturaDeCaida = 20f;   // Desde qué altura cae el cubo
    public float velocidadCaida = 0.5f; // Cuánto tarda en tocar el suelo (en segundos)
    public float tiempoEnArena = 5f;    // Cuánto tiempo se queda estorbando en la arena
    public float tiempoEntreCubos = 4f; // Tiempo de paz entre un cubo y el siguiente

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(MasterClientLoop());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 centro = transform.position;
        float pasos = 50f;
        float perimetro = 2f * Mathf.PI;

        Vector3 puntoAnterior = centro + new Vector3(radioEstadio, 0f, 0f);
        puntoAnterior.y = alturaSuelo;

        for (int i = 1; i <= pasos; i++)
        {
            float angulo = (i / pasos) * perimetro;
            Vector3 puntoSiguiente = centro + new Vector3(Mathf.Cos(angulo) * radioEstadio, 0f, Mathf.Sin(angulo) * radioEstadio);
            puntoSiguiente.y = alturaSuelo;

            Gizmos.DrawLine(puntoAnterior, puntoSiguiente);
            puntoAnterior = puntoSiguiente;
        }
    }

    IEnumerator MasterClientLoop()
    {
        yield return new WaitForSeconds(5f); // Espera inicial

        while (true)
        {
            // 1. Calcular posición aleatoria en el estadio
            Vector2 puntoEnCirculo = Random.insideUnitCircle * radioEstadio;
            Vector3 posicionCaida = transform.position + new Vector3(puntoEnCirculo.x, alturaSuelo, puntoEnCirculo.y);

            // 2. Avisar a todos los jugadores
            photonView.RPC("RPC_LanzarCubo", RpcTarget.All, posicionCaida);

            // 3. Esperar a que termine todo el ciclo del cubo antes de lanzar otro
            yield return new WaitForSeconds(tiempoDeAviso + velocidadCaida + tiempoEnArena + tiempoEntreCubos);
        }
    }

    [PunRPC]
    void RPC_LanzarCubo(Vector3 posicionSuelo)
    {
        // Todos los clientes ejecutan la rutina visual
        StartCoroutine(RutinaCaidaCubo(posicionSuelo));
    }

    IEnumerator RutinaCaidaCubo(Vector3 posicionSuelo)
    {
        // 1. Mostrar el aviso en el suelo
        GameObject aviso = Instantiate(prefabAviso, posicionSuelo, Quaternion.identity);

        // 2. Esperar el tiempo de advertencia
        yield return new WaitForSeconds(tiempoDeAviso);

        // Destruir el aviso justo cuando va a caer
        Destroy(aviso);

        // 3. Crear el cubo en el cielo
        Vector3 posicionCielo = posicionSuelo + new Vector3(0, alturaDeCaida, 0);
        GameObject cubo = Instantiate(prefabCubo, posicionCielo, Quaternion.identity);

        // 4. Animar la caída del cubo suavemente
        float tiempo = 0;
        while (tiempo < velocidadCaida)
        {
            if (cubo != null)
            {
                tiempo += Time.deltaTime;
                float porcentaje = tiempo / velocidadCaida;
                cubo.transform.position = Vector3.Lerp(posicionCielo, posicionSuelo, porcentaje);
            }
            yield return null;
        }

        // Asegurar que quede exactamente en el suelo
        if (cubo != null) cubo.transform.position = posicionSuelo;

        // 5. Dejarlo en la arena un tiempo
        yield return new WaitForSeconds(tiempoEnArena);

        // 6. Destruirlo (puedes agregarle un efecto de partículas aquí antes de destruirlo)
        if (cubo != null) Destroy(cubo);
    }
}