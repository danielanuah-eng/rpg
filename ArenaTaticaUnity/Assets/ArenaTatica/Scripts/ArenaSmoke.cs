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
        IEnumerator MoveAndPass()
        {
            var f=game.Active; var to=Step(f);
            game.Submit("mover",to); yield return Idle();
            game.Submit("passar"); yield return Idle();
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
                Finish(game.State.round>=2&&game.State.F(guest.id).cell!=start,"Host: turno próprio, comando do cliente e volta do turno");
            } else if(game.Client) {
                float deadline=Time.realtimeSinceStartup+25;
                while((!game.Synced||game.Active?.team!=2)&&Time.realtimeSinceStartup<deadline) yield return null;
                if(!game.Synced||game.Active?.team!=2) { Finish(false,"Sem turno sincronizado"); yield break; }
                yield return new WaitForSeconds(.5f);
                var me=game.Active; var to=Step(me);
                yield return MoveAndPass();
                yield return new WaitForSeconds(2);
                Finish(game.State.F(me.id).cell==to&&game.State.rng==0,"Cliente: comando autoritativo e estado filtrado");
            } else {
                Physics.SyncTransforms();
                var me=game.Active;
                var screen=game.View.BoardCamera.WorldToScreenPoint(ArenaPresentation.Position(game.State,me.cell)+Vector3.up*.8f);
                if(!game.View.Pick(screen,out _,out int picked)||picked!=me.id) { Finish(false,"Seleção da miniatura por raycast"); yield break; }
                // Loja: moedas de teste (o jogo local tem autoridade) e compra do slot 0.
                game.State.coins[me.team]=10; var item=Shop.Def(game.State.shops[me.team].slots[0]);
                game.Submit("comprar",slot:0); yield return Idle();
                bool bought=game.State.coins[me.team]==10-item.price&&me.bought;
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
        // Espera 1 s antes de fechar: o último estado ainda precisa sair pela rede.
        void Finish(bool success,string message) { Debug.Log("ARENA_SMOKE_"+(success?"PASS: ":"FAIL: ")+message); StartCoroutine(Quit(success?0:1)); }
        IEnumerator Quit(int code) { yield return new WaitForSecondsRealtime(1); Application.Quit(code); }
    }
}
