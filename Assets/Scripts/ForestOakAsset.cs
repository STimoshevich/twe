using System.Collections.Generic;
using UnityEngine;

namespace TwilightGrove
{
    // Original mesh asset: curved tubes and irregular clusters, no primitive tree parts.
    public static class ForestOakAsset
    {
        static readonly Mesh[] BarkMeshes = new Mesh[3], LeafMeshes = new Mesh[3];
        static Material barkMaterial, leafMaterial;

        public static GameObject Create(Transform parent, Vector3 position, float scale, int variant)
        {
            variant = ((variant % 3) + 3) % 3;
            if (barkMaterial == null)
            {
                var barkTexture=Resources.Load<Texture2D>("Forest/OakBarkV1");
                var leavesTexture=Resources.Load<Texture2D>("Forest/OakLeavesV1");
                if(barkTexture == null || leavesTexture == null)
                    Debug.LogError("Forest oak textures failed to load as Texture2D. Check OakBarkV1/OakLeavesV1: Texture Shape must be 2D, then reimport and restart Play.");
                barkMaterial=GroveBootstrap.Mat(barkTexture != null ? Color.white : new Color(.46f,.29f,.13f));
                leafMaterial=GroveBootstrap.Mat(leavesTexture != null ? Color.white : new Color(.33f,.49f,.15f));
                barkMaterial.name="Original oak - painted bark";
                leafMaterial.name="Original oak - painted leaves";
                if(barkTexture != null) barkMaterial.mainTexture=barkTexture;
                if(leavesTexture != null) leafMaterial.mainTexture=leavesTexture;
                barkMaterial.SetFloat("_VertexTint",1);
                leafMaterial.SetFloat("_VertexTint",1);
                leafMaterial.mainTextureScale=new Vector2(1.8f,1.8f);
            }
            if(BarkMeshes[variant]==null) Build(variant);
            var tree=new GameObject("Forest Oak - original variant " + (variant+1));
            tree.transform.SetParent(parent,false);tree.transform.position=position;
            tree.transform.localScale=Vector3.one*scale;
            tree.transform.rotation=Quaternion.Euler(0,variant*71+position.x*17+position.z*9,0);
            MeshPart(tree.transform,"Curved trunk, roots and branches",BarkMeshes[variant],barkMaterial);
            MeshPart(tree.transform,"Layered leaf clusters",LeafMeshes[variant],leafMaterial);
            var collider=tree.AddComponent<CapsuleCollider>();
            collider.radius=.30f;collider.height=2.5f;collider.center=new Vector3(0,1.25f,0);
            return tree;
        }
        static void MeshPart(Transform parent,string name,Mesh mesh,Material material)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);
            g.AddComponent<MeshFilter>().sharedMesh=mesh;
            g.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        static void Build(int variant)
        {
            var wood=new Builder();var foliage=new Builder();
            float lean=(variant-1)*.12f;
            wood.Tube(new[]{new Vector3(0,0,0),new Vector3(-.1f,.7f,.06f),new Vector3(.16f+lean,1.5f,0),new Vector3(lean,2.4f,.1f),new Vector3(.2f,3,.05f)},new[]{.44f,.29f,.24f,.16f,.08f});
            for(int i=0;i<5;i++)
            {
                float a=i*Mathf.PI*2/5+variant*.5f;
                Vector3 dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                wood.Tube(new[]{dir*.08f+Vector3.up*.45f,dir*.42f+Vector3.up*.12f,dir*.95f+Vector3.up*.015f},new[]{.18f,.12f,.018f});
            }
            for(int i=0;i<5;i++)
            {
                float a=i*Mathf.PI*2/5+variant*.43f;
                Vector3 d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                float y=1.6f+i*.17f;
                wood.Tube(new[]{new Vector3(.08f,y,0),d*.65f+Vector3.up*(y+.25f),d*1.1f+Vector3.up*(y+.8f)},new[]{.16f,.10f,.025f});
                foliage.Cluster(d*.85f+Vector3.up*(y+.85f),new Vector3(.90f,.64f,.85f),i+variant*7);
            }
            foliage.Cluster(new Vector3(.08f,3.4f,0),new Vector3(1.02f,.78f,1.0f),23+variant);
            BarkMeshes[variant]=wood.Finish("Forest Oak bark " +variant);
            LeafMeshes[variant]=foliage.Finish("Forest Oak foliage " +variant);
        }
        internal class Builder
        {
            readonly List<Vector3> vertices=new List<Vector3>();
            readonly List<int> triangles=new List<int>();
            readonly List<Color> colors=new List<Color>();
            readonly List<Vector2> uv=new List<Vector2>();
            void Vertex(Vector3 p,Color c,Vector2 tex){vertices.Add(p);colors.Add(c);uv.Add(tex);}
            public void Tube(Vector3[] centers,float[] radii)
            {
                const int sides=9;int start=vertices.Count;float length=0;
                for(int j=0;j<centers.Length;j++)
                {
                    if(j>0) length+=Vector3.Distance(centers[j-1],centers[j]);
                    Vector3 direction=(centers[Mathf.Min(j+1,centers.Length-1)]-centers[Mathf.Max(j-1,0)]).normalized;
                    Vector3 right=Vector3.Cross(direction,Vector3.forward).normalized;
                    if(right.sqrMagnitude<.1f)right=Vector3.right;
                    Vector3 forward=Vector3.Cross(right,direction).normalized;
                    for(int i=0;i<=sides;i++)
                    {
                        float a=i*Mathf.PI*2/sides;
                        float groove=1+.07f*Mathf.Sin(i*2.3f+j);
                        Vector3 p=centers[j]+(right*Mathf.Cos(a)+forward*Mathf.Sin(a))*radii[j]*groove;
                        float light=.62f+.24f*(.5f+.5f*Mathf.Cos(a))+.09f*Mathf.Sin(i*2.1f);
                        Vertex(p,new Color(light,light*.96f,light*.86f),new Vector2((float)i/sides,length/1.5f));
                        if(j>0 && i<sides)
                        {
                            int n=start+j*(sides+1)+i,previous=n-sides-1;
                            triangles.AddRange(new[]{previous,n,previous+1,previous+1,n,n+1});
                        }
                    }
                }
                // Caps avoid visible holes at cut branch ends and the foot.
                for(int i=1;i<sides-1;i++)
                {
                    triangles.AddRange(new[]{start,start+i+1,start+i});
                    int end=start+(centers.Length-1)*(sides+1);
                    triangles.AddRange(new[]{end,end+i,end+i+1});
                }
            }
            public void Cluster(Vector3 center,Vector3 size,int seed)
            {
                const int rings=7,sides=12;int start=vertices.Count;
                for(int j=0;j<=rings;j++)for(int i=0;i<=sides;i++)
                {
                    float lat=j*Mathf.PI/rings,a=i*Mathf.PI*2/sides;
                    Vector3 d=new Vector3(Mathf.Sin(lat)*Mathf.Cos(a),Mathf.Cos(lat),Mathf.Sin(lat)*Mathf.Sin(a));
                    float shape=1+.09f*Mathf.Sin(a*3+seed)*Mathf.Sin(lat)+.05f*Mathf.Cos(lat*5+seed);
                    float paint=.65f+.30f*(d.y*.5f+.5f)+.06f*Mathf.Sin(a*4+lat*3+seed);
                    Vertex(center+Vector3.Scale(d,size)*shape,new Color(paint,paint,paint*.88f),new Vector2((float)i/sides,(float)j/rings));
                    if(j<rings && i<sides)
                    {
                        int n=start+j*(sides+1)+i;
                        triangles.AddRange(new[]{n,n+1,n+sides+1,n+1,n+sides+2,n+sides+1});
                    }
                }
            }
            public Mesh Finish(string name)
            {
                var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            }
        }
    }
}
