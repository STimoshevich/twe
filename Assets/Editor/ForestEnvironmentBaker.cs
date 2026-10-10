using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TwilightGrove
{
    public static class ForestEnvironmentBaker
    {
        [MenuItem("Twilight Grove/Create spruce and mossy rock prefabs")]
        public static void Bake()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogWarning("Stop Play Mode before baking forest assets.");return;}
            SaveFamily("Assets/Art/OriginalForest/SpruceV1","ForestSpruce",3,i=>ForestSpruceAsset.Create(null,Vector3.zero,1,i));
            SaveFamily("Assets/Art/OriginalForest/RocksV1","ForestRock",5,i=>ForestRockAsset.Create(null,Vector3.zero,1,i));
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("Original spruce and mossy rock prefabs ready in Assets/Art/OriginalForest.");
        }
        static void SaveFamily(string folder,string name,int count,Func<int,GameObject> factory)
        {
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var meshes=new Dictionary<Mesh,Mesh>();var materials=new Dictionary<Material,Material>();
            for(int i=0;i<count;i++)
            {
                string path=folder+"/"+name+(i+1)+".prefab";if(File.Exists(path))continue;
                var g=factory(i);g.transform.rotation=Quaternion.identity;
                foreach(var filter in g.GetComponentsInChildren<MeshFilter>())
                {
                    var source=filter.sharedMesh;
                    if(!meshes.TryGetValue(source,out var saved)){saved=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(saved,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+source.name+".asset"));meshes.Add(source,saved);}
                    filter.sharedMesh=saved;
                }
                foreach(var col in g.GetComponentsInChildren<MeshCollider>())if(meshes.TryGetValue(col.sharedMesh,out var saved))col.sharedMesh=saved;
                foreach(var renderer in g.GetComponentsInChildren<MeshRenderer>())
                {
                    var source=renderer.sharedMaterial;
                    if(!materials.TryGetValue(source,out var saved)){saved=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(saved,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+source.name+".mat"));materials.Add(source,saved);}
                    renderer.sharedMaterial=saved;
                }
                PrefabUtility.SaveAsPrefabAsset(g,path);UnityEngine.Object.DestroyImmediate(g);
            }
        }
    }
}
