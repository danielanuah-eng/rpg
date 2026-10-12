using System.Collections;
using System.IO;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica
{
    // Execução automática opcional num player compilado. Nunca roda no jogo normal.
    public sealed class ArenaSmoke : MonoBehaviour
    {
        ArenaGame game;
        // Casa vizinha alcançável com 1 PM (o mapa é sorteado, então não dá para fixar a casa).
        Cell Step(Fighter f)
        {
            var costs=Board.Costs(game.State,f);
            for(int i=0;i<costs.Length;i++) if(costs[i]==1) return new Cell(i%game.State.width,i/game.State.width);
            return f.cell;
        }
        IEnumerator Idle(float limit=10) { float end=Time.realtimeSinceStartup+limit; while(game.View.Busy&&Time.realtimeSinceStartup<end) yield return null; yield return new WaitForSeconds(.3f); }
        IEnumerator Confirmed(System.Action submit)
        {
            int revision=game.State.revision; submit();
            float end=Time.realtimeSinceStartup+5; while(game.State.revision==revision&&Time.realtimeSinceStartup<end) yield return null;
            yield return Idle();
        }
        IEnumerator MoveAndPass()
        {
            var f=game.Active; var to=Step(f);
            // No cliente o estado volta pela rede: espera a resposta do host antes de esperar a animação.
            yield return Confirmed(()=>game.Submit("mover",to));
            yield return Confirmed(()=>game.Submit("passar"));
        }
        IEnumerator Start()
        {
            game=GetComponent<ArenaGame>();
            yield return new WaitForSeconds(2);
            if(game.Network.IsHost) {
                float deadline=Time.realtimeSinceStartup+25;
                while(!game.Network.IsConnected&&Time.realtimeSinceStartup<deadline) yield return null;
                if(!game.Network.IsConnected) { Finish(false,"Cliente não conectou"); yield break; }
                yield return new WaitForSeconds(1);
                var guest=game.State.fighters.Find(x=>x.team==2&&x.summoner==0); var start=guest.cell;
                yield return MoveAndPass();
                deadline=Time.realtimeSinceStartup+40;
                while((game.Active?.team!=1||game.State.round<2)&&Time.realtimeSinceStartup<deadline) yield return null;
                Finish(game.State.round>=2&&game.State.F(guest.id).cell!=start,$"Host: turno próprio, comando do cliente e volta do turno (rodada {game.State.round}, ativo P{game.Active?.team}, convidado {start.x},{start.z}→{game.State.F(guest.id)?.cell.x},{game.State.F(guest.id)?.cell.z}, status {game.Status})");
            } else if(game.Client) {
                float deadline=Time.realtimeSinceStartup+25;
                while((!game.Synced||game.Active?.team!=2)&&Time.realtimeSinceStartup<deadline) yield return null;
                if(!game.Synced||game.Active?.team!=2) { Finish(false,"Sem turno sincronizado"); yield break; }
                yield return new WaitForSeconds(.5f);
                var me=game.Active; var to=Step(me);
                yield return MoveAndPass();
                yield return new WaitForSeconds(2);
                Finish(game.State.F(me.id).cell==to&&game.State.rng==0,"Cliente: comando autoritativo e estado filtrado");
            } else if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-shots")>=0) {
                yield return Shots();
            } else {
                Physics.SyncTransforms();
                var me=game.Active;
                var screen=game.View.BoardCamera.WorldToScreenPoint(ArenaPresentation.Position(game.State,me.cell)+Vector3.up*.8f);
                if(!game.View.Pick(screen,out _,out int picked)||picked!=me.id) { Finish(false,"Seleção da miniatura por raycast"); yield break; }
                // Loja: moedas de teste (o jogo local tem autoridade) e compra do slot 0.
                // Compra só ao lado da loja: uma casa vizinha vira loja durante a compra.
                game.State.coins[me.team]=10; var item=Shop.Def(game.State.shops[me.team].slots[0]);
                Tile next=null; foreach(var d in Board.Dirs) if(next==null&&Board.Contains(game.State,me.cell+d)) next=game.State.At(me.cell+d);
                var ground=next.terrain; next.terrain=ArenaTatica.Core.Terrain.Loja;
                game.Submit("comprar",slot:0); yield return Idle();
                next.terrain=ground; bool bought=game.State.coins[me.team]==10-item.price&&me.bought;
                // Skill: área de alcance desenhada e invocação numa casa válida.
                Directory.CreateDirectory("Artifacts");
                game.Mode="invocacao-dragao"; yield return new WaitForSeconds(.5f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/arena-skill.png")); yield return new WaitForSeconds(.5f);
                var sk=Content.Skill(me,"invocacao-dragao"); Cell spot=me.cell;
                foreach(var d in Board.Dirs) if(game.Preview.CanTarget(me,sk,me.cell+d,out _)) { spot=me.cell+d; break; }
                game.Submit("skill",spot,"invocacao-dragao"); yield return Idle();
                bool summoned=game.State.fighters.Exists(x=>x.summoner==me.id&&x.Alive);
                var to=Step(me);
                yield return MoveAndPass();
                bool moved=game.State.F(me.id).cell==to;
                // Depois da Ciantia age o dragão invocado; passa a vez dele.
                if(game.Active?.summoner==me.id) { game.Submit("passar"); yield return Idle(); }
                bool switched=game.Active!=null&&game.Active.team!=me.team;
                yield return MoveAndPass();
                string capture=Path.GetFullPath("Artifacts/arena-unity.png");
                ScreenCapture.CaptureScreenshot(capture); yield return new WaitForSeconds(1);
                if(!bought||!summoned||!moved||!switched) { Finish(false,$"Local: compra {bought}, invocação {summoned}, movimento {moved}, troca de time {switched}"); yield break; }
                Finish(game.State.round==2&&File.Exists(capture),"Local: seleção, compra, skill, movimento, turnos, rodada e captura");
            }
        }
        // Capturas para conferência visual (--arena-shots): um inimigo é posto ao lado do ativo, que ataca;
        // fotos durante a rolagem dos dados, na face final e no impacto (Artifacts/shots-<largura>x<altura>-<n>.png).
        IEnumerator Shots()
        {
            var me=game.Active; var foe=game.State.fighters.Find(x=>x.team!=me.team&&x.Alive&&x.summoner==0);
            var costs=Board.Costs(game.State,me); SkillDef attack=null; Cell spot=me.cell;
            for(int i=0;i<costs.Length&&attack==null;i++) {
                if(costs[i]!=1) continue; var c=new Cell(i%game.State.width,i/game.State.width); var old=foe.cell; foe.cell=c;
                foreach(var sk in Content.Def(me).skills) if(sk.damage>0&&Content.Skill(me,sk.id)!=null&&game.Preview.CanTarget(me,sk,c,out _)) { attack=sk; spot=c; break; }
                if(attack==null) foe.cell=old;
            }
            if(attack==null) { Finish(false,"Capturas: nenhum ataque possível"); yield break; }
            game.View.Sync(game.State,true); yield return new WaitForSeconds(1);
            Directory.CreateDirectory("Artifacts"); string prefix=Path.GetFullPath($"Artifacts/shots-{Screen.width}x{Screen.height}-");
            ScreenCapture.CaptureScreenshot(prefix+"0.png"); yield return new WaitForSeconds(.3f);
            game.Submit("skill",spot,attack.id);
            float start=Time.time; int n=1;
            foreach(float at in new[]{.25f,.5f,.8f,1.1f,1.4f,1.8f,2.4f}) { while(Time.time-start<at) yield return null; ScreenCapture.CaptureScreenshot(prefix+(n++)+".png"); }
            yield return new WaitForSeconds(1);
            Finish(true,$"Capturas: {me.def} ataca {foe.def} com {attack.id}");
        }
        // Espera 1 s antes de fechar: o último estado ainda precisa sair pela rede.
        void Finish(bool success,string message) { Debug.Log("ARENA_SMOKE_"+(success?"PASS: ":"FAIL: ")+message); StartCoroutine(Quit(success?0:1)); }
        IEnumerator Quit(int code) { yield return new WaitForSecondsRealtime(1); Application.Quit(code); }
    }
}
