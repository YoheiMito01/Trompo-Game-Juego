using System.Collections;
using UnityEngine;

public class LaserManager : MonoBehaviour
{
    [Header("Referencias de Prefabs")]
    public GameObject prefabAviso;
    public GameObject prefabLaserReal;

    [Header("Límites de la Arena (Círculo)")]
    public float radioArena = 10f;
    public float alturaY = 0.5f;
    [Tooltip("Distancia mínima que debe haber entre los dos láseres")]
    public float distanciaMinima = 5f; // <--- NUEVA VARIABLE

    [Header("Tiempos y Velocidades")]
    public float tiempoDeAviso = 1.5f;
    public float duracionDelLaser = 3f;
    public float velocidadDeGiro = 90f;
    public float tiempoEntreAtaques = 2f;

    void Start()
    {
        StartCoroutine(CicloDeAtaquesDobles());
    }

    IEnumerator CicloDeAtaquesDobles()
    {
        while (true)
        {
            Vector3[] posiciones = new Vector3[2];
            Quaternion[] rotaciones = new Quaternion[2];
            GameObject[] avisos = new GameObject[2];
            GameObject[] lasersReales = new GameObject[2];
            float[] direccionesGiro = new float[2];

            // 1. CALCULAR Y CREAR LOS DOS AVISOS
            for (int i = 0; i < 2; i++)
            {
                Vector3 posicionTentativa = Vector3.zero;
                bool posicionValida = false;
                int intentosSeguridad = 0; // Para evitar bucles infinitos

                // Bucle para buscar una posición que respete la distancia mínima
                do
                {
                    Vector2 puntoEnCirculo = Random.insideUnitCircle * radioArena;
                    posicionTentativa = transform.position + new Vector3(puntoEnCirculo.x, 0f, puntoEnCirculo.y);
                    posicionTentativa.y = alturaY;

                    if (i == 0)
                    {
                        // El primer láser siempre tiene una posición válida
                        posicionValida = true;
                    }
                    else
                    {
                        // Para el segundo láser, medimos la distancia contra el primero
                        float distanciaAlPrimerLaser = Vector3.Distance(posiciones[0], posicionTentativa);

                        if (distanciaAlPrimerLaser >= distanciaMinima)
                        {
                            posicionValida = true; // La distancia es correcta
                        }
                    }

                    intentosSeguridad++;
                    // Mecanismo de seguridad: Si después de 30 intentos no encuentra lugar, lo pone donde sea para no congelar el juego.
                    if (intentosSeguridad > 30)
                    {
                        posicionValida = true;
                        Debug.LogWarning("La distancia mínima es demasiado grande para el tamaño de la arena. Ajusta los valores.");
                    }

                } while (!posicionValida);

                // Guardar la posición válida
                posiciones[i] = posicionTentativa;

                // Calcular rotación y dirección
                float anguloHorizontal = Random.Range(0f, 360f);
                rotaciones[i] = Quaternion.Euler(90f, anguloHorizontal, 0f);
                direccionesGiro[i] = Random.value > 0.5f ? 1f : -1f;

                // Instanciar el aviso
                avisos[i] = Instantiate(prefabAviso, posiciones[i], rotaciones[i]);
            }

            // Esperar el tiempo de advertencia
            yield return new WaitForSeconds(tiempoDeAviso);

            // 2. DESTRUIR AVISOS E INSTANCIAR LÁSERES REALES
            for (int i = 0; i < 2; i++)
            {
                Destroy(avisos[i]);
                lasersReales[i] = Instantiate(prefabLaserReal, posiciones[i], rotaciones[i]);
            }

            // 3. ROTAR AMBOS LÁSERES EN PARALELO
            float tiempoActivo = 0f;
            while (tiempoActivo < duracionDelLaser)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (lasersReales[i] != null)
                    {
                        lasersReales[i].transform.Rotate(
                            Vector3.up,
                            velocidadDeGiro * direccionesGiro[i] * Time.deltaTime,
                            Space.World
                        );
                    }
                }
                tiempoActivo += Time.deltaTime;
                yield return null;
            }

            // 4. DESTRUIR AMBOS LÁSERES
            for (int i = 0; i < 2; i++)
            {
                Destroy(lasersReales[i]);
            }

            // Esperar antes de la siguiente oleada
            yield return new WaitForSeconds(tiempoEntreAtaques);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar el círculo de la arena (rojo)
        Gizmos.color = Color.red;
        Vector3 centro = new Vector3(transform.position.x, alturaY, transform.position.z);
        float pasos = 50f;
        float perimetro = 2f * Mathf.PI;
        Vector3 puntoAnterior = centro + new Vector3(radioArena, 0f, 0f);

        for (int i = 1; i <= pasos; i++)
        {
            float angulo = (i / pasos) * perimetro;
            Vector3 puntoSiguiente = centro + new Vector3(Mathf.Cos(angulo) * radioArena, 0f, Mathf.Sin(angulo) * radioArena);
            Gizmos.DrawLine(puntoAnterior, puntoSiguiente);
            puntoAnterior = puntoSiguiente;
        }
    }
}