using UnityEngine;          // Libreria principal de Unity
using Photon.Pun;           // Libreria para utilizar Photon PUN

// Clase encargada de controlar el funcionamiento del boost de energia
public class SpinBoost : MonoBehaviourPun
{
    // Cantidad de energia que recibe el trompo al recoger el boost
    [SerializeField] private float spinBonus = 300f;

    // Tiempo maximo que permanecera el boost en la arena
    [SerializeField] private float lifeTime = 3f;

    // Referencia al objeto que genera los boosts
    private PowerUpSpawner spawner;

    // Variable que evita que el boost sea recogido o destruido varias veces
    private bool collected = false;

    // Se ejecuta automaticamente al crear el boost
    void Start()
    {
        // Busca el generador de boosts en la escena
        spawner = FindObjectOfType<PowerUpSpawner>();

        // Solo el Host controla el tiempo de vida del boost
        if (PhotonNetwork.IsMasterClient)
        {
            // Programa la destruccion automatica despues de unos segundos
            Invoke(nameof(DestroyBoost), lifeTime);
        }
    }

    // Destruye el boost cuando nadie lo recoge antes del tiempo limite
    void DestroyBoost()
    {
        // Si ya fue recogido no hace nada
        if (collected)
            return;

        // Marca el boost como utilizado
        collected = true;

        // Informa al generador que ya no existe un boost activo
        if (spawner != null)
            spawner.PowerUpCollected();

        // Destruye el boost para todos los jugadores conectados
        PhotonNetwork.Destroy(gameObject);
    }

    // Se ejecuta cuando otro objeto entra en el collider del boost
    private void OnTriggerEnter(Collider other)
    {
        // Si el boost ya fue utilizado no hace nada
        if (collected)
            return;

        // Busca el componente TopController del objeto que entro
        TopController top =
            other.GetComponent<TopController>();

        // Si el objeto no es un trompo termina la funcion
        if (top == null)
            return;

        // Obtiene el PhotonView del trompo
        PhotonView topView =
            top.GetComponent<PhotonView>();

        // Si no existe un PhotonView termina la funcion
        if (topView == null)
            return;

        // Solo el dueño del trompo puede recoger el boost
        if (!topView.IsMine)
            return;

        // Marca el boost como utilizado
        collected = true;

        // Agrega energia extra al trompo
        top.AddSpin(spinBonus);

        // Si este cliente es el Host destruye directamente el boost
        if (PhotonNetwork.IsMasterClient)
        {
            // Informa al generador que puede crear uno nuevo
            if (spawner != null)
                spawner.PowerUpCollected();

            // Destruye el boost para todos los jugadores
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // Si no es el Host envia una solicitud al Host
            // para que este destruya el boost correctamente
            photonView.RPC(
                nameof(RequestDestroy),
                RpcTarget.MasterClient
            );
        }
    }

    // Funcion remota que solo ejecuta el Host
    [PunRPC]
    void RequestDestroy()
    {
        // Si ya fue destruido no hace nada
        if (collected)
            return;

        // Marca el boost como utilizado
        collected = true;

        // Informa al generador que ya no existe un boost activo
        if (spawner != null)
            spawner.PowerUpCollected();

        // Destruye el boost en todos los clientes
        PhotonNetwork.Destroy(gameObject);
    }
}