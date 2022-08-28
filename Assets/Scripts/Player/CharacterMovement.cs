using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour {

    #region Fields

    // Components
    private Camera m_camera;
    private Rigidbody m_rb;
    private CapsuleCollider m_collider;

    // Motion fields
    public float walkSpeed = 10f;
    public float runSpeed = 20f;
    public float jumpHeight = 0.5f;

    private float m_horSmoothTime = 0.3f;
    private float m_vertSmoothTime = 0.3f;
    private float m_jumpFromHeight = 0f;
    private float m_jumpVelocity = 2f;
    private Vector3 m_charAcceleration = Vector3.zero;
    private Vector3 m_moveInput = Vector3.zero;
    public bool m_creative = false;
    private bool m_run = false;
    private bool m_jump = false;
    private bool m_jumpHeld = false;
    private bool m_crouch = false;

    // Look fields
    [Range(1f, 10f)]
    public float lookSensX = 5f, lookSensY = 5f;
    public bool lookInv = false;

    private bool m_look = false;
    private Vector2 m_mouseDelta;
    private Vector2 m_lookRotation;

    #endregion // Fields

    #region Methods

    // Built-In Methods

    private void Start() {

        m_camera = GetComponentInChildren<Camera>();
        m_rb = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
    }

    private void Update() {

        if (m_rb.useGravity == m_creative)
            m_rb.useGravity = !m_creative;
    }

    private void FixedUpdate() {

        CharacterMotion();
    }

    private void OnDrawGizmos() {

        if (Application.isPlaying) {

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.forward * 5 + transform.position);
        }
    }

    // Unity Event methods

    public void OnMove(InputAction.CallbackContext context) {

        m_moveInput = context.ReadValue<Vector3>();
    }

    public void OnRun(InputAction.CallbackContext context) {

        m_run = context.started;

        //if (context.canceled)
        //    m_run = !m_run;

        //m_run = !context.canceled;
    }

    public void OnJump(InputAction.CallbackContext context) {

        m_jumpFromHeight = transform.position.y;

        if(context.performed)
            m_jumpHeld = true;

        m_jump = context.canceled;

        // want to make character jump a certain height if jump is hit and released immediately
        // crouch and fill jump bar if key is held
    }

    public void OnCrouch(InputAction.CallbackContext context) {

        m_crouch = !context.canceled;
    }

    public void OnLook(InputAction.CallbackContext context) {

        m_mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnLookState() {

        m_look = !m_look;
        //Cursor.SetCursor();
        Cursor.lockState = (m_look) ? CursorLockMode.Locked : CursorLockMode.None;
    }

    // Private methods

    void CharacterMotion() {

        if (m_look && m_mouseDelta.sqrMagnitude > 0.01)
            CharacterLook();

        if (m_moveInput.sqrMagnitude > 0.001f || m_rb.velocity.sqrMagnitude > 0f)
            CharacterMove();

        if (!m_creative)
            CharacterJump();

        CharacterCrouch();
        
    }

    void CharacterMove() {

        Vector3 currentVelocity = m_rb.velocity;
        var horVel = new Vector2(currentVelocity.x, currentVelocity.z);
        var vertVel = currentVelocity.y;

        var horAccel = new Vector2(m_charAcceleration.x, m_charAcceleration.z);
        var vertAccel = m_charAcceleration.y;

        // Determine horizontal velocity
        var targetSpeed = (m_run) ? runSpeed : walkSpeed;
        targetSpeed = (m_moveInput.sqrMagnitude > 0.01f) ? targetSpeed : 0;

        var rotation = Quaternion.AngleAxis(transform.localEulerAngles.y, Vector3.up) * m_moveInput;
        horVel = Vector2.SmoothDamp(horVel, targetSpeed * new Vector2(rotation.x, rotation.z), ref horAccel, m_horSmoothTime);

        // Fly
        if (m_creative)
            vertVel = Mathf.SmoothDamp(vertVel, targetSpeed * m_moveInput.y, ref vertAccel, m_vertSmoothTime);
                
        m_charAcceleration = new Vector3(horAccel.x, vertAccel, horAccel.y);
        m_rb.velocity = new Vector3(horVel.x, vertVel, horVel.y);
    }

    void CharacterLook() {

        // global variable?
        var scaledLookSens = LookSensitivity(lookSensX, lookSensY) * Time.deltaTime;
        m_lookRotation.y += m_mouseDelta.x * scaledLookSens.x;
        m_lookRotation.y %= 360; // Makes sure we don't get a crazy big number if rotating around in one direction

        m_mouseDelta.y *= (lookInv) ? -1 : 1; // Invert look option
        m_lookRotation.x = Mathf.Clamp(m_lookRotation.x - m_mouseDelta.y * scaledLookSens.y, -89, 89);

        // Camera's vertical rotation
        var angles = m_camera.transform.localEulerAngles;
        angles.x = m_lookRotation.x;
        m_camera.transform.localEulerAngles = angles;

        // Character's horizontal rotation
        var rotation = transform.localEulerAngles;
        rotation.y = m_lookRotation.y;
        transform.localEulerAngles = rotation;
    }

    void CharacterJump() {

        //if (m_jumpHeld)
        //    m_crouch = true;

        if (m_jump && m_rb.velocity.y == 0) {

            m_jumpVelocity = (m_jumpHeld) ? 7 : 6;
            m_rb.velocity += Vector3.up * m_jumpVelocity;
            m_jump = false;
            m_jumpHeld = false;
            //m_crouch = false;
        }
    }

    void CharacterCrouch() {

        Vector3 pos = m_camera.transform.localPosition;

        if (m_crouch || m_jumpHeld) {
            pos.y = -1f;
            m_collider.height = 5f;
            m_collider.center = -Vector3.up * 2.5f;
        }
        else {
            pos.y = 0f;
            m_collider.height = 6f;
            m_collider.center = -Vector3.up * 2f;
        }

        m_camera.transform.localPosition = pos;
    }

    Vector2 LookSensitivity(float x, float y) {
        var xmin = 2;
        var xmax = 20;
        var ymin = 5;
        var ymax = 10;

        return new Vector2(
            Functions.ScaleToRange(x, 1, 10, xmin, xmax),
            Functions.ScaleToRange(y, 1, 10, ymin, ymax));
    }

    #endregion // Methods
}
