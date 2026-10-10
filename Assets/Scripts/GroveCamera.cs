using UnityEngine;
namespace TwilightGrove
{
    public class GroveCamera : MonoBehaviour
    {
        public Transform target;
        readonly Vector3 offset = new Vector3(12, 16, -12);
        bool ready;
        void LateUpdate()
        {
            var desired = target.position + offset;
            transform.position = ready ? Vector3.Lerp(transform.position, desired,
                1 - Mathf.Exp(-8 * Time.deltaTime)) : desired;
            transform.rotation = Quaternion.LookRotation(-offset);
            ready = true;
        }
    }
}
