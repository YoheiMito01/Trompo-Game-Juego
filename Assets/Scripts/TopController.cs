using UnityEngine;

using Photon.Pun;


public class TopController : MonoBehaviourPun, IPunObservable

{

    private Rigidbody rb;

    [Header("Effects")]
    [Tooltip("Prefab del sistema de partículas para los choques")]
    [SerializeField] private GameObject impactEffectPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // El componente que reproduce el sonido
    [SerializeField] private AudioClip[] sonidosDeChoque; // Lista de sonidos modificable en el Inspector


    [Header("Movement")]

    [SerializeField] private float acceleration = 20f;

    [SerializeField] private float maxSpeed = 10f;


    [Header("Spin")]

    [SerializeField] private float spinSpeed = 1000f;

    [SerializeField] public float currentSpin;

    [SerializeField] private float spinDecay = 5f;


    [Header("Visual References")]

    [SerializeField] private Transform visualPivot;

    [SerializeField] private Transform visualModel;


    [Header("Wobble")]

    [SerializeField] private float maxTiltAngle = 25f;

    [SerializeField] private float wobbleSpeed = 8f;


    [Header("Death")]

    [SerializeField] private float fallTorque = 25f;


    private bool isDragging = false;

    private bool hasFallen = false;

    private Vector3 targetDirection;


    void Start()

    {

        rb = GetComponent<Rigidbody>();

        currentSpin = spinSpeed;

        if (photonView.IsMine)

        {

            MatchManager.Instance.RegistrarTrompo();

        }

    }


    void Update()

    {

        // El dueño controla el Input

        if (photonView.IsMine)

        {

            HandleInput();

        }

        else

        {

            // Los jugadores remotos TAMBIÉN necesitan ver girar y tambalear el trompo

            // basándose en el 'currentSpin' que reciben por red.

            HandleVisualSpin();

            HandleWobble();

        }

    }


    void FixedUpdate()

    {

        if (photonView.IsMine)

        {

            MoveTop();

            HandleSpin();

        }

    }


    void HandleInput()

    {

        if (hasFallen) return;


        if (Input.GetMouseButtonDown(0))

        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))

            {

                if (hit.transform == transform)

                    isDragging = true;

            }

        }


        if (Input.GetMouseButtonUp(0))

        {

            isDragging = false;

            targetDirection = Vector3.zero;

        }


        if (isDragging)

        {

            Vector3 mouseWorldPos = GetMouseWorldPosition();

            Vector3 direction = mouseWorldPos - transform.position;

            direction.y = 0;

            targetDirection = direction.normalized;

        }

    }


    void MoveTop()

    {

        if (hasFallen || targetDirection == Vector3.zero) return;


        rb.AddForce(targetDirection * acceleration, ForceMode.Acceleration);


        Vector3 flatVelocity = rb.velocity;

        flatVelocity.y = 0;


        if (flatVelocity.magnitude > maxSpeed)

        {

            flatVelocity = flatVelocity.normalized * maxSpeed;

            rb.velocity = new Vector3(flatVelocity.x, rb.velocity.y, flatVelocity.z);

        }

    }


    void HandleSpin()

    {

        if (hasFallen) return;


        currentSpin -= spinDecay * Time.fixedDeltaTime;

        currentSpin = Mathf.Max(currentSpin, 0);


        // El dueño procesa sus visuales aquí

        HandleVisualSpin();

        HandleWobble();


        if (currentSpin <= 0)

        {

            FallOver();

        }

        if (photonView.IsMine && currentSpin <= 0)

        {

            // Asegúrate de que el giro no baje de 0

            currentSpin = 0;


            // Le avisamos al juez que nuestro jugador (usando su ActorNumber único) perdió

            MatchManager.Instance.ReportarDerrota(PhotonNetwork.LocalPlayer.ActorNumber);

        }

    }


    void HandleVisualSpin()

    {

        if (visualModel == null) return;


        // Cambiamos Time.fixedDeltaTime por Time.deltaTime ya que se manda a llamar en Update para los remotos

        visualModel.Rotate(Vector3.up * currentSpin * Time.deltaTime, Space.Self);

    }


    void HandleWobble()

    {

        if (visualPivot == null) return;


        float spinPercent = currentSpin / spinSpeed;

        float wobbleAmount = 1f - spinPercent;


        float tiltX = Mathf.Sin(Time.time * wobbleSpeed) * maxTiltAngle * wobbleAmount;

        float tiltZ = Mathf.Cos(Time.time * wobbleSpeed * 1.2f) * maxTiltAngle * wobbleAmount;


        visualPivot.localRotation = Quaternion.Euler(tiltX, 0, tiltZ);

    }


    void FallOver()

    {

        if (hasFallen) return; // Evitar que se ejecute múltiples veces

        hasFallen = true;


        rb.AddTorque(transform.right * fallTorque, ForceMode.Impulse);

    }


    public void AddSpin(float amount)

    {

        if (!photonView.IsMine || hasFallen) return;

        currentSpin += amount;

    }


    private Vector3 GetMouseWorldPosition()

    {

        Plane plane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


        if (plane.Raycast(ray, out float distance))

        {

            return ray.GetPoint(distance);

        }

        return Vector3.zero;

    }


    private void OnCollisionEnter(Collision collision)
    {
        // Solo el dueño procesa el golpe para restar estamina y lanzar el evento de red
        if (!photonView.IsMine) return;

        TopController otherTop = collision.gameObject.GetComponent<TopController>();
        if (otherTop == null) return;

        // Restamos la estamina
        currentSpin -= 50f;

        // Obtener la posición exacta donde los dos trompos se tocaron
        Vector3 contactPoint = collision.GetContact(0).point;

        // Le decimos a TODOS los jugadores conectados que reproduzcan el efecto en ese punto
        photonView.RPC(nameof(RpcPlayImpactEffect), RpcTarget.All, contactPoint);
    }

    // Esta función se ejecuta en las pantallas de todos los jugadores al mismo tiempo
    [PunRPC]
    private void RpcPlayImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab != null)
        {
            // Instanciamos el efecto visual en el punto de contacto
            GameObject effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);

            // Destruimos el objeto del efecto después de 2 segundos para liberar memoria
            // (Ajusta este tiempo según lo que dure la animación de tus partículas)
            Destroy(effect, 2f);
        }
    }
    private void ReproducirSonidoAleatorio()
    {
        // Seguro de vida: Si no has asignado el AudioSource o la lista de sonidos, no hace nada
        if (audioSource == null || sonidosDeChoque == null || sonidosDeChoque.Length == 0) return;

        // Elegimos un sonido al azar de la lista que creaste en el Inspector
        int indiceAleatorio = Random.Range(0, sonidosDeChoque.Length);
        AudioClip clipSeleccionado = sonidosDeChoque[indiceAleatorio];

        // Cambiamos ligeramente el tono (Pitch) para que cada choque suene único y orgánico
        audioSource.pitch = Random.Range(0.85f, 1.15f);

        // Reproduce el sonido sin interrumpir otros audios del trompo (como el zumbido de giro)
        audioSource.PlayOneShot(clipSeleccionado);
    }


    // Sincronización de Red estricta

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

    {

        if (stream.IsWriting)

        {

            // ENVIAR (Dueño)

            stream.SendNext(currentSpin);

            stream.SendNext(hasFallen);

        }

        else

        {

            // RECIBIR (Clientes remotos)

            this.currentSpin = (float)stream.ReceiveNext();


            bool remoteHasFallen = (bool)stream.ReceiveNext();

            // Si el dueño dice que ya cayó y localmente aún no lo sabemos, hacemos que caiga

            if (remoteHasFallen && !this.hasFallen)

            {

                FallOver();

            }

        }

    }

    public void TakeLaserDamage(float amount)

    {

        // Solo el dueño del trompo puede restarse energía, sino se restaría doble en el online

        if (!photonView.IsMine || hasFallen) return;


        currentSpin -= amount;


        // Evitamos que baje de cero

        currentSpin = Mathf.Max(currentSpin, 0);


        // Si llega a 0, el trompo cae

        if (currentSpin <= 0)

        {

            FallOver();

        }

    }

}