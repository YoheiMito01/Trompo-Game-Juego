using UnityEngine;

public class SpinBoost : MonoBehaviour
{
    [SerializeField] private float spinBonus = 300f;
    [SerializeField] private float lifeTime = 3f;

    private PowerUpSpawner spawner;

    private void Start()
    {
        spawner = FindObjectOfType<PowerUpSpawner>();

        // Se destruye automáticamente después de 3 segundos
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        TopController top = other.GetComponent<TopController>();

        if (top == null)
            return;

        top.AddSpin(spinBonus);

        if (spawner != null)
            spawner.PowerUpCollected();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.PowerUpCollected();
    }
}