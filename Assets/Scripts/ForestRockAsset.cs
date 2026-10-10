using System.Collections.Generic;
using UnityEngine;

namespace TwilightGrove
{
    public static class ForestRockAsset
    {
        static readonly Mesh[] Meshes=new Mesh[5];
        static Material material;
        public static GameObject Create(Transform parent,Vector3 position,float scale,int variant)
        {
            variant=((variant%5)+5)%5;
            if(material==null)
            {
                material=GroveBootstrap.Mat(Color.white);material.name="Original forest stone and moss";
                material.mainTexture=Resources.Load<Texture2D>("Forest/ForestStoneV1");
                material.SetTexture("_PathTex",Resources.Load<Texture2D>("Forest/ForestMossV1"));
                material.SetFloat("_TerrainBlend",1);material.SetFloat("_VertexTint",1);
                if(material.mainTexture==null || material.GetTexture("_PathTex")==null) Debug.LogError("Forest stone/moss Texture2D missing.");
            }
            if(Meshes[variant]==null) Build(variant);
            var rock=new GameObject("Original mossy forest boulder "+(variant+1));rock.transform.SetParent(parent,false);
            rock.transform.position=position;rock.transform.localScale=Vector3.one*scale;
            rock.transform.rotation=Quaternion.Euler(0,variant*53+position.x*13+position.z*7,0);
            rock.AddComponent<MeshFilter>().sharedMesh=Meshes[variant];rock.AddComponent<MeshRenderer>().sharedMaterial=material;
            rock.AddComponent<MeshCollider>().sharedMesh=Meshes[variant];return rock;
        }
        static void Build(int variant)
        {
            const int rings=8,sides=14;var v=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var t=new List<int>();
            for(int j=0;j<=rings;j++) for(int i=0;i<=sides;i++)
            {
                float lat=j*Mathf.PI/rings,a=i*Mathf.PI*2/sides;
                Vector3 d=new Vector3(Mathf.Sin(lat)*Mathf.Cos(a),Mathf.Cos(lat),Mathf.Sin(lat)*Mathf.Sin(a));
                float irregular=1+.14f*Mathf.Sin(a*3+variant)*Mathf.Sin(lat)+.08f*Mathf.Cos(lat*4+variant);
                Vector3 p=Vector3.Scale(d,new Vector3(.72f+variant*.025f,.43f,.55f))*irregular;
                p.y=Mathf.Max(p.y,-.40f);v.Add(p);uv.Add(new Vector2((float)i/sides*1.5f,(float)j/rings));
                float patch=Mathf.PerlinNoise(p.x*4+variant*7+20,p.z*4+12);
                float moss=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.15f,.8f,d.y))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.25f,.65f,patch));
                colors.Add(new Color(.94f,.94f,.94f,moss));
                if(j<rings && i<sides){int n=j*(sides+1)+i;t.AddRange(new[]{n,n+1,n+sides+1,n+1,n+sides+2,n+sides+1});}
            }
            var mesh=new Mesh{name="Original irregular forest rock "+variant};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();Meshes[variant]=mesh;
        }
    }
}
