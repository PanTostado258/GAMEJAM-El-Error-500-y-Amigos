using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7.0f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Mouse / Cámara")]
    public Transform cameraPivot;           // referencia al transform de la cámara (CameraPivot)
    public float mouseSensitivity = 2.0f;
    public float maxPitch = 80f;
    public float minPitch = -80f;

    [Header("Interacción")]
    public float pickupRange = 2.5f;
    public LayerMask pickupMask;

    // internos
    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;
    float pitch = 0f; // rotación vertical de la cámara

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraPivot == null)
            Debug.LogWarning("Camera Pivot no asignado en PlayerController.");

        // Bloquear cursor al comenzar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandlePickup();
        // Opcional: liberar cursor con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Yaw en el jugador
        transform.Rotate(Vector3.up * mouseX);

        // Pitch en la cámara (clamp)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (cameraPivot != null)
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void HandleMovement()
    {
        // Ground check (usamos CharacterController.isGrounded para simplicidad)
        isGrounded = controller.isGrounded;

        // Input de movimiento
        float x = Input.GetAxis("Horizontal"); // A/D, izquierda/derecha
        float z = Input.GetAxis("Vertical");   // W/S, adelante/atrás

        // Velocidad objetivo (correr con LeftShift)
        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = running ? runSpeed : walkSpeed;

        // Movimiento relativo a la orientación del jugador
        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 horizontalVelocity = move.normalized * speed;

        // Aplicar movimiento horizontal
        controller.Move(horizontalVelocity * Time.deltaTime);

        // Saltar & gravedad
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // pequeño empuje para mantener al personaje pegado al suelo

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // v = sqrt(2 * g * h) -> gravedad positiva en fórmula
            float jumpVel = Mathf.Sqrt(Mathf.Abs(2f * gravity) * jumpHeight);
            velocity.y = jumpVel;
        }

        // aplicar gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandlePickup()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask))
            {
                // Si el objeto tiene un componente IPickup ejecutable, usarlo; sino, desactivar objeto
                var pickup = hit.collider.GetComponent<PickupItem>();
                if (pickup != null)
                {
                    pickup.OnPickup();
                }
                else
                {
                    // comportamiento por defecto: desactivar el objeto
                    hit.collider.gameObject.SetActive(false);
                }
            }
        }
    }

    // Opcional: dibuja el raycast en la escena para debug
    void OnDrawGizmosSelected()
    {
        if (cameraPivot != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(cameraPivot.position, cameraPivot.position + cameraPivot.forward * pickupRange);
        }
    }
}
