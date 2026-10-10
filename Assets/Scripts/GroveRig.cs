using UnityEngine;
namespace TwilightGrove
{
    public class GroveRig : MonoBehaviour
    {
        Transform body, leftLeg, rightLeg, leftArm, rightArm;
        GroveMotor motor;
        float phase;
        public void Build()
        {
            motor = GetComponent<GroveMotor>();
            body = new GameObject("Visual rig").transform;
            body.SetParent(transform, false);
            var coat = GroveBootstrap.Mat(new Color(.32f, .47f, .8f));
            var skin = GroveBootstrap.Mat(new Color(.91f, .73f, .55f));
            var boots = GroveBootstrap.Mat(new Color(.18f, .19f, .24f));
            Part("Torso", PrimitiveType.Capsule, body, new Vector3(0, 1.1f, 0), new Vector3(.55f, .4f, .4f), coat);
            Part("Head", PrimitiveType.Sphere, body, new Vector3(0, 1.65f, 0), Vector3.one * .4f, skin);
            Part("Face marker", PrimitiveType.Cube, body, new Vector3(0, 1.65f, .19f), new Vector3(.22f, .07f, .05f), boots);
            leftLeg = Limb("Left leg", new Vector3(-.16f, .75f, 0), new Vector3(.19f, .65f, .23f), boots);
            rightLeg = Limb("Right leg", new Vector3(.16f, .75f, 0), new Vector3(.19f, .65f, .23f), boots);
            leftArm = Limb("Left arm", new Vector3(-.37f, 1.35f, 0), new Vector3(.17f, .55f, .17f), coat);
            rightArm = Limb("Right arm", new Vector3(.37f, 1.35f, 0), new Vector3(.17f, .55f, .17f), coat);
        }
        Transform Limb(string label, Vector3 pivot, Vector3 size, Material mat)
        {
            var joint = new GameObject(label + " joint").transform;
            joint.SetParent(body, false); joint.localPosition = pivot;
            Part(label, PrimitiveType.Cube, joint, new Vector3(0, -size.y / 2, 0), size, mat);
            return joint;
        }
        static void Part(string label, PrimitiveType shape, Transform parent, Vector3 local, Vector3 scale, Material mat)
        {
            var part = GroveBootstrap.Piece(label, shape, Vector3.zero, scale, mat, false);
            part.transform.SetParent(parent, false); part.transform.localPosition = local;
        }
        void LateUpdate()
        {
            var moving = motor.Moving;
            phase += Time.deltaTime * 11 * moving;
            float swing = Mathf.Sin(phase) * 32 * moving;
            leftLeg.localRotation = Quaternion.Euler(swing, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);
            leftArm.localRotation = Quaternion.Euler(-swing * .7f, 0, 0);
            rightArm.localRotation = Quaternion.Euler(swing * .7f, 0, 0);
            body.localPosition = Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * .045f * moving +
                Mathf.Sin(Time.time * 2) * .01f * (1 - moving));
        }
    }
}
