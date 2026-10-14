using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Números iniciais; o balanceamento vem depois. Kits do elenco descritos pelo autor em 10/10 (DESIGN-KROSMASTER.md, passo 7).
    public static class Content
    {
        public static readonly Dictionary<string,FighterDef> Fighters=new Dictionary<string,FighterDef>
        {
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
            ["damian"]=new FighterDef { id="damian", name="Damian", sprites="Damian", hp=10, pa=6, pm=4, initiative=5, level=4, startForm="pistola",
                forms=new[] {
                    new FormDef { id="pistola", name="Pistola", armor=0 },
                    new FormDef { id="canhao", name="Canhão de mão", armor=-1, pose="cannon" },
                    new FormDef { id="martelo", name="Martelo", armor=2, pose="hammer" } },
                skills=new[] {
                    new SkillDef { id="tiro-de-pistola", name="Tiro de pistola", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=5, damage=2, precise=true, modifiable=true, form="pistola", anim="pistol" },
                    new SkillDef { id="canhao-de-mao", name="Canhão de mão", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=4, damage=3, area=1, wild=true, form="canhao", anim="cannon" },
                    new SkillDef { id="martelada", name="Martelada", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4, form="martelo", anim="hammer" },
                    new SkillDef { id="camaleao-pistola", name="Camaleão: pistola", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="pistola", anim="morph" },
                    new SkillDef { id="camaleao-canhao", name="Camaleão: canhão", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="canhao", anim="morph" },
                    new SkillDef { id="camaleao-martelo", name="Camaleão: martelo", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="martelo", anim="morph" } } },
            // Ilusionista: começa com uma cópia ilusória (4 PV); ilusões tiram PM; Espelho primordial reflete no inimigo o último ataque dele.
            ["ezequiela"]=new FighterDef { id="ezequiela", name="Ezequiela", sprites="Ezequiela", hp=9, pa=6, pm=3, initiative=4, level=3, illusion=true, skills=new[] {
                new SkillDef { id="ilusao-confusa", name="Ilusão confusa", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=2, pmLoss=1, modifiable=true },
                new SkillDef { id="espelho-primordial", name="Espelho primordial", kind=SkillKind.Espelho, pa=3, minRange=1, maxRange=6, cooldown=3, ultimate=true, sight=false, anim="mirror" } } },
            ["ilusao"]=new FighterDef { id="ilusao", name="Ilusão", sprites="Ezequiela", hp=4, pa=0, pm=3, level=0, picker=false, skills=new SkillDef[0] },
            // Viking tanque: protege aliados perto; o Olho abissal o transforma por 3 turnos numa máquina de combate sem proteção, que pode errar.
            ["tchu-tcha"]=new FighterDef { id="tchu-tcha", name="Tchu tcha", sprites="TchuTcha", hp=15, pa=6, pm=2, initiative=1, level=5, guard=true, startForm="normal",
                forms=new[] {
                    new FormDef { id="normal", name="Normal", armor=1 },
                    new FormDef { id="berserk", name="Berserk", sprites="TchuTchaBerserk", pa=2, pm=2, armor=0, noGuard=true } },
                skills=new[] {
                    new SkillDef { id="escudada", name="Escudada", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, pmLoss=1, form="normal" },
                    new SkillDef { id="olho-abissal", name="Olho abissal", kind=SkillKind.Forma, pa=2, minRange=0, maxRange=0, sight=false, toForm="berserk", turns=3, perMatch=1, ultimate=true, form="normal", anim="transform" },
                    new SkillDef { id="furia-negra", name="Fúria negra", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=5, wild=true, form="berserk" } } },
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
            ["colosso"]=new FighterDef { id="colosso", name="O Colosso", sprites="Colosso", hp=18, pa=6, pm=2, initiative=1, level=6, immovable=true, devour=true, skills=new[] {
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
                    new SkillDef { id="navio-fantasma", name="Navio fantasma", kind=SkillKind.Invocacao, pa=5, minRange=1, maxRange=2, sight=false, summon="navio-fantasma", perMatch=1, ultimate=true, anim="ultship" } } },
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
            // Loja e eventos.
            ["totem-de-cura"]=new FighterDef { id="totem-de-cura", name="Totem de cura", sprites="TotemCura", hp=4, pa=0, pm=0, level=0, picker=false, noTurn=true, noLock=true, healAura=1, skills=new SkillDef[0] },
            ["papa-moedas"]=new FighterDef { id="papa-moedas", name="Papa-moedas", sprites="PapaMoedas", hp=3, pa=0, pm=3, level=0, picker=false, neutral=true, noLock=true, skills=new SkillDef[0] },
            ["brutamontes-errante"]=new FighterDef { id="brutamontes-errante", name="Brutamontes Errante", sprites="BrutamontesErrante", hp=20, pa=6, pm=3, level=0, picker=false, neutral=true, armor=1, immovable=true, skills=new[] {
                new SkillDef { id="pancada-do-errante", name="Pancada do Errante", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="rugido", name="Rugido", kind=SkillKind.Dano, pa=0, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=0, pmLoss=2, anim="roar" } } },
        };
        // Biblioteca de skills genéricas (autor, 13/10): modelos prontos inspirados em D&D, Krosmaster e golpes de anime, montados
        // só com efeitos que o motor já tem. O editor copia a escolhida para a ficha (id novo) e o autor ajusta os números.
        public static readonly (string group,SkillDef skill)[] Generic=
        {
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
                try { var e=UnityEngine.JsonUtility.FromJson<GameEdits>(json); foreach(var d in e?.fighters??new List<FighterDef>()) if(d!=null&&defaults.ContainsKey(d.id)) Fighters[d.id]=Sanitize(d); }
                catch(System.Exception) { json=""; }
            EditsJson=json; ClearCaches();
        }
        static int Clamp(int v,int min,int max) => v<min?min:v>max?max:v;
        // Limites do editor: nada de números que quebrem o tabuleiro ou a rede.
        public static FighterDef Sanitize(FighterDef d)
        {
            d.hp=Clamp(d.hp,1,40); d.pa=Clamp(d.pa,0,12); d.pm=Clamp(d.pm,0,10); d.initiative=Clamp(d.initiative,0,20); d.level=Clamp(d.level,0,12); d.armor=Clamp(d.armor,0,5);
            d.skills=(d.skills??new SkillDef[0]).Where(s=>s!=null&&!string.IsNullOrEmpty(s.id)).Take(8).ToArray();
            foreach(var s in d.skills)
            {
                s.pa=Clamp(s.pa,0,12); s.damage=Clamp(s.damage,0,20); s.heal=Clamp(s.heal,0,20); s.minRange=Clamp(s.minRange,0,12); s.maxRange=Clamp(s.maxRange,s.minRange,s.kind==SkillKind.Comando||s.kind==SkillKind.Desejo?99:12);
                s.area=Clamp(s.area,0,4); s.perTurn=Clamp(s.perTurn,0,5); s.cooldown=Clamp(s.cooldown,0,9); s.push=Clamp(s.push,0,6); s.prison=Clamp(s.prison,0,2);
                s.pmLoss=Clamp(s.pmLoss,0,10); s.paLoss=Clamp(s.paLoss,0,6); s.buffPa=Clamp(s.buffPa,-6,6); s.buffPm=Clamp(s.buffPm,-6,6); s.buffArmor=Clamp(s.buffArmor,-3,4); s.turns=Clamp(s.turns,0,5);
                if(s.ultimate&&s.perTurn==0&&s.perMatch==0&&s.cooldown==0) s.perMatch=1;
                if(s.kind==SkillKind.Invocacao&&(string.IsNullOrEmpty(s.summon)||!s.summon.Split('|').All(Fighters.ContainsKey))) s.kind=SkillKind.Dano;
            }
            d.allyBonus=Clamp(d.allyBonus,0,5); d.auraRange=Clamp(d.auraRange,0,12); d.revive=Clamp(d.revive,0,40); d.steal=Clamp(d.steal,0,100); d.healAura=Clamp(d.healAura,0,10);
            d.passives=(d.passives??new PassiveDef[0]).Where(x=>x!=null).Take(6).ToArray();
            foreach(var x in d.passives) { x.amount=Clamp(x.amount,-5,5); x.range=Clamp(x.range,0,12); }
            return d;
        }

        // ── Textos de regra (HUD, menu, editor) ──
        public static string Describe(SkillDef sk)
        {
            if(sk==null) return "";
            string area=sk.selfArea?"em volta de si":sk.kind==SkillKind.Comando?"ordem a um gato":sk.minRange==sk.maxRange?$"alcance {sk.maxRange}":$"alcance {sk.minRange}–{sk.maxRange}";
            var e=new List<string>();
            switch(sk.kind)
            {
                case SkillKind.Dano: if(sk.damage>0) e.Add($"{sk.damage} de dano"); break;
                case SkillKind.Cura: e.Add($"cura {sk.damage}"); break;
                case SkillKind.Armadilha: e.Add(sk.damage>0?$"armadilha de {sk.damage}":"armadilha"); break;
                case SkillKind.Invocacao: e.Add("invoca "+string.Join(" ou ",(sk.summon??"").Split('|').Where(Fighters.ContainsKey).Select(x=>Fighters[x].name))); break;
                case SkillKind.Teleporte: e.Add("teleporte"); break;
                case SkillKind.Salto: e.Add("salta até o inimigo"); break;
                case SkillKind.Forma:
                    e.Add("muda de forma"+(sk.turns>0?$" por {sk.turns} turno{(sk.turns>1?"s":"")}":""));
                    if(Fighters.Values.Where(x=>x.forms!=null).SelectMany(x=>x.forms).Any(x=>x.id==sk.toForm&&x.intangible)) e.Add("imune a todo dano enquanto dura");
                    break;
                case SkillKind.Esconder: e.Add((sk.noCover?"fica escondido em qualquer lugar":"esconde-se junto a cobertura")+$"; escondido anda até {Match.HiddenPm} casas, e quem encosta revela e acerta"); break;
                case SkillKind.Espelho: e.Add("reflete o último ataque do alvo"); break;
                case SkillKind.Zona: e.Add(sk.freeze?"congela as casas (+1 PM para entrar)":"marca as casas"); break;
                case SkillKind.Comando: e.Add(sk.step>0?$"o gato anda até {sk.step} casas":"o gato dispara e some"); break;
                case SkillKind.Reviver: e.Add($"revive um aliado com {sk.heal} PV"); break;
                case SkillKind.Desejo: e.Add(string.IsNullOrEmpty(sk.wish)?"canaliza 2 turnos e escolhe 1 de 6 desejos":sk.name); break;
                case SkillKind.Aura:
                    e.Add($"liga/desliga; ligada, custa {sk.pa} PA no começo de cada turno seu");
                    if(sk.heal>0) e.Add($"cura {sk.heal} nos aliados"); if(sk.damage>0) e.Add($"{sk.damage} de dano nos inimigos");
                    e.Add($"até {sk.area} casas, 1 vez por turno");
                    break;
            }
            if(sk.kind==SkillKind.Aura) return $"{sk.name}: {string.Join(", ",e)}.";
            if(sk.heal>0&&sk.kind==SkillKind.Suporte) e.Add($"cura {sk.heal}");
            if(sk.buffArmor!=0) e.Add($"{Sign(sk.buffArmor)} armadura"); if(sk.buffPa!=0) e.Add($"{Sign(sk.buffPa)} PA"); if(sk.buffPm!=0) e.Add($"{Sign(sk.buffPm)} PM");
            if(sk.buffRange!=0) e.Add($"{Sign(sk.buffRange)} alcance");
            if((sk.buffArmor|sk.buffPa|sk.buffPm|sk.buffRange)!=0) e[e.Count-1]+=$" por {System.Math.Max(1,sk.turns)} turno{(sk.turns>1?"s":"")}";
            if(sk.area>0) e.Add($"área {sk.area}{(sk.square?" (8 casas)":"")}{(sk.foes?" só inimigos":"")}");
            if(sk.pierce) e.Add("ignora armadura e Escudo"); else if(sk.precise) e.Add("ignora o Escudo");
            if(sk.isolatedBonus>0) e.Add($"+{sk.isolatedBonus} se o alvo estiver isolado");
            if(sk.push>0) e.Add($"empurra {sk.push}"); if(sk.pull) e.Add("puxa até colar");
            if(sk.pmLoss>=10) e.Add("prende no lugar (0 PM)"); else if(sk.pmLoss>0) e.Add($"−{sk.pmLoss} PM"); if(sk.paLoss>0) e.Add($"−{sk.paLoss} PA");
            if(sk.prison>0) e.Add($"aprisiona {sk.prison} turno (Esquiva escapa)");
            if(sk.drain>0) e.Add($"cura {sk.drain} ao acertar"); if(sk.wild) e.Add("pode errar");
            if(sk.summonCount>0&&sk.kind!=SkillKind.Invocacao) e.Add($"invoca até {sk.summonCount}");
            var limits=new List<string>();
            if(sk.perTurn>0) limits.Add($"{sk.perTurn}× por turno"); if(sk.perMatch>0) limits.Add($"{sk.perMatch}× por partida");
            if(sk.cooldown>0) limits.Add($"recarga {sk.cooldown}"); if(sk.minRound>0) limits.Add($"a partir da rodada {sk.minRound}");
            return $"{sk.name}{(sk.ultimate?" (ultimate)":"")}: {sk.pa} PA · {area}{(sk.line?" · linha reta":"")}{(sk.modifiable?" · +alcance no alto":"")}{(e.Count>0?" · "+string.Join(", ",e):"")}{(limits.Count>0?" · "+string.Join(", ",limits):"")}.";
        }
        static string Sign(int v) => v>0?"+"+v:v.ToString();
        public static string Describe(ShopDef d)
        {
            if(d.skill!=null) return Describe(d.skill)+(d.uses>1?$" {d.uses} usos.":" 1 uso.");
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
            if(d.legendary) t.Add("Lendária (1 por time)"); if(d.flying) t.Add("voa"); if(d.armor>0) t.Add($"ignora {d.armor} de dano de qualquer fonte");
            if(d.immovable) t.Add("imóvel (não é empurrado nem trocado)"); if(d.devour) t.Add("absorve a primeira skill de dano de qualquer entidade que morra no mapa (+1 PA no custo, guarda 2)");
            if(d.allyBonus>0) t.Add($"aliados até {d.auraRange} casas causam +{d.allyBonus}"); if(d.guard) t.Add("protege aliados até 2 casas (leva 1 do dano deles)");
            if(d.revive>0) t.Add($"renasce com {d.revive} PV no 1º nocaute"); if(d.steal>0) t.Add($"{d.steal}% de roubar 1 moeda ao acertar");
            if(d.illusion) t.Add("começa com uma ilusão de 4 PV"); if(d.noTurn) t.Add("sem turno próprio"); if(d.noLock) t.Add("não trava");
            if(d.healAura>0) t.Add($"cura {d.healAura} aliados até 2 casas no turno do dono"); if(d.neutral) t.Add("neutro (evento)");
            foreach(var p in d.passives??new PassiveDef[0]) if(p!=null) t.Add(Describe(p));
            return string.Join(" · ",t);
        }
        public static string StatName(PassiveStat s) => s switch { PassiveStat.Dano=>"de dano", PassiveStat.Armadura=>"de armadura", PassiveStat.PA=>"PA", PassiveStat.PM=>"PM",
            PassiveStat.Alcance=>"de alcance", PassiveStat.CuraPorTurno=>"de cura por turno", _=>"de dano por turno" };
        public static string WhoName(PassiveWho w) => w switch { PassiveWho.Si=>"em si", PassiveWho.Aliados=>"nos aliados", PassiveWho.AliadosESi=>"em si e nos aliados",
            PassiveWho.Inimigos=>"nos inimigos", _=>"em todos" };
        public static string Describe(PassiveDef p) =>
            $"{(string.IsNullOrEmpty(p.name)?"Passiva":p.name)}: {Sign(p.amount)} {StatName(p.stat)} {WhoName(p.who)}{(p.who==PassiveWho.Si?"":p.range<=0?" (mapa todo)":$" até {p.range} casa{(p.range>1?"s":"")}")}";
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
            if(own!=null) return string.IsNullOrEmpty(own.form)||own.form==f.form?own:null;
            if(id.StartsWith(Stolen)) { var baseId=id.Substring(Stolen.Length); return f.stolen.Contains(baseId)?StolenSkill(f,baseId):null; }
            if(f.learned.Contains(id)) return AnySkill(id);
            if(f.wishReady&&System.Array.Exists(Wishes,w=>w.id==id)) return AnySkill(id);
            return f.charges.Exists(c=>c.skill==id&&c.uses>0)?AnySkill(id):null;
        }
    }
}
