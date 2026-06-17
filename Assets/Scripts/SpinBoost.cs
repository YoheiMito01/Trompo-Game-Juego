using UnityEngine;
using Photon.Pun;

public class SpinBoost : MonoBehaviourPun
{
    [SerializeField] private float spinBonus = 300f;

    [SerializeField] private float lifeTime = 3f;

    private PowerUpSpawner spawner;

    private bool collected = false;

    void Start()
    {
        spawner = FindObjectOfType<PowerUpSpawner>();

        if (PhotonNetwork.IsMasterClient)
        {
            Invoke(nameof(DestroyBoost), lifeTime);
        }
    }

    void DestroyBoost()
    {
        if (collected)
            return;

        collected = true;

        if (spawner != null)
            spawner.PowerUpCollected();

        PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        TopController top =
            other.GetComponent<TopController>();

        if (top == null)
            return;

        PhotonView topView =
            top.GetComponent<PhotonView>();

        if (topView == null)
            return;

        if (!topView.IsMine)
            return;

        collected = true;

        top.AddSpin(spinBonus);

        if (PhotonNetwork.IsMasterClient)
        {
            if (spawner != null)
                spawner.PowerUpCollected();

            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            photonView.RPC(
                nameof(RequestDestroy),
                RpcTarget.MasterClient
            );
        }
    }

    [PunRPC]
    void RequestDestroy()
    {
        if (collected)
            return;

        collected = true;

        if (spawner != null)
            spawner.PowerUpCollected();

        PhotonNetwork.Destroy(gameObject);
    }
}