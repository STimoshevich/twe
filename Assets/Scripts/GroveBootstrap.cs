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
            RenderSettings.ambientLight = new Color(.48f, .51f, .62f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.12f, .16f, .24f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 32;
            RenderSettings.fogEndDistance = 65;
            var sun = new GameObject("Evening sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, .83f, .64f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, -35, 0);

            var grass = Mat(new Color(.20f, .31f, .26f));
            var stone = Mat(new Color(.37f, .39f, .46f));
            var path = Mat(new Color(.46f, .39f, .30f));
            var wood = Mat(new Color(.29f, .20f, .18f));
            var leaves = Mat(new Color(.12f, .27f, .24f));
            Piece("Ground", PrimitiveType.Cube, new Vector3(0, -.5f, 0), new Vector3(46, 1, 46), grass);
            Piece("Path", PrimitiveType.Cube, new Vector3(0, .012f, 0), new Vector3(3, .02f, 44), path, false);
            Piece("Crossing", PrimitiveType.Cube, new Vector3(0, .013f, 4), new Vector3(43, .02f, 3), path, false);
            Piece("Northern trail", PrimitiveType.Cube, new Vector3(0, .014f, 16), new Vector3(36, .02f, 2), path, false);
            Piece("Eastern trail", PrimitiveType.Cube, new Vector3(15, .015f, 3), new Vector3(2, .02f, 28), path, false);
            // The boundary walls keep the controller on the small playable island.
            Piece("North boundary", PrimitiveType.Cube, new Vector3(0, 1, 23), new Vector3(47, 2, 1), stone);
            Piece("South boundary", PrimitiveType.Cube, new Vector3(0, 1, -23), new Vector3(47, 2, 1), stone);
            Piece("East boundary", PrimitiveType.Cube, new Vector3(23, 1, 0), new Vector3(1, 2, 47), stone);
            Piece("West boundary", PrimitiveType.Cube, new Vector3(-23, 1, 0), new Vector3(1, 2, 47), stone);
            Piece("Northern ruin wall", PrimitiveType.Cube, new Vector3(-14, 1, 19), new Vector3(8, 2, .6f), stone);
            Piece("Northern ruin corner", PrimitiveType.Cube, new Vector3(-18, 1, 17), new Vector3(.6f, 2, 4), stone);
            Piece("Eastern ledge", PrimitiveType.Cube, new Vector3(18, .12f, 16), new Vector3(4, .24f, 5), stone);
            Piece("Ruin wall", PrimitiveType.Cube, new Vector3(-6, 1, 6), new Vector3(6, 2, .6f), stone);
            Piece("Ruin corner", PrimitiveType.Cube, new Vector3(-9, 1, 3.5f), new Vector3(.6f, 2, 5), stone);
            Piece("Ruin pillar", PrimitiveType.Cube, new Vector3(-3, 1.5f, 3), new Vector3(.8f, 3, .8f), stone);
            Piece("Low step", PrimitiveType.Cube, new Vector3(5, .12f, 5), new Vector3(3, .24f, 3), stone);
            Piece("Shrine", PrimitiveType.Cylinder, new Vector3(5, .8f, 5), new Vector3(.8f, .65f, .8f), stone);
            var crystal = Piece("Crystal", PrimitiveType.Sphere, new Vector3(5, 1.8f, 5), Vector3.one * .55f,
                Mat(new Color(.35f, .8f, 1f)), false);
            var glow = crystal.AddComponent<Light>();
            glow.color = Color.cyan; glow.range = 5; glow.intensity = 2;
            Vector2[] trees = { new Vector2(-11,-10), new Vector2(-7,-7), new Vector2(-10,-2),
                new Vector2(7,-9), new Vector2(11,-5), new Vector2(10,10), new Vector2(-6,11), new Vector2(4,11),
                new Vector2(-19,-18), new Vector2(-14,-11), new Vector2(-20,-5), new Vector2(19,-16),
                new Vector2(18,-5), new Vector2(20,8), new Vector2(10,20), new Vector2(-5,20), new Vector2(-20,12) };
            foreach (var t in trees)
            {
                Piece("Tree trunk", PrimitiveType.Cylinder, new Vector3(t.x, 1, t.y), new Vector3(.7f, 1, .7f), wood);
                Piece("Tree canopy", PrimitiveType.Sphere, new Vector3(t.x, 3, t.y), new Vector3(2.7f, 2.7f, 2.7f), leaves, false);
            }
            for (int i = 0; i < 5; i++)
                Piece("Rock", PrimitiveType.Sphere, new Vector3(4 + i * 1.6f, .45f, -2 - i * .65f),
                    new Vector3(1.2f, .9f, 1), stone);

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
            gameObject.AddComponent<GroveExploration>().Initialize(player.transform, glow);
            gameObject.AddComponent<GroveQuest>().Initialize(player.transform, crystal.transform, glow);
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
