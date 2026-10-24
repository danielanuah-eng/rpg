using System;
using System.Collections.Generic;

namespace ArenaTatica
{
    [Serializable] public struct Cell : IEquatable<Cell>
    {
        public int x, z;
        // Visão de rede: mantém o lutador/turno, sem entregar sua casa ao adversário.
        public static Cell Unknown => new Cell(-1,-1);
        public Cell(int x, int z) { this.x=x; this.z=z; }
        public bool Equals(Cell c) => x==c.x && z==c.z;
        public override bool Equals(object o) => o is Cell c && Equals(c);
        public override int GetHashCode() => x*397 ^ z;
        public static bool operator ==(Cell a, Cell b) => a.Equals(b);
        public static bool operator !=(Cell a, Cell b) => !a.Equals(b);
        public static Cell operator +(Cell a, Cell b) => new Cell(a.x+b.x,a.z+b.z);
        public int Distance(Cell b) => Math.Abs(x-b.x)+Math.Abs(z-b.z);
    }
}

// Núcleo estilo Krosmaster (Documentacao/DESIGN-KROSMASTER.md). C# puro, determinístico, roda no Host.
namespace ArenaTatica.Core
{
    // Loja: as 4 casas do meio são a barraca do mascate (decisão do autor, 11/10): ninguém entra e ela tapa a visão.
    // Parede (mapas fechados, 18/10): ninguém entra, nem voador, e tapa a visão. Porta: fechada é igual à parede; quem está
    // colado gasta 1 PM para abrir (comando "abrir") e ela vira casa livre com TileObj.PortaAberta.
    // Agua (expansão de 22/10, Docas inundadas): sempre no FIM do enum, caminhável, custa o dobro do passo normal (Board.StepCost).
    public enum Terrain { Livre, Arbusto, Arvore, Caixa, Loja, Parede, Porta, Agua }
    // Objeto fixo da casa (Tile.obj), trazido do DungeonBDX (18/10). Baú: quem entra ganha BauMoedas e ele some.
    // Espetos: quem entra andando ou empurrado (sem voar) leva 1 de dano e segue. Braseiro: obstáculo; quem começa o turno
    // colado ganha +1 PA e, no Domínio, o único time colado nele no fim da rodada marca 1 ponto. Portal: liga à casa espelhada.
    // Vela (labirinto): obstáculo; colado nela, "acender" tira a névoa do mapa para todos e dá 2 moedas a quem acendeu.
    public enum TileObj { Nada, Bau, Espetos, Braseiro, Portal, PortaAberta, Vela }
    // A face de crítico do Krosmaster virou Cataclisma (decisão 11); o coringa Crítico/Esquiva virou Esquiva/Trava.
    public enum Face { Cataclisma, Escudo, Trava, Esquiva, CoringaEsquivaTrava, Coringa }
    public enum VictoryMode { Galeoes, Eliminacao, Tesouro, Dominio }
    // Ordem dos turnos (autor, 13/10): Completo = Krosmaster (o jogador joga o time inteiro); Alternada = um lutador de cada time
    // por vez, da maior Iniciativa para a menor.
    public enum TurnMode { Completo, Alternada }
    // Teleporte: vai para casa livre sem caminho. Salto: alvo inimigo, cai na casa livre vizinha a ele mais perto de quem salta.
    // Forma: muda a forma do próprio lutador (arma, transformação). Esconder: fica invisível junto a relevo mais alto, árvore ou caixa.
    // Espelho: ganha 1 uso da última skill que o alvo usou.
    // Suporte: cura e/ou buff em aliados (alvo, área ou em volta de si) e pode invocar. Zona: efeito nas casas (chão de gelo).
    // Comando: ordem a uma invocação sem turno (gatos da Asamyh): step = andar; senão ela usa a 1ª skill dela e some.
    // Reviver: volta um aliado nocauteado numa casa vizinha. Desejo: canaliza 2 turnos e escolhe uma opção (wish a..f).
    // Aura (Baltazar): liga/desliga em si; ligada, cobra pa no começo de cada turno do dono e cura heal nos aliados e fere damage nos
    // inimigos até area casas, 1 vez por alvo por turno do dono. Sem PA para manter, desliga. Novos tipos sempre no fim (as edições guardam o número).
    // Troca (Shambles): dois alvos (Command.cell e cell2) trocam de casa. Portal: entrada em cell, saída em cell2; quem entra sai numa casa
    // livre vizinha da saída.
    // Pulso (L.E.L.A, 20/10): dispara agora o efeito das passivas da forma atual nos aliados que elas pegam, multiplicado por
    // amplify, e o mesmo efeito volta no começo do próximo turno de cada aliado.
    public enum SkillKind { Dano, Invocacao, Cura, Armadilha, Teleporte, Salto, Forma, Esconder, Espelho, Suporte, Zona, Comando, Reviver, Desejo, Aura, Troca, Portal, Pulso }
    // As 3 bolinhas do editor (sprint 1): 🔵 si, 🔴 inimigos, 🟢 aliados. SkillDef.hits 0 = regra antiga de cada tipo.
    [Flags] public enum Hit { Padrao=0, Si=1, Inimigo=2, Aliado=4 }
    public enum ShopKind { Item, Magia, Pet, Armadilha, Buff }

    // lava/mark: eventos (maré de lava, fogos). frozenBy: lutador que congelou a casa (+1 PM para entrar). trapPa: mina de gelo.
    // obj: TileObj num único int (o snapshot de 18×18 precisa caber no Relay).
    // shadow: casa de sombra da Ellie (id dela; 0 = nenhuma), refeita no começo de cada turno dela.
    [Serializable] public class Tile { public int height, coins, trap, trapDamage, trapPa, trapBy, frozenBy, obj, shadow; public Terrain terrain; public bool zone, lava, mark;
        public TileObj Obj { get => (TileObj)obj; set => obj=(int)value; } }
    // Portal (sprint 1): casas de entrada e saída (Board.Index), quem abriu e turnos do dono que faltam. Fica numa lista do estado,
    // não em cada casa, para o snapshot de 6 jogadores caber no Relay.
    // allies: só aliados de quem abriu passam (portal do Mordomo Pilis).
    [Serializable] public class Portal { public int a, b, by, turns; public bool allies; }

    [Serializable] public class SkillDef
    {
        public string id, name, summon;
        public SkillKind kind;
        public int pa, minRange=1, maxRange=1, damage, perTurn;
        public bool line, sight=true, modifiable;
        // perMatch: usos na partida (ultimates). form: só nessa forma (vazio = sempre). toForm/turns: a skill de Forma leva a essa forma
        // (turns 0 = até trocar). endsForm: usar a skill encerra a forma. area: atinge inimigos até essa distância do alvo.
        // drain: cura quem acerta. precise: ignora o Escudo do defensor. wild: erra com Esquiva no dado de ataque. pmLoss: alvo perde PM no próximo turno.
        // anim: clipe da apresentação (padrão "attack").
        public int perMatch, turns, area, drain, pmLoss;
        // cooldown: depois de usar, volta a cada N turnos do lutador. ultimate: precisa ter limite (perTurn, perMatch ou cooldown).
        public int cooldown;
        public bool ultimate;
        public string form, toForm, anim;
        public bool endsForm, precise, wild;
        // Efeitos genéricos (editor de skills): pierce ignora armadura e Escudo; push empurra N casas; pull puxa até colar;
        // paLoss: alvo perde PA no próximo turno; prison: preso N turnos (não age nem é alvo; escapa com Esquiva);
        // selfArea: área em volta de quem usa (square = as 8 casas); foes: a área só pega inimigos; isolatedBonus: dano a mais
        // se o alvo não tem aliado colado; minRound: só a partir dessa rodada. Suporte: heal e buffPa/Pm/Armor/Range por turns.
        // freeze (Zona): casas congeladas. noCover (Esconder): esconde sem relevo. maxActive: invocações ativas do mesmo grupo
        // (summon pode sortear entre "a|b|c"); summonCount: quantas invoca de uma vez. step (Comando): casas que a invocação anda.
        public bool pierce, pull, selfArea, square, foes, freeze, noCover;
        public int push, paLoss, prison, isolatedBonus, minRound, heal, buffPa, buffPm, buffArmor, buffRange, maxActive, summonCount, step;
        public string wish;
        // hits: bolinhas (Hit); 0 = padrão do tipo. channeled: resolve no começo do próximo turno de quem usa, que encerra o turno ao canalizar.
        // mine (Armadilha): vira mina terrestre. freeSwap (Forma): custa 0 se não usou skill de arma neste turno (Damian).
        // turns também é a duração do Portal (turnos do dono; 0 = 2).
        public int hits, invertTurns;
        public bool channeled, mine, freeSwap;
        // Elenco de 20/10. dispel: o golpe anula formas, buffs bons, canalização e aura do alvo antes do dano (Pele D. Negra).
        // banish: a prisão vira buraco negro (sem Esquiva, pega aliado também); no retorno o inimigo leva returnDamage.
        // pmRange: +1 de alcance por PM, e só gasta PM no que passar de maxRange (Light Speed). pmCost: custa PM em vez de PA.
        // mark: Marcas da Maldição no alvo ao acertar; markRange: +1 de alcance por marca do alvo; consumeMarks: +2 de dano a
        // cada 2 marcas e zera as marcas (Gumbald). global: alcance no mapa todo, sem linha de visão nem muro (míssil da L.E.L.A).
        // perForm: usos por transformação. notForm: some nessa forma. amplify: multiplicador do Pulso. halfMap: alcance = metade
        // do mapa (Room do Ikaito). noHeal: o alvo não se cura até o fim do turno de quem acertou.
        // Cura com drain: quem usa também cura drain (Injection Shot).
        public bool dispel, banish, pmRange, markRange, consumeMarks, global, halfMap, noHeal;
        public int returnDamage, pmCost, mark, perForm, amplify;
        public string notForm;
        // Elenco de 21/10. gainPm: quem usa ganha PM. shadowStep (Teleporte): destino numa sombra própria ou colado num inimigo.
        // stun: o alvo perde os próximos N turnos (atordoado; pode ser alvo). purge: tira os buffs bons dos inimigos atingidos;
        // cleanse: tira condições ruins dos aliados atingidos. sealUlt: as ultimates do inimigo entram em recarga N turnos e a forma
        // de ultimate cai. buffReflect/buffImmune (Suporte): o próximo golpe que não mata volta em quem bateu / imune a efeitos
        // negativos. reveal (Zona): revela escondidos inimigos na área. alliesOnly (Portal): só aliados passam. doubleDamage: o alvo
        // leva o dobro dos golpes por N turnos. beam: o golpe para no 1º lutador da linha, em obstáculo ou chão mais alto.
        // swapHp: troca os PV de quem usa com os do alvo. props (Troca): troca também caixas, árvores, arbustos, baús e braseiros.
        // halfMapIn: o halfMap só vale nessa forma (fora dela, maxRange). noRefresh: o Eco ancestral não devolve o uso.
        // selfKill: quem usa cai logo depois de resolver o golpe (Explosão do chá, autor 21/10).
        public int gainPm, stun, sealUlt, doubleDamage;
        public bool shadowStep, purge, cleanse, buffReflect, buffImmune, reveal, alliesOnly, beam, swapHp, props, noRefresh, selfKill;
        public string halfMapIn;
        // Chefe (Rei Thaagarys, 22/10). weight: peso na IA do chefe (0 = fora do pool). walls (Zona): ergue até N paredes nas casas
        // livres da área por turns rodadas (0 = 2). resetUlts: reinicia as ultimates e recargas da partida (de todos). cataclysm:
        // dispara um evento aleatório (do pool do chefe, BossDef.events, se quem usa tiver um).
        public int weight, walls;
        public bool resetUlts, cataclysm;
        // Expansão de 22/10 (inventores e luna). effect: etiqueta de comportamento resolvido por Match.InventoresLuna
        // (vazio = skill comum, que segue o kind). twoCells: pede duas casas (Command.cell e cell2), como Troca/Portal/Teleporte.
        public string effect;
        public bool twoCells;
        // mapPct: % do maior lado do mapa que o halfMap alcança (0 = 50%; Room do Ikaito maior, autor 22/10). Com halfMapIn:
        // freeIn = nessa forma ignora visão e muro; hitsOut = alvos fora dessa forma (0 = os mesmos de hits; Shambles só aliados).
        public int mapPct, hitsOut;
        public bool freeIn;
        public bool CanHitSelf => (hits&(int)Hit.Si)!=0;
        public bool CanHitEnemy => (hits&(int)Hit.Inimigo)!=0;
        public bool CanHitAlly => (hits&(int)Hit.Aliado)!=0;
        public bool Custom => hits!=0;
        // Troca, Portal e Teleporte de aliado pedem uma 2ª casa (Command.cell2).
        public bool TwoCells => twoCells||kind==SkillKind.Troca||kind==SkillKind.Portal||(kind==SkillKind.Teleporte&&Custom&&(CanHitAlly||CanHitEnemy));
        // Skill da expansão: o comportamento é do núcleo novo, não do kind (Match.InventoresLuna.cs).
        public bool Custom2 => !string.IsNullOrEmpty(effect);
        public SkillDef Clone() => (SkillDef)MemberwiseClone();
    }
    // Forma do lutador (arma do Damian, Berserk do Tchu tcha): bônus enquanto dura e a pasta de sprites dela.
    // pose: clipe cujo último quadro vira a pose parada nessa forma (Damian segurando o canhão ou o martelo), enquanto não há idle próprio.
    // intangible: golpes inimigos não o acertam nessa forma (forma fantasma do Edgar).
    // dome/color (só visual): contorno neon das casas até dome casas do lutador nessa forma (−1 = domePct% do maior lado, 0 = 50%), na cor hex.
    // extra: +usos por turno de cada skill (Fúria do Death Crimson). endBlast: no fim do turno, dano fixo nas 8 casas em volta e nele.
    // locked: nada tira a forma (Nulification, selar ultimate). unstoppable: não é atordoado nem preso.
    // music: estilo da trilha eletrônica que toca enquanto alguém está nessa forma (ArenaMusica; Modos da L.E.L.A).
    [Serializable] public class FormDef { public string id, name, sprites, pose, color, music; public int pa, pm, armor, dome, domePct, extra, endBlast, reflectDamage; public bool noGuard, intangible, locked, unstoppable, effectImmune; }
    // Expansão de 22/10 (Nikola/Blip/Celinos, Bifia, Desland, Nepturius, tubarões): números das mecânicas novas, todos
    // editáveis pela ficha. Zero = "não configurado" (o núcleo usa o padrão do desenho técnico, ver Match.InventoresLuna).
    // family: grupo de autômatos (nikola, desland, nepturius, tubarao). techProtect: proteção tecnológica do Nikola e robôs.
    // invulnerable: não recebe dano nem habilidade de ninguém (esfera). waterOnly: só anda em água (tubarão).
    // survivesOwner: continua em campo quando o dono cai (robôs e esfera).
    // Engrenagens: gearStep (bônus a cada N), gearDamage/gearRange/gearPm/gearReveal (bônus por passo), gearDrops (por turno),
    // gearSpecial (% de drop especial), gearMorph (engrenagens da transformação), gearHits (marcadores do drop).
    // Bifia: vivaTurns/vivaRadius/vivaInit/vivaPm. Desland: ecoCooldown/cloneTurns/chessTurns/pieceDamage.
    // Nepturius: auraRange/auraPa/auraPm, grabCount/grabRange, blastRadius/blastDamage, cubeDamage/cubeHeal, armorBuff/armorTurns.
    // Robôs: healTurn (cura no começo do turno controlável), revealBase/shotRange/chainRange (Celinos), gearPmBase (PM do Blip).
    [Serializable] public class LunaDef
    {
        public string family;
        public bool techProtect, invulnerable, waterOnly, survivesOwner;
        public int gearStep=2, gearDamage=1, gearRange=1, gearPm, gearReveal, gearDrops=2, gearSpecial=20, gearMorph, gearHits=3, gearValue=1;
        public int vivaTurns=2, vivaRadius=6, vivaInit=1, vivaPm=2;
        public int ecoCooldown=2, cloneTurns=1, chessTurns=2, pieceDamage=1;
        public int auraRange=4, auraPa=2, auraPm=1, grabCount=4, grabRange=4, blastRadius=3, blastDamage=2, cubeDamage=2, cubeHeal=2, armorBuff=1, armorTurns=1;
        public int healTurn, revealBase=4, shotRange=3, chainRange=2, gearPmBase=4;
        public LunaDef Clone() => (LunaDef)MemberwiseClone();
    }
    [Serializable] public class FighterDef
    {
        public string id, name, sprites;
        public int hp, pa=6, pm=3, initiative, level;
        public bool flying;
        public SkillDef[] skills;
        // Passivas: allyBonus = dano a mais dos aliados até auraRange casas; revive = volta com esses PV no 1º nocaute;
        // guard = aliados até 2 casas levam 1 a menos e ele leva esse 1; steal = % de roubar 1 moeda ao acertar; illusion = começa com uma cópia ilusória.
        public int allyBonus, auraRange, revive, steal;
        public bool guard, illusion, picker=true;
        public int illusionCount=1;
        // armor: armadura fixa (Rei Bobett). legendary: 1 por time. immovable: não é empurrado, puxado nem trocado. devour: rouba a
        // skill de quem nocauteia (Colosso). noTurn: invocação sem turno (gatos, totem). noLock: não trava. neutral: time neutro.
        // healAura: no turno do dono, cura aliados até 2 casas.
        // size: lado em casas (Primal Beast 2 = ocupa 2×2; cell é o canto de menor x e z). 0 ou 1 = uma casa.
        public int armor, healAura, size;
        public bool legendary, immovable, devour, noTurn, noLock, neutral;
        // Elenco de 20/10. fieldPa/fieldRange: inimigos até fieldRange casas (diagonal incluída) perdem fieldPa PA (mín. 0) ao
        // começar o turno ou entrar; saindo no mesmo turno, recuperam (Nulification Field). meleeWeak: leva esse dano a mais de
        // golpes colados (Yami-yami). bloodScent: andar em direção a um inimigo com PV ≤ 50% custa metade dos PM. lifesteal: cura
        // ao acertar. bloodlust: PM a mais por acerto em inimigo com PV ≤ 50% (Daemion).
        public int fieldPa, fieldRange, meleeWeak, lifesteal, bloodlust;
        public bool bloodScent;
        // Elenco de 21/10. shadowMin/Max: sombras sorteadas no começo do turno (Ellie); shadowBonus: dano a mais em alvo numa sombra;
        // shadowDodge: % de errar golpe colado nele quando está numa sombra. duelBonus: dano a mais se ninguém além do alvo está até
        // duelRange casas dele ou do alvo. farImmune: imune a golpes de N+ casas. fullHitHeal: cura ao acertar sem o Escudo do
        // defensor. split/splitCount: ao cair, surgem essas réplicas. noReveal: não revela escondidos ao encostar (ilusão).
        // exposed: golpe de longe (2+ casas) não erra nem é bloqueado nele, e efeitos ruins e atordoamento sempre entram (Ellie).
        public int shadowMin, shadowMax, shadowBonus, shadowDodge, duelBonus, duelRange, farImmune, fullHitHeal, splitCount;
        public string split;
        public bool noReveal, exposed;
        // IA de neutro (21/10, mobs de evento): aggro = só caça quem está até N casas (0 = qualquer distância); roam = sem alvo,
        // anda para uma casa sorteada ao alcance.
        public int aggro;
        public bool roam;
        // 22/10. summonSlayer: qualquer dano dele numa invocação inimiga mata na hora (Pistoleira). hpByPlayers: PV máximo ao
        // surgir pelo número de jogadores da partida ([0] = 2 jogadores, [1] = 3...; o último vale para mais; vazio = hp).
        public bool summonSlayer;
        public int[] hpByPlayers;
        public string startForm;
        public FormDef[] forms;
        // Passivas do editor: valem enquanto o dono está vivo (ver PassiveDef).
        public PassiveDef[] passives=new PassiveDef[0];
        // Chefe com IA (Rei Thaagarys): null ou on=false = lutador comum.
        public BossDef boss;
        // Expansão de 22/10: mecânicas novas (família de autômatos, proteção tecnológica, esfera, tubarão).
        public LunaDef luna;
        public FighterDef Clone() => (FighterDef)MemberwiseClone();
    }
    // Passiva genérica (editor de fichas): muda um atributo de quem ela pega. who: Si, Aliados (sem ele), AliadosESi, Inimigos
    // (neutros contam), Todos (menos ele). range: até N casas do dono; 0 = o mapa todo. amount pode ser negativo (ex.: −1 armadura nos inimigos).
    // Dano: dano a mais nos golpes. Armadura: dano a menos recebido. PA/PM: a mais no começo do turno. Alcance: +alcance máximo das skills
    // de distância. CuraPorTurno/DanoPorTurno: no começo do turno de quem é pego.
    public enum PassiveStat { Dano, Armadura, PA, PM, Alcance, CuraPorTurno, DanoPorTurno }
    public enum PassiveWho { Si, Aliados, AliadosESi, Inimigos, Todos }
    [Serializable] public class PassiveDef { public string name; public PassiveStat stat; public PassiveWho who; public int amount=1, range;
        // lonely (Lobo solitário): só vale se o dono não tem aliado até N casas; form: só nessa forma do dono (vazio = sempre).
        public int lonely; public string form; public PassiveDef Clone() => (PassiveDef)MemberwiseClone(); }
    // Loja: Item e Buff valem na hora; Magia, Pet e Armadilha dão skills de uso limitado (cargas). uses negativo (Permanent): a skill
    // fica para sempre e só a recarga dela limita (Blink Dagger).
    [Serializable] public class ShopDef
    {
        public string id, name;
        public ShopKind kind;
        public int price, weight=1, pa, pm, hp, armor, range, turns, uses=1, thorns;
        public bool anchor, climb, focus;
        public SkillDef skill;
        public const int Permanent=-1;
    }
    // extra: +1 uso de cada skill por turno (Desejo C).
    // from: skill que causou (perda de PA/PM), só para a apresentação escolher o laço do estado.
    // dmg: dano a mais nos golpes; regen: cura no começo do turno (Pulso). id "sem-cura": não se cura até o fim do turno de from.
    // reflect: o próximo golpe que não mata volta em quem bateu. immune: imune a efeitos negativos. taken: multiplicador do dano dos golpes recebidos.
    [Serializable] public class Buff { public string id, from; public int turns, pa, pm, armor, range, extra, dmg, regen, reflect, immune, taken, takenDivisor; public bool inverted; }
    [Serializable] public class Charge { public string skill; public int uses; }
    [Serializable] public class TeamShop { public string[] slots=new string[0]; }
    // Fichas editadas pelo autor (Content.ApplyEdits).
    [Serializable] public class GameEdits { public List<FighterDef> fighters=new List<FighterDef>(); }
    [Serializable] public class Fighter : UnityEngine.ISerializationCallbackReceiver
    {
        public NumericRoll roll;
        public bool deathProcessed;
        public int id, team, hp, pa, pm, summoner, bonusPa, bonusPm, bonusHp, armor, bonusRange;
        public string def;
        public Cell cell;
        public bool bought;
        // form/formTurns: forma atual. disguise: como os inimigos veem a ilusão. lastSkill: última skill usada (Espelho primordial).
        public string form, disguise, lastSkill, lastAttack;
        public int formTurns;
        public bool hidden, revived;
        // prisoned: turnos preso. channel: turnos de canalização do Desejo; wishReady: escolhe a opção. carried: moedas comidas
        // (papa-moedas). ai: contador da IA neutra. Itens: anchored (âncora), climber (gancho), focused (cristal), thorns (manto).
        public int prisoned, channel, carried, ai, thorns;
        public bool wishReady, anchored, climber, focused, focusUsed;
        // Aura do Baltazar: ligada e já paga neste turno (desligar e religar no mesmo turno não cobra nem aplica de novo).
        public bool auraOn, auraPaid;
        // Skill canalizada (SkillDef.channeled): "skill;x;z;x2;z2;slot", resolve no começo do próximo turno (vazio = nenhuma).
        public string channeling;
        public int lastDamageBy, inversionTurns, moveBudget;
        public bool recruitedBoss, noLock, noReveal;
        public int banishBy; // Fonte do dano adiado, preservada no snapshot.
        // marks: Marcas da Maldição (Gumbald), ficam até a Cat-a-Clisma. drained: PA tirados pelo Nulification Field neste turno.
        // banished/banishDamage: no buraco negro (prisoned conta os turnos); dano ao voltar. usedForm: skills usadas nesta transformação.
        // Na rede viajam juntos em ext ("marcas.anulados.dano.banido.skill|skill"; vazio = tudo zerado) para o snapshot de 6
        // jogadores caber no Relay.
        // stunned: turnos atordoado (perde o turno, mas pode ser alvo).
        [NonSerialized] public int marks, drained, banishDamage, stunned;
        [NonSerialized] public bool banished;
        [NonSerialized] public List<string> usedForm=new List<string>();
        public string ext;
        public void OnBeforeSerialize() => ext=marks==0&&drained==0&&banishDamage==0&&!banished&&stunned==0&&(usedForm==null||usedForm.Count==0)?""
            :$"{marks}.{drained}.{banishDamage}.{(banished?1:0)}.{string.Join("|",usedForm??new List<string>())}.{stunned}";
        public void OnAfterDeserialize()
        {
            marks=drained=banishDamage=stunned=0; banished=false; usedForm=new List<string>();
            var p=(ext??"").Split('.'); if(p.Length<5) return;
            int.TryParse(p[0],out marks); int.TryParse(p[1],out drained); int.TryParse(p[2],out banishDamage); banished=p[3]=="1";
            usedForm.AddRange(p[4].Split(new[]{'|'},StringSplitOptions.RemoveEmptyEntries));
            if(p.Length>5) int.TryParse(p[5],out stunned);
        }
        // stolen: skills devoradas (Colosso, +1 PA). learned: skills copiadas pelo Desejo.
        public List<string> stolen=new List<string>();
        public List<SkillRoll> stolenPowers=new List<SkillRoll>();
        public List<string> learned=new List<string>();
        // Expansão de 22/10: remoteShop (loja remota do comando-blip neste turno), initBonus (iniciativa efetiva do Viva Luna,
        // sem mexer na ficha; sobrevive ao snapshot) e endedTurn (skill que encerra o turno na hora, só no comando em curso).
        public bool remoteShop;
        public int initBonus;
        [NonSerialized] public bool endedTurn;
        public List<string> usedMatch=new List<string>();
        public List<string> used=new List<string>();
        public List<Charge> charges=new List<Charge>();
        // Recargas: skill e turnos que faltam (uses).
        public List<Charge> cooldowns=new List<Charge>();
        public List<Buff> buffs=new List<Buff>();
        public bool Alive => hp>0;
    }
    // Ciclo de jogadores-alvo de cada chefe automático (Boss.cs): assento = Fighter.team. order: a ordem sorteada do ciclo
    // em andamento (a anterior até o próximo sorteio); pending: assentos ainda não atacados nele (oculto/preso espera a vez).
    // Fica no MatchState para sobreviver a save/reconnect.
    [Serializable] public class BossCycle
    {
        public int boss;
        public List<int> order=new List<int>(), pending=new List<int>();
    }
    // Jogador (assento 1..6): controla lutadores, tem moedas e loja próprias. Time: divide Galeões e vitória (0 = time próprio).
    [Serializable] public class PlayerSetup { public string[] fighters={"ciantia"}; public int team; }
    // Resultado compacto do sorteio do Host; nunca altera as fichas globais ou o Editor.
    [Serializable] public class SkillRoll { public string id; public int damage, range; }
    [Serializable] public class NumericRoll
    {
        public bool stats, skills;
        public int hp, pa, pm, initiative;
        public SkillRoll[] powers=new SkillRoll[0];
    }
    [Serializable] public class MatchSetup
    {
        public uint seed=1;
        public VictoryMode mode=VictoryMode.Galeoes;
        public int galeoes=12, target;
        public bool flatMap;
        // Começa com a fase de posicionamento (cada jogador arruma os heróis nas suas casas de início).
        public bool placement;
        public bool randomSkills, randomStats, randomBots, randomPlayers;
        public int[] botSeats=new int[0];
        // Cenário (MapGen.Themes); -1 = sorteado pela seed, igual em todos os aparelhos.
        public int theme=-1;
        // Vazio = duelo com team1 e team2.
        public PlayerSetup[] players;
        // Mapa feito no editor (aba Mapas); null ou de outro tamanho = mapa sorteado.
        public MapDef map;
        public TurnMode turns=TurnMode.Completo;
        // Itens que podem sair na loja desta sala (ids de Shop.Catalog); vazio = todos.
        public string[] shopItems;
        // Ilusão bloqueia casas (colisão); desligado, dá para atravessar a ilusão (sem parar nela).
        public bool solidIllusions=true;
        public string[] team1={"ciantia"}, team2={"ciantia"};
        // Batalha contra o chefe (Boss.cs): id da ficha do chefe; vazio = partida comum.
        public string boss="";
    }
    // Mapa do editor: relevo, peças (árvore, arbusto, caixa) e zona central de cada casa. width = Board.SizeFor(jogadores).
    // theme -1 = cenário sorteado. As casas de início ficam sempre livres (MapGen.Load).
    [Serializable] public class MapDef { public string name=""; public int width=Board.Size, theme=-1; public Tile[] tiles; public List<Decor> decor=new List<Decor>(); }
    // Peça do kit Blender do Codex (Models/Mapa/<cenário>/<piece>) em coordenadas de casa (x,z podem ter fração); rot em graus.
    // Só visual. Numa casa com árvore/arbusto/caixa, a peça do mesmo tipo (MapGen.SkinTerrain) troca o modelo padrão;
    // as demais (enfeite_*, relicario_solar...) são enfeites por cima do piso.
    [Serializable] public class Decor { public string piece=""; public float x, z, rot, scale=1; }
    // Expansão de 22/10: estado visível das engrenagens, da caixa da matriz, das marcas do Celinos, dos clones do Desland,
    // do domínio de xadrez, do Viva Luna, da esfera do Nepturius e dos tubarões. Fica no MatchState para sobreviver ao
    // snapshot/reconnect; não guarda RNG nem destino de invisível.
    // Gear: engrenagem no chão (cell = Board.Index, owner = Fighter.id do dono da família, value = 1 ou 2, hits = marcadores).
    [Serializable] public class Gear { public int cell, owner, value, hits; }
    // família: contador de engrenagens compartilhado (owner = Fighter.id do Nikola; os robôs leem mesmo com ele morto).
    [Serializable] public class LunaFamily { public int owner, gears; }
    // Marca do Celinos: casa em que o alvo foi marcado (trocar de casa ou morrer desfaz a marca).
    [Serializable] public class LunaMark { public int by, id, x, z; }
    // Clone do eco-de-aliado: turns = turnos do Desland que faltam para o clone sumir.
    [Serializable] public class LunaClone { public int id, by, turns; }
    [Serializable] public class LunaSeen { public int team, id; }
    // Caixa da matriz: by = Nikola, hp = PV da caixa, cell = Board.Index, turns = turnos do Nikola que faltam para expirar.
    [Serializable] public class LunaBox { public int by, hp, cell, turns; }
    // Viva Luna: centro e raio do efeito, turnos da Bifia que faltam, quem foi pego e o bônus de iniciativa aplicado.
    [Serializable] public class LunaField { public int by, center, radius, turns, init; public List<int> affected=new List<int>(); public List<int> enemies=new List<int>(); }
    [Serializable] public class LunaMachine { public int by, chessBy, chessTurns, sphere, sphereBy; public bool sphereUsed,sphereCrazy,sphereMad,chessFoe; public LunaBox box=new LunaBox(); public List<int> chess=new List<int>(); }
    [Serializable] public class LunaState
    {
        public List<LunaMachine> machines=new List<LunaMachine>();
        public List<Gear> gears=new List<Gear>();
        public List<LunaFamily> families=new List<LunaFamily>();
        public List<LunaMark> marks=new List<LunaMark>();
        public List<LunaClone> clones=new List<LunaClone>();
        public List<LunaField> fields=new List<LunaField>();
        public LunaBox box=new LunaBox();
        // Domínio de xadrez: dono, turnos que faltam e as peças invocadas (Fighter.id).
        public int chessBy, chessTurns;
        public List<int> chess=new List<int>();
        // Esfera do Nepturius: id, dono, habilidade de esfera usada neste turno, loucura negra neste turno e esfera órfã.
        public int sphere, sphereBy;
        public bool sphereUsed, sphereCrazy, sphereMad;
        // Direito de escolha do eco-de-aliado (ids do Desland que mataram um inimigo) e clones por id.
        public List<int> rights=new List<int>();
        // Radar do Celinos (autor, 23/10): escondido no raio fica visível só para o time do robô (team = assento do robô).
        public List<LunaSeen> radar=new List<LunaSeen>();
        public LunaState Clone() => (LunaState)MemberwiseClone();
    }
    [Serializable] public class MatchState
    {
        // Neutral: time dos neutros dos eventos (papa-moedas, Brutamontes Errante); não tem moedas, loja nem Galeões.
        public const int MaxPlayers=6, Neutral=7;
        public int width=Board.Size, height=Board.Size, round=1, active, revision=1, winner, nextId=1, target, players=2;
        public uint rng;
        // Só visual: paleta e enfeites do cenário (MapGen.Themes).
        public int theme;
        // Labirinto (cenário fechado): névoa (cada time só vê as casas coladas nos seus lutadores, Fog) até a vela ser acesa;
        // boss: o evento do boss já saiu nesta partida (sai uma vez só).
        public bool fog, revealed, boss;
        // Ilusões ocupam a casa para o movimento (MatchSetup.solidIllusions).
        public bool solidIllusions=true;
        // Névoa (Fog): casas que cada time (alliance) já viu, em hexadecimal (4 casas por letra); índice = time.
        public string[] explored=new string[0];
        // Espetos do labirinto que já saíram do chão (Board.Index): só eles aparecem (autor, 18/10: ocultos até pisar).
        public List<int> sprung=new List<int>();
        // Sala de início de cada grupo de spawn no labirinto: x0,z0,x1,z1 por slot (vazio = casas fixas de MapGen.Spawns).
        public int[] rooms=new int[0];
        // Salas do labirinto (início, salão do meio e extras): x0,z0,x1,z1 cada; na névoa, quem está numa vê a sala inteira.
        public int[] areas=new int[0];
        // Casas compactadas só para a rede (Pack): o snapshot de 42×42 não cabia nos 64 KB do WebSocket.
        public string packed;
        // Mapa com paredes (labirinto).
        public bool Walled() { foreach(var t in tiles) if(t.terrain==Terrain.Parede) return true; return false; }
        // Mesmo estado com outras casas (o que um jogador conhece na névoa); listas e lutadores são os mesmos.
        public MatchState With(Tile[] other) { var c=(MatchState)MemberwiseClone(); c.tiles=other; return c; }
        // Casa por casa: terreno.relevo.objeto.marcas.moedas.armadilha.dano.pa.gelo.sombra, sem os zeros do fim; casa vazia = "".
        public static string Pack(Tile[] tiles)
        {
            var sb=new System.Text.StringBuilder(); var v=new int[11];
            for(int i=0;i<tiles.Length;i++) {
                var t=tiles[i]; if(i>0) sb.Append(',');
                v[0]=(int)t.terrain; v[1]=t.height; v[2]=t.obj; v[3]=(t.zone?1:0)|(t.lava?2:0)|(t.mark?4:0); v[4]=t.coins; v[5]=t.trap; v[6]=t.trapDamage; v[7]=t.trapPa; v[8]=t.frozenBy; v[9]=t.shadow; v[10]=t.trapBy;
                int n=v.Length; while(n>0&&v[n-1]==0) n--;
                for(int k=0;k<n;k++) { if(k>0) sb.Append('.'); sb.Append(v[k]); }
            }
            return sb.ToString();
        }
        public static Tile[] Unpack(string packed,int count)
        {
            var parts=(packed??"").Split(','); if(parts.Length!=count) return null;
            var tiles=new Tile[count];
            for(int i=0;i<count;i++) {
                var v=new int[11]; var p=parts[i].Length==0?new string[0]:parts[i].Split('.');
                for(int k=0;k<p.Length&&k<v.Length;k++) if(!int.TryParse(p[k],out v[k])) return null;
                tiles[i]=new Tile{ terrain=(Terrain)v[0], height=v[1], obj=v[2], zone=(v[3]&1)!=0, lava=(v[3]&2)!=0, mark=(v[3]&4)!=0, coins=v[4], trap=v[5], trapDamage=v[6], trapPa=v[7], frozenBy=v[8], shadow=v[9], trapBy=v[10] };
            }
            return tiles;
        }
        public bool over;
        // Fase de posicionamento (Sprint 1.3): assento que está arrumando os heróis nas casas de início; 0 = partida em curso.
        public int placing;
        public VictoryMode mode;
        public TurnMode turnMode;
        // Itens liberados na loja (vazio = catálogo inteiro).
        public string[] shopPool=new string[0];
        // Evento com duração ativo (EVENTOS-LOJA-ELENCO.md): id e rodadas que faltam; vazio = nenhum.
        public string ev="";
        public int evRounds;
        // Edições do autor em vigor nesta partida (as do Host valem para todos).
        public string edits="";
        public Tile[] tiles;
        // Enfeites e modelos do mapa (só visual, iguais em todos os aparelhos).
        public List<Decor> decor=new List<Decor>();
        // Minas terrestres (Board.Index das casas com trap que não param o movimento) e portais abertos.
        public List<int> mines=new List<int>();
        public List<Portal> portals=new List<Portal>();
        // Paredes erguidas por skill (SkillDef.walls): Board.Index*16 + rodadas que faltam; no fim voltam a ser chão livre.
        public List<int> walls=new List<int>();
        // Batalha contra o chefe: id da ficha (MatchSetup.boss); vazio = partida comum.
        public string bossFight="";
        public int[] bossAllyHp=new int[MaxPlayers+1];
        // Um ciclo de jogadores-alvo por chefe automático (BossCycle, Boss.cs); serializado junto do resto do estado.
        public List<BossCycle> bossCycles=new List<BossCycle>();
        // Expansão de 22/10 (Match.InventoresLuna.cs): engrenagens, caixa da matriz, marcas, clones, xadrez, Viva Luna, esfera.
        public LunaState luna=new LunaState();
        public LunaState Luna => luna??=new LunaState();
        public List<Fighter> fighters=new List<Fighter>();
        public List<int> timeline=new List<int>();
        // Índice 0 sem uso. Fighter.team, coins e shops são por jogador; galeoes, collected, domain e eliminated por time (alliance[jogador]).
        public int[] alliance={0,1,2,3,4,5,6}, galeoes=new int[MaxPlayers+1], coins=new int[MaxPlayers+1], collected=new int[MaxPlayers+1], domain=new int[MaxPlayers+1];
        public bool[] eliminated=new bool[MaxPlayers+1];
        public TeamShop[] shops=Shops();
        static TeamShop[] Shops() { var a=new TeamShop[MaxPlayers+1]; for(int i=0;i<a.Length;i++) a[i]=new TeamShop(); return a; }
        public Tile At(Cell c) => tiles[c.z*width+c.x];
        public Fighter F(int id) => fighters.Find(f=>f.id==id);
        public Fighter Active => over||placing>0||timeline.Count==0?null:F(timeline[active]);
        public Cell Mirror(Cell c) => new Cell(width-1-c.x,height-1-c.z);
        public int Team(int player) => player>=1&&player<=MaxPlayers?alliance[player]:0;
        public bool Ally(int a,int b) => a==b||(Team(a)!=0&&Team(a)==Team(b));
        public static bool Player(int team) => team>=1&&team<=MaxPlayers;
        // Times ainda na partida, na ordem dos assentos.
        public List<int> Teams() { var list=new List<int>(); for(int p=1;p<=players;p++) if(!list.Contains(alliance[p])) list.Add(alliance[p]); return list; }
        // Times de verdade (3v3, 2v2v2) ou todos contra todos.
        public bool TeamPlay => Teams().Count<players;
    }
    [Serializable] public class Command
    {
        public string kind, skill;
        public int revision, fighter, slot;
        public Cell cell;
        // 2ª casa: destino do aliado no Teleporte, 2º alvo da Troca, saída do Portal.
        public Cell cell2;
    }
    // kind: turno, mover, trava, moeda, armadilha, armadilha-posta, compra, loja, cura, ataque, cataclisma, invocar, nocaute, eliminado, rodada, dominio, vitoria
    [Serializable] public class GameEvent
    {
        public string kind, info;
        public int actor, target, value;
        public Cell cell;
        public Cell aim=Cell.Unknown;
        public Cell[] path;
        public Face[] dice;
    }

    public static class Dice
    {
        public static uint Next(MatchState s) { uint x=s.rng==0?0x9E3779B9u:s.rng; x^=x<<13; x^=x>>17; x^=x<<5; s.rng=x; return x; }
        public static int Range(MatchState s,int n) => (int)(Next(s)%(uint)n);
        public static Face Roll(MatchState s) => (Face)Range(s,6);
        public static bool Lock(Face f) => f==Face.Trava||f==Face.CoringaEsquivaTrava||f==Face.Coringa;
        public static bool Dodge(Face f) => f==Face.Esquiva||f==Face.CoringaEsquivaTrava||f==Face.Coringa;
        public static bool Armor(Face f) => f==Face.Escudo||f==Face.Coringa;
    }
}
