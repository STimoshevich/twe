using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TwilightGrove
{
    public static class ForestFortBaker
    {
        const string Folder="Assets/Art/OriginalForest/RuinedFortV1";
        [MenuItem("Twilight Grove/Create ruined fort prefab")]
        public static void Bake()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogWarning("Stop Play Mode before saving the fort prefab.");return;}
            string path=Folder+"/ForestFort.prefab";
            if(File.Exists(path)){Debug.Log("Fort prefab already exists; keeping your edits: "+path);return;}
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            var g=ForestFortAsset.Create(null,Vector3.zero);
            try
            {
                var meshes=new Dictionary<Mesh,Mesh>();var materials=new Dictionary<Material,Material>();
                foreach(var filter in g.GetComponentsInChildren<MeshFilter>())
                {
                    var source=filter.sharedMesh;
                    if(!meshes.TryGetValue(source,out var saved))
                    {
                        saved=Object.Instantiate(source);
                        AssetDatabase.CreateAsset(saved,AssetDatabase.GenerateUniqueAssetPath(Folder+"/"+source.name+".asset"));
                        meshes.Add(source,saved);
                    }
                    filter.sharedMesh=saved;
                }
                foreach(var col in g.GetComponentsInChildren<MeshCollider>())
                    if(meshes.TryGetValue(col.sharedMesh,out var saved))col.sharedMesh=saved;
                foreach(var renderer in g.GetComponentsInChildren<MeshRenderer>())
                {
                    var source=renderer.sharedMaterial;
                    if(!materials.TryGetValue(source,out var saved))
                    {
                        saved=Object.Instantiate(source);
                        AssetDatabase.CreateAsset(saved,AssetDatabase.GenerateUniqueAssetPath(Folder+"/"+source.name+".mat"));
                        materials.Add(source,saved);
                    }
                    renderer.sharedMaterial=saved;
                }
                PrefabUtility.SaveAsPrefabAsset(g,path);AssetDatabase.SaveAssets();
                Debug.Log("Ruined fort prefab saved: "+path);
            }
            finally {Object.DestroyImmediate(g);}
        }
    }
}
