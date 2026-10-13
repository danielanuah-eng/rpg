import {spawn} from 'node:child_process';
import {existsSync,readFileSync,writeFileSync,unlinkSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {join} from 'node:path';
const root=fileURLToPath(new URL('../',import.meta.url));
const codeFile=join(root,'Artifacts/019-relay-code.txt');
const delay=ms=>new Promise(r=>setTimeout(r,ms));
const children=[];
function player(name,args) {
  const log=join(root,'Artifacts',name+'.log');
  const child=spawn(join(root,'Builds/Windows/ArenaTatica.exe'),['-batchmode','-screen-width','1280','-screen-height','720','-logFile',log,'--arena-smoke',...args],{cwd:root,windowsHide:true,stdio:'ignore'});
  children.push(child);
  const done=new Promise((resolve,reject)=>{child.on('exit',resolve);child.on('error',reject);});
  return {child,log,done};
}
async function deadline(promise,ms) {
  let timer;
  try {return await Promise.race([promise,new Promise((_,reject)=>{timer=setTimeout(()=>reject(Error('Timeout Relay')),ms);})]);}
  finally {clearTimeout(timer);}
}
const report={transport:'Unity Relay DTLS',platform:'Windows',sameComputer:true,completedAt:null,passed:false};
try {
  if(existsSync(codeFile)) unlinkSync(codeFile);
  const host=player('019-relay-host',['--arena-room','--arena-relay-report']);
  await deadline((async()=>{while(!existsSync(codeFile)) {if(host.child.exitCode!==null) throw Error('Host terminou antes de criar Relay');await delay(100);}})(),60000);
  const code=readFileSync(codeFile,'utf8').trim();
  if(!/^[A-Z0-9]{6}$/.test(code)) throw Error('Código Relay inválido');
  const client=player('019-relay-client',['--arena-room-join',code]);
  const exits=await deadline(Promise.all([host.done,client.done]),90000);
  report.players=[host,client].map((p,i)=>{
    const text=readFileSync(p.log,'utf8');
    const passed=exits[i]===0&&text.includes('ARENA_SMOKE_PASS')&&!/ARENA_SMOKE_FAIL|NullReferenceException|MissingReferenceException/.test(text);
    return {role:i?'client':'host',exit:exits[i],passed,evidence:(text.match(/^.*ARENA_SMOKE_(?:PASS|FAIL).*$/m)||[])[0]??'Sem marcador'};
  });
  report.passed=report.players.every(p=>p.passed);
  if(!report.passed) throw Error('Falhou a troca de comandos/turnos pelo Relay');
} catch(error) {report.error=error.message;process.exitCode=1;}
finally {
  for(const child of children) if(child.exitCode===null) child.kill();
  report.completedAt=new Date().toISOString();
  writeFileSync(join(root,'Artifacts/019-relay.json'),JSON.stringify(report,null,2));
  console.log(JSON.stringify(report,null,2));
}
