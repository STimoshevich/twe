using UnityEngine;

namespace TwilightGrove
{
    public class GroveQuest : MonoBehaviour
    {
        const string SaveKey = "TwilightGrove.quest.crystals.v1.local-hero-001";
        const int All = 31;
        readonly Vector3[] locations = {
            new Vector3(-6, .9f, 3), new Vector3(15, .9f, -10),
            new Vector3(-16, .9f, -15), new Vector3(13, .9f, 16), new Vector3(-15, .9f, 16) };
        readonly GameObject[] crystals = new GameObject[5];
        Transform player, shrine;
        Light shrineLight;
        int collected;
        bool delivered, confirmReset;
        string message;
        float messageUntil;
        int nearby = -1;
        bool nearShrine;

        public void Initialize(Transform target, Transform altarCrystal, Light light)
        {
            player = target; shrine = altarCrystal; shrineLight = light;
            int state = PlayerPrefs.GetInt(SaveKey, 0);
            collected = state & All;
            delivered = (state & 32) != 0 && collected == All;
            var mat = GroveBootstrap.Mat(new Color(.7f, .4f, 1));
            for (int i = 0; i < locations.Length; i++)
                crystals[i] = GroveBootstrap.Piece("Collectible crystal " + (i + 1), PrimitiveType.Cube,
                    locations[i], new Vector3(.4f, .65f, .4f), mat, false);
            UpdateReward();
            RefreshTargets();
        }
        int Count
        {
            get { int count = 0; for (int i = 0; i < 5; i++) if ((collected & (1 << i)) != 0) count++; return count; }
        }
        void Update()
        {
            if (player == null) return;
            RefreshTargets();
            if (Input.GetKeyDown(KeyCode.E)) Interact();
            for (int i = 0; i < crystals.Length; i++)
            {
                if (!crystals[i].activeSelf) continue;
                crystals[i].transform.position = locations[i] + Vector3.up * Mathf.Sin(Time.time * 2 + i) * .12f;
                crystals[i].transform.rotation = Quaternion.Euler(15, Time.time * 55 + i * 30, 15);
            }
        }
        void RefreshTargets()
        {
            nearby = -1;
            float nearest = 1.8f;
            var origin = player.position + Vector3.up * .9f;
            for (int i = 0; i < crystals.Length; i++)
            {
                bool available = (collected & (1 << i)) == 0;
                float distance = Vector2.Distance(new Vector2(player.position.x, player.position.z),
                    new Vector2(locations[i].x, locations[i].z));
                // A collectible is current activity, not permanent geographic information.
                crystals[i].SetActive(available && distance < 5.8f);
                if (available && distance < nearest && !Physics.Linecast(origin, locations[i]))
                { nearby = i; nearest = distance; }
            }
            nearShrine = Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(5,5)) < 2.3f;
        }
        void Interact()
        {
            if (nearby >= 0)
            {
                collected |= 1 << nearby;
                Say("Кристалл найден! " + Count + "/5");
                Save(); RefreshTargets();
            }
            else if (nearShrine)
            {
                if (delivered) Say("Алтарь уже восстановлен. Можно начать новый сбор.");
                else if (collected != All) Say("Для алтаря нужны все 5 кристаллов. Собрано: " + Count);
                else { delivered = true; Save(); UpdateReward(); Say("Алтарь восстановлен!"); }
            }
        }
        void UpdateReward()
        {
            shrine.localScale = Vector3.one * (delivered ? .9f : .55f);
            shrine.GetComponent<Renderer>().material.color = delivered ? new Color(1,.8f,.25f) : new Color(.35f,.8f,1);
            shrineLight.color = delivered ? new Color(1,.8f,.25f) : Color.cyan;
        }
        void Say(string text) { message = text; messageUntil = Time.unscaledTime + 4; }
        void Save()
        {
            PlayerPrefs.SetInt(SaveKey, collected | (delivered ? 32 : 0));
            PlayerPrefs.Save();
        }
        void OnGUI()
        {
            if (player == null) return;
            var safe = Screen.safeArea;
            float top = Screen.height - safe.yMax;
            var style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(Mathf.Clamp(safe.height / 38, 14, 24)), wordWrap = true };
            GUI.color = Color.white;
            float width = Mathf.Min(400, safe.width * .53f);
            GUI.Label(new Rect(safe.xMin + 18, top + 88, width, 85), delivered ?
                "Алтарь восстановлен!\nКристаллы: 5/5" : "Восстановить алтарь: " + Count + "/5\nНайди фиолетовые кристаллы и вернись к алтарю.", style);
            string action = nearby >= 0 ? "Собрать" : nearShrine ? "Алтарь" : "Подойди к кристаллу";
            var buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = style.fontSize, wordWrap = true };
            var actionRect = new Rect(safe.xMax - 180, Screen.height - safe.yMin - 95, 164, 72);
            GUI.enabled = nearby >= 0 || nearShrine;
            if (GUI.Button(actionRect, action + " [E]", buttonStyle)) Interact();
            GUI.enabled = true;
            if (Time.unscaledTime < messageUntil)
                GUI.Label(new Rect(safe.xMin + 18, top + 178, width, 90), message, style);
            if (delivered || confirmReset)
            {
                if (GUI.Button(new Rect(safe.xMin + 18, top + 275, Mathf.Min(width, 260), 44),
                    confirmReset ? "Подтвердить новый сбор" : "Начать новый сбор", buttonStyle))
                {
                    if (!confirmReset) confirmReset = true;
                    else
                    {
                        collected = 0; delivered = false; confirmReset = false;
                        Save(); UpdateReward(); RefreshTargets(); Say("Новый сбор начат. Исследование карты сохранено.");
                    }
                }
            }
        }
    }
}
