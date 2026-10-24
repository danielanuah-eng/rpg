using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Números iniciais; o balanceamento vem depois. Kits do elenco descritos pelo autor em 10/10 (DESIGN-KROSMASTER.md, passo 7).
    public static partial class Content
    {
        public static readonly Dictionary<string,FighterDef> Fighters=new Dictionary<string,FighterDef>
        {
            // Somnax: ficha do autor, escala regular 1x1 e uma lendária por time.
            ["somnax"]=new FighterDef { id="somnax", name="Somnax", sprites="Somnax", hp=11, pa=6, pm=4, initiative=6, level=5, armor=0, size=1, legendary=true, skills=new[] {
                new SkillDef { id="pacto", name="Pacto", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2, precise=true, perTurn=2, anim="pacto" },
                new SkillDef { id="renuncia", name="Renúncia", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2, pierce=true, perTurn=1, anim="renuncia" },
                new SkillDef { id="passo-do-crepusculo", name="Passo do Crepúsculo", kind=SkillKind.Teleporte, pa=2, minRange=1, maxRange=3, cooldown=1, sight=true, anim="blinkout" },
                new SkillDef { id="doce-e-salgado", name="Doce e Salgado", kind=SkillKind.DoceSalgado, pa=5, minRange=0, maxRange=2, heal=2, perMatch=1, twoCells=true, hits=(int)(Hit.Si|Hit.Aliado), anim="support" },
                new SkillDef { id="ruptura-das-memorias", name="Ruptura das Memórias", kind=SkillKind.Dano, pa=5, minRange=1, maxRange=3, area=1, square=true, foes=true, damage=5, perMatch=1, ultimate=true, anim="ult" } } },
            ["ciantia"]=new FighterDef { id="ciantia", name="Ciantia", sprites="Ciantia", hp=11, pa=6, pm=3, initiative=5, level=4, skills=new[] {
                new SkillDef { id="dragao-dourado", name="Dragão Dourado", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=4, damage=2, line=true, modifiable=true },
                new SkillDef { id="invocacao-dragao", name="Invocação de dragão", kind=SkillKind.Invocacao, pa=4, minRange=1, maxRange=2, perTurn=1, sight=false, summon="dragao-espectral", anim="summon" } } },
            // Guerreira de espada rosa: corpo a corpo, resistente na medida; aliados perto dela batem mais forte.
            ["talyssa"]=new FighterDef { id="talyssa", name="Talyssa", sprites="Talyssa", hp=12, pa=6, pm=3, initiative=4, level=4, allyBonus=1, auraRange=2, skills=new[] {
                new SkillDef { id="lamina-rosa", name="Lâmina rosa", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="corte-em-arco", name="Corte em arco", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=2, area=1, perTurn=1, anim="attack" } } },
            // Foice: acerta colado ou a 2 casas, anda muito e se teleporta girando a foice.
            ["teantha"]=new FighterDef { id="teantha", name="Teantha", sprites="Teantha", hp=10, pa=6, pm=4, initiative=6, level=3, skills=new[] {
                new SkillDef { id="foice-verde", name="Foice verde", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=2, damage=3 },
                new SkillDef { id="giro-sombrio", name="Giro sombrio", kind=SkillKind.Teleporte, pa=2, minRange=1, maxRange=5, perTurn=1, sight=false, anim="teleport" } } },
            // Bruto de lâmina necrótica: golpe pesado; no primeiro nocaute ele renasce.
            ["devon"]=new FighterDef { id="devon", name="Devon", sprites="Devon", hp=13, pa=6, pm=3, initiative=2, level=5, revive=5, skills=new[] {
                new SkillDef { id="golpe-necrotico", name="Golpe necrótico", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4 } } },
            // Esgrimista: salta para junto do inimigo e se cura ao acertar.
            ["diantus"]=new FighterDef { id="diantus", name="Diantus", sprites="Diantus", hp=11, pa=6, pm=3, initiative=6, level=4, skills=new[] {
                new SkillDef { id="estocada", name="Estocada flamejante", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2, drain=1 },
                new SkillDef { id="salto-do-duelista", name="Salto do duelista", kind=SkillKind.Salto, pa=2, minRange=2, maxRange=4, perTurn=1, sight=true, anim="leap" } } },
            // Pistoleiro: o camaleão vira pistola (precisa, armadura normal), canhão (área, impreciso, armadura menor) ou martelo (perto, armadura maior).
            ["damian"]=new FighterDef { id="damian", name="Damian", sprites="Damian", hp=10, pa=6, pm=4, initiative=5, level=4, legendary=true, startForm="pistola",
                forms=new[] {
                    new FormDef { id="pistola", name="Pistola", armor=0 },
                    new FormDef { id="canhao", name="Canhão de mão", armor=-1, pose="cannon" },
                    new FormDef { id="martelo", name="Martelo", armor=2, pose="hammer" } },
                skills=new[] {
                    new SkillDef { id="tiro-de-pistola", name="Tiro de pistola", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=5, damage=2, precise=true, modifiable=true, form="pistola", anim="pistol" },
                    new SkillDef { id="canhao-de-mao", name="Canhão de mão", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=4, damage=3, area=1, wild=true, form="canhao", anim="cannon" },
                    new SkillDef { id="martelada", name="Martelada", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4, form="martelo", anim="hammer" },
                    new SkillDef { id="camaleao-pistola", name="Camaleão: pistola", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="pistola", anim="morph", freeSwap=true },
                    new SkillDef { id="camaleao-canhao", name="Camaleão: canhão", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="canhao", anim="morph", freeSwap=true },
                    new SkillDef { id="camaleao-martelo", name="Camaleão: martelo", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="martelo", anim="morph", freeSwap=true } } },
            // Ezequiela: duas ilusões iniciais de 1 PV; espelho inverte efeitos ativos e novos.
            ["ezequiela"]=new FighterDef { id="ezequiela", name="Ezequiela", sprites="Ezequiela", hp=6, pa=6, pm=3, initiative=4, level=3, illusion=true, illusionCount=2, skills=new[] {
                new SkillDef { id="ilusao-confusa", name="Ilusão confusa", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=2, pmLoss=2, modifiable=true },
                new SkillDef { id="espelho-primordial", name="Espelho primordial", kind=SkillKind.Suporte, pa=3, minRange=1, maxRange=6, cooldown=2, invertTurns=2, hits=(int)(Hit.Aliado|Hit.Inimigo), sight=false, anim="mirror" },
                new SkillDef { id="copia-ezequiela", name="Cópia", kind=SkillKind.Invocacao, summon="ilusao", pa=2, minRange=1, maxRange=2, maxActive=2, perMatch=3, perTurn=1, anim="summon" },
                new SkillDef { id="medo-ezequiela", name="Fear", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=2, damage=0, push=4, perMatch=1, anim="fear" },
                new SkillDef { id="portal-ezequiela", name="Portal", kind=SkillKind.Portal, pa=2, minRange=0, maxRange=6, sight=false, minRound=2, perMatch=1, ultimate=true, turns=2, anim="portal" } } },
            ["ilusao"]=new FighterDef { id="ilusao", name="Ilusão", sprites="Ezequiela", hp=4, pa=0, pm=3, level=0, picker=false, noReveal=true, skills=new SkillDef[0] },
            // Viking tanque: protege aliados perto; o Olho abissal o transforma por 3 turnos numa máquina de combate sem proteção, que pode errar.
            ["tchu-tcha"]=new FighterDef { id="tchu-tcha", name="Tchu tcha", sprites="TchuTcha", hp=15, pa=6, pm=2, initiative=1, level=5, guard=true, startForm="normal",
                forms=new[] {
                    new FormDef { id="normal", name="Normal", armor=1 },
                    new FormDef { id="berserk", name="Berserk", sprites="TchuTchaBerserk", pa=2, pm=2, armor=0, noGuard=true, effectImmune=true, reflectDamage=1 } },
                skills=new[] {
                    new SkillDef { id="guarda-direcional", name="Guarda direcional", kind=SkillKind.GuardaDirecional, pa=0, minRange=0, maxRange=1, sight=false, area=2, buffArmor=1, anim="idle", form="normal" },
                    new SkillDef { id="escudada", name="Escudada", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, pmLoss=1, form="normal" },
                    new SkillDef { id="olho-abissal", name="Olho abissal", kind=SkillKind.Forma, pa=2, minRange=0, maxRange=0, sight=false, toForm="berserk", turns=3, perMatch=1, ultimate=true, form="normal", anim="transform" },
                    new SkillDef { id="furia-negra", name="Fúria negra", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=5, wild=true, missPct=15, form="berserk" } } },
            // Anjo caído: tiro carregado de longe; a ultimate abre as asas e prepara uma flecha devastadora.
            ["arqueiro-jack"]=new FighterDef { id="arqueiro-jack", name="Arqueiro Jack", sprites="ArqueiroJack", hp=9, pa=6, pm=4, initiative=7, level=4, startForm="capuz",
                forms=new[] { new FormDef { id="capuz", name="Capuz" }, new FormDef { id="asas", name="Asas abertas", armor=-1 } },
                skills=new[] {
                    new SkillDef { id="tiro-carregado", name="Tiro carregado", kind=SkillKind.Dano, pa=4, minRange=3, maxRange=7, damage=3, perTurn=1, modifiable=true },
                    new SkillDef { id="asas-abissais", name="Asas abissais", kind=SkillKind.Forma, pa=2, minRange=0, maxRange=0, sight=false, toForm="asas", turns=2, perMatch=1, ultimate=true, form="capuz", anim="ultcharge" },
                    new SkillDef { id="flecha-abissal", name="Flecha abissal", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=8, damage=7, line=true, endsForm=true, perTurn=1, ultimate=true, form="asas", anim="ultshot" } } },
            // Halfling ladino: mais PA e PM, se esconde junto a relevo; acertos podem roubar moedas (mais forte se estava escondido).
            ["perry"]=new FighterDef { id="perry", name="Perry", sprites="Perry", hp=8, pa=7, pm=4, initiative=8, level=3, steal=20, skills=new[] {
                new SkillDef { id="adagas-gemeas", name="Adagas gêmeas", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2, perTurn=2 },
                new SkillDef { id="esconder", name="Esconder-se", kind=SkillKind.Esconder, pa=1, minRange=0, maxRange=0, sight=false, perTurn=1, anim="hide" } } },
            ["dragao-espectral"]=new FighterDef { id="dragao-espectral", name="Dragão espectral", sprites="DragaoEspectral", hp=6, pa=4, pm=4, level=0, flying=true, picker=false, skills=new[] {
                new SkillDef { id="baforada", name="Baforada estelar", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=2, damage=2, line=true } } },
            ["lobo-de-cinzas"]=new FighterDef { id="lobo-de-cinzas", name="Lobo de cinzas", sprites="LoboCinzas", hp=3, pa=3, pm=3, level=0, picker=false, skills=new[] {
                new SkillDef { id="mordida", name="Mordida", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=1 } } },

            // ── Elenco de 10/10 (EVENTOS-LOJA-ELENCO.md). Números provisórios. ──
            // Suporte Lendária: gatos sem turno que ela comanda com os próprios PA; o disparo do gato depende da cor e o gato some.
            ["asamyh"]=new FighterDef { id="asamyh", name="Asamyh", sprites="Asamyh", hp=9, pa=6, pm=3, initiative=4, level=5, legendary=true, skills=new[] {
                new SkillDef { id="chamar-gato", name="Chamar gato", kind=SkillKind.Invocacao, pa=2, minRange=1, maxRange=3, perTurn=2, sight=false, summon=Cats, maxActive=2, anim="summon" },
                new SkillDef { id="mover-gato", name="Mover gato", kind=SkillKind.Comando, pa=1, minRange=0, maxRange=99, step=2, sight=false, anim="idle" },
                new SkillDef { id="faisca-da-varinha", name="Faísca da varinha", kind=SkillKind.Comando, pa=2, minRange=0, maxRange=99, sight=false, anim="idle" },
                new SkillDef { id="aura-de-gatinho", name="Aura de gatinho", kind=SkillKind.Cura, pa=3, minRange=0, maxRange=5, damage=3, perTurn=1, anim="heal" },
                new SkillDef { id="noite-dos-gatos", name="Noite dos Gatos", kind=SkillKind.Suporte, pa=4, minRange=0, maxRange=0, sight=false, selfArea=true, area=3, heal=2, buffArmor=1, turns=1, summon=Cats, maxActive=2, summonCount=2, perMatch=1, ultimate=true, anim="ult" } } },
            ["gato-laranja"]=Cat("gato-laranja","Gato laranja","GatoLaranja",new SkillDef { id="faisca-laranja", name="Faísca laranja", kind=SkillKind.Dano, minRange=1, maxRange=3, damage=2, anim="zap" }),
            ["gato-branco"]=Cat("gato-branco","Gato branco","GatoBranco",new SkillDef { id="faisca-branca", name="Faísca branca", kind=SkillKind.Cura, minRange=0, maxRange=3, damage=2, anim="zap" }),
            ["gato-malhado"]=Cat("gato-malhado","Gato preto e branco","GatoMalhado",new SkillDef { id="faisca-malhada", name="Faísca malhada", kind=SkillKind.Dano, minRange=1, maxRange=3, damage=1, pmLoss=2, anim="zap" }),
            ["gato-manchado"]=Cat("gato-manchado","Gato marrom manchado","GatoManchado",new SkillDef { id="faisca-manchada", name="Faísca manchada", kind=SkillKind.Dano, minRange=1, maxRange=3, damage=1, push=1, anim="zap" }),
            // Tanque gigante e lento: qualquer morte no mapa alimenta Devorar (+1 PA no custo da skill absorvida).
            ["colosso"]=new FighterDef { id="colosso", name="Primal Beast", sprites="Colosso", hp=18, pa=6, pm=2, initiative=1, level=6, size=2, immovable=true, devour=true, skills=new[] {
                new SkillDef { id="pancada-colossal", name="Pancada colossal", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4 },
                new SkillDef { id="pisao", name="Pisão", kind=SkillKind.Dano, pa=4, minRange=0, maxRange=0, sight=false, selfArea=true, square=true, area=1, damage=2, perTurn=1, anim="stomp" } } },
            // Pirata de óleo: tentáculos em volta, canhão de peito em 2 tempos e o gancho-broca que arremessa o alvo.
            ["cheiro-pneu"]=new FighterDef { id="cheiro-pneu", name="Cheiro D.Pneu Queimado", sprites="CheiroPneu", hp=14, pa=6, pm=3, initiative=2, level=5, startForm="normal",
                forms=new[] { new FormDef { id="normal", name="Normal" }, new FormDef { id="motor-quente", name="Motor quente" } },
                skills=new[] {
                    new SkillDef { id="gancho", name="Gancho", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3 },
                    new SkillDef { id="tentaculo-de-oleo", name="Tentáculo de óleo", kind=SkillKind.Dano, pa=3, minRange=0, maxRange=0, sight=false, selfArea=true, square=true, area=1, damage=2, pmLoss=2, perTurn=1, anim="tentacle" },
                    new SkillDef { id="aquecer-motor", name="Aquecer motor", kind=SkillKind.Forma, pa=2, minRange=0, maxRange=0, sight=false, toForm="motor-quente", turns=2, form="normal", anim="charge" },
                    new SkillDef { id="canhao-de-peito", name="Canhão de peito", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=5, damage=4, line=true, endsForm=true, form="motor-quente", anim="cannon" },
                    new SkillDef { id="gancho-broca", name="Gancho-broca", kind=SkillKind.Dano, pa=5, minRange=1, maxRange=1, damage=4, push=4, cooldown=4, ultimate=true, anim="drill" } } },
            // Básico e barato: bate com a coroa e ignora 1 de dano de qualquer fonte.
            ["rei-bobett"]=new FighterDef { id="rei-bobett", name="Rei Bobett", sprites="ReiBobett", hp=12, pa=6, pm=3, initiative=3, level=2, armor=1, skills=new[] {
                new SkillDef { id="coroada", name="Coroada", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2 } } },
            // Gnomo mago de papel: gelo, fogo, prisão e o Desejo (rodada 10+, 2 turnos canalizando, 1 de 6 opções).
            ["principe-kellen"]=new FighterDef { id="principe-kellen", name="Príncipe Kellen", sprites="PrincipeKellen", hp=7, pa=6, pm=3, initiative=6, level=4, skills=new[] {
                new SkillDef { id="pauzinho", name="Pauzinho", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=1 },
                new SkillDef { id="chao-de-gelo", name="Chão de gelo", kind=SkillKind.Zona, pa=3, minRange=1, maxRange=4, area=1, freeze=true, perTurn=1, anim="ice" },
                new SkillDef { id="bola-de-fogo", name="Bola de fogo", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=5, damage=3, area=1, modifiable=true, anim="fireball" },
                new SkillDef { id="prisao-dos-pesadelos", name="Prisão dos pesadelos", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=0, prison=1, cooldown=3, anim="prison" },
                new SkillDef { id="desejo", name="Desejo", kind=SkillKind.Desejo, pa=0, minRange=0, maxRange=0, sight=false, minRound=10, perMatch=1, ultimate=true, anim="wishchannel" } } },
            // Draconato paladino (tarefa 012, ficha proposta do Codex): tanque/suporte. A Dualidade guardiã liga um anjo e um demônio
            // em órbita; mantida, custa 1 PA por turno, cura 1 nos aliados e fere 1 nos inimigos até 2 casas.
            ["baltazar"]=new FighterDef { id="baltazar", name="Baltazar", sprites="Baltazar", hp=14, pa=6, pm=3, initiative=2, level=4, skills=new[] {
                new SkillDef { id="baltazar-martelo", name="Martelo de guerra", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2 },
                new SkillDef { id="baltazar-dualidade", name="Dualidade guardiã", kind=SkillKind.Aura, pa=1, minRange=0, maxRange=0, sight=false, area=2, heal=1, damage=1, anim="idle" } } },
            // Fichas extras: mobilidade, armadura perfurante/posicional, defesa, controle de grupo e muito PV.
            ["vespa-cinzenta"]=new FighterDef { id="vespa-cinzenta", name="Vespa Cinzenta", sprites="VespaCinzenta", hp=6, pa=6, pm=6, initiative=9, level=2, flying=true, skills=new[] {
                new SkillDef { id="ferrao", name="Ferrão", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=1, perTurn=2, pmLoss=1 } } },
            ["viuva-da-agulha"]=new FighterDef { id="viuva-da-agulha", name="Viúva da Agulha", sprites="ViuvaAgulha", hp=9, pa=6, pm=4, initiative=7, level=4, skills=new[] {
                new SkillDef { id="agulha", name="Agulha", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=2, damage=2, pierce=true, isolatedBonus=2 } } },
            ["guardiao-de-musgo"]=new FighterDef { id="guardiao-de-musgo", name="Guardião de Musgo", sprites="GuardiaoMusgo", hp=13, pa=6, pm=2, initiative=3, level=4, skills=new[] {
                new SkillDef { id="casca", name="Casca", kind=SkillKind.Suporte, pa=2, minRange=0, maxRange=3, buffArmor=2, turns=1, anim="bark" },
                new SkillDef { id="raizes", name="Raízes", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=1, pmLoss=10, cooldown=2, anim="roots" } } },
            ["domador-de-correntes"]=new FighterDef { id="domador-de-correntes", name="Domador de Correntes", sprites="DomadorCorrentes", hp=11, pa=6, pm=3, initiative=4, level=5, skills=new[] {
                new SkillDef { id="corrente", name="Corrente", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=4, damage=2, line=true, pull=true, anim="chain" },
                new SkillDef { id="grito-de-comando", name="Grito de comando", kind=SkillKind.Dano, pa=4, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=0, pmLoss=2, paLoss=1, perMatch=1, ultimate=true, anim="shout" } } },
            ["ogro-de-pedra"]=new FighterDef { id="ogro-de-pedra", name="Ogro de Pedra", sprites="OgroPedra", hp=16, pa=6, pm=1, initiative=0, level=3, skills=new[] {
                new SkillDef { id="clava", name="Clava", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="arremessar-pedra", name="Arremessar pedra", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=4, damage=2, anim="throw" } } },
            // ── Personagens novos de 10/10 (DIRECAO-DO-AUTOR.md). Números provisórios. ──
            // Edgar (autor, 11/10): corpo a corpo, bate e aguenta na medida. Punhos de energia fantasmagórica; a forma fantasma dura
            // 1 turno (golpes inimigos não o acertam) e a ultimate chama o navio fantasma.
            ["edgar"]=new FighterDef { id="edgar", name="Edgar", sprites="Edgar", hp=13, pa=6, pm=3, initiative=4, level=4, startForm="normal",
                forms=new[] { new FormDef { id="normal", name="Normal" }, new FormDef { id="fantasma", name="Fantasma", sprites="EdgarFantasma", intangible=true } },
                skills=new[] {
                    new SkillDef { id="punho-espectral", name="Punho espectral", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3 },
                    new SkillDef { id="forma-fantasma", name="Forma fantasma", kind=SkillKind.Forma, pa=2, minRange=0, maxRange=0, sight=false, toForm="fantasma", turns=1, cooldown=3, form="normal", anim="transformghost" },
                    new SkillDef { id="navio-fantasma", name="Navio fantasma", kind=SkillKind.Invocacao, pa=5, minRange=1, maxRange=2, sight=false, summon="navio-fantasma", perMatch=1, ultimate=true, noRefresh=true, anim="ultship" } } },
            ["navio-fantasma"]=new FighterDef { id="navio-fantasma", name="Navio fantasma", sprites="EdgarNavio", hp=8, pa=6, pm=3, level=0, flying=true, picker=false, skills=new[] {
                new SkillDef { id="canhonaco-fantasma", name="Canhonaço fantasma", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=4, damage=3, line=true } } },
            // Kits genéricos do Claude (autor liberou, 11/10), só com os clipes que existem: ataque, andar, golpe sofrido e morte.
            // Minotauro de basalto: tanque de choque; a chifrada empurra e a investida cobre a distância.
            ["brumak"]=new FighterDef { id="brumak", name="Brumak", sprites="Brumak", hp=16, pa=6, pm=3, initiative=2, level=5, skills=new[] {
                new SkillDef { id="chifrada", name="Chifrada", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3, push=2 },
                new SkillDef { id="investida", name="Investida", kind=SkillKind.Salto, pa=2, minRange=2, maxRange=4, cooldown=2, anim="walk" } } },
            // Dragão do ossário: voa, morde colado e sopra ossos em linha que devolvem vida.
            ["ossyro"]=new FighterDef { id="ossyro", name="Ossyro", sprites="Ossyro", hp=12, pa=6, pm=4, initiative=5, level=5, flying=true, skills=new[] {
                new SkillDef { id="mordida-ossea", name="Mordida óssea", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="sopro-do-ossario", name="Sopro do ossário", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=3, damage=2, line=true, area=1, drain=1, perTurn=1, anim="breath" } } },
            // Grifo de vidro: voa entre pontos do tabuleiro gastando PM (voo), garra afiada e mergulho sobre o inimigo.
            ["gryvro"]=new FighterDef { id="gryvro", name="Gryvro", sprites="Gryvro", hp=9, pa=6, pm=5, initiative=7, level=4, flying=true, skills=new[] {
                new SkillDef { id="garra-de-vidro", name="Garra de vidro", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, isolatedBonus=2 },
                new SkillDef { id="mergulho", name="Mergulho", kind=SkillKind.Salto, pa=2, minRange=2, maxRange=5, cooldown=2, anim="walk" } } },
            // Cervo branco: suporte que cura à distância e afasta quem chega perto com a galhada.
            ["alvyr"]=new FighterDef { id="alvyr", name="Alvyr", sprites="Alvyr", hp=10, pa=6, pm=4, initiative=5, level=4, skills=new[] {
                new SkillDef { id="galhada", name="Galhada", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, push=1 },
                new SkillDef { id="luz-da-clareira", name="Luz da clareira", kind=SkillKind.Cura, pa=3, minRange=0, maxRange=3, damage=3, perTurn=1, anim="idle" } } },
            // Urso-raiz: casca grossa (ignora 1 de dano) e o abraço de raiz que segura o alvo.
            ["raizor"]=new FighterDef { id="raizor", name="Raizor", sprites="Raizor", hp=14, pa=6, pm=2, initiative=1, level=5, armor=1, skills=new[] {
                new SkillDef { id="patada", name="Patada", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="abraco-de-raiz", name="Abraço de raiz", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=1, pmLoss=2, paLoss=1, perTurn=1 } } },
            // Goblin dos cogumelos: atiradora de área frágil; esporos tiram PA.
            ["nixira"]=new FighterDef { id="nixira", name="Nixira", sprites="Nixira", hp=8, pa=6, pm=4, initiative=6, level=3, skills=new[] {
                new SkillDef { id="cogumelo-explosivo", name="Cogumelo explosivo", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=4, damage=2, area=1, modifiable=true },
                new SkillDef { id="esporos", name="Esporos", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=1, paLoss=1, perTurn=1 } } },
            // ── Elenco de 20/10 (pedido do autor). PV, Iniciativa, Nível e números que o autor não deu são provisórios. ──
            // Pirata das trevas: anula tudo no alvo, tira PA de quem fica perto, engole alguém no buraco negro e corta o mapa em linha.
            ["pele-negra"]=new FighterDef { id="pele-negra", name="Pele D. Ark", sprites="PeleNegra", hp=14, pa=6, pm=2, initiative=2, level=5, legendary=true, fieldPa=3, fieldRange=1, meleeWeak=1, skills=new[] {
                new SkillDef { id="nulification", name="Nulification", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, dispel=true, anim="attack" },
                new SkillDef { id="black-hole", name="Black Hole", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=0, prison=1, banish=true, returnDamage=3, cooldown=3, hits=(int)(Hit.Inimigo|Hit.Aliado), anim="cast" },
                new SkillDef { id="light-speed", name="Light Speed", kind=SkillKind.Teleporte, pa=3, minRange=1, maxRange=3, line=true, sight=false, pmRange=true, perTurn=1, anim="teleport" } } },
            // Suporte Lendária: cada Modo de Emoção é uma forma com aura (passiva da forma); o Pulso dispara o Modo atual em dobro.
            // Modo Guerra: mech por 3 turnos com as 4 auras juntas, míssil global (1 por transformação) e a guitarra.
            ["lela"]=new FighterDef { id="lela", name="L.E.L.A - 0", sprites="Lela", hp=10, pa=6, pm=3, initiative=4, level=6, legendary=true, startForm="alegria",
                forms=new[] {
                    new FormDef { id="alegria", name="Alegria", color="#33ff88", dome=2, music="alegria" },
                    new FormDef { id="ira", name="Ira", color="#ff3355", dome=1, music="ira" },
                    new FormDef { id="calma", name="Calma", color="#3399ff", dome=1, music="calma" },
                    new FormDef { id="ansiedade", name="Ansiedade", color="#ff9922", dome=2, music="ansiedade" },
                    new FormDef { id="guerra", name="Modo Guerra", sprites="LelaGuerra", color="#ff1133", dome=2, music="guerra" } },
                passives=new[] {
                    new PassiveDef { name="Modo Ira", stat=PassiveStat.Dano, who=PassiveWho.Aliados, amount=2, range=1, form="ira" },
                    new PassiveDef { name="Modo Alegria", stat=PassiveStat.CuraPorTurno, who=PassiveWho.Aliados, amount=1, range=2, form="alegria" },
                    new PassiveDef { name="Modo Calma", stat=PassiveStat.PA, who=PassiveWho.Aliados, amount=2, range=1, form="calma" },
                    new PassiveDef { name="Modo Ansiedade", stat=PassiveStat.PM, who=PassiveWho.Aliados, amount=2, range=2, form="ansiedade" },
                    new PassiveDef { name="Guerra: Ira", stat=PassiveStat.Dano, who=PassiveWho.Aliados, amount=2, range=1, form="guerra" },
                    new PassiveDef { name="Guerra: Alegria", stat=PassiveStat.CuraPorTurno, who=PassiveWho.Aliados, amount=1, range=2, form="guerra" },
                    new PassiveDef { name="Guerra: Calma", stat=PassiveStat.PA, who=PassiveWho.Aliados, amount=2, range=1, form="guerra" },
                    new PassiveDef { name="Guerra: Ansiedade", stat=PassiveStat.PM, who=PassiveWho.Aliados, amount=2, range=2, form="guerra" } },
                skills=new[] {
                    new SkillDef { id="soquinho", name="Soquinho", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=1, notForm="guerra", anim="attack" },
                    new SkillDef { id="modo-ira", name="Modo Ira", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="ira", notForm="guerra", anim="pulse" },
                    new SkillDef { id="modo-alegria", name="Modo Alegria", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="alegria", notForm="guerra", anim="pulse" },
                    new SkillDef { id="modo-calma", name="Modo Calma", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="calma", notForm="guerra", anim="pulse" },
                    new SkillDef { id="modo-ansiedade", name="Modo Ansiedade", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="ansiedade", notForm="guerra", anim="pulse" },
                    new SkillDef { id="lela-pulse", name="L.E.L.A Pulse", kind=SkillKind.Pulso, pa=3, minRange=0, maxRange=0, sight=false, amplify=2, cooldown=2, notForm="guerra", anim="pulse" },
                    new SkillDef { id="modo-guerra", name="Modo Guerra", kind=SkillKind.Forma, pa=3, minRange=0, maxRange=0, sight=false, toForm="guerra", turns=3, perMatch=1, ultimate=true, anim="transform" },
                    new SkillDef { id="missil-lela", name="Míssil", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=5, global=true, sight=false, perForm=1, form="guerra", anim="missile" },
                    new SkillDef { id="guitarrada", name="Guitarrada", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3, form="guerra", anim="guitar" } } },
            // Gato espadachim: a garra amaldiçoa (marca permanente), o pulo vai mais longe até quem está marcado e a ultimate cobra as marcas.
            ["gumbald"]=new FighterDef { id="gumbald", name="Gumbald", sprites="Gumbald", hp=10, pa=6, pm=4, initiative=7, level=4, skills=new[] {
                new SkillDef { id="garra-do-gato", name="Garra do Gato", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2, mark=1, anim="attack" },
                new SkillDef { id="pulo-do-gato", name="Pulo do Gato", kind=SkillKind.Salto, pa=0, pmCost=4, minRange=1, maxRange=4, sight=false, markRange=true, perTurn=1, anim="leap" },
                new SkillDef { id="cat-a-clisma", name="Cat-a-Clisma", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=0, consumeMarks=true, perMatch=1, ultimate=true, anim="ult" } } },
            // Vampiro moderno (21/10: visual de motoqueiro): fareja sangue (anda o dobro até quem está ferido), todo golpe cura, acertar
            // ferido dá PM e Transformar em Vampiro levanta uma Cria Vampírica (o visual antigo de Drácula).
            // Nome exibido Damon (autor 22/10); o id "daemion" fica (DungeonBDX, rede e saves usam o id).
            ["daemion"]=new FighterDef { id="daemion", name="Damon", sprites="Daemion", hp=12, pa=6, pm=3, initiative=5, level=5, legendary=true, bloodScent=true, lifesteal=3, bloodlust=2, skills=new[] {
                new SkillDef { id="mordida-vampirica", name="Mordida vampírica", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, anim="attack" },
                new SkillDef { id="lanca-de-sangue", name="Lança de sangue", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=4, damage=2, perTurn=1, anim="blood" },
                new SkillDef { id="transformar-em-vampiro", name="Transformar em Vampiro", kind=SkillKind.Invocacao, pa=4, minRange=1, maxRange=2, sight=false, summon="cria-vampirica", maxActive=2, cooldown=4, anim="summon" } } },
            // O zumbi do Damon: PV máximo pelo número de jogadores (2=2, 3=4, 4=6, 5+=6; autor 22/10).
            ["cria-vampirica"]=new FighterDef { id="cria-vampirica", name="Cria Vampírica", sprites="CriaVampirica", hp=6, hpByPlayers=new[]{2,4,6,6}, pa=6, pm=3, level=0, picker=false, lifesteal=1, skills=new[] {
                new SkillDef { id="garras-da-cria", name="Garras da cria", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2 } } },
            // Ellie, a Sangrenta (21/10): sombras sorteadas a cada turno dela; salta entre elas e emerge colada no inimigo.
            ["ellie"]=new FighterDef { id="ellie", name="Ellie, a Sangrenta", sprites="Ellie", hp=9, pa=6, pm=4, initiative=7, level=4, shadowMin=4, shadowMax=10, shadowBonus=3, shadowDodge=50, exposed=true, skills=new[] {
                new SkillDef { id="kunai-corrente", name="Kunai com corrente", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=4, damage=1, anim="kunai" },
                new SkillDef { id="passo-das-sombras", name="Teleporte entre sombras", kind=SkillKind.Teleporte, pa=3, minRange=0, maxRange=1, global=true, shadowStep=true, gainPm=5, sight=false, perTurn=1, anim="teleport" } } },
            // ── Extras de 21/10 (fotos do autor). Nomes que o autor não deu e todos os números são provisórios. ──
            // Pistoleira: duas pistolas fantasmas, braços espectrais gigantes que socam e atordoam, e a bala que empurra.
            // Qualquer dano dela numa invocação inimiga mata na hora (autor 22/10).
            ["pistoleira"]=new FighterDef { id="pistoleira", name="Pistoleira Fantasma", sprites="Pistoleira", hp=10, pa=6, pm=3, initiative=6, level=4, summonSlayer=true, skills=new[] {
                new SkillDef { id="pistolas-espectrais", name="Pistolas espectrais", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=5, damage=2, precise=true, perTurn=2, anim="attack" },
                new SkillDef { id="bracos-fantasmas", name="Braços fantasmas", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4, anim="punch" },
                new SkillDef { id="agarrao-espectral", name="Agarrão espectral", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=3, damage=1, stun=1, cooldown=3, anim="punch" },
                new SkillDef { id="bala-de-impacto", name="Bala de impacto", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=1, push=2, anim="attack" } } },
            // Reflectius (antes Homem dos Espelhos; id "espelhos" mantido): tira buffs de inimigos e males de aliados, sela ultimates,
            // devolve um golpe e revela escondidos de longe.
            ["espelhos"]=new FighterDef { id="espelhos", name="Mirhor", sprites="Espelhos", hp=11, pa=6, pm=3, initiative=5, level=5, skills=new[] {
                new SkillDef { id="estilhaco", name="Estilhaço", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=2, anim="shatter" },
                new SkillDef { id="espelho-purificador", name="Espelho purificador", kind=SkillKind.Dano, pa=2, minRange=0, maxRange=4, damage=0, purge=true, cleanse=true, cooldown=1, hits=(int)(Hit.Si|Hit.Inimigo|Hit.Aliado), anim="reflect" },
                new SkillDef { id="selar-ultimate", name="Selar ultimate", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=5, damage=0, sealUlt=3, cooldown=3, anim="cast" },
                new SkillDef { id="espelho-de-retorno", name="Espelho de retorno", kind=SkillKind.Suporte, pa=2, minRange=0, maxRange=0, sight=false, selfArea=true, buffReflect=true, turns=2, cooldown=3, anim="reflect" },
                new SkillDef { id="olhar-do-espelho", name="Olhar do espelho", kind=SkillKind.Zona, pa=2, minRange=0, maxRange=1, global=true, sight=false, area=6, reveal=true, cooldown=2, anim="storm" } } },
            // Mordomo Pilis: portais só para aliados, chá que protege de efeitos ruins e a explosão que dobra o dano no alvo.
            ["pilis"]=new FighterDef { id="pilis", name="Mordomo Pilis", sprites="Pilis", hp=12, pa=6, pm=3, initiative=3, level=5, skills=new[] {
                new SkillDef { id="xicara-voadora", name="Xícara voadora", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=2, anim="attack" },
                new SkillDef { id="portal-do-mordomo", name="Portal do mordomo", kind=SkillKind.Portal, pa=3, minRange=1, maxRange=6, sight=false, turns=2, alliesOnly=true, cooldown=2, anim="cast" },
                new SkillDef { id="cha-protetor", name="Chá protetor", kind=SkillKind.Suporte, pa=3, minRange=0, maxRange=0, sight=false, selfArea=true, area=2, buffImmune=true, turns=1, cooldown=3, anim="cast" },
                new SkillDef { id="explosao-do-cha", name="Explosão do chá", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=5, damage=0, doubleDamage=2, selfKill=true, perMatch=1, ultimate=true, anim="ult" } } },
            // PatoMacho (antes Homem Ganso Macho; id "ganso" mantido): só bate forte e aguenta; o bando inteiro anda +2.
            ["ganso"]=new FighterDef { id="ganso", name="PatoMacho", sprites="Ganso", hp=18, pa=6, pm=3, initiative=1, level=5, armor=1,
                passives=new[] { new PassiveDef { name="Bando unido", stat=PassiveStat.PM, who=PassiveWho.AliadosESi, amount=2, range=0 } }, skills=new[] {
                new SkillDef { id="socao-do-ganso", name="Socão", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3, anim="attack" },
                new SkillDef { id="pancada-do-ganso", name="Pancada do ganso", kind=SkillKind.Dano, pa=5, minRange=1, maxRange=1, damage=6, perTurn=1, anim="smash" } } },
            // Espadachim cósmico: forte no duelo, raio de corte em linha e imune a golpes de longe.
            ["espadachim"]=new FighterDef { id="espadachim", name="Espadachim Cósmico", sprites="Espadachim", hp=11, pa=6, pm=4, initiative=6, level=5, duelBonus=2, duelRange=2, farImmune=3, skills=new[] {
                new SkillDef { id="laminas-gemeas", name="Lâminas gêmeas", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3, anim="attack" },
                new SkillDef { id="corte-cosmico", name="Corte cósmico", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=6, damage=2, line=true, beam=true, sight=false, pmLoss=3, perTurn=1, anim="slash" } } },
            // Death Crimson: cyborg das mãos; a Fúria (imparável) bate 2×, anda mais e explode tudo em volta, ele incluso.
            ["death-crimson"]=new FighterDef { id="death-crimson", name="Death Crimson", sprites="DeathCrimson", hp=14, pa=6, pm=3, initiative=4, level=5, fullHitHeal=4, startForm="normal",
                forms=new[] {
                    new FormDef { id="normal", name="Normal" },
                    new FormDef { id="furia", name="Fúria", pm=2, extra=1, endBlast=3, locked=true, unstoppable=true, color="#ff2a2a", dome=1 } },
                skills=new[] {
                    new SkillDef { id="garras-crimson", name="Garras crimson", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3, perTurn=1, anim="attack" },
                    new SkillDef { id="furia-crimson", name="Fúria", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="furia", turns=1, cooldown=2, form="normal", anim="rage" },
                    new SkillDef { id="troca-de-vida", name="Troca de vida", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=2, damage=0, swapHp=true, perMatch=1, ultimate=true, notForm="furia", hits=(int)(Hit.Inimigo|Hit.Aliado), anim="cast" } } },
            // Capitão Lendário: a Room (domo azul até metade do mapa, 3 turnos) leva o Shambles (lutadores e peças do mapa) a metade do mapa; espada negra fura armadura e corta a cura.
            ["ikaito"]=new FighterDef { id="ikaito", name="Capitão Ikaito D. Race", sprites="Ikaito", hp=11, pa=6, pm=3, initiative=6, level=5, legendary=true, startForm="normal",
                forms=new[] { new FormDef { id="normal", name="Normal" }, new FormDef { id="room", name="Room", color="#3aa0ff", dome=-1, domePct=75 } },
                skills=new[] {
                    new SkillDef { id="room", name="Room", kind=SkillKind.Forma, pa=3, minRange=0, maxRange=0, sight=false, toForm="room", turns=3, cooldown=5, anim="room" },
                    new SkillDef { id="shambles-ikaito", name="Shambles", kind=SkillKind.Troca, pa=2, minRange=0, maxRange=4, halfMap=true, halfMapIn="room", mapPct=75, freeIn=true, hitsOut=(int)(Hit.Si|Hit.Aliado), props=true, sight=true, perTurn=2, hits=(int)(Hit.Si|Hit.Inimigo|Hit.Aliado), anim="shambles" },
                    new SkillDef { id="corte-negro", name="Corte negro", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3, pierce=true, noHeal=true, anim="attack" },
                    new SkillDef { id="injection-shot", name="Injection Shot", kind=SkillKind.Cura, pa=5, minRange=1, maxRange=1, damage=2, drain=2, perTurn=1, hits=(int)Hit.Aliado, anim="injection" } } },
            // Loja e eventos.
            ["totem-de-cura"]=new FighterDef { id="totem-de-cura", name="Totem de cura", sprites="TotemCura", hp=4, pa=0, pm=0, level=0, picker=false, noTurn=true, noLock=true, healAura=1, skills=new SkillDef[0] },
            ["papa-moedas"]=new FighterDef { id="papa-moedas", name="Papa-moedas", sprites="PapaMoedas", hp=3, pa=0, pm=3, level=0, picker=false, neutral=true, noLock=true, skills=new SkillDef[0] },
            ["brutamontes-errante"]=new FighterDef { id="brutamontes-errante", name="Brutamontes Errante", sprites="BrutamontesErrante", hp=20, pa=6, pm=3, level=0, picker=false, neutral=true, armor=1, immovable=true, skills=new[] {
                new SkillDef { id="pancada-do-errante", name="Pancada do Errante", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="rugido", name="Rugido", kind=SkillKind.Dano, pa=0, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=0, pmLoss=2, anim="roar" } } },
            // Monstros de farm do labirinto (autor, 18/10 noite): neutros que guardam o lugar, caçam quem chega a 3 casas e
            // dão Match.MonsterGold moedas a quem derruba.
            ["monstro-esqueleto"]=new FighterDef { id="monstro-esqueleto", name="Esqueleto Lanceiro", sprites="EsqueletoLanceiro", hp=6, pa=6, pm=2, level=0, picker=false, neutral=true, skills=new[] {
                new SkillDef { id="lancada-do-esqueleto", name="Lançada", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=2 } } },
            ["monstro-cultista"]=new FighterDef { id="monstro-cultista", name="Cultista Encapuzado", sprites="CultistaEncapuzado", hp=5, pa=6, pm=2, level=0, picker=false, neutral=true, skills=new[] {
                new SkillDef { id="adaga-ritual", name="Adaga ritual", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=2, paLoss=1 } } },
            ["monstro-troll"]=new FighterDef { id="monstro-troll", name="Troll do Esgoto", sprites="TrollEsgoto", hp=8, pa=6, pm=2, level=0, picker=false, neutral=true, armor=1, skills=new[] {
                new SkillDef { id="porrada-do-esgoto", name="Porrada do esgoto", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3 } } },
            // Bosses do labirinto (evento "chefe", 18/10): neutros; skills[0] = golpe colado, skills[1] = especial em volta a cada 2 rodadas.
            ["chefe-dragao"]=new FighterDef { id="chefe-dragao", name="Dragão Espectral Ancião", sprites="DragaoEspectral", hp=22, pa=6, pm=4, level=0, picker=false, neutral=true, flying=true, armor=1, immovable=true, skills=new[] {
                new SkillDef { id="garra-espectral", name="Garra espectral", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="sopro-do-alem", name="Sopro do além", kind=SkillKind.Dano, pa=0, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=2, anim="breath" } } },
            ["chefe-necromante"]=new FighterDef { id="chefe-necromante", name="Necromante da Cripta", sprites="NecromanteCripta", hp=18, pa=6, pm=3, level=0, picker=false, neutral=true, immovable=true, skills=new[] {
                new SkillDef { id="toque-sombrio", name="Toque sombrio", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3, paLoss=1 },
                new SkillDef { id="lamento-da-cripta", name="Lamento da cripta", kind=SkillKind.Dano, pa=0, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=1, paLoss=2 } } },
            ["chefe-rei-goblin"]=new FighterDef { id="chefe-rei-goblin", name="Rei Goblin Ferrugem", sprites="ReiGoblinFerrugem", hp=24, pa=6, pm=3, level=0, picker=false, neutral=true, armor=2, immovable=true, skills=new[] {
                new SkillDef { id="machadada-enferrujada", name="Machadada enferrujada", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4 },
                new SkillDef { id="grito-de-guerra", name="Grito de guerra", kind=SkillKind.Dano, pa=0, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=0, pmLoss=2, anim="roar" } } },
            // Cria Vampírica chefe (21/10): ao cair, se divide em réplicas menores.
            ["chefe-cria-vampirica"]=new FighterDef { id="chefe-cria-vampirica", name="Cria Vampírica Ancestral", sprites="CriaVampirica", hp=18, pa=6, pm=3, level=0, picker=false, neutral=true, immovable=true, lifesteal=2, split="replica-vampirica", splitCount=3, skills=new[] {
                new SkillDef { id="garras-ancestrais", name="Garras ancestrais", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="revoada-de-morcegos", name="Revoada de morcegos", kind=SkillKind.Dano, pa=0, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=1, drain=2, anim="blood" } } },
            ["replica-vampirica"]=new FighterDef { id="replica-vampirica", name="Réplica vampírica", sprites="CriaVampirica", hp=5, pa=6, pm=3, level=0, picker=false, neutral=true, skills=new[] {
                new SkillDef { id="garras-da-replica", name="Garras da réplica", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=2 } } },
            // Rei Thaagarys (batalha contra o chefe, autor 22/10; Boss.cs): 2×2, começa sentado no Trono e levanta no fim da rodada 1.
            // Pool da IA = skills com weight (fase 1 = notForm duas-espadas, fase 2 = form duas-espadas) + movimento (boss.moveWeight).
            // Fase 2 (≤ 50%): saca a segunda espada, 2 ações por turno e +1 uso de cada skill por turno. Números provisórios do Claude.
            ["rei-thaagarys"]=new FighterDef { id="rei-thaagarys", name="Rei Thaagarys", sprites="ReiThaagarys", hp=50, pa=6, pm=3, level=0, picker=false, size=2, armor=1, immovable=true, startForm="espada",
                boss=new BossDef { on=true, every=2, phase2=50, actions1=1, actions2=2, wake=1, moveWeight=3, hpPlayer=15 },
                forms=new[] {
                    new FormDef { id="trono", name="No trono", sprites="ReiThaagarysTrono", music="ansiedade", locked=true, unstoppable=true },
                    new FormDef { id="espada", name="Espada real", sprites="ReiThaagarys", music="ira", locked=true, unstoppable=true },
                    new FormDef { id="duas-espadas", name="Duas espadas", sprites="ReiThaagarysFase2", music="guerra", pa=4, pm=1, extra=1, locked=true, unstoppable=true } },
                skills=new[] {
                    new SkillDef { id="lamina-real", name="Lâmina real", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=4, perTurn=1, weight=6, anim="attack" },
                    new SkillDef { id="passo-do-trono", name="Passo do trono", kind=SkillKind.Teleporte, pa=2, minRange=1, maxRange=6, sight=false, cooldown=3, weight=2, anim="cast" },
                    new SkillDef { id="chamado-da-guarda", name="Chamado da guarda", kind=SkillKind.Invocacao, pa=2, minRange=1, maxRange=2, sight=false, summon="guarda-do-trono", maxActive=2, cooldown=3, weight=2, anim="cast" },
                    new SkillDef { id="decreto-do-caos", name="Decreto do caos", kind=SkillKind.Zona, pa=2, minRange=0, maxRange=0, sight=false, cataclysm=true, cooldown=4, weight=1, anim="cast" },
                    new SkillDef { id="troca-da-coroa", name="Troca da coroa", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=4, sight=false, damage=0, swapHp=true, cooldown=5, weight=3, hits=(int)Hit.Inimigo, anim="cast" },
                    new SkillDef { id="muralhas-do-labirinto", name="Muralhas do labirinto", kind=SkillKind.Zona, pa=2, minRange=1, maxRange=6, sight=false, area=2, walls=6, turns=2, cooldown=3, weight=2, anim="cast" },
                    new SkillDef { id="olhar-do-rei", name="Olhar do rei", kind=SkillKind.Zona, pa=1, minRange=0, maxRange=0, sight=false, global=true, reveal=true, cooldown=2, weight=3, anim="cast" },
                    new SkillDef { id="coroacao-renovada", name="Coroação renovada", kind=SkillKind.Zona, pa=2, minRange=0, maxRange=0, sight=false, resetUlts=true, cooldown=6, weight=1, anim="cast" },
                    new SkillDef { id="vortice-real", name="Vórtice real", kind=SkillKind.Dano, pa=3, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=3, damage=2, pull=true, cooldown=2, weight=4, form="duas-espadas", anim="attack" } } },
            ["guarda-do-trono"]=new FighterDef { id="guarda-do-trono", name="Guarda do Trono", sprites="EsqueletoLanceiro", hp=5, pa=6, pm=3, level=0, picker=false, skills=new[] {
                new SkillDef { id="lanca-do-guarda", name="Lança do guarda", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=2 } } },
            // Mob da Lua de Sangue (MapEvents): 1 PV, caça quem está até 4 casas e vaga sem alvo.
            ["lobo-lua-de-sangue"]=new FighterDef { id="lobo-lua-de-sangue", name="Lobo da Lua de Sangue", sprites="LoboCinzas", hp=1, pa=6, pm=3, level=0, picker=false, neutral=true, noLock=true, aggro=4, roam=true, skills=new[] {
                new SkillDef { id="mordida-sangrenta", name="Mordida sangrenta", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=2 } } },
        };
        // Expansão de 22/10: registra as fichas novas (Content.InventoresLuna.cs) no dicionário acima. Fica como inicializador
        // para rodar depois de Fighters (os inicializadores estáticos rodam na ordem de declaração).
        static readonly bool lunaRegistered=RegisterLuna();
        public static readonly string[] Bosses={"chefe-dragao","chefe-necromante","chefe-rei-goblin","chefe-cria-vampirica"};
        // Biblioteca de skills genéricas (autor, 13/10): modelos prontos inspirados em D&D, Krosmaster e golpes de anime, montados
        // só com efeitos que o motor já tem. O editor copia a escolhida para a ficha (id novo) e o autor ajusta os números.
        public static readonly (string group,SkillDef skill)[] Generic=
        {
            // Sprint 1 (17/10): troca, portal, passo nebuloso com aliado, mina terrestre e skill canalizada.
            ("Sprint", new SkillDef { id="shambles", name="Shambles", kind=SkillKind.Troca, pa=3, minRange=1, maxRange=5, sight=false, cooldown=2, hits=(int)(Hit.Inimigo|Hit.Aliado|Hit.Si), anim="teleport" }),
            ("Sprint", new SkillDef { id="portal", name="Portal", kind=SkillKind.Portal, pa=3, minRange=1, maxRange=6, sight=false, turns=2, cooldown=3, anim="cast" }),
            ("Sprint", new SkillDef { id="passo-nebuloso-aliado", name="Passo nebuloso", kind=SkillKind.Teleporte, pa=2, minRange=0, maxRange=4, sight=false, perTurn=1, hits=(int)(Hit.Si|Hit.Aliado), anim="teleport" }),
            ("Sprint", new SkillDef { id="mina-terrestre", name="Mina terrestre", kind=SkillKind.Armadilha, pa=2, minRange=1, maxRange=3, damage=2, mine=true, perTurn=1 }),
            ("Sprint", new SkillDef { id="meteoro-canalizado", name="Meteoro canalizado", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=6, damage=5, area=1, channeled=true, cooldown=3, hits=(int)(Hit.Inimigo|Hit.Aliado|Hit.Si) }),
            ("D&D", new SkillDef { id="bola-de-fogo", name="Bola de fogo", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=5, damage=2, area=1, modifiable=true, perTurn=1 }),
            ("D&D", new SkillDef { id="misseis-magicos", name="Mísseis mágicos", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=6, damage=2, precise=true }),
            ("D&D", new SkillDef { id="relampago", name="Relâmpago", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=5, damage=3, line=true, sight=false, cooldown=1 }),
            ("D&D", new SkillDef { id="cone-de-frio", name="Cone de frio", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=2, damage=2, area=1, pmLoss=1 }),
            ("D&D", new SkillDef { id="golpe-divino", name="Golpe divino", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4, pierce=true, cooldown=2 }),
            ("D&D", new SkillDef { id="ataque-furtivo", name="Ataque furtivo", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, isolatedBonus=2 }),
            ("D&D", new SkillDef { id="teia", name="Teia", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=4, damage=0, pmLoss=10, cooldown=3 }),
            ("D&D", new SkillDef { id="imobilizar-pessoa", name="Imobilizar pessoa", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=4, damage=0, prison=1, cooldown=4 }),
            ("D&D", new SkillDef { id="curar-ferimentos", name="Curar ferimentos", kind=SkillKind.Cura, pa=3, minRange=0, maxRange=1, damage=4, perTurn=1 }),
            ("D&D", new SkillDef { id="palavra-curativa", name="Palavra curativa", kind=SkillKind.Cura, pa=2, minRange=0, maxRange=4, damage=2, perTurn=1 }),
            ("D&D", new SkillDef { id="bencao", name="Bênção", kind=SkillKind.Suporte, pa=3, minRange=0, maxRange=0, sight=false, selfArea=true, area=2, buffPa=1, turns=1, cooldown=3 }),
            ("D&D", new SkillDef { id="escudo-arcano", name="Escudo arcano", kind=SkillKind.Suporte, pa=2, minRange=0, maxRange=0, sight=false, selfArea=true, buffArmor=2, turns=1, cooldown=2 }),
            ("D&D", new SkillDef { id="passo-nebuloso", name="Passo nebuloso", kind=SkillKind.Teleporte, pa=2, minRange=1, maxRange=3, sight=false, cooldown=2 }),
            ("D&D", new SkillDef { id="furia-barbara", name="Fúria bárbara", kind=SkillKind.Suporte, pa=1, minRange=0, maxRange=0, sight=false, selfArea=true, buffArmor=1, buffPm=1, turns=2, cooldown=4 }),
            ("D&D", new SkillDef { id="chamar-lobo", name="Chamar lobo", kind=SkillKind.Invocacao, pa=4, minRange=1, maxRange=1, summon="lobo-de-cinzas", cooldown=3 }),
            ("Krosmaster", new SkillDef { id="pancada", name="Pancada", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2 }),
            ("Krosmaster", new SkillDef { id="flecha", name="Flecha", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=5, damage=2, modifiable=true }),
            ("Krosmaster", new SkillDef { id="flecha-atordoante", name="Flecha atordoante", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=4, damage=1, paLoss=2, cooldown=2 }),
            ("Krosmaster", new SkillDef { id="bomba", name="Bomba", kind=SkillKind.Armadilha, pa=2, minRange=1, maxRange=3, damage=3, perTurn=1 }),
            ("Krosmaster", new SkillDef { id="empurrao-sismico", name="Empurrão sísmico", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=1, push=3 }),
            ("Krosmaster", new SkillDef { id="arpao", name="Arpão", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=5, damage=1, line=true, pull=true }),
            ("Krosmaster", new SkillDef { id="roubo-de-vida", name="Roubo de vida", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=2, damage=2, drain=2 }),
            ("Krosmaster", new SkillDef { id="tremor", name="Tremor", kind=SkillKind.Dano, pa=4, minRange=0, maxRange=0, sight=false, selfArea=true, square=true, area=1, damage=2, cooldown=2 }),
            ("Krosmaster", new SkillDef { id="dispersao", name="Dispersão", kind=SkillKind.Dano, pa=3, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=1, damage=1, push=1 }),
            ("Krosmaster", new SkillDef { id="corrida", name="Corrida", kind=SkillKind.Suporte, pa=1, minRange=0, maxRange=0, sight=false, selfArea=true, buffPm=2, turns=1, perTurn=1 }),
            ("Krosmaster", new SkillDef { id="esconder-se", name="Esconder-se", kind=SkillKind.Esconder, pa=1, minRange=0, maxRange=0, sight=false, perTurn=1 }),
            ("Anime", new SkillDef { id="onda-de-energia", name="Onda de energia", kind=SkillKind.Dano, pa=5, minRange=1, maxRange=6, damage=4, line=true, sight=false, cooldown=2 }),
            ("Anime", new SkillDef { id="esfera-espiral", name="Esfera espiral", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4, push=2, cooldown=2 }),
            ("Anime", new SkillDef { id="lamina-relampago", name="Lâmina relâmpago", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3, pierce=true, paLoss=1, cooldown=3 }),
            ("Anime", new SkillDef { id="corte-instantaneo", name="Corte instantâneo", kind=SkillKind.Salto, pa=2, minRange=2, maxRange=4, cooldown=2 }),
            ("Anime", new SkillDef { id="clone-das-sombras", name="Clone das sombras", kind=SkillKind.Invocacao, pa=3, minRange=1, maxRange=1, summon="ilusao", maxActive=2, cooldown=3 }),
            ("Anime", new SkillDef { id="punho-do-dragao", name="Punho do dragão", kind=SkillKind.Dano, pa=5, minRange=1, maxRange=1, damage=6, push=2, perMatch=1, ultimate=true }),
            ("Anime", new SkillDef { id="explosao-final", name="Explosão final", kind=SkillKind.Dano, pa=6, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=4, perMatch=1, ultimate=true }),
            ("Anime", new SkillDef { id="selo-de-prisao", name="Selo de prisão", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=0, prison=1, cooldown=3 }),
            ("Anime", new SkillDef { id="aura-de-batalha", name="Aura de batalha", kind=SkillKind.Suporte, pa=1, minRange=0, maxRange=0, sight=false, selfArea=true, buffPa=2, turns=1, cooldown=4 }),
            ("Anime", new SkillDef { id="dominio-congelante", name="Domínio congelante", kind=SkillKind.Zona, pa=3, minRange=1, maxRange=4, area=1, freeze=true, perTurn=1 }),
            ("Suporte", new SkillDef { id="chuva-de-cura", name="Chuva de cura", kind=SkillKind.Suporte, pa=4, minRange=1, maxRange=4, area=1, heal=2, perTurn=1 }),
            ("Suporte", new SkillDef { id="grito-de-guerra", name="Grito de guerra", kind=SkillKind.Suporte, pa=3, minRange=0, maxRange=0, sight=false, selfArea=true, area=2, buffPm=1, buffArmor=1, turns=1, cooldown=3 }),
            ("Suporte", new SkillDef { id="pele-de-pedra", name="Pele de pedra", kind=SkillKind.Suporte, pa=3, minRange=0, maxRange=3, buffArmor=2, turns=1, cooldown=2 }),
            ("Suporte", new SkillDef { id="chama-da-fenix", name="Chama da fênix", kind=SkillKind.Reviver, pa=6, minRange=1, maxRange=1, sight=false, heal=4, perMatch=1, ultimate=true }),
        };
        public const string Cats="gato-laranja|gato-branco|gato-malhado|gato-manchado";
        // Gato da Asamyh: sem turno, 1 PV, não trava; a skill dele é disparada pela Faísca da varinha.
        static FighterDef Cat(string id,string name,string sprites,SkillDef zap) =>
            new FighterDef { id=id, name=name, sprites=sprites, hp=1, pa=0, pm=0, level=0, picker=false, noTurn=true, noLock=true, skills=new[]{ zap } };
        // Opções do Desejo (Príncipe Kellen), disponíveis no turno em que a canalização termina.
        public static readonly SkillDef[] Wishes=
        {
            new SkillDef { id="desejo-a", name="Desejo: reviver", kind=SkillKind.Desejo, wish="a", minRange=1, maxRange=1, sight=false, anim="wish" },
            new SkillDef { id="desejo-b", name="Desejo: cura total", kind=SkillKind.Desejo, wish="b", minRange=0, maxRange=99, sight=false, anim="wish" },
            new SkillDef { id="desejo-c", name="Desejo: PA dobrado", kind=SkillKind.Desejo, wish="c", minRange=0, maxRange=99, sight=false, anim="wish" },
            new SkillDef { id="desejo-d", name="Desejo: copiar skill", kind=SkillKind.Desejo, wish="d", minRange=1, maxRange=99, sight=false, anim="wish" },
            new SkillDef { id="desejo-e", name="Desejo: 2 itens grátis", kind=SkillKind.Desejo, wish="e", minRange=0, maxRange=99, sight=false, anim="wish" },
            new SkillDef { id="desejo-f", name="Desejo: cancelar evento", kind=SkillKind.Desejo, wish="f", minRange=0, maxRange=0, sight=false, anim="wish" },
        };
        public const string Stolen="roubada:";
        static readonly Dictionary<string,SkillDef> stolenCache=new Dictionary<string,SkillDef>();
        // Skill devorada pelo Colosso: os números do dono com +1 PA, sem forma, e um dos 3 clipes dele conforme o formato.
        public static SkillDef StolenSkill(string id)
        {
            if(stolenCache.TryGetValue(id,out var s)) return s;
            var o=AnySkill(id); if(o==null) return null;
            s=o.Clone(); s.id=Stolen+id; s.pa=o.pa+1; s.form=null; s.endsForm=false; s.ultimate=false; s.perMatch=0;
            s.anim=o.kind==SkillKind.Dano&&(o.maxRange>1||o.line)?"steal-reach":o.kind==SkillKind.Dano||o.kind==SkillKind.Armadilha||o.selfArea?"steal-slam":"steal-cast";
            return stolenCache[id]=s;
        }
        // Base de uma skill devorada (para partículas e projéteis da original).
        public static string BaseSkill(string id) => id==null?null:id.StartsWith(Stolen)?BaseSkill(id.Substring(Stolen.Length)):id.StartsWith("sorteio:")?id.Substring(8):id;
        public static SkillDef StolenSkill(Fighter f,string id)
        {
            var sk=StolenSkill(id); var power=f.stolenPowers?.Find(p=>p.id==id);
            if(sk==null||power==null) return sk;
            sk=sk.Clone(); sk.damage=power.damage; sk.maxRange=power.range; sk.minRange=System.Math.Min(sk.minRange,sk.maxRange); return sk;
        }
        public static void ClearCaches() => stolenCache.Clear();

        // ── Edições do autor (aba Editor do menu): fichas inteiras salvas por id; o Host manda as dele no estado da partida. ──
        static Dictionary<string,FighterDef> defaults;
        public static string EditsJson { get; private set; }="";
        public static FighterDef Copy(FighterDef d) => UnityEngine.JsonUtility.FromJson<FighterDef>(UnityEngine.JsonUtility.ToJson(d));
        public static FighterDef Default(string id) { defaults??=new Dictionary<string,FighterDef>(Fighters); return defaults.TryGetValue(id,out var d)?d:null; }
        public static void ApplyEdits(string json)
        {
            json??="";
            if(json==EditsJson) return;
            defaults??=new Dictionary<string,FighterDef>(Fighters);
            foreach(var kv in defaults) Fighters[kv.Key]=kv.Value;
            if(json!="")
                try { var e=UnityEngine.JsonUtility.FromJson<GameEdits>(json); foreach(var d in e?.fighters??new List<FighterDef>()) if(d!=null&&defaults.ContainsKey(d.id)) { d.size=defaults[d.id].size; if(d.id=="pele-negra"&&new[]{"Pele D. N","Pele D. Negra","Pele de Negra","Pele Negra"}.Contains(d.name)||d.id=="espelhos"&&new[]{"Reflectius","Homem dos Espelhos","Homem ods espelhos"}.Contains(d.name)) d.name=defaults[d.id].name; if(d.id=="ikaito"&&d.name=="Capitão Ikaito D. Razer") d.name=defaults[d.id].name; d.skills=WithNewSkills(d,defaults[d.id]); if((d.knownSkills?.Length??0)==0) d.legendary|=defaults[d.id].legendary; Fighters[d.id]=Sanitize(d); } }
                catch(System.Exception) { json=""; }
            EditsJson=json; ClearCaches();
        }
        // Skills que o padrão ganhou depois de a ficha ser editada (ex.: Guarda direcional do Tchu tcha, autor 23/10).
        public static SkillDef[] WithNewSkills(FighterDef edit,FighterDef original)
        {
            var have=(edit.skills??new SkillDef[0]).Where(s=>s!=null).ToList();
            var known=new HashSet<string>(edit.knownSkills??new string[0]);
            // Ficha salva antes de 24/10 (sem knownSkills) não tinha a chance de erro: herda a do padrão.
            if((edit.knownSkills?.Length??0)==0) foreach(var s in have) if(s.missPct==0&&original?.skills?.FirstOrDefault(x=>x.id==s.id) is SkillDef o) s.missPct=o.missPct;
            foreach(var s in original?.skills??new SkillDef[0]) if(!known.Contains(s.id)&&!have.Any(x=>x.id==s.id)) have.Add(s.Clone());
            return have.ToArray();
        }
        public static string[] SkillIds(string id) => Default(id)?.skills?.Select(s=>s.id).ToArray()??new string[0];
        static int Clamp(int v,int min,int max) => v<min?min:v>max?max:v;
        // Limites do editor: nada de números que quebrem o tabuleiro ou a rede.
        // Alcance do halfMap e raio do domo −1: pct% do maior lado do mapa (0 = metade).
        public static int MapReach(int w,int h,int pct) => System.Math.Max(1,System.Math.Max(w,h)*(pct<=0?50:pct)/100);
        static string Reach(SkillDef sk) => sk.mapPct<=0||sk.mapPct==50?"metade":sk.mapPct+"%";
        public static FighterDef Sanitize(FighterDef d)
        {
            bool boss=d.boss!=null&&d.boss.on; d.hp=Clamp(d.hp,1,boss?300:40); d.pa=Clamp(d.pa,0,12); d.pm=Clamp(d.pm,0,10); d.initiative=Clamp(d.initiative,0,20); d.level=Clamp(d.level,0,12); d.armor=Clamp(d.armor,0,5);
            // 16 skills: a ficha do Desland tem 9 (5 de movimento do domínio de xadrez). O resto do limite continua igual.
            d.skills=(d.skills??new SkillDef[0]).Where(s=>s!=null&&!string.IsNullOrEmpty(s.id)).Take(boss?12:16).ToArray();
            foreach(var s in d.skills)
            {
                s.pa=Clamp(s.pa,0,12); s.missPct=Clamp(s.missPct,0,100); s.damage=Clamp(s.damage,0,20); s.maxDamage=s.maxDamage>0?Clamp(s.maxDamage,s.damage,20):0; s.heal=Clamp(s.heal,0,20); s.minRange=Clamp(s.minRange,0,12); s.maxRange=Clamp(s.maxRange,s.minRange,s.kind==SkillKind.Comando||s.kind==SkillKind.Desejo?99:12);
                s.area=Clamp(s.area,0,6); s.perTurn=Clamp(s.perTurn,0,5); s.cooldown=Clamp(s.cooldown,0,9); s.push=Clamp(s.push,0,6); s.prison=Clamp(s.prison,0,2);
                s.pmLoss=Clamp(s.pmLoss,0,10); s.paLoss=Clamp(s.paLoss,0,6); s.buffPa=Clamp(s.buffPa,-6,6); s.buffPm=Clamp(s.buffPm,-6,6); s.buffArmor=Clamp(s.buffArmor,-3,4); s.turns=Clamp(s.turns,0,5); s.hits&=7;
                s.amplify=Clamp(s.amplify,0,4); s.gainPm=Clamp(s.gainPm,0,10); s.stun=Clamp(s.stun,0,2); s.sealUlt=Clamp(s.sealUlt,0,5); s.doubleDamage=Clamp(s.doubleDamage,0,3); s.mark=Clamp(s.mark,0,5); s.pmCost=Clamp(s.pmCost,0,10); s.returnDamage=Clamp(s.returnDamage,0,20); s.perForm=Clamp(s.perForm,0,5); s.mapPct=Clamp(s.mapPct,0,100); s.hitsOut&=7; s.weight=Clamp(s.weight,0,20); s.walls=Clamp(s.walls,0,12);
                if(s.ultimate&&s.perTurn==0&&s.perMatch==0&&s.cooldown==0) s.perMatch=1;
                if(s.kind==SkillKind.Invocacao&&(string.IsNullOrEmpty(s.summon)||!s.summon.Split('|').All(Fighters.ContainsKey))) s.kind=SkillKind.Dano;
            }
            d.allyBonus=Clamp(d.allyBonus,0,5); d.auraRange=Clamp(d.auraRange,0,12); d.revive=Clamp(d.revive,0,40); d.steal=Clamp(d.steal,0,100); d.healAura=Clamp(d.healAura,0,10);
            d.shadowMin=Clamp(d.shadowMin,0,20); d.shadowMax=Clamp(d.shadowMax,d.shadowMin,20); d.shadowBonus=Clamp(d.shadowBonus,0,6); d.shadowDodge=Clamp(d.shadowDodge,0,90);
            d.duelBonus=Clamp(d.duelBonus,0,6); d.duelRange=Clamp(d.duelRange,0,6); d.farImmune=Clamp(d.farImmune,0,12); d.fullHitHeal=Clamp(d.fullHitHeal,0,10); d.splitCount=Clamp(d.splitCount,0,4);
            if(!string.IsNullOrEmpty(d.split)&&(!Fighters.ContainsKey(d.split)||d.split==d.id)) d.split=null;
            d.fieldPa=Clamp(d.fieldPa,0,6); d.fieldRange=Clamp(d.fieldRange,0,4); d.meleeWeak=Clamp(d.meleeWeak,0,5); d.lifesteal=Clamp(d.lifesteal,0,10); d.bloodlust=Clamp(d.bloodlust,0,6);
            if(boss) { var b=d.boss; b.every=Clamp(b.every,1,6); b.phase2=Clamp(b.phase2,0,99); b.actions1=Clamp(b.actions1,1,4); b.actions2=Clamp(b.actions2,1,4); b.wake=Clamp(b.wake,0,9); b.moveWeight=Clamp(b.moveWeight,0,20); b.hpPlayer=Clamp(b.hpPlayer,0,60); }
            d.passives=(d.passives??new PassiveDef[0]).Where(x=>x!=null).Take(8).ToArray();
            foreach(var x in d.passives) { x.amount=Clamp(x.amount,-5,5); x.range=Clamp(x.range,0,12); x.lonely=Clamp(x.lonely,0,12); }
            return d;
        }

        // ── Textos de regra (HUD, menu, editor) ──
        public static string Describe(SkillDef sk)
        {
            if(sk==null) return "";
            string luna=DescribeLuna(sk);if(luna!=null)return luna;
            string area=sk.selfArea?"em volta de si":sk.kind==SkillKind.Comando?"ordem a um gato":sk.global?"mapa todo":sk.halfMap&&!string.IsNullOrEmpty(sk.halfMapIn)?$"alcance {sk.minRange}–{sk.maxRange} ({Reach(sk)} do mapa na forma {sk.halfMapIn})":sk.halfMap?Reach(sk)+" do mapa":sk.minRange==sk.maxRange?$"alcance {sk.maxRange}":$"alcance {sk.minRange}–{sk.maxRange}";
            var e=new List<string>();
            switch(sk.kind)
            {
                case SkillKind.Dano: if(sk.damage>0) e.Add(sk.maxDamage>sk.damage?$"{sk.damage}–{sk.maxDamage} de dano":$"{sk.damage} de dano"); break;
                case SkillKind.Cura: e.Add($"cura {sk.damage}"); break;
                case SkillKind.Armadilha: e.Add((sk.mine?"mina":"armadilha")+(sk.damage>0?$" de {sk.damage}":"")+(sk.mine?" (não para o movimento)":"")); break;
                case SkillKind.Invocacao: e.Add("invoca "+string.Join(" ou ",(sk.summon??"").Split('|').Where(Fighters.ContainsKey).Select(x=>Fighters[x].name))); break;
                case SkillKind.Teleporte: e.Add(sk.shadowStep?"teleporta para uma sombra sua ou para junto de um inimigo":sk.TwoCells?"teleporta o alvo para outra casa":"teleporte"); break;
                case SkillKind.Troca: e.Add(sk.props?"troca de lugar lutadores ou peças do mapa (caixa, árvore, arbusto, baú, braseiro)":"troca dois lutadores de lugar"); break;
                case SkillKind.Portal: e.Add($"abre um portal (entrada e saída) por {(sk.turns>0?sk.turns:2)} turnos"+(sk.alliesOnly?", só aliados passam":"")); break;
                case SkillKind.Salto: e.Add("salta até o inimigo"); break;
                case SkillKind.Forma:
                    e.Add("muda de forma"+(sk.turns>0?$" por {sk.turns} turno{(sk.turns>1?"s":"")}":""));
                    if(Fighters.Values.Where(x=>x.forms!=null).SelectMany(x=>x.forms).Any(x=>x.id==sk.toForm&&x.intangible)) e.Add("imune a todo dano enquanto dura");
                    break;
                case SkillKind.Esconder: e.Add((sk.noCover?"fica escondido em qualquer lugar":"esconde-se junto a cobertura")+$"; escondido anda até {Match.HiddenPm} casas mais os bônus de PM, e quem encosta revela e acerta"); break;
                case SkillKind.Espelho: e.Add("reflete o último ataque do alvo"); break;
                case SkillKind.Zona: e.Add(sk.reveal?"revela os inimigos escondidos na área":sk.freeze?"congela as casas (+1 PM para entrar)":"marca as casas"); break;
                case SkillKind.Comando: e.Add(sk.step>0?$"o gato anda até {sk.step} casas":"o gato dispara e some"); break;
                case SkillKind.Reviver: e.Add($"revive um aliado com {sk.heal} PV"); break;
                case SkillKind.Desejo: e.Add(string.IsNullOrEmpty(sk.wish)?"canaliza 2 turnos e escolhe 1 de 6 desejos":sk.name); break;
                case SkillKind.Aura:
                    e.Add($"liga/desliga; ligada, custa {sk.pa} PA no começo de cada turno seu");
                    if(sk.heal>0) e.Add($"cura {sk.heal} nos aliados"); if(sk.damage>0) e.Add($"{sk.damage} de dano nos inimigos");
                    e.Add($"até {sk.area} casas, 1 vez por turno");
                    break;
                case SkillKind.GuardaDirecional: e.Add($"liga/desliga o escudo; aliados até {sk.area} casas ganham +{sk.buffArmor} armadura contra golpes vindos do lado defendido; não anda nem ataca; desligue antes de mudar direção"); break;
                case SkillKind.DoceSalgado: e.Add($"cura {sk.heal} PV em Somnax e até dois aliados vivos; escolha um aliado e depois outro aliado ou Somnax para concluir"); break;
                case SkillKind.Pulso: e.Add($"dispara o Modo atual ×{System.Math.Max(1,sk.amplify)} nos aliados da aura agora e de novo no começo do turno deles"); break;
            }
            if(sk.kind==SkillKind.Cura&&sk.drain>0) e.Add($"quem usa cura {sk.drain}");
            if(sk.ignoreArmor) e.Add("ignora somente armadura; o dado Escudo funciona");
            if(sk.dispel) e.Add("anula formas, buffs, canalização e aura do alvo");
            if(sk.banish) e.Add($"buraco negro: o alvo some por {System.Math.Max(1,sk.prison)} turno (sem Esquiva){(sk.returnDamage>0?$"; inimigo leva {sk.returnDamage} ao voltar":"")}");
            if(sk.pmRange) e.Add("+1 alcance por PM; só gasta PM além do alcance base");
            if(sk.pmCost>0) e.Add($"custa {sk.pmCost} PM"); if(sk.markRange) e.Add("+1 alcance por marca do alvo");
            if(sk.mark>0) e.Add($"+{sk.mark} Marca da Maldição"); if(sk.consumeMarks) e.Add("consome as marcas: +2 de dano a cada 2");
            if(sk.noHeal) e.Add("o alvo não se cura até o fim do turno");
            if(sk.gainPm>0) e.Add($"+{sk.gainPm} PM para quem usa"); if(sk.stun>0) e.Add($"atordoa {sk.stun} turno");
            if(sk.purge) e.Add("tira os buffs dos inimigos"); if(sk.cleanse) e.Add("tira condições ruins dos aliados");
            if(sk.sealUlt>0) e.Add($"sela as ultimates do inimigo por {sk.sealUlt} turnos e desfaz a transformação"); if(sk.buffReflect) e.Add("o próximo golpe que não o derrubar volta em quem bateu");
            if(sk.invertTurns>0) e.Add($"inverte buffs e debuffs ativos e novos por {sk.invertTurns} turnos");
            if(sk.buffImmune) e.Add("imunes a efeitos negativos"); if(sk.doubleDamage>0) e.Add($"o alvo leva o dobro dos golpes por {sk.doubleDamage} turnos"); if(sk.selfKill) e.Add("quem usa cai logo depois");
            if(sk.beam) e.Add("para no 1º lutador, em obstáculo ou chão mais alto"); if(sk.swapHp) e.Add("troca os PV com o alvo"); if(sk.walls>0) e.Add($"ergue até {sk.walls} paredes por {(sk.turns>0?sk.turns:2)} rodadas"); if(sk.resetUlts) e.Add("reinicia as ultimates e recargas de todos"); if(sk.cataclysm) e.Add("dispara um evento aleatório"); if(sk.weight>0) e.Add($"peso {sk.weight} na IA do chefe"); if(sk.noRefresh) e.Add("o Eco ancestral não devolve"); if(sk.perForm>0) e.Add($"{sk.perForm}× por transformação");
            if(sk.kind==SkillKind.Aura) return $"{sk.name}: {string.Join(", ",e)}.";
            if(sk.heal>0&&sk.kind==SkillKind.Suporte) e.Add($"cura {sk.heal}");
            if(sk.buffArmor!=0) e.Add($"{Sign(sk.buffArmor)} armadura"); if(sk.buffPa!=0) e.Add($"{Sign(sk.buffPa)} PA"); if(sk.buffPm!=0) e.Add($"{Sign(sk.buffPm)} PM");
            if(sk.buffRange!=0) e.Add($"{Sign(sk.buffRange)} alcance");
            if((sk.buffArmor|sk.buffPa|sk.buffPm|sk.buffRange)!=0||sk.buffImmune||sk.buffReflect) e[e.Count-1]+=$" por {System.Math.Max(1,sk.turns)} turno{(sk.turns>1?"s":"")}";
            if(sk.area>0) e.Add($"área {sk.area}{(sk.square?" (8 casas)":"")}{(sk.foes?" só inimigos":"")}");
            if(sk.pierce) e.Add("ignora armadura e Escudo"); else if(sk.precise) e.Add("ignora o Escudo");
            if(sk.isolatedBonus>0) e.Add($"+{sk.isolatedBonus} se o alvo estiver isolado");
            if(sk.push>0) e.Add($"empurra {sk.push}"); if(sk.pull) e.Add("puxa até colar");
            if(sk.pmLoss>=10) e.Add("prende no lugar (0 PM)"); else if(sk.pmLoss>0) e.Add($"−{sk.pmLoss} PM"); if(sk.paLoss>0) e.Add($"−{sk.paLoss} PA");
            if(sk.prison>0&&!sk.banish) e.Add($"aprisiona {sk.prison} turno (Esquiva escapa)");
            if(sk.drain>0&&sk.kind!=SkillKind.Cura) e.Add($"cura {sk.drain} ao acertar"); if(sk.missPct>0) e.Add($"{sk.missPct}% de chance de errar"); else if(sk.wild) e.Add("pode errar");
            if(sk.freeIn&&!string.IsNullOrEmpty(sk.halfMapIn)) e.Add($"na forma {sk.halfMapIn} ignora visão e muro");
            if(sk.hitsOut!=0&&!string.IsNullOrEmpty(sk.halfMapIn)) e.Add($"fora da forma {sk.halfMapIn} só pega "+string.Join("/",new[]{(sk.hitsOut&(int)Hit.Si)!=0?"si":null,(sk.hitsOut&(int)Hit.Inimigo)!=0?"inimigos":null,(sk.hitsOut&(int)Hit.Aliado)!=0?"aliados":null}.Where(x=>x!=null)));
            if(sk.Custom) e.Add("pega "+string.Join("/",new[]{sk.CanHitSelf?"si":null,sk.CanHitEnemy?"inimigos":null,sk.CanHitAlly?"aliados":null}.Where(x=>x!=null)));
            if(sk.channeled) e.Add("canalizada (resolve no próximo turno)"); if(sk.freeSwap) e.Add("grátis se ainda não usou arma no turno");
            if(sk.summonCount>0&&sk.kind!=SkillKind.Invocacao) e.Add($"invoca até {sk.summonCount}");
            var limits=new List<string>();
            if(sk.perTurn>0) limits.Add($"{sk.perTurn}× por turno"); if(sk.perMatch>0) limits.Add($"{sk.perMatch}× por partida");
            if(sk.cooldown>0) limits.Add($"recarga {sk.cooldown}"); if(sk.minRound>0) limits.Add($"a partir da rodada {sk.minRound}");
            return $"{sk.name}{(sk.ultimate?" (ultimate)":"")}: {sk.pa} PA · {area}{(sk.line?" · linha reta":"")}{(sk.modifiable?" · +alcance no alto":"")}{(e.Count>0?" · "+string.Join(", ",e):"")}{(limits.Count>0?" · "+string.Join(", ",limits):"")}.";
        }
        static string Sign(int v) => v>0?"+"+v:v.ToString();
        public static string Describe(ShopDef d)
        {
            if(d.skill!=null) return Describe(d.skill)+(d.uses<0?" Permanente.":d.uses>1?$" {d.uses} usos.":" 1 uso.");
            var parts=new List<string>();
            if(d.pa!=0) parts.Add($"+{d.pa} PA"); if(d.pm!=0) parts.Add($"+{d.pm} PM"); if(d.hp!=0) parts.Add($"+{d.hp} PV");
            if(d.armor!=0) parts.Add($"+{d.armor} armadura"); if(d.range!=0) parts.Add($"+{d.range} alcance");
            if(d.anchor) parts.Add("não é empurrado, puxado nem trocado de lugar"); if(d.climb) parts.Add("sobe 2 níveis com 1 PM e até 3 níveis");
            if(d.focus) parts.Add("+1 de dano na 1ª skill de dano do turno"); if(d.thorns>0) parts.Add($"quem acerta colado leva {d.thorns}");
            return d.name+": "+string.Join(", ",parts)+(d.kind==ShopKind.Buff?$" por {d.turns} turnos.":" permanente.");
        }
        // Passivas e traços da ficha, em texto.
        public static string Traits(FighterDef d)
        {
            var t=new List<string>();
            string luna=LunaTraits(d);if(luna!=null)t.Add(luna);
            if(d.legendary) t.Add("Lendária (limite configurável na partida)"); if(d.flying) t.Add("voa"); if(d.armor>0) t.Add($"ignora {d.armor} de dano de qualquer fonte");
            if(d.immovable) t.Add("imóvel (não é empurrado nem trocado)"); if(d.devour) t.Add("absorve a primeira skill de dano de qualquer entidade que morra no mapa (+1 PA no custo, guarda 2)");
            if(d.id=="somnax") t.Add("Elo das Duas Lâminas: Pacto e Renúncia no mesmo inimigo no seu turno dão +1 de dano no segundo golpe, antes das defesas, uma vez por turno");
            if(d.allyBonus>0) t.Add($"aliados até {d.auraRange} casas causam +{d.allyBonus}"); if(d.guard) t.Add("protege aliados até 2 casas (leva 1 do dano deles)");
            if(d.revive>0) t.Add($"renasce com {d.revive} PV no 1º nocaute"); if(d.steal>0) t.Add($"{d.steal}% de roubar 1 moeda ao acertar");
            if(d.illusion) t.Add($"começa com {d.illusionCount} ilusões de {(d.id=="ezequiela"?1:4)} PV"); if(d.noTurn) t.Add("sem turno próprio"); if(d.noLock) t.Add("não trava");
            if(d.healAura>0) t.Add($"cura {d.healAura} aliados até 2 casas no turno do dono"); if(d.neutral) t.Add("neutro (evento)");
            if(d.aggro>0) t.Add($"caça quem está até {d.aggro} casas"); if(d.roam) t.Add("vaga sem rumo quando não há alvo");
            if(d.fieldPa>0) t.Add($"Nulification Field: inimigos até {d.fieldRange} casa{(d.fieldRange>1?"s":"")} (diagonal incluída) perdem {d.fieldPa} PA; saindo no mesmo turno, recuperam");
            if(d.meleeWeak>0) t.Add($"Yami-yami: leva +{d.meleeWeak} de golpes colados");
            if(d.bloodScent) t.Add("Cheiro de Sangue: andar até um inimigo com PV ≤ 50% custa metade dos PM");
            if(d.shadowMax>0) t.Add($"Sombras: no começo do turno surgem {d.shadowMin}–{d.shadowMax} casas de sombra (as anteriores somem)");
            if(d.shadowBonus>0) t.Add($"+{d.shadowBonus} de dano em alvo numa sombra"); if(d.shadowDodge>0) t.Add($"numa sombra, golpes colados nele têm {d.shadowDodge}% de errar"); if(d.exposed) t.Add("golpes de longe, males e atordoamento sempre entram nele");
            if(d.duelBonus>0) t.Add($"Duelo: +{d.duelBonus} de dano se ninguém mais está até {d.duelRange} casas dele ou do alvo"); if(d.farImmune>0) t.Add($"imune a golpes de {d.farImmune}+ casas");
            if(d.fullHitHeal>0) t.Add($"cura {d.fullHitHeal} ao acertar sem o Escudo do defensor"); if(!string.IsNullOrEmpty(d.split)&&d.splitCount>0) t.Add($"ao cair, se divide em {d.splitCount} réplicas");
            if(d.noReveal) t.Add("não revela escondidos");
            foreach(var fm in d.forms??new FormDef[0]) { if(fm.extra>0) t.Add($"{fm.name}: +{fm.extra} uso por turno das skills"); if(fm.endBlast>0) t.Add($"{fm.name}: no fim do turno, {fm.endBlast} de dano em volta (nele também)"); if(fm.unstoppable||fm.locked) t.Add($"{fm.name}: não pode ser cancelada{(fm.unstoppable?" e não é atordoado":"")}"); }
            if(d.lifesteal>0) t.Add($"todo ataque que fere cura {d.lifesteal}"); if(d.bloodlust>0) t.Add($"acertar inimigo com PV ≤ 50% dá +{d.bloodlust} PM");
            foreach(var p in d.passives??new PassiveDef[0]) if(p!=null) t.Add(Describe(p));
            return string.Join(" · ",t);
        }
        public static string StatName(PassiveStat s) => s switch { PassiveStat.Dano=>"de dano", PassiveStat.Armadura=>"de armadura", PassiveStat.PA=>"PA", PassiveStat.PM=>"PM",
            PassiveStat.Alcance=>"de alcance", PassiveStat.CuraPorTurno=>"de cura por turno", _=>"de dano por turno" };
        public static string WhoName(PassiveWho w) => w switch { PassiveWho.Si=>"em si", PassiveWho.Aliados=>"nos aliados", PassiveWho.AliadosESi=>"em si e nos aliados",
            PassiveWho.Inimigos=>"nos inimigos", _=>"em todos" };
        public static string Describe(PassiveDef p) =>
            $"{(string.IsNullOrEmpty(p.name)?"Passiva":p.name)}: {Sign(p.amount)} {StatName(p.stat)} {WhoName(p.who)}{(p.who==PassiveWho.Si?"":p.range<=0?" (mapa todo)":$" até {p.range} casa{(p.range>1?"s":"")}")}{(p.lonely>0?$" se não houver aliado até {p.lonely} casa{(p.lonely>1?"s":"")} em qualquer direção":"")}{(string.IsNullOrEmpty(p.form)?"":$" (só na forma {p.form})")}";
        // Lutadores que dá para escolher (sem invocações), na ordem do seletor.
        public static readonly string[] Roster=Fighters.Values.Where(d=>d.picker).Select(d=>d.id).ToArray();
        public static FighterDef Def(Fighter f)
        {
            var original=Fighters[f.def]; var r=f.roll; if(r==null) return original;
            var d=original.Clone();
            if(r.stats) { d.hp=r.hp; d.pa=r.pa; d.pm=r.pm; d.initiative=r.initiative; }
            if(r.skills||r.stats) d.skills=r.powers.Select(p=>{
                var sk=(r.skills?AnySkill(p.id):original.skills.FirstOrDefault(s=>s.id==p.id)??AnySkill(p.id))?.Clone();
                if(sk!=null&&r.stats) { sk.damage=p.damage; sk.maxRange=p.range; sk.minRange=System.Math.Min(sk.minRange,sk.maxRange); }
                return sk;
            }).Where(sk=>sk!=null).ToArray();
            return d;
        }
        public static int MaxHp(Fighter f) => Def(f).hp+f.bonusHp;
        // Cópias mantêm o orçamento da original, mas gastam seu próprio movimento.
        public static int MaxPm(Fighter f,MatchState s)
        {
            var owner=f.def=="ilusao"&&f.summoner!=0?s.F(f.summoner):null;
            var basis=owner!=null&&owner.def=="ezequiela"?owner:f;
            return System.Math.Max(0,Def(basis).pm+basis.bonusPm+basis.buffs.Sum(b=>b.pm)+(Form(basis)?.pm??0)+Match.FromState(s).Passive(basis,PassiveStat.PM));
        }
        public static FormDef Form(Fighter f) => Def(f).forms?.FirstOrDefault(x=>x.id==f.form);
        // Qualquer skill do jogo pelo id: de um lutador ou da loja (Espelho primordial e cargas).
        public static SkillDef AnySkill(string id) =>
            id==null?null:id.StartsWith(Stolen)?StolenSkill(id.Substring(Stolen.Length)):
            id.StartsWith("sorteio:")?RandomSkill(id):
            Fighters.Values.SelectMany(d=>d.skills).FirstOrDefault(s=>s.id==id)??Shop.BySkill(id)?.skill??System.Array.Find(Wishes,w=>w.id==id)??Generic.Select(x=>x.skill).FirstOrDefault(s=>s.id==id);
        static SkillDef RandomSkill(string id)
        {
            var sk=Generic.Select(x=>x.skill).FirstOrDefault(x=>"sorteio:"+x.id==id)?.Clone();
            if(sk!=null) sk.id=id; return sk;
        }
        // Skill própria (na forma atual) ou carga (loja, espelho) enquanto houver usos.
        public static SkillDef Skill(Fighter f,string id)
        {
            if(id==null) return null;
            var own=System.Array.Find(Def(f).skills,s=>s.id==id);
            if(own!=null) return (string.IsNullOrEmpty(own.form)||own.form==f.form)&&(string.IsNullOrEmpty(own.notForm)||own.notForm!=f.form)?own:null;
            if(id.StartsWith(Stolen)) { var baseId=id.Substring(Stolen.Length); return f.stolen.Contains(baseId)?StolenSkill(f,baseId):null; }
            if(f.learned.Contains(id)) return AnySkill(id);
            if(f.wishReady&&System.Array.Exists(Wishes,w=>w.id==id)) return AnySkill(id);
            return f.charges.Exists(c=>c.skill==id&&c.uses!=0)?AnySkill(id):null;
        }
    }
}
