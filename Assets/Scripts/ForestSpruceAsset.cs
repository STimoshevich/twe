using System.Collections.Generic;
using UnityEngine;

namespace TwilightGrove
{
    public static class ForestSpruceAsset
    {
        static readonly Mesh[] Trunks=new Mesh[3],Crowns=new Mesh[3];
        static Material bark,needles;
        public static GameObject Create(Transform parent,Vector3 position,float scale,int variant)
        {
            variant=((variant%3)+3)%3;
            if(bark==null)
            {
                bark=GroveBootstrap.Mat(new Color(.78f,.70f,.63f));
                bark.mainTexture=Resources.Load<Texture2D>("Forest/OakBarkV1");bark.SetFloat("_VertexTint",1);
                needles=GroveBootstrap.Mat(Color.white);
                needles.mainTexture=Resources.Load<Texture2D>("Forest/SpruceNeedlesV1");needles.SetFloat("_VertexTint",1);
                if(needles.mainTexture==null) Debug.LogError("Spruce needle Texture2D missing.");
            }
            if(Trunks[variant]==null) Build(variant);
            var tree=new GameObject("Original forest spruce "+(variant+1));tree.transform.SetParent(parent,false);
            tree.transform.position=position;tree.transform.localScale=Vector3.one*scale;
            tree.transform.rotation=Quaternion.Euler(0,variant*47+position.x*11+position.z*9,0);
            Part(tree.transform,"Trunk and drooping branches",Trunks[variant],bark);
            Part(tree.transform,"Layered needle crown",Crowns[variant],needles);
            var collider=tree.AddComponent<CapsuleCollider>();collider.radius=.23f;collider.height=3;collider.center=Vector3.up*1.5f;
            return tree;
        }
        static void Part(Transform parent,string name,Mesh mesh,Material material)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);
            g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        static void Build(int variant)
        {
            var wood=new ForestOakAsset.Builder();
            wood.Tube(new[]{Vector3.zero,new Vector3(.05f,1.6f,0),new Vector3(-.08f,3.3f,.04f),new Vector3(.06f,4.7f,.06f)},new[]{.29f,.20f,.12f,.025f});
            var v=new List<Vector3>();var uv=new List<Vector2>();var c=new List<Color>();var t=new List<int>();
            const int sides=16;
            for(int layer=0;layer<5;layer++)
            {
                float bottom=1f+layer*.68f,radius=(1.22f-layer*.19f)*(1+variant*.035f);
                int start=v.Count;
                for(int ring=0;ring<4;ring++) for(int i=0;i<=sides;i++)
                {
                    float a=i*Mathf.PI*2/sides;
                    float profile=ring==0?.82f:ring==1?1f:ring==2?.65f:.03f;
                    float y=ring==0?bottom:ring==1?bottom+.12f:ring==2?bottom+.62f:bottom+1.02f;
                    float jitter=1+.075f*Mathf.Sin(a*5+layer+variant);
                    Vector3 p=new Vector3(Mathf.Cos(a)*radius*profile*jitter,y+.045f*Mathf.Sin(a*7+layer),Mathf.Sin(a)*radius*profile*jitter);
                    v.Add(p);uv.Add(new Vector2((float)i/sides*1.8f,(float)ring/3));
                    float shade=ring==0?.52f:ring==1?.72f:ring==2?.9f:1f;
                    c.Add(new Color(shade,shade,shade));
                    if(ring<3 && i<sides){int n=start+ring*(sides+1)+i;t.AddRange(new[]{n,n+sides+1,n+1,n+1,n+sides+1,n+sides+2});}
                }
                int baseCenter=v.Count;v.Add(new Vector3(0,bottom,0));uv.Add(Vector2.zero);c.Add(new Color(.48f,.48f,.48f));
                for(int i=0;i<sides;i++) t.AddRange(new[]{baseCenter,start+i,start+i+1});
                for(int branch=0;branch<4;branch++)
                {
                    float a=branch*Mathf.PI/2+layer*.45f;
                    Vector3 dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                    wood.Tube(new[]{Vector3.up*(bottom+.25f),dir*radius*.55f+Vector3.up*(bottom+.12f),dir*radius*.88f+Vector3.up*bottom},new[]{.08f,.045f,.012f});
                }
            }
            var mesh=new Mesh{name="Original spruce crown "+variant};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetColors(c);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            Trunks[variant]=wood.Finish("Original spruce bark "+variant);Crowns[variant]=mesh;
        }
    }
}
