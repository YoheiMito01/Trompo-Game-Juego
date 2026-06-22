using System.Collections;
using UnityEngine;
using Photon.Pun;

public class NetworkLaserManager : MonoBehaviourPun
{
    [Header("Referencias de Prefabs (NO necesitan PhotonView)")]
    public GameObject prefabAviso;
    public GameObject prefabLaserReal;

    [Header("Límites de la Arena (Círculo)")]
    public float radioArena = 10f;
    public float alturaY = 0.5f;
    public float distanciaMinima = 5f;

    [Header("Tiempos y Velocidades")]
    public float tiempoDeAviso = 1.5f;
    public float duracionDelLaser = 3f;
    public float velocidadDeGiro = 90f;
    public float tiempoEntreAtaques = 2f;

    void Start()
    {
        // SOLO el creador de la sala (MasterClient) decide cuándo y dónde salen
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(MasterClientLoop());
        }
    }

    IEnumerator MasterClientLoop()
    {
        // Esperar un poco al inicio de la partida
        yield return new WaitForSeconds(3f);

        while (true)
        {
            Vector3[] pos = new Vector3[2];
            float[] angulos = new float[2];
            float[] direcciones = new float[2];

            // --- 1. MASTER CLIENT CALCULA LAS POSICIONES ---
            for (int i = 0; i < 2; i++)
            {
                Vector3 posicionTentativa = Vector3.zero;
                bool posicionValida = false;
                int intentosSeguridad = 0;

                do
                {
                    Vector2 puntoEnCirculo = Random.insideUnitCircle * radioArena;
                    posicionTentativa = transform.position + new Vector3(puntoEnCirculo.x, 0f, puntoEnCirculo.y);
                    posicionTentativa.y = alturaY;

                    if (i == 0)
                    {
                        posicionValida = true;
                    }
                    else
                    {
                        if (Vector3.Distance(pos[0], posicionTentativa) >= distanciaMinima)
                        {
                            posicionValida = true;
                        }
                    }

                    intentosSeguridad++;
                    if (intentosSeguridad > 30) posicionValida = true;

                } while (!posicionValida);

                pos[i] = posicionTentativa;
                angulos[i] = Random.Range(0f, 360f);
                direcciones[i] = Random.value > 0.5f ? 1f : -1f;
            }

            // --- 2. MASTER CLIENT AVISA A TODOS LOS JUGADORES ---
            // Enviamos las coordenadas calculadas a todos a través de un RPC
            photonView.RPC("RPC_SpawnLasers", RpcTarget.All,
                pos[0], angulos[0], direcciones[0],
                pos[1], angulos[1], direcciones[1]);

            // El MasterClient espera a que termine el ataque antes de calcular el siguiente
            yield return new WaitForSeconds(tiempoDeAviso + duracionDelLaser + tiempoEntreAtaques);
        }
    }

    // --- 3. EJECUCIÓN LOCAL EN TODOS LOS CLIENTES ---
    [PunRPC]
    void RPC_SpawnLasers(Vector3 pos1, float angulo1, float dir1, Vector3 pos2, float angulo2, float dir2)
    {
        // Convertimos los datos recibidos en arrays para reusar nuestra lógica limpia
        Vector3[] posiciones = new Vector3[] { pos1, pos2 };
        Quaternion[] rotaciones = new Quaternion[] {
            Quaternion.Euler(90f, angulo1, 0f),
            Quaternion.Euler(90f, angulo2, 0f)
        };
        float[] direccionesGiro = new float[] { dir1, dir2 };

        // Todos los clientes inician la rutina visual exactamente con los mismos datos
        StartCoroutine(RutinaVisualLaser(posiciones, rotaciones, direccionesGiro));
    }

    IEnumerator RutinaVisualLaser(Vector3[] pos, Quaternion[] rot, float[] dirGiro)
    {
        GameObject[] avisos = new GameObject[2];
        GameObject[] lasersReales = new GameObject[2];

        // Crear avisos
        for (int i = 0; i < 2; i++)
        {
            avisos[i] = Instantiate(prefabAviso, pos[i], rot[i]);
        }

        yield return new WaitForSeconds(tiempoDeAviso);

        // Crear reales y destruir avisos
        for (int i = 0; i < 2; i++)
        {
            Destroy(avisos[i]);
            lasersReales[i] = Instantiate(prefabLaserReal, pos[i], rot[i]);
        }

        // Rotar
        float tiempoActivo = 0f;
        while (tiempoActivo < duracionDelLaser)
        {
            for (int i = 0; i < 2; i++)
            {
                if (lasersReales[i] != null)
                {
                    lasersReales[i].transform.Rotate(Vector3.up, velocidadDeGiro * dirGiro[i] * Time.deltaTime, Space.World);
                }
            }
            tiempoActivo += Time.deltaTime;
            yield return null;
        }

        // Destruir
        for (int i = 0; i < 2; i++)
        {
            Destroy(lasersReales[i]);
        }
    }
}
