using System.Collections.Generic;

namespace ArenaTatica.Core
{
    // Expansão de 22/10 (pedido do autor, desenho técnico do Codex): inventores e luna.
    // Aqui ficam só as fichas e as skills novas; o comportamento mora em Match.InventoresLuna.cs e é ligado pela etiqueta
    // SkillDef.effect (skill comum = effect vazio). Todos os números abaixo são os iniciais editáveis da ficha.
    // Heróis novos: PV 11, PA 6, PM 3, iniciativa 5, nível 4; skill básica custa 2 PA e as especiais 3 PA (salvo indicado).
    public static partial class Content
    {
        static bool RegisterLuna()
        {
            // ── NIKOLA (inventor): comanda dois autômatos e vive de engrenagens. ──
            Fighters["nikola"]=new FighterDef { id="nikola", name="Nikola", sprites="Nikola", hp=11, pa=6, pm=3, initiative=5, level=4, legendary=true,
                luna=new LunaDef { family="nikola", techProtect=true, gearStep=2, gearDamage=1, gearRange=1, gearDrops=3, gearSpecial=20, gearHits=2 },
                skills=new[] {
                    new SkillDef { id="comando-blip", name="Comando: Blip", kind=SkillKind.Invocacao, effect="comando-blip", pa=3, minRange=0, maxRange=1, sight=false, anim="summon" },
                    new SkillDef { id="comando-celinos", name="Comando: Celinos", kind=SkillKind.Invocacao, effect="comando-celinos", pa=3, minRange=0, maxRange=99, sight=false, global=true, anim="cast" },
                    new SkillDef { id="eletaser", name="Eletaser", kind=SkillKind.Dano, effect="eletaser", pa=2, minRange=1, maxRange=3, damage=1, modifiable=true, anim="attack" },
                    new SkillDef { id="matriz-protecao", name="Matriz de proteção", kind=SkillKind.Suporte, effect="matriz-protecao", pa=3, minRange=0, maxRange=0, sight=false, anim="cast" },
                    new SkillDef { id="saida-matriz", name="Saída da matriz", kind=SkillKind.Teleporte, effect="saida-matriz", pa=3, minRange=0, maxRange=99, sight=false, global=true, anim="teleport" } } };
            // Blip: sem turno, 3 PV. Aos 10 de engrenagem vira ModoBlipware (ficha separada, com turno controlável).
            Fighters["blip"]=new FighterDef { id="blip", name="Blip", sprites="Blip", hp=3, pa=0, pm=4, level=0, picker=false, noTurn=true, noLock=true, noReveal=true,
                luna=new LunaDef { family="nikola", techProtect=true, survivesOwner=true, gearMorph=10, gearPmBase=4 }, skills=new SkillDef[0] };
            Fighters["blipware"]=new FighterDef { id="blipware", name="Blipware", sprites="Blipware", hp=3, pa=6, pm=4, initiative=5, level=0, picker=false, noLock=true, noReveal=true,
                luna=new LunaDef { family="nikola", techProtect=true, survivesOwner=true, healTurn=2 },
                skills=new[] {
                    new SkillDef { id="ataque-blipware", name="Braço pesado", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=6, anim="attack" },
                    new SkillDef { id="puxao-blipware", name="Braço de aço", kind=SkillKind.Dano, effect="puxao-blipware", pa=3, minRange=1, maxRange=10, damage=0, sight=false, anim="cast" } } };
            // Celinos: sem turno, 3 PV, 5 PM, revela escondidos até 4 casas e atira (1 de dano, alcance 3, até 2 inimigos).
            Fighters["celinos"]=new FighterDef { id="celinos", name="Celinos", sprites="Celinos", hp=3, pa=0, pm=5, level=0, picker=false, noTurn=true, noLock=true, noReveal=true,
                luna=new LunaDef { family="nikola", techProtect=true, survivesOwner=true, gearStep=3, gearDamage=1, gearPm=1, gearReveal=1, revealBase=4, shotRange=3, chainRange=2 },
                skills=new[] { new SkillDef { id="tiro-celinos", name="Tiro certeiro", kind=SkillKind.Dano, pa=0, minRange=1, maxRange=3, damage=1, anim="attack" } } };
            // ── BIFIA (arco lunar): rouba PM com a flecha, cura aliados com o sonus e abre o Viva Luna. ──
            Fighters["bifia"]=new FighterDef { id="bifia", name="Bifia", sprites="Bifia", hp=11, pa=6, pm=3, initiative=5, level=4,
                luna=new LunaDef { family="bifia", vivaTurns=2, vivaRadius=6, vivaInit=1, vivaPm=2 },
                skills=new[] {
                    new SkillDef { id="flecha-lunar", name="Flecha lunar", kind=SkillKind.Dano, effect="flecha-lunar", pa=2, minRange=1, maxRange=8, damage=1, modifiable=true, anim="attack" },
                    new SkillDef { id="tiro-sonus", name="Tiro sonus", kind=SkillKind.Cura, effect="tiro-sonus", pa=2, minRange=1, maxRange=4, damage=0, cooldown=2, sight=false, anim="cast" },
                    new SkillDef { id="viva-luna", name="Viva Luna", kind=SkillKind.Suporte, effect="viva-luna", pa=6, minRange=0, maxRange=99, sight=false, global=true, perMatch=1, ultimate=true, anim="cast" } } };
            // ── DESLAND (roque e xadrez): move aliados, clona quem ele matou e abre o domínio de xadrez. ──
            // A forma "dominio" existe só para as skills de movimento do xadrez aparecerem na ficha enquanto o domínio dura.
            Fighters["desland"]=new FighterDef { id="desland", name="Desland", sprites="Desland", hp=11, pa=6, pm=3, initiative=5, level=4,
                luna=new LunaDef { family="desland", ecoCooldown=2, cloneTurns=1, chessTurns=4, pieceDamage=2, queenDamage=4 },
                forms=new[] { new FormDef { id="dominio", name="Domínio de xadrez", color="#E8D8A0", armor=1 } },
                skills=new[] {
                    new SkillDef { id="roque", name="Roque", kind=SkillKind.Suporte, effect="roque", pa=3, minRange=0, maxRange=8, sight=false, twoCells=true, anim="cast" },
                    new SkillDef { id="xeque-mate", name="Xeque-mate", kind=SkillKind.Dano, effect="xeque-mate", pa=3, minRange=1, maxRange=5, damage=2, modifiable=true, anim="attack" },
                    new SkillDef { id="eco-de-aliado", name="Eco de aliado", kind=SkillKind.Invocacao, effect="eco-de-aliado", pa=0, minRange=0, maxRange=99, sight=false, global=true, anim="summon" },
                    new SkillDef { id="dominio-xadrez", name="Domínio de xadrez", kind=SkillKind.Invocacao, effect="dominio-xadrez", pa=3, minRange=0, maxRange=0, cooldown=2, sight=false, anim="cast" },
                    new SkillDef { id="mover-peao", name="Mover peão", kind=SkillKind.Teleporte, effect="mover-peao", pa=2, minRange=0, maxRange=99, sight=false, global=true, twoCells=true, form="dominio", anim="teleport" },
                    new SkillDef { id="mover-torre", name="Mover torre", kind=SkillKind.Teleporte, effect="mover-torre", pa=2, minRange=0, maxRange=99, sight=false, global=true, twoCells=true, form="dominio", anim="teleport" },
                    new SkillDef { id="mover-cavalo", name="Mover cavalo", kind=SkillKind.Teleporte, effect="mover-cavalo", pa=2, minRange=0, maxRange=99, sight=false, global=true, twoCells=true, form="dominio", anim="teleport" },
                    new SkillDef { id="mover-bispo", name="Mover bispo", kind=SkillKind.Teleporte, effect="mover-bispo", pa=2, minRange=0, maxRange=99, sight=false, global=true, twoCells=true, form="dominio", anim="teleport" },
                    new SkillDef { id="mover-rainha", name="Mover rainha", kind=SkillKind.Teleporte, effect="mover-rainha", pa=2, minRange=0, maxRange=99, sight=false, global=true, twoCells=true, form="dominio", anim="teleport" } } };
            // ── NEPTURIUS (esfera de antimatéria): a esfera é invulnerável e só as skills da família a movem. ──
            Fighters["nepturius"]=new FighterDef { id="nepturius", name="Nepturius", sprites="Nepturius", hp=11, pa=6, pm=3, initiative=5, level=4,
                luna=new LunaDef { family="nepturius", auraRange=4, auraPa=2, auraPm=1, grabCount=4, grabRange=4, blastRadius=3, blastDamage=2, cubeDamage=2, cubeHeal=2, armorBuff=1, armorTurns=1 },
                skills=new[] {
                    new SkillDef { id="mover-esfera", name="Mover esfera", kind=SkillKind.Teleporte, effect="mover-esfera", pa=0, minRange=0, maxRange=8, sight=false, anim="cast" },
                    new SkillDef { id="agarrao-negro", name="Agarrão negro", kind=SkillKind.Dano, effect="agarrao-negro", pa=3, minRange=0, maxRange=0, sight=false, damage=0, anim="cast" },
                    new SkillDef { id="loucura-negra", name="Loucura negra", kind=SkillKind.Suporte, effect="loucura-negra", pa=3, minRange=0, maxRange=0, sight=false, anim="cast" },
                    new SkillDef { id="explosao-antimateria", name="Explosão de antimatéria", kind=SkillKind.Dano, effect="explosao-antimateria", pa=3, minRange=0, maxRange=0, sight=false, selfArea=true, damage=2, anim="attack" } } };
            // Esfera: invulnerável, sem turno próprio, PM 4 (recarrega a cada turno do dono) e imune a qualquer habilidade alheia.
            Fighters["esfera-nepturius"]=new FighterDef { id="esfera-nepturius", name="Esfera de antimatéria", sprites="EsferaNepturius", hp=1, pa=0, pm=4, level=0, picker=false, noTurn=true, noLock=true, noReveal=true,
                luna=new LunaDef { family="nepturius", invulnerable=true, survivesOwner=true, auraRange=4, auraPa=2, auraPm=1, grabCount=4, grabRange=4, blastRadius=3, blastDamage=2 }, skills=new SkillDef[0] };
            // ── TUBARÃO (evento das Docas inundadas): neutro, 3 PV, 5 PM, mordida de 1 por 3 PA, anda só na água. ──
            Fighters["tubarao"]=new FighterDef { id="tubarao", name="Tubarão", sprites="Tubarao", hp=3, pa=6, pm=5, level=0, picker=false, neutral=true, noLock=true, noReveal=true,
                luna=new LunaDef { family="tubarao", waterOnly=true },
                skills=new[] { new SkillDef { id="mordida-tubarao", name="Mordida", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=1, anim="attack" } } };
            // Peças do domínio de xadrez: 1 PV cada, sem turno (agem no fim do turno do Desland), nenhum rei.
            Piece("peca-peao","Peão de xadrez","XadrezPeao");
            Piece("peca-torre","Torre de xadrez","XadrezTorre",3);
            Piece("peca-cavalo","Cavalo de xadrez","XadrezCavalo");
            Piece("peca-bispo","Bispo de xadrez","XadrezBispo");
            Piece("peca-rainha","Rainha de xadrez","XadrezRainha");
            return true;
        }
        // Peça do domínio de xadrez: 1 PV, sem turno, sem skill própria (o motor move e ataca pelas regras do xadrez).
        // Peças (autor 24/10): imunes a condições (proteção tecnológica), terreno não as atrapalha; a torre tem 3 PV.
        static void Piece(string id,string name,string sprites,int hp=1) =>
            Fighters[id]=new FighterDef { id=id, name=name, sprites=sprites, hp=hp, pa=0, pm=0, level=0, picker=false, noTurn=true, noLock=true, noReveal=true,
                luna=new LunaDef { family="desland", techProtect=true }, skills=new SkillDef[0] };
    }
}
