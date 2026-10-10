using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwilightGrove
{
    public class GroveBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (SceneManager.GetActiveScene().name != "TwilightGrove") return;
            new GameObject("Grove Prototype").AddComponent<GroveBootstrap>();
        }

        void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            RenderSettings.ambientLight = new Color(.48f, .53f, .45f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.20f, .27f, .24f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 32;
            RenderSettings.fogEndDistance = 65;
            var sun = new GameObject("Evening sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, .83f, .64f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, -35, 0);

            OldForestLandscape.Build();
            var stone = Mat(new Color(.37f, .39f, .34f));
            ForestFortAsset.Wall(null, "Northern ruin wall", new Vector3(-14, 0, 19), 0, 8, 2, 31);
            ForestFortAsset.Wall(null, "Northern ruin corner", new Vector3(-18, 0, 17), 90, 4, 1.6f, 37);
            Piece("Eastern ledge", PrimitiveType.Cube, new Vector3(18, .12f, 16), new Vector3(4, .24f, 5), stone);
            ForestFortAsset.Create(null, new Vector3(-6, 0, 3));
            Piece("Low step", PrimitiveType.Cube, new Vector3(5, .12f, 5), new Vector3(3, .24f, 3), stone);
            Piece("Shrine", PrimitiveType.Cylinder, new Vector3(5, .8f, 5), new Vector3(.8f, .65f, .8f), stone);
            var crystal = Piece("Crystal", PrimitiveType.Sphere, new Vector3(5, 1.8f, 5), Vector3.one * .55f,
                Mat(new Color(.35f, .8f, 1f)), false);
            var glow = crystal.AddComponent<Light>();
            glow.color = Color.cyan; glow.range = 5; glow.intensity = 2;
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, .1f, -9);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f; controller.radius = .32f;
            controller.center = new Vector3(0, .9f, 0);
            controller.stepOffset = .3f; controller.slopeLimit = 45;
            var motor = player.AddComponent<GroveMotor>();
            var rig = player.AddComponent<GroveRig>();
            rig.Build();
            var camera = new GameObject("Isometric camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true; camera.orthographicSize = 8;
            camera.nearClipPlane = .1f; camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.gameObject.AddComponent<AudioListener>();
            var follow = camera.gameObject.AddComponent<GroveCamera>();
            follow.target = player.transform;
            motor.view = camera.transform;
            motor.joystick = gameObject.AddComponent<GroveJoystick>();
            var exploration = gameObject.AddComponent<GroveExploration>();
            exploration.Initialize(player.transform, glow);
            gameObject.AddComponent<GroveQuest>().Initialize(player.transform, crystal.transform, glow, exploration.sightRadius);
        }

        public static Material Mat(Color color)
        {
            var shader = Resources.Load<Shader>("GroveFog");
            var material = new Material(shader != null ? shader : Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Glossiness", .12f);
            return material;
        }

        public static GameObject Piece(string label, PrimitiveType shape, Vector3 position,
            Vector3 scale, Material material, bool solid = true)
        {
            var obj = GameObject.CreatePrimitive(shape);
            obj.name = label;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) { var col = obj.GetComponent<Collider>(); col.enabled = false; Destroy(col); }
            return obj;
        }
    }
}
