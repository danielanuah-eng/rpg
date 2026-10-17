import { spawn } from 'node:child_process';
import { mkdirSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';
import assert from 'node:assert/strict';
const root=fileURLToPath(new URL('../',import.meta.url));
mkdirSync(join(root,'Artifacts'),{recursive:true});
const host=spawn(join(root,'Builds/Windows/ArenaTatica.exe'),['-batchmode','-logFile',join(root,'Artifacts/network-contract-host.log'),'--arena-host','--arena-smoke'],{cwd:root,windowsHide:true,stdio:'ignore'});
const delay=ms=>new Promise(r=>setTimeout(r,ms));
let ws,state,snapshot,checks=0;
let messages=[];
function open() {
  return new Promise((resolve,reject)=>{
    const candidate=new WebSocket('ws://127.0.0.1:9080');
    candidate.addEventListener('error',()=>reject(Error('Host ainda indisponível')),{once:true});
    candidate.addEventListener('open',()=>{ws=candidate;candidate.send(JSON.stringify({protocol:6,kind:'hello',token:'contrato'}));resolve();},{once:true});
    candidate.addEventListener('message',event=>{
      const m=JSON.parse(event.data); messages.push(m);
      if(m.kind==='state'||m.kind==='snapshot') state=m.state;
      if(m.kind==='snapshot') snapshot=m;
      if(m.kind==='ping') candidate.send(JSON.stringify({protocol:6,kind:'pong',ping:m.ping}));
    });
  });
}
async function waitFor(test,label,timeout=20000) { const end=Date.now()+timeout; while(!test()) {if(Date.now()>end) throw Error('Timeout: '+label); await delay(30);} }
function send(m) { ws.send(JSON.stringify({protocol:6,...m})); }
const active=()=>state.fighters.find(f=>f.id===state.timeline[state.active]);
const tile=c=>state.tiles[c.z*state.width+c.x];
// Casa vizinha livre, sem lutador e sem subida de 2+ níveis.
function step(f) {
  for(const [dx,dz] of [[1,0],[-1,0],[0,1],[0,-1]]) {
    const c={x:f.cell.x+dx,z:f.cell.z+dz};
    if(c.x<0||c.z<0||c.x>=state.width||c.z>=state.height) continue;
    const t=tile(c); if(t.terrain!==0||t.height-tile(f.cell).height>=2) continue;
    if(state.fighters.some(o=>o.hp>0&&o.cell.x===c.x&&o.cell.z===c.z)) continue;
    return c;
  }
  throw Error('sem casa vizinha livre');
}
async function rejected(command) {
  const before=messages.filter(m=>m.kind==='reject').length;
  send({kind:'command',command});
  await waitFor(()=>messages.filter(m=>m.kind==='reject').length>before,'comando inválido recusado'); checks++;
}
try {
  for(let i=0;i<360;i++) {try {await open();break;} catch {await delay(250);} }
  if(!ws) throw Error('O processo Host não iniciou a escuta em 90 segundos.');
  await waitFor(()=>state&&active()?.team===2,'turno do time 2');
  const me=active(), enemy=state.fighters.find(f=>f.team===1&&f.summoner===0);
  await rejected({kind:'passar',fighter:enemy.id,revision:state.revision});
  await delay(150);
  await rejected({kind:'passar',fighter:me.id,revision:0});
  await delay(150);
  assert.equal(state.rng,0); checks++;
  assert.ok(state.tiles.every(t=>t.trap===0||t.trap===2)); checks++;
  assert.equal(state.players,2); checks++;
  const to=step(me);
  send({kind:'command',command:{kind:'mover',fighter:me.id,cell:to,revision:state.revision}});
  await waitFor(()=>{const f=state.fighters.find(x=>x.id===me.id);return f.cell.x===to.x&&f.cell.z===to.z;},'movimento confirmado'); checks++;
  const before=JSON.stringify(state.fighters);
  snapshot=null; ws.close(); await delay(300); await open();
  await waitFor(()=>snapshot,'snapshot após reconexão');
  assert.equal(JSON.stringify(snapshot.state.fighters),before); checks++;
  // JsonUtility escreve lista vazia no lugar de eventos nulos: o snapshot não repete efeitos.
  assert.ok(!snapshot.events||snapshot.events.length===0); checks++;
  const forged=structuredClone(state); forged.fighters.find(f=>f.team===1).hp=999;
  send({kind:'state',state:forged}); await delay(150);
  snapshot=null; ws.close(); await delay(300); await open(); await waitFor(()=>snapshot,'snapshot após tentativa de forjar estado');
  assert.equal(JSON.stringify(snapshot.state.fighters),before); checks++;
  send({kind:'command',command:{kind:'passar',fighter:me.id,revision:state.revision}});
  await waitFor(()=>active()?.team===1,'turno devolvido'); checks++;
  writeFileSync(join(root,'Artifacts/network-contract.json'),JSON.stringify({passed:true,checks,scenarios:['ownership','stale revision','hidden rng','hidden traps','authoritative movement','reconnection','no replayed VFX','forged snapshot rejected','turn handoff']},null,2));
  console.log(`NETWORK CONTRACT PASS: ${checks} cenários`);
} catch(error) { console.error(error.message); console.error(messages.slice(-6).map(m=>m.kind+(m.text?':'+m.text:'')+(m.state?' rev'+m.state.revision+' ativo P'+(m.state.fighters.find(f=>f.id===m.state.timeline[m.state.active])?.team):'')).join(' | ')); process.exitCode=1; }
finally {ws?.close();host.kill();}
