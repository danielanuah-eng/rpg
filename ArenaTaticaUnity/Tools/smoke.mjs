import { spawn } from 'node:child_process';
import { readFileSync, mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';
const root=fileURLToPath(new URL('../',import.meta.url));
mkdirSync(join(root,'Artifacts'),{recursive:true});
const executable=join(root,'Builds/Windows/ArenaTatica.exe');
function run(name, args) {
  const log=join(root,'Artifacts',`${name}.log`);
  const child=spawn(executable,['-batchmode','-screen-width','1280','-screen-height','720','-logFile',log,...args,'--arena-smoke'],{cwd:root,windowsHide:true,stdio:'ignore'});
  return new Promise((resolve,reject)=>{
    const timeout=setTimeout(()=>{child.kill();reject(Error(`Timeout ${name}`));},60000);
    child.on('error',e=>{clearTimeout(timeout);reject(e);});
    child.on('exit',code=>{
      clearTimeout(timeout);
      const text=readFileSync(log,'utf8');
      const pass=code===0&&text.includes('ARENA_SMOKE_PASS')&&!/NullReferenceException|MissingReferenceException|error CS|ARENA_SMOKE_FAIL/.test(text);
      console.log(`${name}: ${pass?'PASS':'FAIL'} (exit ${code})`);
      if(!pass) console.log(text.slice(-4000));
      resolve(pass);
    });
  });
}
let ok=await run('local',[]);
const host=run('host',['--arena-host']);
await new Promise(r=>setTimeout(r,2500));
const client=run('client',['--arena-join','ws://127.0.0.1:9080']);
ok=(await Promise.all([host,client])).every(Boolean)&&ok;
process.exitCode=ok?0:1;
