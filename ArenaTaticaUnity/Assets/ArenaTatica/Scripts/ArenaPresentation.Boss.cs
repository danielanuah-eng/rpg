using System.Collections;
using System.Linq;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica
{
    // Batalha contra o chefe (Core/Boss.cs): o Trono no lugar da barraca do mascate e os eventos do chefe.
    public sealed partial class ArenaPresentation
    {
        const string ThroneArt="Sprites/Trono/trono";
        Sprite throneSprite;
        // Nas batalhas contra o chefe a loja é o Trono. Com o chefe sentado nele, o sprite sentado já traz o trono (só ele aparece).
        // true = não põe a barraca do mascate.
        bool Throne(MatchState s,Vector3 center)
        {
            if(string.IsNullOrEmpty(s.bossFight)) return false;
            if(s.fighters.Any(f=>f.Alive&&Match.Seated(f))) return true;
            if(!throneSprite) { var tex=Resources.Load<Texture2D>(ThroneArt); if(tex) throneSprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,.04f),tex.width/2.1f); }
            if(!throneSprite) {
                // Sem a arte: trono de blocos (assento, encosto alto e braços), pedra escura com ouro.
                var stone=Material(new Color(.22f,.2f,.24f)); var gold=Material(Gold);
                Cube("TronoAssento",center+new Vector3(0,.25f,0),new Vector3(1.5f,.5f,1.3f),stone,boardRoot);
                Cube("TronoEncosto",center+new Vector3(0,1.1f,.55f),new Vector3(1.5f,1.7f,.25f),stone,boardRoot);
                Cube("TronoCoroa",center+new Vector3(0,2f,.55f),new Vector3(.9f,.18f,.3f),gold,boardRoot);
                foreach(float side in new[]{-.7f,.7f}) Cube("TronoBraco",center+new Vector3(side,.6f,0),new Vector3(.18f,.3f,1.2f),gold,boardRoot);
                return true;
            }
            var go=new GameObject("Trono",typeof(SpriteRenderer)); go.transform.SetParent(boardRoot,false); go.transform.localPosition=center+Vector3.up*.01f;
            var sr=go.GetComponent<SpriteRenderer>(); sr.sprite=throneSprite; if(spriteMaterial) sr.sharedMaterial=spriteMaterial; ArenaLuz.Sombra(sr);
            var loop=go.AddComponent<SpriteLoop>(); loop.frames=new[]{throneSprite}; loop.billboard=true;
            return true;
        }

        IEnumerator BossEvent(GameEvent e,MatchState s,Transform actor)
        {
            switch(e.kind)
            {
                case "chefe-evento":
                    if(actor) {
                        float cast=PlayClip(actor,"event-cast");
                        StartCoroutine(Ring(actor.position+Vector3.up*.05f,2,Violet));
                        Flash(actor.position+Vector3.up*1.7f,Violet,35);
                        Float(actor.position+Vector3.up*2.3f,"INVOCAÇÃO DO REI",Violet);
                        yield return Wait(Mathf.Max(.65f,cast));
                    }
                    break;
                case "chefe-aliado":
                    if(s.F(e.target) is Fighter ally) {
                        if(units.TryGetValue(ally.id,out var old)&&old) { if(old.childCount>0) rigs.Remove(old.GetChild(0)); Destroy(old.gameObject); }
                        var u=units[ally.id]=CreateUnit(ally); u.position=Spot(s,ally,ally.cell); Turn(u,-u.position);
                        Flash(u.position+Vector3.up,Gold,45); StartCoroutine(Ring(u.position,2,Gold));
                        Float(u.position+Vector3.up*2.2f,$"ALIADO · {e.value} PV",Gold);
                        Game.Hud?.Log($"{Name(s,e.target)} agora luta por {TeamName(s,ally.team)}");
                        yield return Wait(Mathf.Max(.65f,PlayClip(u,"spawn")));
                    }
                    break;
                case "espelho-inversao":
                    if(actor) PlayClip(actor,"mirror");
                    if(units.TryGetValue(e.target,out var inverted)&&inverted) {
                        Flash(inverted.position+Vector3.up,MirrorColor,30);
                        StartCoroutine(Ring(inverted.position,1,MirrorColor));
                        Float(inverted.position+Vector3.up*1.8f,$"EFEITOS INVERTIDOS · {e.value}",MirrorColor);
                    }
                    Game.Hud?.Log($"Efeitos de {Name(s,e.target)} invertidos por {e.value} turnos");
                    yield return Wait(.55f);
                    break;
                case "chefe-trono":
                    if(actor) Float(actor.position+Vector3.up*1.6f,"Observa do trono…",Gold);
                    yield return Wait(.35f);
                    break;
                case "chefe-levanta":
                    Game.Hud?.Log($"{Name(s,e.actor)} se levanta do Trono!");
                    if(actor&&e.path!=null&&e.path.Length==2&&s.F(e.actor) is Fighter king) {
                        var to=Spot(s,king,e.path[1]); Flash(actor.position+Vector3.up,Gold,40);
                        float t=0, d=.45f; var from=actor.position;
                        while(t<d) { t+=Time.deltaTime; actor.position=Vector3.Lerp(from,to,t/d)+Vector3.up*Mathf.Sin(Mathf.PI*t/d)*.6f; yield return null; }
                        actor.position=to; Float(to+Vector3.up*2f,"O REI SE LEVANTA",Red);
                    }
                    BuildBoard(s); RefreshMarkers(s);
                    yield return Wait(.5f);
                    break;
                case "chefe-fase":
                    Game.Hud?.Log($"{Name(s,e.actor)} saca a segunda espada: FASE 2!");
                    if(actor) { Flash(actor.position+Vector3.up*1.2f,Violet,60); Float(actor.position+Vector3.up*2.2f,"FASE 2 · DUAS ESPADAS",Violet); }
                    yield return Wait(.6f);
                    break;
                case "muralha":
                    BuildBoard(s); RefreshMarkers(s);
                    foreach(var c in e.path??new Cell[0]) Flash(Position(s,c)+Vector3.up*.5f,Violet,10);
                    Game.Hud?.Log($"{Name(s,e.actor)} ergue {e.path?.Length??0} muralhas por {e.value} rodada{(e.value>1?"s":"")}");
                    yield return Wait(.4f);
                    break;
                case "muralha-fim":
                    BuildBoard(s); RefreshMarkers(s); Game.Hud?.Log("As muralhas do labirinto desabam");
                    break;
                case "reinicio":
                    if(actor) { Flash(actor.position+Vector3.up,Gold,40); Float(actor.position+Vector3.up*2f,"Ultimates reiniciadas!",Gold); }
                    Game.Hud?.Log($"{Name(s,e.actor)} reinicia as ultimates e recargas de todos");
                    yield return Wait(.4f);
                    break;
            }
        }
    }
}
