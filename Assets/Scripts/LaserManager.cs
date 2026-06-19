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
            // Creamos contenedores (arrays) para guardar los datos de AMBOS láseres
            Vector3[] posiciones = new Vector3[2];
            Quaternion[] rotaciones = new Quaternion[2];
            GameObject[] avisos = new GameObject[2];
            GameObject[] lasersReales = new GameObject[2];
            float[] direccionesGiro = new float[2];

            // 1. CALCULAR Y CREAR LOS DOS AVISOS
            for (int i = 0; i < 2; i++)
            {
                // Calcular posición aleatoria para el láser i
                Vector2 puntoEnCirculo = Random.insideUnitCircle * radioArena;
                Vector3 posicionLocal = new Vector3(puntoEnCirculo.x, 0f, puntoEnCirculo.y);
                posiciones[i] = transform.position + posicionLocal;
                posiciones[i].y = alturaY;

                // Calcular rotación aleatoria para el láser i
                float anguloHorizontal = Random.Range(0f, 360f);
                rotaciones[i] = Quaternion.Euler(90f, anguloHorizontal, 0f);

                // Instanciar el aviso y guardarlo en el array
                avisos[i] = Instantiate(prefabAviso, posiciones[i], rotaciones[i]);

                // Elegir si este láser en particular gira a la derecha (1) o izquierda (-1)
                direccionesGiro[i] = Random.value > 0.5f ? 1f : -1f;
            }

            // Esperar el tiempo de advertencia (ambos avisos están visibles a la vez)
            yield return new WaitForSeconds(tiempoDeAviso);

            // 2. DESTRUIR AVISOS E INSTANCIAR LÁSERES REALES
            for (int i = 0; i < 2; i++)
            {
                Destroy(avisos[i]);
                // Nacen en la misma posición y rotación que su respectivo aviso
                lasersReales[i] = Instantiate(prefabLaserReal, posiciones[i], rotaciones[i]);
            }

            // 3. ROTAR AMBOS LÁSERES EN PARALELO
            float tiempoActivo = 0f;
            while (tiempoActivo < duracionDelLaser)
            {
                // Este bucle interno rota ambos láseres en el mismo frame
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
                yield return null; // Avanzar al siguiente frame
            }

            // 4. DESTRUIR AMBOS LÁSERES
            for (int i = 0; i < 2; i++)
            {
                Destroy(lasersReales[i]);
            }

            // Esperar antes de la siguiente oleada de dos láseres
            yield return new WaitForSeconds(tiempoEntreAtaques);
        }
    }

    private void OnDrawGizmosSelected()
    {
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