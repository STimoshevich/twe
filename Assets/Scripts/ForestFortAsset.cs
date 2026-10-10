using System.Collections.Generic;
using UnityEngine;

namespace TwilightGrove
{
    public static class ForestFortAsset
    {
        static Material stone;
        static readonly Dictionary<string,Mesh> Cached=new Dictionary<string,Mesh>();

        public static GameObject Create(Transform parent,Vector3 position)
        {
            var fort=new GameObject("Original ruined forest fort");fort.transform.SetParent(parent,false);fort.transform.position=position;
            Wall(fort.transform,"Broken northern curtain",new Vector3(0,0,3),0,6,2.5f,3);
            Wall(fort.transform,"Western defensive wall",new Vector3(-3,0,.5f),90,5,2.1f,7);
            Wall(fort.transform,"Eastern wall remnant",new Vector3(3,0,2.3f),90,1.5f,1.2f,12);
            Wall(fort.transform,"Entrance left pier",new Vector3(-2.2f,0,-2),0,.85f,2.35f,17);
            Wall(fort.transform,"Entrance right pier",new Vector3(2.2f,0,-2),0,.85f,1.55f,19);
            // Small rubble stays outside the clear entry and crystal position.
            Rubble(fort.transform,new Vector3(-2.5f,.1f,2.15f),new Vector3(.55f,.22f,.36f),24);
            Rubble(fort.transform,new Vector3(.65f,.1f,2.35f),new Vector3(.64f,.20f,.38f),27);
            Rubble(fort.transform,new Vector3(2.2f,.1f,1.5f),new Vector3(.48f,.20f,.35f),29);
            return fort;
        }
        public static GameObject Wall(Transform parent,string name,Vector3 position,float yaw,float length,float height,int seed)
        {
            string key=seed+":"+length+":"+height;
            if(!Cached.TryGetValue(key,out var mesh)){mesh=BuildWall(length,height,seed);Cached.Add(key,mesh);}
            return Place(parent,name,position,yaw,mesh,true);
        }
        static Material Material()
        {
            if(stone!=null)return stone;
            stone=GroveBootstrap.Mat(new Color(.89f,.86f,.78f));stone.name="Weathered fort stone and moss";
            stone.mainTexture=Resources.Load<Texture2D>("Forest/ForestStoneV1");
            stone.SetTexture("_PathTex",Resources.Load<Texture2D>("Forest/ForestMossV1"));
            stone.SetFloat("_VertexTint",1);stone.SetFloat("_TerrainBlend",1);
            if(stone.mainTexture==null)Debug.LogError("Fort stone texture is missing.");return stone;
        }
        static GameObject Place(Transform parent,string name,Vector3 position,float yaw,Mesh mesh,bool solid)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localRotation=Quaternion.Euler(0,yaw,0);
            g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=Material();
            if(solid)g.AddComponent<MeshCollider>().sharedMesh=mesh;return g;
        }
        static void Rubble(Transform parent,Vector3 position,Vector3 size,int seed)
        {
            var b=new Builder();b.Block(Vector3.zero,size,seed,.86f);
            Place(parent,"Fallen masonry",position,seed*13,b.Finish("Rubble "+seed),false);
        }
        static Mesh BuildWall(float length,float height,int seed)
        {
            var b=new Builder();const float course=.42f;int rows=Mathf.CeilToInt(height/course);
            for(int row=0;row<rows;row++)
            {
                int count=Mathf.Max(1,Mathf.CeilToInt(length/.95f));float width=length/count;
                float offset=row%2==0?0:width*.45f;
                for(int column=-1;column<count;column++)
                {
                    float left=Mathf.Max(-length*.5f,-length*.5f+column*width+offset);
                    float right=Mathf.Min(length*.5f,-length*.5f+(column+1)*width+offset);
                    if(right-left<.12f)continue;
                    float x=(left+right)*.5f;
                    // Rows break off at different heights instead of ending in one flat line.
                    float silhouette=height*(.68f+.23f*Mathf.Sin(x*1.7f+seed)+.09f*Mathf.Cos(x*3+seed));
                    if(row*course>silhouette && row>0)continue;
                    float tone=.81f+.14f*Mathf.PerlinNoise(column+seed*.31f,row*.7f);
                    b.Block(new Vector3(x,row*course+course*.5f,0),new Vector3(right-left-.035f,course-.025f,.7f),seed+row*11+column,tone);
                }
            }
            return b.Finish("Original ruined masonry "+seed);
        }
        public static bool IsWallOnMap(Vector2 p)
        {
            return (p.x>-9.4f && p.x<-2.6f && Mathf.Abs(p.y-6)<.42f) ||
                (Mathf.Abs(p.x+9)<.42f && p.y>1 && p.y<6) ||
                (Mathf.Abs(p.x+3)<.42f && p.y>4.5f && p.y<6.1f) ||
                (Mathf.Abs(p.y-1)<.42f && (Mathf.Abs(p.x+8.2f)<.48f || Mathf.Abs(p.x+3.8f)<.48f)) ||
                (Mathf.Abs(p.y-19)<.42f && p.x>-18 && p.x<-10) ||
                (Mathf.Abs(p.x+18)<.42f && p.y>15 && p.y<19);
        }
        class Builder
        {
            readonly List<Vector3> vertices=new List<Vector3>();readonly List<int> triangles=new List<int>();
            readonly List<Vector2> uv=new List<Vector2>();readonly List<Color> colors=new List<Color>();
            public void Block(Vector3 center,Vector3 size,int seed,float tone)
            {
                Vector3 h=size*.5f;
                var p=new[]{new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z)};
                for(int i=0;i<8;i++)p[i]+=center+new Vector3(Mathf.Sin(seed+i*1.4f),Mathf.Cos(seed+i*.8f),Mathf.Sin(seed+i*2.3f))*.014f;
                Face(p,0,3,2,1,tone,.08f);Face(p,4,5,6,7,tone,.12f);
                Face(p,0,4,7,3,tone,.18f);Face(p,1,2,6,5,tone,.1f);
                Face(p,3,7,6,2,tone,.45f+.4f*(.5f+.5f*Mathf.Sin(seed)));Face(p,0,1,5,4,tone,0);
            }
            void Face(Vector3[] p,int a,int b,int c,int d,float tone,float moss)
            {
                int n=vertices.Count;vertices.AddRange(new[]{p[a],p[b],p[c],p[d]});uv.AddRange(new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right});
                for(int i=0;i<4;i++)colors.Add(new Color(tone,tone,tone,moss));triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            public Mesh Finish(string name)
            {
                var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            }
        }
    }
}
