using System.Collections.Generic;
using UnityEngine;

namespace TwilightGrove
{
    public static class ForestGrass
    {
        public static void Build(Transform parent)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();
            var random=new System.Random(1907);
            for(int i=0;i<650;i++)
            {
                float x=(float)random.NextDouble()*41-20.5f,z=(float)random.NextDouble()*41-20.5f;
                if(OldForestLandscape.PathWeight(x,z)>.18f) continue;
                if(x>-10 && x<-2 && z>1 && z<7) continue;
                if(x>1 && x<6 && z>-17 && z<-10) continue;
                if(Vector2.Distance(new Vector2(x,z),new Vector2(5,5))<2.2f) continue;
                for(int blade=0;blade<4;blade++)
                {
                    float a=(float)random.NextDouble()*Mathf.PI*2;
                    float h=.18f+(float)random.NextDouble()*.25f,w=.035f+(float)random.NextDouble()*.025f;
                    Vector3 basePoint=new Vector3(x+(float)random.NextDouble()*.35f-.175f,0,z+(float)random.NextDouble()*.35f-.175f);
                    basePoint.y=OldForestLandscape.Height(basePoint.x,basePoint.z)+.012f;
                    Vector3 side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*w;
                    Vector3 bend=new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a))*.12f;
                    int n=vertices.Count;
                    vertices.AddRange(new[]{basePoint-side,basePoint+side,basePoint+Vector3.up*h*.6f+bend*.4f-side*.55f,basePoint+Vector3.up*h*.6f+bend*.4f+side*.55f,basePoint+Vector3.up*h+bend});
                    colors.AddRange(new[]{new Color(.58f,.67f,.42f),new Color(.58f,.67f,.42f),new Color(.88f,.95f,.7f),new Color(.88f,.95f,.7f),new Color(1,1,.83f)});
                    // Both sides visible without changing the terrain shader's culling.
                    triangles.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3,n+2,n+4,n+3,n+1,n+2,n,n+3,n+2,n+1,n+3,n+4,n+2});
                }
            }
            var mesh=new Mesh{name="Original forest grass tufts"};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var patch=new GameObject("Low grass - combined mesh");patch.transform.SetParent(parent,false);
            patch.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=patch.AddComponent<MeshRenderer>();
            var material=GroveBootstrap.Mat(new Color(.36f,.47f,.20f));material.SetFloat("_VertexTint",1);
            renderer.sharedMaterial=material;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
