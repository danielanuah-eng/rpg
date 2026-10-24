using System.Collections.Generic;

namespace ArenaTatica.Core
{
    // Espelho primordial (inversao): enquanto fighter.inversionTurns>0 os buffs do alvo ficam de cabeca para baixo (negativos
    // viram positivos e vice-versa) e buffs novos ja entram invertidos. Ao encerrar (Clear/Tick) so os buffs ainda marcados
    // inverted e ainda vivos voltam ao normal; os que expiraram no caminho nao ressuscitam. Formas com effectImmune ignoram
    // a inversao e recusam efeitos negativos (os positivos passam).
    public sealed partial class Match
    {
        // Espelho-cura: espelho do sem-cura (NoHealBuff). Enquanto ativo, cura positiva no dono vale em dobro.
        public const string InvertedHealBuff="espelho-cura";
        // Turnos de inversao aplicados por cast (o 2o cast renova, nao inverte de novo).
        public const int InversionTurns=2;
        // kind do GameEvent da inversao.
        public const string InversionEvent="espelho-inversao";

        // Vira um buff de cabeca para baixo: nega os numeros, troca taken com takenDivisor, alterna inverted e troca os ids
        // sem-cura/espelho-cura. E involutivo: aplicar de novo restaura (fora a troca de id, que tambem volta).
        static void FlipBuff(Buff b)
        {
            if(b==null) return;
            b.pa=-b.pa; b.pm=-b.pm; b.armor=-b.armor; b.range=-b.range; b.extra=-b.extra;
            b.dmg=-b.dmg; b.regen=-b.regen; b.reflect=-b.reflect; b.immune=-b.immune;
            int taken=b.taken; b.taken=b.takenDivisor; b.takenDivisor=taken;
            b.inverted=!b.inverted;
            if(b.id==NoHealBuff) b.id=InvertedHealBuff;
            else if(b.id==InvertedHealBuff) b.id=NoHealBuff;
        }

        // Inverte o alvo por turns turnos (aliados e inimigos valem; forma com effectImmune recusa). Buffs ativos entram na
        // inversao so no 1o cast: recast so renova a duracao, sem inverter duas vezes.
        void ApplyInversion(Fighter target,int turns,Fighter by)
        {
            if(target==null||turns<=0) return;
            if(Content.Form(target)?.effectImmune??false) return;
            bool recast=target.inversionTurns>0;
            target.inversionTurns=recast?System.Math.Max(target.inversionTurns,turns):turns;
            if(!recast) foreach(var b in new List<Buff>(target.buffs)) FlipBuff(b);
            Emit(new GameEvent{kind=InversionEvent,actor=by!=null?by.id:0,target=target.id,value=turns,cell=target.cell});
        }

        // Entrada de buff: com inversao ativa o buff novo ja entra invertido; em forma com effectImmune os efeitos negativos
        // sao anulados (estatisticas no minimo 0, reflect/immune negativos viram 0, taken>1 vira 0, sem-cura recusado)
        // e os positivos ficam como estao.
        void AddBuff(Fighter target,Buff buff)
        {
            if(target==null||buff==null||TechProtected(target)||Unbreakable(target)) return;
            if(target.inversionTurns>0&&!buff.inverted) FlipBuff(buff);
            if(Content.Form(target)?.effectImmune??false) { if(!Neutralize(buff)) return; }
            target.buffs.Add(buff);
        }

        // Forma effectImmune: zera os efeitos negativos do buff. Devolve false quando o buff inteiro e recusado (sem-cura).
        static bool Neutralize(Buff b)
        {
            if(b.id==NoHealBuff) return false;
            if(b.pa<0) b.pa=0; if(b.pm<0) b.pm=0; if(b.armor<0) b.armor=0; if(b.range<0) b.range=0;
            if(b.extra<0) b.extra=0; if(b.dmg<0) b.dmg=0; if(b.regen<0) b.regen=0;
            if(b.reflect<0) b.reflect=0; if(b.immune<0) b.immune=0;
            if(b.taken>1) b.taken=0;
            return true;
        }

        // Fim da inversao: devolve so os buffs ainda marcados inverted e ainda na lista (os que expiraram nao voltam).
        void ClearInversion(Fighter f)
        {
            if(f==null) return;
            foreach(var b in f.buffs) if(b.inverted) FlipBuff(b);
            f.inversionTurns=0;
        }

        // Passa um turno do alvo: quando a contagem zera, restaura os buffs marcados (sem mexer nos turns deles).
        void TickInversion(Fighter f)
        {
            if(f==null||f.inversionTurns<=0) return;
            if(--f.inversionTurns<=0) ClearInversion(f);
        }

        // Cura passando pelo espelho-cura: cura positiva vale em dobro; dano/cura zero ficam como estao.
        int InvertedHealing(Fighter f,int amount) =>
            amount>0&&f!=null&&f.buffs.Exists(b=>b.id==InvertedHealBuff)?amount*2:amount;
    }
}
