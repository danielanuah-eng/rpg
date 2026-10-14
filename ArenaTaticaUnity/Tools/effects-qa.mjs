import {spawn} from 'node:child_process';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {join} from 'node:path';
const root=fileURLToPath(new URL('../',import.meta.url));
mkdirSync(join(root,'Artifacts'),{recursive:true});
const log=join(root,'Artifacts/021-effects.log');
const child=spawn(join(root,'Builds/Windows/ArenaTatica.exe'),['-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',log,'--arena-smoke','--arena-effects-qa'],{cwd:root,windowsHide:true,stdio:'ignore'});
const timer=setTimeout(()=>{child.kill();},120000);
child.on('error',error=>{clearTimeout(timer);console.error(error.message);process.exitCode=1;});
child.on('exit',code=>{
  clearTimeout(timer);
  const text=readFileSync(log,'utf8');
  const phases=[...text.matchAll(/ARENA_EFFECT_PHASE: (\S+) quadros=(\d+)/g)].map(x=>({phase:x[1],frames:Number(x[2])}));
  const result={passed:code===0&&text.includes('ARENA_SMOKE_PASS')&&phases.length===11&&!/ARENA_SMOKE_FAIL|NullReferenceException|MissingReferenceException|IndexOutOfRangeException/.test(text),platform:'Windows',completedAt:new Date().toISOString(),phases,fixture:'Eventos de apresentação: forma, esconder, armadilha, invocação, cura, zona e compra. Não simula o balanceamento de uma partida.'};
  writeFileSync(join(root,'Artifacts/021-effects.json'),JSON.stringify(result,null,2));
  console.log(JSON.stringify(result,null,2)); if(!result.passed) process.exitCode=1;
});
