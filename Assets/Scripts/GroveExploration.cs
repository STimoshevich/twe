using System;
using UnityEngine;

namespace TwilightGrove
{
    // One local character for now. Change characterId to give another character its own map.
    public class GroveExploration : MonoBehaviour
    {
        public Transform player;
        public Light shrineLight;
        public string characterId = "local-hero-001";
        public float sightRadius = 12f;
        public float sightFeather = 4f;
        const int Cells = 96;
        const float Min = -24f, Size = 48f;
        byte[] explored = new byte[Cells * Cells];
        readonly float[] softCoverage = new float[Cells * Cells];
        static readonly int[] BlurWeights = { 1, 4, 6, 4, 1 };
        Color32[] maskPixels = new Color32[Cells * Cells];
        Color32[] mapPixels = new Color32[Cells * Cells];
        Texture2D mask, map;
        bool dirty;
        float nextUpdate, nextSave;
        string SaveKey => "TwilightGrove.exploration.v2." + characterId;

        public void Initialize(Transform target, Light light)
        {
            player = target;
            shrineLight = light;
            mask = new Texture2D(Cells, Cells, TextureFormat.RGBA32, false);
            map = new Texture2D(Cells, Cells, TextureFormat.RGBA32, false);
            mask.filterMode = FilterMode.Bilinear;
            map.filterMode = FilterMode.Point;
            mask.wrapMode = map.wrapMode = TextureWrapMode.Clamp;
            Load();
            Shader.SetGlobalTexture("_GroveExplored", mask);
            Shader.SetGlobalVector("_GroveBounds", new Vector4(Min, Min, Size, Size));
            Shader.SetGlobalFloat("_GroveFogReady", 1);
            Refresh();
        }
        void Load()
        {
            var saved = PlayerPrefs.GetString(SaveKey, "");
            bool migrate = saved.Length == 0;
            if (migrate) saved = PlayerPrefs.GetString("TwilightGrove.exploration.v1." + characterId, "");
            if (saved.Length == 0) return;
            try
            {
                var bytes = Convert.FromBase64String(saved);
                if (!migrate && bytes.Length == explored.Length) explored = bytes;
                else if (migrate && bytes.Length == 64 * 64)
                {
                    // Both versions use half-metre cells; the old map sits centrally in the new one.
                    for (int z = 0; z < 64; z++) for (int x = 0; x < 64; x++)
                        explored[(z + 16) * Cells + x + 16] = bytes[z * 64 + x];
                    dirty = true;
                }
                else Debug.LogWarning("Exploration save has a different map size; starting a new map.");
            }
            catch (FormatException) { Debug.LogWarning("Exploration save is unreadable; starting a new map."); }
        }
        void LateUpdate()
        {
            if (player == null || mask == null) return;
            Shader.SetGlobalVector("_GroveViewer", new Vector4(player.position.x, player.position.z, sightRadius, Mathf.Clamp(sightFeather, .1f, sightRadius)));
            if (shrineLight != null)
                shrineLight.enabled = Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(5,5)) < sightRadius;
            if (Time.unscaledTime >= nextUpdate) { Refresh(); nextUpdate = Time.unscaledTime + .1f; }
            if (dirty && Time.unscaledTime >= nextSave) { Save(); nextSave = Time.unscaledTime + 2; }
        }
        void Refresh()
        {
            var position = new Vector2(player.position.x, player.position.z);
            for (int z = 0; z < Cells; z++) for (int x = 0; x < Cells; x++)
            {
                int i = z * Cells + x;
                var point = new Vector2(Min + (x + .5f) * Size / Cells, Min + (z + .5f) * Size / Cells);
                float distance = Vector2.Distance(point, position);
                bool visible = distance <= sightRadius;
                if (visible && explored[i] == 0) { explored[i] = 1; dirty = true; }
                byte known = explored[i] == 0 ? (byte)0 : (byte)255;
                maskPixels[i] = new Color32(known, known, known, 255);
                Color terrain = TerrainColor(point);
                mapPixels[i] = explored[i] == 0 ? new Color(.018f,.025f,.04f) : terrain * Mathf.Lerp(.45f, 1f, 1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(sightRadius - Mathf.Clamp(sightFeather, .1f, sightRadius), sightRadius, distance)));
                mapPixels[i].a = 255;
            }
            SoftenExploredEdge();
            mask.SetPixels32(maskPixels); mask.Apply(false);
            map.SetPixels32(mapPixels); map.Apply(false);
        }
        // Blur the display mask only. Persistent cells and event visibility stay unchanged.
        void SoftenExploredEdge()
        {
            for(int z=0;z<Cells;z++) for(int x=0;x<Cells;x++)
            {
                float sum=0;
                for(int k=-2;k<=2;k++)
                    sum+=(explored[z*Cells+Mathf.Clamp(x+k,0,Cells-1)] != 0 ? 1f : 0f)*BlurWeights[k+2];
                softCoverage[z*Cells+x]=sum/16f;
            }
            for(int z=0;z<Cells;z++) for(int x=0;x<Cells;x++)
            {
                float sum=0;
                for(int k=-2;k<=2;k++) sum+=softCoverage[Mathf.Clamp(z+k,0,Cells-1)*Cells+x]*BlurWeights[k+2];
                byte value=(byte)Mathf.RoundToInt(sum*255f/16f);
                maskPixels[z*Cells+x]=new Color32(value,value,value,255);
            }
        }
        static Color TerrainColor(Vector2 p)
        {
            if (Mathf.Abs(p.x) > 22.5f || Mathf.Abs(p.y) > 22.5f) return new Color(.5f,.52f,.6f);
            if (p.x > 3.5f && p.x < 6.5f && p.y > 3.5f && p.y < 6.5f) return Color.cyan;
            if (ForestFortAsset.IsWallOnMap(p)) return new Color(.65f,.65f,.7f);
            return OldForestLandscape.MapColor(p);
        }
        void Save()
        {
            if (!dirty) return;
            PlayerPrefs.SetString(SaveKey, Convert.ToBase64String(explored));
            PlayerPrefs.Save(); dirty = false;
        }
        void OnApplicationPause(bool pause) { if (pause) Save(); }
        void OnApplicationFocus(bool focus) { if (!focus) Save(); }
        void OnApplicationQuit() { Save(); }
        void OnDisable() { Save(); }
        void OnGUI()
        {
            if (map == null || player == null) return;
            var safe = Screen.safeArea;
            float side = Mathf.Clamp(safe.height * .27f, 100, 200);
            var rect = new Rect(safe.xMax - side - 16, Screen.height - safe.yMax + 16, side, side);
            GUI.color = Color.white;
            GUI.DrawTexture(rect, map);
            float px = rect.x + Mathf.Clamp01((player.position.x - Min) / Size) * side;
            float py = rect.yMax - Mathf.Clamp01((player.position.z - Min) / Size) * side;
            GUI.color = Color.yellow;
            GUI.DrawTexture(new Rect(px - 3, py - 3, 6, 6), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x, rect.yMax + 3, side, 25), "N ↑  Explored map");
        }
        void OnDestroy()
        {
            Shader.SetGlobalFloat("_GroveFogReady", 0);
            if (mask != null) Destroy(mask);
            if (map != null) Destroy(map);
        }
    }
}
