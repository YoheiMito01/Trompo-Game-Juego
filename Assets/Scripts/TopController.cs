using UnityEngine;

public class TopController : MonoBehaviour
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
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        MoveTop();
        HandleSpin();
    }

    void HandleInput()
    {
        if (hasFallen)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                }
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

            direction.y = 0f;

            targetDirection = direction.normalized;
        }
    }

    void MoveTop()
    {
        if (hasFallen)
            return;

        if (targetDirection == Vector3.zero)
            return;

        rb.AddForce(targetDirection * acceleration, ForceMode.Acceleration);

        Vector3 flatVelocity = rb.velocity;
        flatVelocity.y = 0f;

        if (flatVelocity.magnitude > maxSpeed)
        {
            flatVelocity = flatVelocity.normalized * maxSpeed;

            rb.velocity = new Vector3(
                flatVelocity.x,
                rb.velocity.y,
                rb.velocity.z
            );
        }
    }

    void HandleSpin()
    {
        if (hasFallen)
            return;

        currentSpin -= spinDecay * Time.fixedDeltaTime;
        currentSpin = Mathf.Max(currentSpin, 0);

        HandleVisualSpin();
        HandleWobble();

        if (currentSpin <= 0)
        {
            FallOver();
        }
    }

    void HandleVisualSpin()
    {
        if (visualModel == null)
            return;

        visualModel.Rotate(
            Vector3.up * currentSpin * Time.fixedDeltaTime,
            Space.Self
        );
    }

    void HandleWobble()
    {
        if (visualPivot == null)
            return;

        float spinPercent = currentSpin / spinSpeed;

        float wobbleAmount = 1f - spinPercent;

        float tiltX =
            Mathf.Sin(Time.time * wobbleSpeed)
            * maxTiltAngle
            * wobbleAmount;

        float tiltZ =
            Mathf.Cos(Time.time * wobbleSpeed * 1.2f)
            * maxTiltAngle
            * wobbleAmount;

        visualPivot.localRotation =
            Quaternion.Euler(tiltX, 0f, tiltZ);
    }

    void FallOver()
    {
        hasFallen = true;

        rb.AddTorque(
            transform.right * fallTorque,
            ForceMode.Impulse
        );
    }
    public void AddSpin(float amount)
    {
        if (hasFallen)
            return;

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
        TopController otherTop =
            collision.gameObject.GetComponent<TopController>();

        if (otherTop == null)
            return;

        Vector3 hitDirection =
            (collision.transform.position - transform.position).normalized;

        float impactForce = currentSpin * 0.01f;

        otherTop.rb.AddForce(
            hitDirection * impactForce,
            ForceMode.Impulse
        );

        currentSpin -= 50f;
        otherTop.currentSpin -= 50f;
    }
}