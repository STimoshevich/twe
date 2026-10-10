using UnityEngine;
namespace TwilightGrove
{
    // Tracks one finger by ID; other fingers cannot steal an active drag.
    public class GroveJoystick : MonoBehaviour
    {
        public Vector2 Value { get; private set; }
        int finger = -1;
        bool mouse;
        Vector2 center;
        float radius;
        Texture2D circle;
        void Awake()
        {
            circle = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                circle.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(31.5f - distance)));
            }
            circle.Apply();
        }
        void Update()
        {
            var safe = Screen.safeArea;
            radius = Mathf.Clamp(Mathf.Min(safe.width, safe.height) * .15f, 45, 120);
            center = new Vector2(safe.xMin + radius * 1.45f, safe.yMin + radius * 1.45f);
            if (Input.touchCount > 0)
            {
                mouse = false;
                bool found = false;
                foreach (var touch in Input.touches)
                {
                    if (finger == -1 && touch.phase == TouchPhase.Began &&
                        Vector2.Distance(touch.position, center) <= radius * 1.3f) finger = touch.fingerId;
                    if (touch.fingerId != finger) continue;
                    found = true;
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    { finger = -1; Value = Vector2.zero; }
                    else Set(touch.position);
                    break;
                }
                if (!found) { finger = -1; Value = Vector2.zero; }
            }
            else
            {
                finger = -1;
                Vector2 position = Input.mousePosition;
                if (Input.GetMouseButtonDown(0) && Vector2.Distance(position, center) <= radius * 1.3f) mouse = true;
                if (!Input.GetMouseButton(0)) mouse = false;
                if (mouse) Set(position); else Value = Vector2.zero;
            }
        }
        void Set(Vector2 position)
        {
            var raw = Vector2.ClampMagnitude((position - center) / radius, 1);
            const float deadZone = .12f;
            Value = raw.magnitude <= deadZone ? Vector2.zero : raw.normalized *
                ((raw.magnitude - deadZone) / (1 - deadZone));
        }
        void OnApplicationFocus(bool focus) { if (!focus) ResetInput(); }
        void OnApplicationPause(bool pause) { if (pause) ResetInput(); }
        void ResetInput() { finger = -1; mouse = false; Value = Vector2.zero; }
        void OnGUI()
        {
            if (circle == null) return;
            DrawCircle(center, radius, new Color(.12f, .17f, .25f, .65f));
            DrawCircle(center + Value * radius * .7f, radius * .36f, new Color(.55f, .8f, 1, .9f));
            GUI.color = Color.white;
            var style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 35f, 14, 26)) };
            var safe = Screen.safeArea;
            GUI.Label(new Rect(safe.xMin + 18, Screen.height - safe.yMax + 15, safe.width - 36, 80),
                "СТАРЫЙ ЛЕС\nWASD / стрелки / джойстик · Исследуй лес и руины", style);
        }
        void DrawCircle(Vector2 position, float r, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(position.x - r, Screen.height - position.y - r, r * 2, r * 2), circle);
        }
        void OnDestroy() { if (circle != null) Destroy(circle); }
    }
}
