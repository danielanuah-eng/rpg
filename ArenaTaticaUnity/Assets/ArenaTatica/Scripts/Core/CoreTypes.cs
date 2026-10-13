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
    public enum Terrain { Livre, Arbusto, Arvore, Caixa, Loja }
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
    public enum SkillKind { Dano, Invocacao, Cura, Armadilha, Teleporte, Salto, Forma, Esconder, Espelho, Suporte, Zona, Comando, Reviver, Desejo, Aura }
    public enum ShopKind { Item, Magia, Pet, Armadilha, Buff }

    // lava/mark: eventos (maré de lava, fogos). frozenBy: lutador que congelou a casa (+1 PM para entrar). trapPa: mina de gelo.
    [Serializable] public class Tile { public int height, coins, trap, trapDamage, trapPa, frozenBy; public Terrain terrain; public bool zone, lava, mark; }

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
        public SkillDef Clone() => (SkillDef)MemberwiseClone();
    }
    // Forma do lutador (arma do Damian, Berserk do Tchu tcha): bônus enquanto dura e a pasta de sprites dela.
    // pose: clipe cujo último quadro vira a pose parada nessa forma (Damian segurando o canhão ou o martelo), enquanto não há idle próprio.
    // intangible: golpes inimigos não o acertam nessa forma (forma fantasma do Edgar).
    [Serializable] public class FormDef { public string id, name, sprites, pose; public int pa, pm, armor; public bool noGuard, intangible; }
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
        // armor: armadura fixa (Rei Bobett). legendary: 1 por time. immovable: não é empurrado, puxado nem trocado. devour: rouba a
        // skill de quem nocauteia (Colosso). noTurn: invocação sem turno (gatos, totem). noLock: não trava. neutral: time neutro.
        // healAura: no turno do dono, cura aliados até 2 casas.
        public int armor, healAura;
        public bool legendary, immovable, devour, noTurn, noLock, neutral;
        public string startForm;
        public FormDef[] forms;
        // Passivas do editor: valem enquanto o dono está vivo (ver PassiveDef).
        public PassiveDef[] passives=new PassiveDef[0];
    }
    // Passiva genérica (editor de fichas): muda um atributo de quem ela pega. who: Si, Aliados (sem ele), AliadosESi, Inimigos
    // (neutros contam), Todos (menos ele). range: até N casas do dono; 0 = o mapa todo. amount pode ser negativo (ex.: −1 armadura nos inimigos).
    // Dano: dano a mais nos golpes. Armadura: dano a menos recebido. PA/PM: a mais no começo do turno. Alcance: +alcance máximo das skills
    // de distância. CuraPorTurno/DanoPorTurno: no começo do turno de quem é pego.
    public enum PassiveStat { Dano, Armadura, PA, PM, Alcance, CuraPorTurno, DanoPorTurno }
    public enum PassiveWho { Si, Aliados, AliadosESi, Inimigos, Todos }
    [Serializable] public class PassiveDef { public string name; public PassiveStat stat; public PassiveWho who; public int amount=1, range; public PassiveDef Clone() => (PassiveDef)MemberwiseClone(); }
    // Loja: Item e Buff valem na hora; Magia, Pet e Armadilha dão skills de uso limitado (cargas).
    [Serializable] public class ShopDef
    {
        public string id, name;
        public ShopKind kind;
        public int price, weight=1, pa, pm, hp, armor, range, turns, uses=1, thorns;
        public bool anchor, climb, focus;
        public SkillDef skill;
    }
    // extra: +1 uso de cada skill por turno (Desejo C).
    // from: skill que causou (perda de PA/PM), só para a apresentação escolher o laço do estado.
    [Serializable] public class Buff { public string id, from; public int turns, pa, pm, armor, range, extra; }
    [Serializable] public class Charge { public string skill; public int uses; }
    [Serializable] public class TeamShop { public string[] slots=new string[0]; }
    // Fichas editadas pelo autor (Content.ApplyEdits).
    [Serializable] public class GameEdits { public List<FighterDef> fighters=new List<FighterDef>(); }
    [Serializable] public class Fighter
    {
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
        // stolen: skills devoradas (Colosso, +1 PA). learned: skills copiadas pelo Desejo.
        public List<string> stolen=new List<string>();
        public List<string> learned=new List<string>();
        public List<string> usedMatch=new List<string>();
        public List<string> used=new List<string>();
        public List<Charge> charges=new List<Charge>();
        // Recargas: skill e turnos que faltam (uses).
        public List<Charge> cooldowns=new List<Charge>();
        public List<Buff> buffs=new List<Buff>();
        public bool Alive => hp>0;
    }
    // Jogador (assento 1..6): controla lutadores, tem moedas e loja próprias. Time: divide Galeões e vitória (0 = time próprio).
    [Serializable] public class PlayerSetup { public string[] fighters={"ciantia"}; public int team; }
    [Serializable] public class MatchSetup
    {
        public uint seed=1;
        public VictoryMode mode=VictoryMode.Galeoes;
        public int galeoes=12, target;
        public bool flatMap;
        // Cenário (MapGen.Themes); -1 = sorteado pela seed, igual em todos os aparelhos.
        public int theme=-1;
        // Vazio = duelo com team1 e team2.
        public PlayerSetup[] players;
        // Mapa feito no editor (aba Mapas); null ou de outro tamanho = mapa sorteado.
        public MapDef map;
        public TurnMode turns=TurnMode.Completo;
        // Itens que podem sair na loja desta sala (ids de Shop.Catalog); vazio = todos.
        public string[] shopItems;
        public string[] team1={"ciantia"}, team2={"ciantia"};
    }
    // Mapa do editor: relevo, peças (árvore, arbusto, caixa) e zona central de cada casa. width = Board.SizeFor(jogadores).
    // theme -1 = cenário sorteado. As casas de início ficam sempre livres (MapGen.Load).
    [Serializable] public class MapDef { public string name=""; public int width=Board.Size, theme=-1; public Tile[] tiles; public List<Decor> decor=new List<Decor>(); }
    // Peça do kit Blender do Codex (Models/Mapa/<cenário>/<piece>) em coordenadas de casa (x,z podem ter fração); rot em graus.
    // Só visual. Numa casa com árvore/arbusto/caixa, a peça do mesmo tipo (MapGen.SkinTerrain) troca o modelo padrão;
    // as demais (enfeite_*, relicario_solar...) são enfeites por cima do piso.
    [Serializable] public class Decor { public string piece=""; public float x, z, rot, scale=1; }
    [Serializable] public class MatchState
    {
        // Neutral: time dos neutros dos eventos (papa-moedas, Brutamontes Errante); não tem moedas, loja nem Galeões.
        public const int MaxPlayers=6, Neutral=7;
        public int width=Board.Size, height=Board.Size, round=1, active, revision=1, winner, nextId=1, target, players=2;
        public uint rng;
        // Só visual: paleta e enfeites do cenário (MapGen.Themes).
        public int theme;
        public bool over;
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
        public List<Fighter> fighters=new List<Fighter>();
        public List<int> timeline=new List<int>();
        // Índice 0 sem uso. Fighter.team, coins e shops são por jogador; galeoes, collected, domain e eliminated por time (alliance[jogador]).
        public int[] alliance={0,1,2,3,4,5,6}, galeoes=new int[MaxPlayers+1], coins=new int[MaxPlayers+1], collected=new int[MaxPlayers+1], domain=new int[MaxPlayers+1];
        public bool[] eliminated=new bool[MaxPlayers+1];
        public TeamShop[] shops=Shops();
        static TeamShop[] Shops() { var a=new TeamShop[MaxPlayers+1]; for(int i=0;i<a.Length;i++) a[i]=new TeamShop(); return a; }
        public Tile At(Cell c) => tiles[c.z*width+c.x];
        public Fighter F(int id) => fighters.Find(f=>f.id==id);
        public Fighter Active => over||timeline.Count==0?null:F(timeline[active]);
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
    }
    // kind: turno, mover, trava, moeda, armadilha, armadilha-posta, compra, loja, cura, ataque, cataclisma, invocar, nocaute, eliminado, rodada, dominio, vitoria
    [Serializable] public class GameEvent
    {
        public string kind, info;
        public int actor, target, value;
        public Cell cell;
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
