using UnityEngine;

namespace ArenaTatica
{
    // Mãos de energia segurando quem está atordoado (Agarrão espectral da Pistoleira e todo atordoamento, autor 21/10).
    // Duas mãos desenhadas na hora (sem arquivo), uma de cada lado do alvo, apertando em laço, com faíscas subindo.
    public sealed class GripHands : MonoBehaviour
    {
        public Transform target; public float height=.75f;
        SpriteRenderer left, right; Color tint; float t;
        static Sprite hand;

        public void Build(Material sprite,Material spark,Color color)
        {
            tint=color; hand??=HandSprite();
            left=Hand("MaoEsquerda",sprite); right=Hand("MaoDireita",sprite); right.flipX=true;
            var go=new GameObject("Faiscas"); go.transform.SetParent(transform,false);
            var ps=go.AddComponent<ParticleSystem>(); ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var m=ps.main; m.loop=true; m.startLifetime=new ParticleSystem.MinMaxCurve(.4f,.8f); m.startSpeed=new ParticleSystem.MinMaxCurve(.2f,.6f);
            m.startSize=new ParticleSystem.MinMaxCurve(.04f,.09f); m.startColor=color; m.simulationSpace=ParticleSystemSimulationSpace.World; m.maxParticles=50; m.gravityModifier=-.25f;
            var e=ps.emission; e.rateOverTime=22; var sh=ps.shape; sh.shapeType=ParticleSystemShapeType.Sphere; sh.radius=.4f;
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=spark; ps.Play();
        }
        SpriteRenderer Hand(string name,Material material)
        {
            var r=new GameObject(name,typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); r.transform.SetParent(transform,false);
            r.sprite=hand; if(material) r.sharedMaterial=material; r.sortingOrder=5; return r;
        }
        void LateUpdate()
        {
            var cam=Camera.main; if(!target||!cam||!left) return;
            t+=Time.deltaTime; float squeeze=Mathf.Sin(t*5);
            transform.position=target.position+Vector3.up*height*.35f;
            foreach(var (r,side) in new[]{(left,-1f),(right,1f)}) {
                float open=.34f-.05f*squeeze;
                r.transform.position=transform.position+cam.transform.right*side*open+cam.transform.up*.04f*Mathf.Sin(t*3+side)-cam.transform.forward*.05f;
                r.transform.rotation=cam.transform.rotation*Quaternion.Euler(0,0,side*(32+10*squeeze));
                r.transform.localScale=Vector3.one*.62f*(1+.06f*squeeze);
                r.color=new Color(tint.r,tint.g,tint.b,.72f+.2f*Mathf.Sin(t*7+side));
            }
        }

        // Mão aberta de dedos para cima (64×64, pivô no pulso): palma, 4 dedos e polegar por distância a cápsulas; borda mais
        // forte que o miolo para parecer energia.
        static Sprite HandSprite()
        {
            const int N=64; var tex=new Texture2D(N,N,TextureFormat.RGBA32,false){ filterMode=FilterMode.Point, wrapMode=TextureWrapMode.Clamp };
            var px=new Color[N*N];
            (Vector2 a,Vector2 b,float r)[] parts={
                (new Vector2(32,12),new Vector2(32,26),11f),                                     // palma
                (new Vector2(21,28),new Vector2(19,50),3.6f), (new Vector2(28,30),new Vector2(28,57),3.8f),
                (new Vector2(36,30),new Vector2(37,56),3.8f), (new Vector2(43,28),new Vector2(46,48),3.4f),
                (new Vector2(22,16),new Vector2(10,32),3.6f) };                                 // polegar
            for(int y=0;y<N;y++) for(int x=0;x<N;x++) {
                var p=new Vector2(x+.5f,y+.5f); float d=999;
                foreach(var (a,b,r) in parts) { var ab=b-a; float k=Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude); d=Mathf.Min(d,Vector2.Distance(p,a+ab*k)-r); }
                float alpha=d>1?0:d>-1.5f?1:Mathf.Lerp(.55f,.8f,Mathf.Clamp01(-d/6f)*0+Mathf.PingPong(-d/4f,1)*.5f);
                float glow=d>1&&d<3.5f?(3.5f-d)/2.5f*.35f:0;
                px[y*N+x]=new Color(1,1,1,Mathf.Max(alpha,glow));
            }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex,new Rect(0,0,N,N),new Vector2(.5f,.12f),64);
        }
    }
}
