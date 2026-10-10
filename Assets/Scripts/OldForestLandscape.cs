using System.Collections.Generic;
using UnityEngine;

namespace TwilightGrove
{
    // Shared landscape queries keep the mesh and minimap in the same coordinates.
    public static class OldForestLandscape
    {
        static Transform root;
        static Material bark, leaf, leafLight, rock, moss, timber;
        static readonly Vector2[] Objectives = {
            new Vector2(-6,3), new Vector2(15,-10), new Vector2(-16,-15),
            new Vector2(13,16), new Vector2(-15,16), new Vector2(5,5), new Vector2(0,-9) };

        public static float Height(float x, float z)
        {
            return Hill(x,z,-15,9,4,1.5f) + Hill(x,z,10,-17,4.5f,1.1f);
        }
        static float Hill(float x,float z,float cx,float cz,float radius,float height)
        {
            float t = Mathf.Clamp01(1 - Vector2.Distance(new Vector2(x,z),new Vector2(cx,cz))/radius);
            return height*t*t*(3-2*t);
        }
        public static float PathWeight(float x,float z)
        {
            float noise=(Mathf.PerlinNoise(x*.65f+16,z*.65f+24)-.5f)*.32f;
            float primary=Mathf.Abs(x-.16f*Mathf.Sin(z*.4f))-1.35f;
            float crossing=Mathf.Max(Mathf.Abs(z-4-.12f*Mathf.Sin(x*.5f))-1.25f,Mathf.Abs(x)-21.5f);
            float northern=Mathf.Max(Mathf.Abs(z-16)-.8f,Mathf.Abs(x)-18);
            float eastern=Mathf.Max(Mathf.Abs(x-15)-.8f,Mathf.Abs(z-3)-14);
            float distance=Mathf.Min(Mathf.Min(primary,crossing),Mathf.Min(northern,eastern))+noise;
            return 1f-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.15f,.65f,distance));
        }
        public static Color MapColor(Vector2 p)
        {
            if(Mathf.Abs(p.x)>22.4f || Mathf.Abs(p.y)>22.4f) return new Color(.33f,.37f,.30f);
            var grass=Color.Lerp(new Color(.26f,.38f,.22f),new Color(.36f,.42f,.25f),Height(p.x,p.y)/1.5f);
            return Color.Lerp(grass,new Color(.49f,.40f,.25f),PathWeight(p.x,p.y));
        }
        public static void Build()
        {
            root = new GameObject("Old Forest - landscape").transform;
            bark=GroveBootstrap.Mat(new Color(.24f,.16f,.10f));
            leaf=GroveBootstrap.Mat(new Color(.12f,.28f,.17f));
            leafLight=GroveBootstrap.Mat(new Color(.24f,.38f,.18f));
            rock=GroveBootstrap.Mat(new Color(.40f,.43f,.37f));
            moss=GroveBootstrap.Mat(new Color(.31f,.42f,.19f));
            timber=GroveBootstrap.Mat(new Color(.46f,.30f,.15f));
            Ground();
            // Invisible containment remains behind the rocks, inside the explored-map bounds.
            Border(new Vector3(0,2,23),new Vector3(47,6,1));
            Border(new Vector3(0,2,-23),new Vector3(47,6,1));
            Border(new Vector3(23,2,0),new Vector3(1,6,47));
            Border(new Vector3(-23,2,0),new Vector3(1,6,47));
            var random=new System.Random(17041);
            for(int z=-20;z<=20;z+=4) for(int x=-20;x<=20;x+=4)
            {
                float px=x+(float)random.NextDouble()*1.6f-.8f;
                float pz=z+(float)random.NextDouble()*1.6f-.8f;
                if(!ClearForTree(px,pz)) continue;
                Tree(px,pz,.85f+(float)random.NextDouble()*.4f);
            }
            for(int i=-21;i<=21;i+=3)
            {
                Boulder(i,22,1.6f); Boulder(i,-22,1.6f);
                Boulder(22,i,1.6f); Boulder(-22,i,1.6f);
            }
            // Southern rest stop: low objects keep the approach and spawn readable.
            Part("Trail marker",PrimitiveType.Cylinder,-2.8f,-10,.8f,new Vector3(.16f,.8f,.16f),timber);
            Part("Old wooden sign",PrimitiveType.Cube,-2.8f,-10,1.5f,new Vector3(1.15f,.36f,.16f),timber);
            Part("Resting bench",PrimitiveType.Cube,3.4f,-12,.42f,new Vector3(2.4f,.28f,.65f),timber);
            Part("Bench support",PrimitiveType.Cube,2.6f,-12,.18f,new Vector3(.25f,.36f,.5f),bark);
            Part("Bench support",PrimitiveType.Cube,4.2f,-12,.18f,new Vector3(.25f,.36f,.5f),bark);
            for(int i=0;i<8;i++) {
                float a=i*Mathf.PI/4;
                Part("Cold fire ring",PrimitiveType.Sphere,4+Mathf.Cos(a)*.65f,-15+Mathf.Sin(a)*.65f,.14f,new Vector3(.35f,.28f,.3f),rock);
            }
            var log=Part("Fallen trunk",PrimitiveType.Cylinder,-13,-8,.36f,new Vector3(.55f,1.8f,.55f),bark);
            log.transform.rotation=Quaternion.Euler(0,25,90);
            // A climbable grassy mound with a small landmark, rather than another floor.
            Boulder(-15,9,.8f);
            Part("Hill standing stone",PrimitiveType.Cube,-16.2f,9,1.05f,new Vector3(.6f,2.1f,.55f),rock);
            // Broken entrance: two posts, no overhead beam hiding the hero.
            Part("Forest gate left",PrimitiveType.Cube,-2.6f,-18,1.25f,new Vector3(.7f,2.5f,.75f),rock);
            Part("Forest gate right",PrimitiveType.Cube,2.6f,-18,.95f,new Vector3(.7f,1.9f,.75f),rock);
            ForestGrass.Build(root);
        }
        static bool ClearForTree(float x,float z)
        {
            if(Mathf.Abs(x)<3 || Mathf.Abs(z-4)<3 || Mathf.Abs(z-16)<2.5f || (Mathf.Abs(x-15)<2.6f && z>-13)) return false;
            if(x>-11 && x<-.5f && z>0 && z<8) return false;
            if(x>-20 && x<-9 && z>14) return false;
            if(x>1 && x<6 && z<-10 && z>-18) return false;
            if(Vector2.Distance(new Vector2(x,z),new Vector2(-15,9))<4.5f) return false;
            foreach(var p in Objectives) if(Vector2.Distance(p,new Vector2(x,z))<3) return false;
            return true;
        }
        static void Tree(float x,float z,float s)
        {
            int variant=Mathf.RoundToInt(x*7+z*11);
            var position=new Vector3(x,Height(x,z),z);
            if(Mathf.Abs(variant)%3==0) ForestSpruceAsset.Create(root,position,s,variant);
            else ForestOakAsset.Create(root,position,s,variant);
        }
        static void Boulder(float x,float z,float s)
        {
            ForestRockAsset.Create(root,new Vector3(x,Height(x,z)+.42f*s,z),s,
                Mathf.RoundToInt(x*13+z*7));
        }
        static GameObject Part(string name,PrimitiveType shape,float x,float z,float y,Vector3 scale,Material mat,bool solid=true)
        {
            var p=GroveBootstrap.Piece(name,shape,new Vector3(x,Height(x,z)+y,z),scale,mat,solid);
            p.transform.SetParent(root,true); return p;
        }
        static void Border(Vector3 p,Vector3 size)
        {
            var g=new GameObject("Forest boundary");g.transform.SetParent(root);g.transform.position=p;
            g.AddComponent<BoxCollider>().size=size;
        }
        static void Ground()
        {
            const int n=92;
            var v=new Vector3[(n+1)*(n+1)];
            var uv=new Vector2[v.Length];
            var tris=new List<int>();
            var colors=new Color[v.Length];
            for(int z=0;z<=n;z++) for(int x=0;x<=n;x++) {
                float wx=-23+x*.5f,wz=-23+z*.5f;
                int k=z*(n+1)+x;v[k]=new Vector3(wx,Height(wx,wz),wz);uv[k]=new Vector2(wx/3.5f,wz/3.5f);
                float tone=.88f+.12f*Mathf.PerlinNoise(wx*.18f+8,wz*.18f+8);
                colors[k]=new Color(tone,tone,tone,PathWeight(wx,wz));
            }
            for(int z=0;z<n;z++) for(int x=0;x<n;x++) {

                int a=z*(n+1)+x,b=a+n+1;
                tris.AddRange(new[]{a,b,a+1,a+1,b,b+1});
            }
            var mesh=new Mesh {name="Old Forest walkable terrain",vertices=v,uv=uv,colors=colors};
            mesh.SetTriangles(tris,0);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var ground=new GameObject("Sculpted forest floor");ground.transform.SetParent(root);
            ground.AddComponent<MeshFilter>().sharedMesh=mesh;
            var grass=Resources.Load<Texture2D>("Forest/GrassGroundV1");
            var path=Resources.Load<Texture2D>("Forest/ForestPathV1");
            var material=GroveBootstrap.Mat(Color.white);
            material.name="Old Forest - grass and blended trails";
            material.SetFloat("_TerrainBlend",1);material.SetFloat("_VertexTint",1);
            if(grass != null) material.mainTexture=grass;
            if(path != null) material.SetTexture("_PathTex",path);
            if(grass == null || path == null) Debug.LogError("Forest ground textures missing: check Resources/Forest and Texture Shape 2D.");
            ground.AddComponent<MeshRenderer>().sharedMaterial=material;
            ground.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
    }
}
