using UnityEngine;
using Photon.Pun;

public class TopController : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Spin")]
    [SerializeField] private float spinSpeed = 1000f;
    [SerializeField] private float currentSpin;
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

        // NOTA: Quitamos el rb.isKinematic = true. 
        // Permitiremos que la física reaccione en ambos lados, 
        // y Photon Rigidbody View se encargará de corregir las posiciones.
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
        // Las colisiones de trompos son mutuas, dejamos que cada dueño procese cuando le pegan
        if (!photonView.IsMine) return;

        TopController otherTop = collision.gameObject.GetComponent<TopController>();
        if (otherTop == null) return;

        currentSpin -= 50f;
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
}