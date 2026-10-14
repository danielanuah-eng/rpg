import {spawn} from 'node:child_process';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {join} from 'node:path';
const root=fileURLToPath(new URL('../',import.meta.url));
mkdirSync(join(root,'Artifacts'),{recursive:true});
const log=join(root,'Artifacts/020-ux.log');
const child=spawn(join(root,'Builds/Windows/ArenaTatica.exe'),['-screen-fullscreen','0','-screen-width','960','-screen-height','540','-logFile',log,'--arena-smoke','--arena-qa-render','--arena-ux-qa'],{cwd:root,windowsHide:true,stdio:'ignore'});
const result={passed:false,platform:'Windows',touchDevice:'simulated camera gesture; physical Android pending',captures:['020-ux-landscape.png','020-ux-portrait.png','020-opcoes-portrait.png','020-ux-six-players.png']};
const timer=setTimeout(()=>{result.error='Timeout UX';child.kill();},120000);
child.on('error',error=>{clearTimeout(timer);result.error=error.message;process.exitCode=1;});
child.on('exit',code=>{
  clearTimeout(timer); const text=readFileSync(log,'utf8');
  result.passed=code===0&&text.includes('ARENA_SMOKE_PASS')&&!/ARENA_SMOKE_FAIL|NullReferenceException|MissingReferenceException|IndexOutOfRangeException/.test(text);
  result.evidence=(text.match(/^.*ARENA_SMOKE_(?:PASS|FAIL).*$/m)||[])[0]??'Sem marcador';
  result.completedAt=new Date().toISOString();
  writeFileSync(join(root,'Artifacts/020-ux.json'),JSON.stringify(result,null,2));
  console.log(JSON.stringify(result,null,2)); if(!result.passed) process.exitCode=1;
});
