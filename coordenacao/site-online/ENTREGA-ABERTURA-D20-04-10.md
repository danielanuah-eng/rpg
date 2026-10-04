# D20 Transplanar e abertura da arena — 04/10/2026

As 210 artes correntes foram aprovadas pelo autor nesta conversa. Carlos está PAUSED.
Esta etapa entrega os assets e componentes de apresentação pedidos pelo autor.
**A integração no jogo continua com Claude e ainda não foi confirmada.** Não altera
o sorteio, primeiro jogador, permissões, prioridade, duração de turnos ou cartas.

## Arquivos e prévia

- Prévia local: Arcanoclisma/efeitos/previas/abertura-partida/index.html.
- Modelo autocontido: Arcanoclisma/efeitos/assets/abertura-partida/v01/d20-transplanar.glb.
- GLTF + malha binária, vinte faces PNG/SVG, atlas, seis vistas SVG e mapa JSON na mesma pasta.
- O GLB inclui o clipe rolagem-exemplo-20, de 4,2 segundos, com giros e translação.
  É um exemplo que termina em 20, não um sorteio.
- Componente para navegador: Arcanoclisma/online/public/kit/abertura-v14/.
- Manifesto e hashes: Arcanoclisma/efeitos/entregas/abertura-partida.visual.json.

Azul profundo, azul-violeta, energia azul suave e números de ouro claro.
Todas as vistas vêm do mesmo icosaedro. Faces opostas somam 21, em uma
distribuição própria documentada. A face confirmada termina voltada à câmera.
O atlas do componente é desenhado localmente: tipografia vetorial, canvas de
alta densidade e WebGL com antialias, sem fetch ou dependência externa.
Faces PNG têm alfa medido no mapa. Não há uso de chaves nem serviço de IA.

## Ligação do D20

Carregar nesta ordem:

~~~html
<link rel="stylesheet" href="/kit/abertura-v14/abertura.css">
<script src="/kit/abertura-v14/d20-modelo.js"></script>
<script src="/kit/abertura-v14/abertura.js"></script>
~~~

O host é um contêiner da apresentação de abertura; removê-lo depois do resultado.
Não colocar informação permanente no centro da mesa.

~~~js
const { D20Transplanar, entrarArena } = globalThis.ArcanoclismaAbertura;
const dado = new D20Transplanar(hostDado);
const apresentacao = await dado.rolar(numeroConfirmadoPeloMotor, { duracao: 4200 });
// O número 1–20 já foi decidido pelo jogo.
// Não extrair resultado dos pixels ou dos números que passam.
dado.destruir(); // ao encerrar ou desmontar a tela
~~~

mostrarFace(n) e mostrarVista('frente'|'tras'|'esquerda'|'direita'|'superior'|'inferior')
permitem consultar lados. cancelar({ concluir: true }) acomoda no resultado;
concluir: false retorna apresentacaoConcluida: false. Uma segunda rolagem
encerra a anterior. O código exige um resultado válido e não escolhe vencedor.
Movimento reduzido e ausência de WebGL apresentam a face final diretamente.

## Entrada gradual da arena

Claude fornece os wrappers reais da mesa, não os elementos internos da arte.
O componente não conhece nomes de eventos ou funções do motor.

~~~js
import { RITMO_VISUAL } from './ritmo-visual-v13.js';

await entrarArena(rootPersistenteDaMesa, {
  piso: elementos.piso,
  zonas: [elementos.campoLocal, elementos.campoRival],
  conjuradores: [elementos.conjuradorLocal, elementos.conjuradorRival],
  baralhosMao: [elementos.baralhos, elementos.maoLocal, elementos.maoRival],
  controles: [elementos.statusDiscretos, elementos.controlesDaDireita]
}, {
  partidaId: idRealDaPartida,
  duracao: RITMO_VISUAL.entradaArena,
  intervalo: RITMO_VISUAL.intervaloEntradaArena
});
~~~

As variáveis do exemplo são entradas do adaptador do Claude, não APIs existentes
supostas. Filtrar elementos ausentes ao formar os grupos.

Piso, zonas, conjuradores, baralhos/mãos e controles entram nessa ordem. Cada
grupo usa 1600 ms, atraso de 180 ms e sobreposição; total máximo 2320 ms.
Não bloqueia comandos nem altera timers. Usa translate individual, opacity e
desfoque transitório, preservando transform/perspectiva do wrapper. Nenhum
desfoque permanece no campo depois da entrada.

A mesma partidaId não repete a abertura, inclusive ao redesenhar o DOM.
Na saída da tela, chamar limparEntradaArena(root) para cancelar e liberar o
registro. Não chamar essa limpeza a cada atualização de estado.
As cartas ocultas conservam o verso. Não ler identidade nem raridade privada.
Movimento reduzido mostra o estado final. Evitar áudio automático.

## Ritmo e confirmação de integração

O perfil ritmo-visual-v13.js já fornece compra 780 ms, mão 360 ms, suspensão
540 ms, ataque/bloqueio 1320 ms e entrada no campo 930 ms.
Continuam valendo RITMO-E-ATRIBUTOS-04-10.md e a limpeza do ataque que antes
terminava em 880 ms. Tempos do CSS e remoção do wrapper precisam coincidir.
Não modificar o motor para obter o polimento.

Entrega de componentes visuais. Não foram executados testes de interface nesta
etapa nem confirmado uso no site. Claude confirma hooks/integração em
Arcanoclisma/efeitos/entregas/abertura-partida.integracao.claude.json.
A expansão de 90 cartas continua bloqueada para jogo até liberação específica.

