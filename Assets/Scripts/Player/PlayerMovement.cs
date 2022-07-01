using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {

    // Motion fields
    //public float minSpeed = 0f;
    public float walkSpeed = 10f;
    public float runSpeed = 20f;
    public float moveAcceleration = 2f;
    public float friction = 0.05f;

    private float _speed;
    private Vector2 _moveInput2D = Vector2.zero;
    private Vector3 _moveInput3D = Vector3.zero;
    private Vector3 _moveVector;
    private bool _run = false;

    // Look fields
    [Range(1f, 10f)]
    public float lookSensX = 5f, lookSensY = 5f;
    public bool lookInv = false;

    private bool _look = false;
    // gloabl Vector2 scaledLookSens?
    private Vector2 _lookDelta;
    private Vector2 _lookVector;

    public void OnMove2D(InputAction.CallbackContext context) {

        _moveInput2D = context.ReadValue<Vector2>();
    }

    public void OnMove3D(InputAction.CallbackContext context) {

        _moveInput3D = context.ReadValue<Vector3>();
    }

    public void OnAccelerate(InputAction.CallbackContext context) {

        _run = !context.canceled;
    }

    public void OnJump(InputAction.CallbackContext context) {


    }

    public void OnLook(InputAction.CallbackContext context) {

        _lookDelta = context.ReadValue<Vector2>();
    }

    public void OnLookState(InputAction.CallbackContext context) {

        _look = !_look;

        Cursor.lockState = (_look) ? CursorLockMode.Locked : CursorLockMode.None;

        // Move cursor back to original position when it becomes visible?

        //if (look) {
        //    mouseVector = Mouse.current.position.ReadValue();
        //    Cursor.lockState = CursorLockMode.Locked;

        //}
        //else {
        //    Cursor.lockState = CursorLockMode.None;
        //    Mouse.current.WarpCursorPosition(mouseVector);
        //}
    }

    private void Update() {

        // Move look if 'look' is enabled
        if (_look && _lookDelta.sqrMagnitude > 0.01)
            PlayerLook(_lookDelta);

        //if (_moveInput2D.sqrMagnitude > 0.01f || _speed != 0f)
        //    PlayerMove2D(_moveInput2D);

        if (_moveInput3D.sqrMagnitude > 0.01f || _speed != 0f)
            PlayerMove3D(_moveInput3D);
    }

    
    void PlayerMove2D(Vector2 targetVector) {

        // Determine target speed
        var targetSpeed = (_run) ? runSpeed : walkSpeed;
        targetSpeed = (targetVector.sqrMagnitude > 0.01f) ? targetSpeed : 0;

        // Accelerate/Deccelerate speed to target speed and adjust for time
        _speed = Accelerate(_speed, targetSpeed, moveAcceleration * Time.deltaTime);
        float scaledSpeed = _speed * Time.deltaTime;

        Debug.Log("Target: " + targetSpeed);
        Debug.Log("Speed: " + _speed);

        // Adds momentum and friction to movement vector
        _moveVector = Vector3.Lerp(_moveVector, new Vector3(targetVector.x, 0, targetVector.y), friction);

        var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * _moveVector;
        transform.position += move * scaledSpeed;
    }

    void PlayerMove3D(Vector3 targetVector) {
        // Needs work...

        // Determine target speed
        var targetSpeed = (_run) ? runSpeed : walkSpeed;
        targetSpeed = (targetVector.sqrMagnitude > 0.01f) ? targetSpeed : 0;

        // Accelerate/Deccelerate speed to target speed and adjust for time
        _speed = Accelerate(_speed, targetSpeed, moveAcceleration * Time.deltaTime);
        float scaledSpeed = _speed * Time.deltaTime;

        //Debug.Log("3D: " + _speed);

        // Adds momentum and friction to movement vector
        _moveVector = Vector3.Lerp(_moveVector, targetVector, friction);

        // Limit rotation to Y-axis
        var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * _moveVector;
        transform.position += move * scaledSpeed;
    }

    void PlayerLook(Vector2 delta) {

        // global variable?
        var scaledLookSens = LookSensitivity(lookSensX, lookSensY) * Time.deltaTime;
        _lookVector.y += delta.x * scaledLookSens.x;
        _lookVector.y %= 360; // Makes sure we don't get a crazy big number if rotating around in one direction
        delta.y *= (lookInv) ? -1 : 1; // Invert look option
        _lookVector.x = Mathf.Clamp(_lookVector.x - delta.y * scaledLookSens.y, -89, 89);
        transform.localEulerAngles = _lookVector;
    }

    Vector2 LookSensitivity(float x, float y) {
        var xmin = 2;
        var xmax = 20;
        var ymin = 5;
        var ymax = 10;

        return new Vector2(
            ScaleToRange(x, 1, 10, xmin, xmax),
            ScaleToRange(y, 1, 10, ymin, ymax));
    }

    // Scales float value from one scale to another
    float ScaleToRange(float v, float min, float max, float smin, float smax) {
        return smin + (v - min) / (max - min) * (smax - smin);
    }

    float Accelerate(float speed, float targetSpeed, float acceleration) {
        if (speed < targetSpeed - acceleration)
            speed += acceleration;
        else if (speed > targetSpeed + acceleration)
            speed -= acceleration;
        else
            speed = targetSpeed;

        return speed;
    }

    //private void OnDrawGizmosSelected() {

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(Vector3Int.FloorToInt(transform.position), 10f);
    //}
}
