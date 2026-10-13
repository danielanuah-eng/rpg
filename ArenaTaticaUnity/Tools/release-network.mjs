import {spawn} from 'node:child_process';
import {existsSync,readFileSync,writeFileSync,mkdirSync,unlinkSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {join} from 'node:path';
import assert from 'node:assert/strict';
const root=fileURLToPath(new URL('../',import.meta.url));
const executable=join(root,'Builds/Windows/ArenaTatica.exe');
const delay=ms=>new Promise(r=>setTimeout(r,ms));
async function wait(test,label,limit=120000) { const end=Date.now()+limit; while(!test()&&Date.now()<end) await delay(100); if(!test()) throw Error(label); }
async function run(players,cup) {
  const prefix=join(root,'Artifacts',`019-${cup?'cup':'room'}-${players}`), clients=[], pending=[];
  for(const ext of ['ready','reconnected','revealed','restarted','lobby','again','result']) if(existsSync(prefix+'.'+ext)) unlinkSync(prefix+'.'+ext);
  const host=spawn(executable,['-batchmode','-screen-width','1280','-screen-height','720','-logFile',prefix+'.log','--arena-seats',String(players),'--arena-smoke','--arena-release-qa','--arena-qa-prefix',prefix,...(cup?['--arena-qa-cup']:[])],{cwd:root,windowsHide:true,stdio:'ignore'});
  const exited=new Promise((resolve,reject)=>{host.on('exit',resolve);host.on('error',reject);});
  let failure=null, concealed=null, reconnected=false, revealed=false, restarted=false, lobby=false, checkingReconnect=false;
  function connect(seat) {
    const ws=new WebSocket('ws://127.0.0.1:9080'); clients.push(ws);
    const send=m=>ws.send(JSON.stringify({protocol:4,...m}));
    ws.addEventListener('open',()=>send({kind:'hello',token:'019-qa-'+seat,fighters:['ciantia'],pool:['ciantia','perry','ezequiela','damian','baltazar']}));
    ws.addEventListener('message',event=>{
      try {
        const m=JSON.parse(event.data); assert.equal(m.protocol,4);
        if(m.kind==='ping') send({kind:'pong',ping:m.ping});
        if(m.kind==='full') throw Error(m.text);
        if(m.kind==='draft') send({kind:'draft',text:m.text,fighters:m.text==='ban'?m.fighters.slice(0,2):m.fighters});
        if(cup||seat!==2) return;
        if(m.state) {
          assert.equal(m.state.players,players); assert.equal(m.state.rng,0);
          const secret=m.state.fighters.find(f=>f.team===1&&f.hidden);
          if(secret) {
            assert.deepEqual(secret.cell,{x:-1,z:-1});
            assert.ok(!(m.events??[]).some(e=>(e.actor===secret.id||e.target===secret.id)&&(e.path?.length||e.cell?.x>=0&&e.cell?.z>=0)),'evento entrega posição oculta');
            concealed=secret.id;
            if(m.kind==='snapshot'&&checkingReconnect) { reconnected=true; writeFileSync(prefix+'.reconnected','pass'); }
            else if(!checkingReconnect) {
              checkingReconnect=true; ws.close(); pending.push(delay(350).then(()=>connect(seat)));
            }
          } else if(concealed&&!revealed) {
            const f=m.state.fighters.find(f=>f.id===concealed); assert.ok(f&&!f.hidden&&f.cell.x>=0);
            assert.ok((m.events??[]).some(e=>e.kind==='revelar'&&e.target===concealed));
            revealed=true; writeFileSync(prefix+'.revealed','pass');
          } else if(revealed&&m.kind==='snapshot'&&!restarted) { restarted=true; writeFileSync(prefix+'.restarted','pass'); }
          else if(lobby&&m.kind==='snapshot') writeFileSync(prefix+'.again','pass');
        }
        if(restarted&&m.kind==='lobby') {lobby=true;writeFileSync(prefix+'.lobby','pass');}
      } catch(error) {failure=error;}
    });
    ws.addEventListener('error',()=>{if(!checkingReconnect) failure=Error('WebSocket não conectou');});
    return ws;
  }
  try {
    await wait(()=>existsSync(prefix+'.ready'),'Host não liberou a sala');
    for(let seat=2;seat<=players;seat++) connect(seat);
    await wait(()=>failure||existsSync(prefix+'.result'),'Ciclo de sala/torneio não terminou',180000);
    if(failure) throw failure;
    assert.notEqual(readFileSync(prefix+'.result','utf8'),'FAIL');
    if(!cup) assert.ok(reconnected&&revealed&&restarted&&lobby);
    const code=await exited; assert.equal(code,0);
    const log=readFileSync(prefix+'.log','utf8'); assert.ok(log.includes('ARENA_SMOKE_PASS')); assert.ok(!/NullReferenceException|IndexOutOfRangeException|ARENA_SMOKE_FAIL/.test(log));
    console.log(`${cup?'CUP':'ROOM'} ${players}: PASS`);
    return {players,mode:cup?'cup':'room',passed:true,result:readFileSync(prefix+'.result','utf8')};
  } finally {for(const ws of clients) ws.close();host.kill();await Promise.allSettled(pending);}
}
mkdirSync(join(root,'Artifacts'),{recursive:true});
const results=[];
try {for(const cup of [false,true]) for(const players of [2,3,6]) results.push(await run(players,cup));}
catch(error) {console.error(error.stack);process.exitCode=1;}
writeFileSync(join(root,'Artifacts/019-release-network.json'),JSON.stringify({passed:!process.exitCode,results},null,2));
