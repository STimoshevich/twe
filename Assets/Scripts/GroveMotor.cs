using UnityEngine;

namespace TwilightGrove
{
    [RequireComponent(typeof(CharacterController))]
    public class GroveMotor : MonoBehaviour
    {
        public Transform view;
        public GroveJoystick joystick;
        public float speed = 4.2f;
        public float Moving { get; private set; }
        CharacterController controller;
        float vertical;
        void Awake() { controller = GetComponent<CharacterController>(); }
        void Update()
        {
            var input = new Vector2(
                (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) -
                (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) -
                (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
            if (joystick != null && joystick.Value.sqrMagnitude > input.sqrMagnitude) input = joystick.Value;
            input = Vector2.ClampMagnitude(input, 1);
            var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(view.right, Vector3.up).normalized;
            var direction = forward * input.y + right * input.x;
            if (controller.isGrounded && vertical < 0) vertical = -2;
            vertical = Mathf.Max(vertical - 22 * Time.deltaTime, -40);
            var before = transform.position;
            controller.Move((direction * speed + Vector3.up * vertical) * Time.deltaTime);
            var delta = Vector3.ProjectOnPlane(transform.position - before, Vector3.up);
            Moving = Mathf.Clamp01(delta.magnitude / Mathf.Max(speed * Time.deltaTime, .0001f));
            if (direction.sqrMagnitude > .001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
                    1 - Mathf.Exp(-14 * Time.deltaTime));
        }
    }
}
