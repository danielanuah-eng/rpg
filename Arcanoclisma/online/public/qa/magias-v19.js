// Fixture local: a mesma montagem por zonas do tutorial, com o motor e a mesa reais.
// Jogadas, pagamentos, alvos e resolução passam pela interface original da partida.
import {criarJogo,executar} from '/jogo/motor.js';
import {DADOS,conj} from '/js/base.js';
import {abrirMesa} from '/js/mesa.js';
import {criarPartidaLocal} from '/js/partida-local.js';
const specs = {
  campos: {eu:{conj:'ricardo',mao:['r6-furia-c3'],campo:['base-aliado-c4-defensivo'],grau:Array(3).fill('base-recurso'),arcano:3},bot:{conj:'mitonho',campo:['base-aliado-c4-defensivo'],grau:Array(3).fill('base-recurso')}},
  ressurgir: {eu:{conj:'diastaros',mao:['col-cemiterio-que-respira'],cemiterio:['base-aliado-c4-defensivo'],grau:Array(6).fill('base-recurso'),arcano:6},bot:{conj:'mitonho',campo:['base-aliado-c4-defensivo'],grau:Array(3).fill('base-recurso')}},
};
function montar(spec) {
  const deck=s=>[...(s.mao??[]),...(s.campo??[]),...(s.cemiterio??[]),...(s.grau??[]),...Array(24).fill('base-recurso')];
  const e=criarJogo({catalogo:DADOS.catalogo,regras:DADOS.regras,semente:19,baralhos:[spec.eu,spec.bot].map(s=>({conjurador:s.conj,cartas:deck(s),itens:['item-canal-cajado','item-armadura-gibao']}))});
  executar(e,{tipo:'escolherInicio',jogador:e.pendencia.jogador,primeiro:0});
  [spec.eu,spec.bot].forEach((s,j)=>{
    const z=e.jogadores[j],todas=[...z.baralho,...z.mao];
    const tirar=id=>{const i=todas.findIndex(x=>e.instancias[x].defId===id);if(i<0)throw new Error('Carta ausente no cenário: '+id);return todas.splice(i,1)[0];};
    for(const zona of ['mao','campo','grau','cemiterio'])z[zona]=(s[zona]??[]).map(tirar);
    z.campo.forEach(id=>e.instancias[id].entrouNoTurno=1);z.grau.forEach(id=>e.instancias[id].revelada=false);
    z.baralho=todas;z.arcano=s.arcano??0;e.instancias[z.conjuradorId].dano=0;
    if(!conj(s.conj))throw new Error('Conjurador ausente: '+s.conj);
  });
  Object.assign(e,{turno:3,ativo:0,primeiro:0,etapa:'principal',prioridade:0,passes:0,pendencia:null,combate:null,grauNoTurno:false,agiuNoTurno:false,semAcaoTurnos:[0,0],log:['QA visual: cenário local montado.']});
  return e;
}
let fonte,mesa,escolhido='campos';
function abrir(id) {
  mesa?.fechar();fonte?.parar();escolhido=id;
  // A mesa registra eventos na raiz: cada cenário recebe uma raiz nova, como o roteador do aplicativo.
  const raiz=document.createElement('div');raiz.id='mesa-raiz';document.querySelector('#mesa-raiz').replaceWith(raiz);
  document.querySelector('#resultado-qa').textContent=id==='campos'?'Jogue Charco Fervente. Os dois Aliados têm 8 de Vida.':'Jogue Cemitério que Respira e escolha o Aliado do cemitério.';
  fonte=criarPartidaLocal({estado:montar(specs[id]),semente:19,modo:'tutorial',nomes:['QA · Jogador','QA · Oponente'],roteiroBot:()=>({tipo:'passar',jogador:1})});
  mesa=abrirMesa(document.querySelector('#mesa-raiz'),fonte,{aoSair:()=>abrir(escolhido)});
}
document.querySelector('#dois-campos').onclick=()=>abrir('campos');
document.querySelector('#ressurgimento').onclick=()=>abrir('ressurgir');
document.querySelector('#reiniciar').onclick=()=>abrir(escolhido);
addEventListener('pagehide',()=>{mesa?.fechar();fonte?.parar();});
abrir(escolhido);
