using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonBDX.Arcadia
{
    [Serializable] public struct AqCell : IEquatable<AqCell>
    {
        public int X, Y;
        public AqCell(int x,int y) { X=x;Y=y; }
        public int Distance(AqCell b)=>Math.Abs(X-b.X)+Math.Abs(Y-b.Y);
        public bool Equals(AqCell b)=>X==b.X&&Y==b.Y;
        public override bool Equals(object b)=>b is AqCell c&&Equals(c);
        public override int GetHashCode()=>X*397^Y;
        public override string ToString()=>$"{X},{Y}";
    }
    public sealed class AqSpace
    {
        public AqCell Cell;
        public string Surface="street", Room="", Prop="", Wall="";
        public int Tile;
    }
    public sealed class AqEdge
    {
        public AqCell A,B;
        public bool Door,Open;
        public bool Connects(AqCell a,AqCell b)=>(A.Equals(a)&&B.Equals(b))||(A.Equals(b)&&B.Equals(a));
    }
    public sealed class AqToken
    {
        public string Id,Kind,Color="blue";
        public AqCell Cell;
        public int Owner=-1;
        public bool Taken;
    }
    public sealed class AqSpawn
    {
        public string HeroId,Name,SpriteFolder,Profile,Ability,AbilityText;
        public AqCell Cell;
        public bool Ranged,Huge;
        // Arcadia levels: minion (minor minion), major (major minion), villain (minor villain), boss (major villain), dragon.
        public string Tier="minion";
        public int Health=3,Defense,Dice=3,Movement=2,Overkill=5,Reward=1;
    }
    // Level-1 stat lines of the Arcadia Quest card list (KrosArcano/Arcadia_Quest_-_Card_List_1.2.xlsx, Monsters/Dragons).
    // Only the numbers and the ability rule are reused; DungeonBDX enemies keep their own names and art.
    // Attack dice above MaxDice (dragons roll 10) are capped by the session.
    public sealed class AqProfile
    {
        public string Id,Reference,Tier,Ability,AbilityText;
        public int Life,Overkill,Defense,Move,Attack,Coins;
        public bool Ranged;
    }
    public static class AqProfiles
    {
        public static readonly IReadOnlyDictionary<string,AqProfile> All=Build();
        static Dictionary<string,AqProfile> Build()
        {
            var d=new Dictionary<string,AqProfile>(StringComparer.Ordinal);
            void P(string id,string reference,string tier,int life,int overkill,int defense,int move,bool ranged,int attack,int coins,string ability=null,string text=null)
                =>d[id]=new AqProfile{Id=id,Reference=reference,Tier=tier,Life=life,Overkill=overkill,Defense=defense,Move=move,Ranged=ranged,Attack=attack,Coins=coins,Ability=ability,AbilityText=text};
            // Minor minions.
            P("marauder","Orc Marauder","minion",2,3,0,1,false,3,1);
            P("skelebone","Skelebone","minion",2,5,1,1,false,3,1);
            P("jackal","Jacklol","minion",2,3,1,3,false,2,1);
            P("demoness","Demoness","minion",2,4,0,2,false,3,1);
            P("pet","Wild Pet","minion",2,3,1,1,false,2,1);
            P("zombie","Slasher Zombie","minion",2,2,0,1,false,2,1,"undying","Morto-vivo: descarta os ferimentos se não sofrer overkill.");
            P("archer","Goblin Archer","minion",1,2,0,1,true,2,1);
            P("harpy","Harpy","minion",1,2,0,3,true,2,1);
            P("flinger","Axe Flinger Zombie","minion",2,2,0,1,true,2,1,"undying","Morto-vivo: descarta os ferimentos se não sofrer overkill.");
            // Major minions.
            P("hammer","Hammer Beastman","major",3,5,0,1,false,4,2);
            P("korilla","Korilla","major",3,4,1,2,false,4,2);
            P("ghost","Ghost","major",4,3,0,3,false,3,2);
            P("spear","Spear Beastman","major",3,5,0,1,true,3,2,"pack","Matilha: +1 dado se estiver Perto de outro monstro.");
            P("hornet","Hedgehornet","major",2,4,2,2,true,3,2,"pierce","Ferrão: com crítico, o alvo não rola defesa.");
            P("sister","Sister of Pleasure","major",3,5,0,1,true,3,2,"mend","Deleite: cada crítico cura 2 ferimentos dele.");
            // Minor villains.
            P("troll","Troll","villain",4,6,2,2,false,4,3,"cleave","Varredura: ataca todos os heróis Perto.");
            P("minotaur","Minotaur","villain",5,6,1,2,false,4,3,"brutal","Brutal: cada crítico causa 1 acerto extra.");
            P("leah","Leah","villain",4,6,2,3,false,4,3,"quake","Rasante: depois de atacar, cada herói Perto sofre 1 ferimento.");
            P("owlbunny","Owlbunny","villain",3,4,0,2,false,3,2,"prickly","Espinhento: quem o ataca sofre 1 ferimento antes.");
            // Major villains.
            P("dread","Dread King","boss",3,4,0,2,false,4,3,"might","Couraça: só críticos acertam.");
            P("malkhor","Malkhor","boss",6,6,1,2,true,4,3,"brutal","Brutal: cada crítico causa 1 acerto extra.");
            P("fang","Lord Fang","boss",5,7,0,1,true,4,3,"breath","Sede: também ataca os heróis Perto do alvo.");
            P("underlord","The Underlord","boss",6,7,1,0,true,4,3,"echo","Eco: cada crítico fere 1 vez os outros heróis da guilda do alvo.");
            // Young dragon, initial side (Havok).
            P("dragon","Havok (Young - Initial)","dragon",10,11,2,1,true,10,4,"breath+might","Bafo e Couraça: atinge os heróis Perto do alvo; só críticos o acertam.");
            return d;
        }
        public static bool Has(string ability,string id)=>!string.IsNullOrEmpty(ability)&&ability.Split('+').Contains(id);
    }
    public sealed class AqScenario
    {
        public string Id,Name,Subtitle,Story,Ending,Source;
        public string[] Quests;
        public AqBoard Board;
        public AqCell[][] Starts;
        public List<AqSpawn> Monsters=new List<AqSpawn>();
        public List<AqToken> Tokens=new List<AqToken>();
    }
    public sealed class AqBoard
    {
        public int Width {get;}
        public int Height {get;}
        public readonly List<AqSpace> Spaces=new List<AqSpace>();
        public readonly List<AqEdge> Edges=new List<AqEdge>();
        static readonly AqCell[] Steps={new AqCell(0,1),new AqCell(1,0),new AqCell(0,-1),new AqCell(-1,0)};
        public AqBoard(int width,int height)
        {
            if(width<3||height<3||width>18||height>18||width%3!=0||height%3!=0)throw new ArgumentOutOfRangeException();
            Width=width;Height=height;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)Spaces.Add(new AqSpace{Cell=new AqCell(x,y),Tile=x/3+(y/3)*(width/3)});
        }
        public bool Contains(AqCell c)=>c.X>=0&&c.Y>=0&&c.X<Width&&c.Y<Height;
        public AqSpace Space(AqCell c)=>Contains(c)?Spaces[c.Y*Width+c.X]:null;
        public AqEdge Edge(AqCell a,AqCell b)=>Edges.FirstOrDefault(e=>e.Connects(a,b));
        public bool Passage(AqCell a,AqCell b)=>Contains(a)&&Contains(b)&&a.Distance(b)==1&&(Edge(a,b)==null||Edge(a,b).Door&&Edge(a,b).Open);
        public bool Close(AqCell a,AqCell b)=>Contains(a)&&Contains(b)&&(a.Equals(b)||Passage(a,b));
        public IEnumerable<AqCell> Neighbours(AqCell c)
        {foreach(var d in Steps){var n=new AqCell(c.X+d.X,c.Y+d.Y);if(Passage(c,n))yield return n;}}
        public void SetEdge(AqCell a,AqCell b,bool door=false,bool open=false)
        {
            if(!Contains(a)||!Contains(b)||a.Distance(b)!=1)throw new ArgumentException("Invalid edge");
            var e=Edge(a,b);if(e==null){e=new AqEdge{A=a,B=b};Edges.Add(e);}e.Door=door;e.Open=open;
        }
        public List<AqCell> Path(AqCell from,AqCell to,Func<AqCell,bool> blocked=null,Func<AqCell,IEnumerable<AqCell>> portals=null)
        {
            var result=new List<AqCell>();if(!Contains(from)||!Contains(to))return result;
            var prev=new Dictionary<AqCell,AqCell>{{from,from}};var q=new Queue<AqCell>();q.Enqueue(from);
            while(q.Count>0)
            {
                var c=q.Dequeue();if(c.Equals(to)){for(var at=to;;at=prev[at]){result.Add(at);if(at.Equals(from))break;}result.Reverse();return result;}
                var next=Neighbours(c);if(portals!=null)next=next.Concat(portals(c));
                foreach(var n in next)if(!prev.ContainsKey(n)&&!(blocked?.Invoke(n)??false)){prev[n]=c;q.Enqueue(n);}
            }
            return result;
        }
        // Segment/edge intersection, including the FAQ exception for grazing one corner.
        public bool Sight(AqCell a,AqCell b,Func<AqCell,bool> blocked=null)
        {
            if(!Contains(a)||!Contains(b))return false;if(a.Equals(b))return true;
            // Official FAQ: grazing ONE blocked corner is visible, at any angle.
            // Trace both infinitesimal sides; opposite blocked sides still occlude.
            return SightSide(a,b,blocked,1e-6)||SightSide(a,b,blocked,-1e-6);
        }
        bool SightSide(AqCell a,AqCell b,Func<AqCell,bool> blocked,double epsilon)
        {
            if(!Contains(a)||!Contains(b))return false;if(a.Equals(b))return true;
            double ax=a.X-(b.Y-a.Y)*epsilon,ay=a.Y+(b.X-a.X)*epsilon;
            foreach(var e in Edges.Where(e=>!e.Door||!e.Open))
            {
                double ex=(e.A.X+e.B.X)*.5,ey=(e.A.Y+e.B.Y)*.5;
                if(e.A.X!=e.B.X)
                {
                    if(a.X==b.X)continue;double t=(ex-ax)/(b.X-a.X);
                    if(t>0&&t<1){double y=ay+t*(b.Y-a.Y);if(y>=ey-.5&&y<=ey+.5)return false;}
                }
                else
                {
                    if(a.Y==b.Y)continue;double t=(ey-ay)/(b.Y-a.Y);
                    if(t>0&&t<1){double x=ax+t*(b.X-a.X);if(x>=ex-.5&&x<=ex+.5)return false;}
                }
            }
            if(blocked!=null)
                foreach(var s in Spaces)
                    if(!s.Cell.Equals(a)&&!s.Cell.Equals(b)&&blocked(s.Cell)&&CrossesInterior(a,b,s.Cell))return false;
            return true;
        }
        static bool CrossesInterior(AqCell a,AqCell b,AqCell c)
        {
            double lo=0,hi=1;int[] p={a.X,a.Y},d={b.X-a.X,b.Y-a.Y},v={c.X,c.Y};
            for(int i=0;i<2;i++)
            {
                if(d[i]==0){if(p[i]<=v[i]-.5||p[i]>=v[i]+.5)return false;continue;}
                double l=(v[i]-.5-p[i])/d[i],h=(v[i]+.5-p[i])/d[i];if(l>h){var t=l;l=h;h=t;}
                lo=Math.Max(lo,l);hi=Math.Min(hi,h);if(hi-lo<1e-8)return false;
            }
            return true;
        }
    }
    public static class AqScenarios
    {
        public static readonly string[] Ids={"portais-em-ruinas","praca-dos-quatro-emblemas","resgate-nas-catacumbas"};
        // Quest strip icon (Iso/UI/Icons) by what the quest asks for; the first match wins ("relíquia em um portal" is a relic).
        static readonly string[][] questIcons={new[]{"pista","clue"},new[]{"relíquia","relic"},new[]{"cativo","captive"},new[]{"ritual","ritual"},
            new[]{"emblema","emblem"},new[]{"tesouro","treasure"},new[]{"portal","portal"},new[]{"derrotar","death"}};
        public static string QuestIcon(string quest)
        {
            string q=(quest??"").ToLowerInvariant();
            foreach(var pair in questIcons)if(q.Contains(pair[0]))return pair[1];
            return "quest";
        }
        public static AqScenario Create(string id)
        {
            if(!Ids.Contains(id))throw new ArgumentException("Cenário desconhecido.");
            var s=new AqScenario{Id=id,Board=new AqBoard(9,9),Starts=new[]{new[]{new AqCell(1,6),new AqCell(2,6)},new[]{new AqCell(6,2),new AqCell(7,2)},new[]{new AqCell(1,2),new AqCell(2,2)},new[]{new AqCell(6,6),new AqCell(7,6)}}};
            // Nine 3x3 tiles: streets surround connected rooms, rather than a carved dungeon.
            Room(s,"Arquivo",2,8,2,8,"wood","books");Room(s,"Observatório",4,8,5,8,"marble","table");
            Room(s,"Santuário",6,8,6,8,"marble","statue");Room(s,"Taverna",8,7,8,8,"wood","barrels");
            Room(s,"Oficina",0,6,0,7,"wood","table");Room(s,"Guilda do Sol",1,6,2,6,"terracotta","");
            Room(s,"Fonte",3,6,3,6,"marble","fountain");Room(s,"Torre",5,6,5,6,"marble","pillar");
            Room(s,"Guilda da Lua",6,6,7,6,"terracotta","");Room(s,"Alquimia",0,5,0,5,"mosaic","mushrooms");
            Room(s,"Arsenal",2,5,2,5,"wood","weapons");Room(s,"Salão",3,5,5,5,"marble","table");
            Room(s,"Mercado",6,5,7,5,"mosaic","crates");Room(s,"Passagem",4,4,4,4,"wood","");
            Room(s,"Relicário",0,3,0,3,"mosaic","candles");Room(s,"Biblioteca",2,3,2,3,"wood","books");
            Room(s,"Cripta",3,3,5,3,"mosaic","sarcophagus");Room(s,"Ritual",6,3,7,3,"marble","rune");
            Room(s,"Laboratório",0,1,0,2,"wood","books");Room(s,"Guilda da Rosa",1,2,2,2,"terracotta","");
            Room(s,"Memorial",3,2,3,2,"marble","statue");Room(s,"Despensa",5,2,5,2,"wood","barrels");
            Room(s,"Guilda do Mar",6,2,7,2,"terracotta","");Room(s,"Escritório",8,2,8,2,"wood","table");
            Room(s,"Tesouro",2,0,2,0,"mosaic","chest");Room(s,"Câmara",3,0,5,0,"terracotta","table");
            Room(s,"Jardim",6,0,6,0,"garden","tree");
            // Closed guild exits; other chambers have their own open passages.
            Door(s,2,6,2,7,false);Door(s,6,6,6,7,false);Door(s,2,2,2,1,false);Door(s,6,2,6,1,false);
            Door(s,0,5,1,5,true);Door(s,2,5,1,5,true);Door(s,4,6,4,5,true);Door(s,4,5,4,4,true);
            Door(s,4,4,4,3,true);Door(s,4,3,4,2,true);Door(s,0,3,1,3,true);Door(s,2,3,1,3,true);Door(s,6,3,6,4,false);
            int n=0;
            foreach(var c in new[]{new AqCell(1,8),new AqCell(7,8),new AqCell(3,4),new AqCell(5,4),new AqCell(1,0),new AqCell(7,0)})
                s.Tokens.Add(new AqToken{Id="portal"+n++,Kind="portal",Cell=c});
            s.Tokens.Add(new AqToken{Id="violet0",Kind="portal",Color="violet",Cell=new AqCell(6,4)});
            s.Tokens.Add(new AqToken{Id="violet1",Kind="portal",Color="violet",Cell=new AqCell(6,3)});
            var treasures=new[]{new AqCell(1,5),new AqCell(2,4),new AqCell(4,5),new AqCell(4,3),new AqCell(7,4),new AqCell(1,8),new AqCell(7,0)};
            foreach(var c in treasures)s.Tokens.Add(new AqToken{Id="explore"+n++,Kind="explore",Cell=c});
            foreach(var c in new[]{new AqCell(0,5),new AqCell(2,5),new AqCell(0,3),new AqCell(2,3)})
                s.Tokens.Add(new AqToken{Id="clue"+n++,Kind="clue",Color="gold",Cell=c});
            for(int i=0;i<4;i++)s.Tokens.Add(new AqToken{Id="relic"+i,Kind="relic",Color="gold",Cell=new AqCell(6,3)});
            foreach(var c in new[]{new AqCell(4,7),new AqCell(1,4),new AqCell(7,4),new AqCell(4,1)})Monster(s,c.X,c.Y);
            foreach(var c in new[]{new AqCell(0,5),new AqCell(2,3),new AqCell(7,3),new AqCell(4,7)})Monster(s,c.X,c.Y,true);
            Spawn(s,"rei-goblin-ferrugem","Rei Goblin Ferrugem",4,0,"dread");Spawn(s,"brumak","Capataz da ferrugem",6,0,"troll");
            Spawn(s,"papa-moedas","Papa-moedas do tesouro",2,0,"owlbunny");
            s.Name="PORTAIS EM RUÍNAS";s.Subtitle="Capítulo I · investigação e transporte";
            s.Story="Os portais da cidade foram sabotados. Investigue duas pistas nas oficinas ou retire uma relíquia da câmara do ritual e entregue-a em um portal azul para restaurar a rede. As guildas rivais também procuram o responsável.";
            s.Ending="A rede volta a brilhar. Entre os vestígios surge o selo de uma confraria que opera sob a praça.";
            s.Source="Um Portal Turbulento, preparação Base, páginas 1–2: referência de composição e desafios; cenário DungeonBDX com heróis e arte próprios.";
            s.Quests=new[]{"Reunir 2 pistas na guilda", "Entregar 1 relíquia em um portal azul"};
            if(id==Ids[1])
            {
                s.Name="PRAÇA DOS QUATRO EMBLEMAS";s.Subtitle="Capítulo II · controle e confronto";
                s.Story="Os sinos chamam as guildas à praça. Reivindique dois emblemas de pedra ou derrube o guardião que protege o estandarte central. Cada emblema exige atravessar uma sala vigiada.";
                s.Ending="O guardião cai, revelando uma escadaria para as catacumbas. Os desaparecidos ainda podem estar vivos.";
                s.Quests=new[]{"Reivindicar 2 emblemas", "Derrotar o guardião da praça"};
                Plaza(s);
                s.Source="Composição urbana em 9 tiles examinada em O Dia de Ação de Tacos, p.2; desafio dos emblemas autoral.";
            }
            else if(id==Ids[2])
            {
                s.Name="RESGATE NAS CATACUMBAS";s.Subtitle="Capítulo III · escolta e ameaça";
                s.Story="Dois moradores foram aprisionados nas criptas laterais. Liberte um cativo e conduza-o até a área de sua guilda, ou desative dois dos quatro focos do ritual. A passagem central é curta, mas os círculos vermelhos anunciam o perigo.";
                s.Ending="As guildas retornam com sobreviventes e fragmentos do ritual. A cidade ganha uma noite de paz.";
                s.Quests=new[]{"Resgatar 1 cativo até a área inicial", "Desativar 2 focos de ritual"};
                Catacombs(s);
                s.Source="Desafios de escolta examinados em O Último é a Mulher do Padre e Dungeons of Arcadia; topologia autoral em 9 tiles.";
            }
            // Symbol pairs follow the two-die entry procedure; placements and pairs are DungeonBDX content.
            var entries=id==Ids[0]?new[]{new AqCell(4,6),new AqCell(1,4),new AqCell(7,4),new AqCell(4,2)}:
                id==Ids[1]?new[]{new AqCell(4,6),new AqCell(2,4),new AqCell(6,4),new AqCell(4,2)}:
                new[]{new AqCell(2,7),new AqCell(6,7),new AqCell(2,1),new AqCell(6,1)};
            var symbols=new[]{"melee-melee","melee-ranged","ranged-ranged","melee-critical"};
            for(int i=0;i<entries.Length;i++)Token(s,"spawn",entries[i].X,entries[i].Y,symbols[i]);
            return s;
        }
        static void CornerGuilds(AqScenario s,string floor)
        {
            s.Starts=new[]{new[]{new AqCell(0,8),new AqCell(1,8)},new[]{new AqCell(7,0),new AqCell(8,0)},new[]{new AqCell(0,0),new AqCell(1,0)},new[]{new AqCell(7,8),new AqCell(8,8)}};
            Room(s,"Guilda do Sol",0,7,1,8,floor,"");Room(s,"Guilda do Mar",7,0,8,1,floor,"");
            Room(s,"Guilda da Rosa",0,0,1,1,floor,"");Room(s,"Guilda da Lua",7,7,8,8,floor,"");
            Door(s,1,7,2,7,false);Door(s,7,1,6,1,false);Door(s,1,1,2,1,false);Door(s,7,7,6,7,false);
        }
        static void Token(AqScenario s,string kind,int x,int y,string color="blue")
            =>s.Tokens.Add(new AqToken{Id=kind+s.Tokens.Count,Kind=kind,Color=color,Cell=new AqCell(x,y)});
        // Each chapter alternates its own minions (one of each per spawn row): three minor minions and one major minion per side,
        // each on the Arcadia level-1 line of its profile. Non-playable Arena characters keep chapter names; an id is never both melee and ranged in a chapter.
        static readonly Dictionary<string,(string id,string name,string profile)[]> Melee=new Dictionary<string,(string,string,string)[]>{
            ["portais-em-ruinas"]=new[]{("ogro-de-pedra","Saqueador de pedra","marauder"),("raizor","Espinheiro do canal","jackal"),("guardiao-de-musgo","Musgoso das ruínas","skelebone"),("troll-esgoto","Troll do esgoto","hammer")},
            ["praca-dos-quatro-emblemas"]=new[]{("domador-de-correntes","Carcereiro da confraria","marauder"),("lobo-de-cinzas","Lobo de cinzas","jackal"),("gryvro","Grifo de rapina","demoness"),("brumak","Arrombador da confraria","korilla")},
            ["resgate-nas-catacumbas"]=new[]{("esqueleto-lanceiro","Esqueleto lanceiro","skelebone"),("devon","Morto inquieto","zombie"),("viuva-da-agulha","Viúva da agulha","demoness"),("ossyro","Ceifador da cripta","ghost")}};
        static readonly Dictionary<string,(string id,string name,string profile)[]> Shooters=new Dictionary<string,(string,string,string)[]>{
            ["portais-em-ruinas"]=new[]{("cultista-encapuzado","Cultista encapuzado","archer"),("nixira","Xamã dos cogumelos","archer"),("viuva-da-agulha","Tecelã de teias","harpy"),("alvyr","Aprendiz renegado","spear")},
            ["praca-dos-quatro-emblemas"]=new[]{("vespa-cinzenta","Vespa cinzenta","harpy"),("cultista-encapuzado","Cultista encapuzado","archer"),("alvyr","Arcanista da confraria","archer"),("rei-bobett","Coletor de tributos","hornet")},
            ["resgate-nas-catacumbas"]=new[]{("nixira","Xamã dos cogumelos","archer"),("cultista-encapuzado","Cultista encapuzado","flinger"),("teantha","Espectro da foice","harpy"),("alvyr","Vidente do ritual","sister")}};
        // PixelLab enemies without an Arena fighter entry; their folders are passed straight to the unit.
        public static readonly IReadOnlyDictionary<string,string> EnemyArt=new Dictionary<string,string>{
            ["troll-esgoto"]="TrollEsgoto",["cultista-encapuzado"]="CultistaEncapuzado",["esqueleto-lanceiro"]="EsqueletoLanceiro",
            ["rei-goblin-ferrugem"]="ReiGoblinFerrugem",["conde-vesperal"]="CondeVesperal",["necromante-cripta"]="NecromanteCripta",
            ["papa-moedas"]="PapaMoedas",["gato-malhado"]="GatoMalhado",["gato-laranja"]="GatoLaranja",["gato-branco"]="GatoBranco",["gato-manchado"]="GatoManchado"};
        // Every enemy takes the whole line of its profile: life, overkill, defense dice, move, attack dice, coins and ability.
        static AqSpawn Spawn(AqScenario s,string id,string name,int x,int y,string profile)
        {
            var p=AqProfiles.All[profile];
            var m=new AqSpawn{HeroId=id,Name=name,Cell=new AqCell(x,y),SpriteFolder=EnemyArt.TryGetValue(id,out var f)?f:null,Profile=profile,Tier=p.Tier,
                Ranged=p.Ranged,Health=p.Life,Overkill=p.Overkill,Defense=p.Defense,Movement=p.Move,Dice=p.Attack,Reward=p.Coins,Ability=p.Ability,AbilityText=p.AbilityText,Huge=p.Tier=="dragon"};
            s.Monsters.Add(m);return m;
        }
        static void Monster(AqScenario s,int x,int y,bool ranged=false)
        {
            (string id,string name,string profile)[] table=(ranged?Shooters:Melee).TryGetValue(s.Id??"",out var list)?list:new[]{ranged?("cultista-encapuzado","Cultista encapuzado","archer"):("ogro-de-pedra","Saqueador de pedra","marauder")};
            var pick=table[s.Monsters.Count(m=>m.Ranged==ranged&&(m.Tier=="minion"||m.Tier=="major"))%table.Length];
            Spawn(s,pick.id,pick.name,x,y,pick.profile);
        }
        static void Plaza(AqScenario s)
        {
            s.Board=new AqBoard(9,9);s.Tokens.Clear();s.Monsters.Clear();
            // A broad central square and a walkable ring contrast with the small rooms of chapter I.
            CornerGuilds(s,"terracotta");
            Room(s,"Casa do Poente",0,3,1,5,"wood","books");Room(s,"Casa da Aurora",7,3,8,5,"mosaic","table");
            Room(s,"Casa da Coroa",3,7,5,8,"marble","statue");Room(s,"Casa das Brasas",3,0,5,1,"terracotta","barrels");
            Door(s,1,4,2,4,false);Door(s,7,4,6,4,false);Door(s,4,7,4,6,false);Door(s,4,1,4,2,false);
            for(int y=3;y<=5;y++)for(int x=3;x<=5;x++)
            {var p=s.Board.Space(new AqCell(x,y));p.Surface="marble";p.Room="Praça central";}
            s.Board.Space(new AqCell(4,4)).Prop="rune";
            foreach(var c in new[]{new AqCell(0,4),new AqCell(8,4),new AqCell(4,8),new AqCell(4,0)})Token(s,"emblem",c.X,c.Y,"gold");
            foreach(var c in new[]{new AqCell(2,2),new AqCell(6,2),new AqCell(2,6),new AqCell(6,6)})Token(s,"portal",c.X,c.Y);
            Deco(s,"tree",2,0);Deco(s,"tree",6,8);Deco(s,"bush",0,6);Deco(s,"bush",8,2);Deco(s,"pillar",3,3);Deco(s,"pillar",5,5);
            Token(s,"portal",0,4,"violet");Token(s,"portal",8,4,"violet");
            foreach(var c in new[]{new AqCell(1,3),new AqCell(7,3),new AqCell(3,7),new AqCell(3,1)})Monster(s,c.X,c.Y);
            foreach(var c in new[]{new AqCell(1,5),new AqCell(7,5),new AqCell(5,7),new AqCell(5,1)})Monster(s,c.X,c.Y,true);
            foreach(var c in new[]{new AqCell(0,2),new AqCell(8,6),new AqCell(2,8),new AqCell(6,0),new AqCell(3,4),new AqCell(5,4)})Token(s,"explore",c.X,c.Y);
            Spawn(s,"brutamontes-errante","Guardião da praça",4,4,"underlord").Huge=true;
            // Wild pets of the square: stray cats around the central plaza.
            Spawn(s,"gato-malhado","Gato de rua",3,3,"pet");Spawn(s,"gato-laranja","Gato de rua",5,5,"pet");
            Spawn(s,"gato-branco","Gato de rua",3,5,"pet");Spawn(s,"gato-manchado","Gato de rua",5,3,"pet");
            Spawn(s,"ossyro","Carrasco da confraria",5,8,"minotaur");
        }
        static void Catacombs(AqScenario s)
        {
            s.Board=new AqBoard(9,9);s.Tokens.Clear();s.Monsters.Clear();
            foreach(var p in s.Board.Spaces)p.Surface="crypt";
            CornerGuilds(s,"wood");
            // Four crypt wings and a gated ritual chamber create short dangerous routes and a safe outer loop.
            Room(s,"Cripta do Poente",0,3,2,5,"crypt","sarcophagus");Room(s,"Cripta da Aurora",6,3,8,5,"crypt","sarcophagus");
            Room(s,"Arquivo dos Mortos",3,6,5,8,"wood","books");Room(s,"Santuário Submerso",3,0,5,2,"mosaic","fountain");
            Room(s,"Câmara do Ritual",3,3,5,5,"mosaic","rune");
            Deco(s,"bones",2,5);Deco(s,"candles",8,5);Deco(s,"mushrooms",6,5);Deco(s,"pillar",2,0);Deco(s,"deadtree",6,8);Deco(s,"statue",5,0);
            Door(s,2,4,3,4,false);Door(s,5,4,6,4,false);Door(s,4,5,4,6,false);Door(s,4,2,4,3,false);
            Door(s,0,5,0,6,true);Door(s,8,3,8,2,true);Door(s,5,7,6,7,true);Door(s,3,1,2,1,true);
            Token(s,"captive",0,4,"gold");Token(s,"captive",8,4,"gold");
            foreach(var c in new[]{new AqCell(3,3),new AqCell(5,5),new AqCell(4,8),new AqCell(4,0)})Token(s,"ritual",c.X,c.Y,"violet");
            Token(s,"hazard",3,4,"red");Token(s,"hazard",5,4,"red");
            Token(s,"portal",2,6);Token(s,"portal",6,2);Token(s,"portal",2,2,"violet");Token(s,"portal",6,6,"violet");
            foreach(var c in new[]{new AqCell(1,4),new AqCell(7,4),new AqCell(4,6),new AqCell(4,2)})Monster(s,c.X,c.Y);
            foreach(var c in new[]{new AqCell(1,5),new AqCell(7,3),new AqCell(3,7),new AqCell(5,1)})Monster(s,c.X,c.Y,true);
            Spawn(s,"necromante-cripta","Necromante da Cripta",3,5,"malkhor");Spawn(s,"conde-vesperal","Conde Vesperal",5,3,"fang");
            Spawn(s,"gryvro","Grifo das criptas",6,5,"leah");Spawn(s,"dragao-espectral","Dragão espectral",4,4,"dragon");
            foreach(var c in new[]{new AqCell(0,6),new AqCell(8,2),new AqCell(2,8),new AqCell(6,0),new AqCell(4,4),new AqCell(0,2),new AqCell(8,6)})Token(s,"explore",c.X,c.Y);
        }
        // Extra scenery only: never replaces a room prop and has no rule effect.
        static void Deco(AqScenario s,string prop,int x,int y){var p=s.Board.Space(new AqCell(x,y));if(string.IsNullOrEmpty(p.Prop))p.Prop=prop;}
        static void Door(AqScenario s,int x,int y,int xx,int yy,bool open)=>s.Board.SetEdge(new AqCell(x,y),new AqCell(xx,yy),true,open);
        static void Room(AqScenario s,string name,int x0,int y0,int x1,int y1,string surface,string prop)
        {
            for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
            {
                var p=s.Board.Space(new AqCell(x,y));p.Room=name;p.Surface=surface;
                // "books" rooms wear bookshelf walls instead of a shelf prop.
                if(prop=="books")p.Wall="books";else if(x==x0&&y==y0)p.Prop=prop;
                foreach(var d in new[]{new AqCell(1,0),new AqCell(-1,0),new AqCell(0,1),new AqCell(0,-1)})
                {
                    var b=new AqCell(x+d.X,y+d.Y);
                    // Adjacent rooms must retain entrances established by the earlier room.
                    if(s.Board.Contains(b)&&(b.X<x0||b.X>x1||b.Y<y0||b.Y>y1)&&s.Board.Edge(p.Cell,b)?.Door!=true)s.Board.SetEdge(p.Cell,b);
                }
            }
            // Open chamber entrances, with explicit doors overriding these below.
            var entry=new AqCell(x0,y0>0?y0-1:y1+1);var inside=new AqCell(x0,y0>0?y0:y1);
            if(s.Board.Contains(entry)&&!name.StartsWith("Guilda"))Door(s,inside.X,inside.Y,entry.X,entry.Y,true);
        }
    }
}
