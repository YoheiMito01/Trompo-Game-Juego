using System.Collections;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private GameObject powerUpPrefab;

    [SerializeField] private float spawnTime = 10f;

    [SerializeField] private Vector2 arenaSize = new Vector2(20, 20);

    private GameObject currentPowerUp;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(arenaSize.x, 0.1f, arenaSize.y)
        );
    }
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

    void SpawnPowerUp()
    {
        Vector3 pos = transform.position;

        pos.x += Random.Range(-arenaSize.x / 2f, arenaSize.x / 2f);
        pos.z += Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f);
        pos.y = 0.5f;

        currentPowerUp = Instantiate(
            powerUpPrefab,
            pos,
            Quaternion.identity
        );
    }

    public void PowerUpCollected()
    {
        currentPowerUp = null;
    }
}
