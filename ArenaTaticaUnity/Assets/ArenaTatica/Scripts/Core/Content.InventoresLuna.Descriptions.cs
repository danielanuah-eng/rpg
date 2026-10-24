namespace ArenaTatica.Core {
public static partial class Content {
 static string DescribeLuna(SkillDef sk) {
  string detail=null;
  switch(sk.effect){
   case "comando-blip":detail="Se Blip estiver vivo, abre a loja de qualquer lugar. Se estiver caído, invoca somente Blip perto de Nikola.";break;
   case "comando-celinos":detail="Se Celinos estiver vivo, marca um inimigo em qualquer lugar do mapa para ele priorizar. Se estiver caído, invoca somente Celinos.";break;
   case "eletaser":detail=$"{sk.damage} de dano, alcance {sk.maxRange}. Reduz os PA restantes do alvo pela metade (mínimo 1, sem criar PA). A cada 2 engrenagens: +1 dano e +1 alcance.";break;
   case "matriz-protecao":detail="Fecha Nikola numa caixa de 3 PV, +1 PV por 2 engrenagens. Todo dano vai para a caixa, sem excedente no corpo. Dentro dela não ataca nem usa skills. Sai no fim do próximo turno ou usando Saída da matriz a partir do próximo turno.";break;
   case "saida-matriz":detail="Consome uma engrenagem do campo e faz a caixa voar até aquela casa para Nikola sair.";break;
   case "flecha-lunar":detail=$"Flecha azul: {sk.damage} de dano, alcance {sk.maxRange}. Ao acertar um inimigo, rouba 1 PM dele.";break;
   case "tiro-sonus":detail=$"Alcance {sk.maxRange}. Zera os PM do aliado e cura metade dos PM restantes da Bifia, arredondada para baixo. Bifia conserva seus PM. Recarrega durante o próximo turno.";break;
   case "viva-luna":detail="Uma vez por partida; consome o turno inteiro. Escolha um centro em qualquer lugar: raio 6. Aliados recebem +1 iniciativa e +2 PM; inimigos recebem -1 iniciativa e -2 PM.";break;
   case "roque":detail="Escolha dois aliados em linha ou coluna, separados por 3 a 8 casas, com caminho livre. Vale para quaisquer duas unidades aliadas, em qualquer ponto do mapa. O primeiro é o rei: avança 2 casas. O segundo vai para a casa ao lado da origem do primeiro.";break;
   case "xeque-mate":detail=$"Alcance {sk.maxRange}; {sk.damage} de dano +2 por aliado do Desland adjacente ao alvo. Desland não entra nessa contagem.";break;
   case "eco-de-aliado":detail="Após eliminar um inimigo, escolha um aliado para criar seu eco por um turno, com suas habilidades. A passiva tem recarga de 2 turnos.";break;
   case "dominio-xadrez":detail="Durante 2 turnos, o chão vira tabuleiro. Invoca peão, torre, cavalo, bispo e rainha com 1 PV cada. Agem em sequência após Desland conforme o xadrez; libera movimentos de peças para personagens dos dois lados.";break;
   case "mover-peao":case "mover-torre":case "mover-cavalo":case "mover-bispo":case "mover-rainha":detail="Durante o domínio, escolha um personagem ou peça de qualquer time e depois um destino que siga o movimento desta peça de xadrez. Inimigo: só um movimento por domínio. Respeita obstáculos e imunidades.";break;
   case "mover-esfera":detail="Move a esfera a partir da posição e dos PM dela. Não depende da distância até Nepturius. Mover conta como habilidade usada pela esfera.";break;
   case "agarrao-negro":detail="Mãos negras saem da esfera e atordoam até 4 personagens próximos, aliados ou inimigos, no raio 4. Proteções contra controle continuam valendo.";break;
   case "loucura-negra":detail="A esfera ganha +4 PM neste turno. Ainda pode se mover, mas não pode usar Agarrão negro nem Explosão de antimatéria até o turno terminar.";break;
   case "explosao-antimateria":detail=$"A esfera causa {sk.damage} de dano em todos os personagens até 3 casas dela, inclusive aliados.";break;
   case "puxao-blipware":detail=$"Estica o braço mecânico e puxa um inimigo a até {sk.maxRange} casas para perto de Blipware.";break;
  }
  return detail==null?null:sk.name+": "+sk.pa+" PA · "+detail;
 }
 static string LunaTraits(FighterDef d){
  switch(d.id){
   case "nikola":return "Funilaria: começa com Blip e Celinos; surgem 3 engrenagens rodada sim, rodada não (na 1ª rodada já surgem), às vezes especiais que valem 2. O contador é compartilhado com seus robôs. Outro aliado coleta +2 PM e +1 PA por valor até acabar o turno. Cada engrenagem exige 3 golpes para ser destruída. Proteção tecnológica: Nikola e seus robôs recebem dano, mas ignoram efeitos e controle externos. Os robôs sobrevivem à morte dele.";
   case "blip":return "Autômato coletor: alterna entre engrenagens e ouro, procurando o mais próximo. Tudo vai para Nikola, mesmo se Blip cair. Tem +1 PM por engrenagem. Com 10, transforma-se em Blipware.";
   case "blipware":return "Modo armado de Blip, controlável como invocação. Recupera 2 PV por turno. Mantém a proteção tecnológica.";
   case "celinos":return "Autômato: revela ao time do dono os invisíveis até 4 casas (para os outros times continuam escondidos); tiro até 3, atingindo até 2 inimigos a até 2 casas entre si. A cada 3 engrenagens: +1 dano, +1 PM e +1 detecção. Prioriza marca de Nikola, depois proximidade e menor PV no empate. Proteção tecnológica.";
   case "bifia":return "Arqueira da lua: cada flecha que acerta um inimigo rouba 1 PM dele. As flechas são azuis e o arco é prateado.";
   case "desland":return "Mestre do xadrez: ao eliminar um inimigo, pode criar o eco de qualquer aliado por um turno, com suas habilidades. Recarga de 2 turnos.";
   case "nepturius":return "Começa com uma esfera invulnerável. Quem inicia o turno até 4 casas dela perde 2 PA e 1 PM, incluindo aliados e Nepturius. Se ela não usar habilidades no turno, uma casa aleatória recebe 2 de dano; acertar alguém cura 2 PV de Nepturius. Quando ele é atacado, ela se aproxima e concede 1 armadura. Após a morte dele, vaga com 8 PM e usa suas áreas contra todos.";
   case "esfera-nepturius":return "Invulnerável e imune a habilidades externas. Movimento e ataques são comandados por Nepturius; após a morte dele, age sozinha contra todos.";
   case "tubarao":return "Evento das docas: espreita, nada e rodeia alvos na água. Só se move e morde dentro da água.";
   default:return d.id!=null&&d.id.StartsWith("peca-")?"Peça do domínio: 1 PV, age automaticamente após Desland seguindo seu movimento de xadrez.":null;
  }
 }
}
}
