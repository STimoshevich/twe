using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TwilightGrove
{
    public static class ForestOakBaker
    {
        [MenuItem("Twilight Grove/Create original forest oak prefabs")]
        public static void Bake()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogWarning("Stop Play Mode before baking oak prefabs.");return; }
            const string folder="Assets/Art/OriginalForest/TexturedV1";
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var meshes=new Dictionary<Mesh,Mesh>();var materials=new Dictionary<Material,Material>();
            for(int i=0;i<3;i++)
            {
                string prefabPath=folder+"/ForestOak"+(i+1)+".prefab";
                if(File.Exists(prefabPath)) continue; // Existing edited assets are never overwritten.
                var tree=ForestOakAsset.Create(null,Vector3.zero,1,i);
                tree.transform.rotation=Quaternion.identity;
                foreach(var filter in tree.GetComponentsInChildren<MeshFilter>())
                {
                    var source=filter.sharedMesh;
                    if(!meshes.TryGetValue(source,out var saved))
                    {
                        saved=Object.Instantiate(source);
                        string path=AssetDatabase.GenerateUniqueAssetPath(folder+"/"+source.name+".asset");
                        AssetDatabase.CreateAsset(saved,path);meshes.Add(source,saved);
                    }
                    filter.sharedMesh=saved;
                }
                foreach(var renderer in tree.GetComponentsInChildren<MeshRenderer>())
                {
                    var source=renderer.sharedMaterial;
                    if(!materials.TryGetValue(source,out var saved))
                    {
                        saved=Object.Instantiate(source);
                        string path=AssetDatabase.GenerateUniqueAssetPath(folder+"/OakMaterial.mat");
                        AssetDatabase.CreateAsset(saved,path);materials.Add(source,saved);
                    }
                    renderer.sharedMaterial=saved;
                }
                PrefabUtility.SaveAsPrefabAsset(tree,prefabPath);
                Object.DestroyImmediate(tree);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Selection.activeObject=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/ForestOak1.prefab");
            Debug.Log("Original oak prefabs ready in " + folder);
        }
    }
}
