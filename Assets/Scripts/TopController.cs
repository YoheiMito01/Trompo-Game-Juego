using UnityEngine;          // Libreria principal de Unity
using Photon.Pun;           // Libreria para utilizar Photon PUN y sincronizar objetos en red

// Clase encargada de controlar el movimiento y comportamiento del trompo
public class TopController : MonoBehaviourPun, IPunObservable
{
    // Referencia al Rigidbody utilizado para la fisica
    private Rigidbody rb;

    [Header("Movement")]

    // Fuerza de aceleracion aplicada al mover el trompo
    [SerializeField] private float acceleration = 20f;

    // Velocidad maxima permitida
    [SerializeField] private float maxSpeed = 10f;

    [Header("Spin")]

    // Cantidad inicial de giro del trompo
    [SerializeField] private float spinSpeed = 1000f;

    // Giro actual del trompo
    [SerializeField] private float currentSpin;

    // Velocidad con la que disminuye el giro
    [SerializeField] private float spinDecay = 5f;

    [Header("Visual References")]

    // Objeto que se inclina visualmente
    [SerializeField] private Transform visualPivot;

    // Modelo visual que gira constantemente
    [SerializeField] private Transform visualModel;

    [Header("Wobble")]

    // Angulo maximo de inclinacion
    [SerializeField] private float maxTiltAngle = 25f;

    // Velocidad de la oscilacion
    [SerializeField] private float wobbleSpeed = 8f;

    [Header("Death")]

    // Fuerza aplicada cuando el trompo cae
    [SerializeField] private float fallTorque = 25f;

    // Indica si el jugador esta arrastrando el mouse
    private bool isDragging = false;

    // Indica si el trompo ya cayo
    private bool hasFallen = false;

    // Direccion hacia donde debe moverse el trompo
    private Vector3 targetDirection;

    // Se ejecuta automaticamente al crear el objeto
    void Start()
    {
        // Obtiene el Rigidbody del objeto
        rb = GetComponent<Rigidbody>();

        // Solo el dueño del trompo inicializa su energia
        if (photonView.IsMine)
        {
            currentSpin = spinSpeed;
        }
    }

    // Se ejecuta una vez por cada frame
    void Update()
    {
        // Solo el dueño del trompo puede controlarlo
        if (!photonView.IsMine)
            return;

        // Gestiona la entrada del mouse
        HandleInput();
    }

    // Se ejecuta a intervalos fijos para la fisica
    void FixedUpdate()
    {
        // Si este cliente es el dueño controla movimiento y energia
        if (photonView.IsMine)
        {
            MoveTop();

            HandleSpin();
        }
        else
        {
            // Los demas clientes solo actualizan la parte visual
            HandleVisualSpin();

            HandleWobble();
        }
    }

    // Gestiona el movimiento mediante el mouse
    void HandleInput()
    {
        // Si el trompo ya cayo no puede moverse
        if (hasFallen)
            return;

        // Detecta cuando se presiona el boton izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            // Crea un rayo desde la camara hacia el mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Comprueba si golpea algun objeto
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Solo comienza a mover si se hizo clic sobre este trompo
                if (hit.transform == transform)
                    isDragging = true;
            }
        }

        // Detecta cuando se suelta el boton del mouse
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            targetDirection = Vector3.zero;
        }

        // Mientras el jugador arrastra el mouse
        if (isDragging)
        {
            // Obtiene la posicion del mouse sobre el suelo
            Vector3 mouseWorldPos = GetMouseWorldPosition();

            // Calcula la direccion desde el trompo hasta el mouse
            Vector3 direction = mouseWorldPos - transform.position;

            // Elimina el movimiento vertical
            direction.y = 0;

            // Convierte la direccion en un vector normalizado
            targetDirection = direction.normalized;
        }
    }

    // Aplica el movimiento fisico del trompo
    void MoveTop()
    {
        // Si el trompo ya cayo no se mueve
        if (hasFallen)
            return;

        // Si no existe direccion no hace nada
        if (targetDirection == Vector3.zero)
            return;

        // Aplica una fuerza continua
        rb.AddForce(targetDirection * acceleration, ForceMode.Acceleration);

        // Obtiene la velocidad horizontal
        Vector3 flatVelocity = rb.velocity;

        flatVelocity.y = 0;

        // Limita la velocidad maxima
        if (flatVelocity.magnitude > maxSpeed)
        {
            flatVelocity = flatVelocity.normalized * maxSpeed;

            rb.velocity = new Vector3(
                flatVelocity.x,
                rb.velocity.y,
                flatVelocity.z
            );
        }
    }

    // Controla la perdida gradual de energia del trompo
    void HandleSpin()
    {
        // Si el trompo ya cayo termina la funcion
        if (hasFallen)
            return;

        // Reduce la energia con el paso del tiempo
        currentSpin -= spinDecay * Time.fixedDeltaTime;

        // Evita valores negativos
        currentSpin = Mathf.Max(currentSpin, 0);

        // Actualiza el giro visual
        HandleVisualSpin();

        // Actualiza la inclinacion visual
        HandleWobble();

        // Si ya no queda energia el trompo cae
        if (currentSpin <= 0)
        {
            FallOver();
        }
    }

    // Hace girar el modelo visual
    void HandleVisualSpin()
    {
        // Comprueba que exista un modelo asignado
        if (visualModel == null)
            return;

        // Aplica una rotacion continua
        visualModel.Rotate(
            Vector3.up * currentSpin * Time.fixedDeltaTime,
            Space.Self
        );
    }

    // Simula la oscilacion del trompo cuando pierde energia
    void HandleWobble()
    {
        // Comprueba que exista el pivote visual
        if (visualPivot == null)
            return;

        // Calcula el porcentaje de energia restante
        float spinPercent = currentSpin / spinSpeed;

        // Cuanto menos energia tenga mayor sera la inclinacion
        float wobbleAmount = 1f - spinPercent;

        // Calcula la inclinacion en X
        float tiltX =
            Mathf.Sin(Time.time * wobbleSpeed)
            * maxTiltAngle
            * wobbleAmount;

        // Calcula la inclinacion en Z
        float tiltZ =
            Mathf.Cos(Time.time * wobbleSpeed * 1.2f)
            * maxTiltAngle
            * wobbleAmount;

        // Aplica la rotacion al pivote visual
        visualPivot.localRotation =
            Quaternion.Euler(tiltX, 0, tiltZ);
    }

    // Hace caer el trompo cuando pierde toda su energia
    void FallOver()
    {
        // Marca el trompo como derrotado
        hasFallen = true;

        // Aplica una fuerza de giro para simular la caida
        rb.AddTorque(
            transform.right * fallTorque,
            ForceMode.Impulse
        );
    }

    // Agrega energia al trompo
    public void AddSpin(float amount)
    {
        // Solo el dueño puede modificar su energia
        if (!photonView.IsMine)
            return;

        // Si el trompo ya cayo no puede recuperar energia
        if (hasFallen)
            return;

        // Suma la cantidad recibida
        currentSpin += amount;
    }

    // Convierte la posicion del mouse en una posicion sobre el suelo
    private Vector3 GetMouseWorldPosition()
    {
        // Crea un plano horizontal
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        // Crea un rayo desde la camara
        Ray ray =
            Camera.main.ScreenPointToRay(Input.mousePosition);

        // Comprueba donde intersecta el plano
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Si falla devuelve un vector vacio
        return Vector3.zero;
    }

    // Se ejecuta cuando el trompo colisiona con otro objeto
    private void OnCollisionEnter(Collision collision)
    {
        // Solo el dueño procesa la colision
        if (!photonView.IsMine)
            return;

        // Comprueba si el objeto golpeado tambien es un trompo
        TopController otherTop =
            collision.gameObject.GetComponent<TopController>();

        if (otherTop == null)
            return;

        // Resta energia al producirse el impacto
        currentSpin -= 50f;
    }

    // Sincroniza datos entre todos los jugadores
    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {
        // Si este cliente es el dueño envia informacion
        if (stream.IsWriting)
        {
            // Envia la energia actual
            stream.SendNext(currentSpin);

            // Envia si el trompo ya cayo
            stream.SendNext(hasFallen);
        }
        else
        {
            // Recibe la energia enviada por el dueño
            currentSpin = (float)stream.ReceiveNext();

            // Recibe el estado de caida
            hasFallen = (bool)stream.ReceiveNext();
        }
    }
}