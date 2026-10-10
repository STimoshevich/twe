using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace TwilightGrove
{
    [InitializeOnLoad]
    public static class GroveSetup
    {
        const string ScenePath = "Assets/Scenes/TwilightGrove.unity";
        static GroveSetup() { EditorApplication.delayCall += FirstOpen; }
        static void FirstOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
            Create();
        }
        [MenuItem("Twilight Grove/Create or open prototype scene")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory("Assets/Scenes");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorSceneManager.OpenScene(ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.companyName = "Prototype";
            PlayerSettings.productName = "Twilight Grove";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.prototype.twilightgrove");
            Debug.Log("Twilight Grove ready. Press Play: the world is generated at runtime.");
        }
    }
}
