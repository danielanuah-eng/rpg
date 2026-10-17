// Checagem de compilação da Arena fora do Unity (Roslyn do Editor 6000.6.4f1). Uso, na pasta ArenaTaticaUnity:
//   node Tools/cc-check.js   (compila Assets/ArenaTatica/Scripts + Tests/Editor com as DLLs de Library/ScriptAssemblies)
const fs=require('fs'),path=require('path'),cp=require('child_process');
const E='C:/Program Files/Unity/Hub/Editor/6000.6.4f1/Editor/Data';
const refs=[E+'/NetStandard/ref/2.1.0/netstandard.dll'];
for(const d of [E+'/Managed/UnityEngine',E+'/NetStandard/compat/2.1.0/shims/netfx'])
  for(const f of fs.readdirSync(d)) if(f.endsWith('.dll')&&!/UnityEditor|Cecil|Bee|AsmDef|MSBuild/.test(f)) refs.push(d+'/'+f);
const SA=path.resolve('Library/ScriptAssemblies');
for(const n of ['Unity.InputSystem','UnityEngine.UI','Unity.Collections','Unity.Networking.Transport','Unity.Burst','Unity.Mathematics']) if(fs.existsSync(SA+'/'+n+'.dll')) refs.push(SA+'/'+n+'.dll');
for(const f of fs.readdirSync(SA)) if(/^Unity.Services..*.dll$/.test(f)) refs.push(SA+'/'+f);
const nunit=fs.readdirSync('Library/PackageCache').find(d=>d.startsWith('com.unity.ext.nunit@')); if(nunit) refs.push(path.resolve('Library/PackageCache',nunit,'net472/unity-custom/nunit.framework.dll'));
refs.push(E+'/Managed/UnityEngine/UnityEditor.CoreModule.dll');
const files=[];
const args=process.argv.slice(2); if(!args.length) args.push('Assets/ArenaTatica/Scripts','Assets/ArenaTatica/Tests/Editor');
for(const a of args) { const s=fs.statSync(a); if(s.isDirectory()) (function walk(d){ for(const f of fs.readdirSync(d)){ const p=path.join(d,f); if(fs.statSync(p).isDirectory()) walk(p); else if(p.endsWith('.cs')) files.push(p);} })(a); else files.push(a); }
const tmp=require('os').tmpdir(); const out=path.join(tmp,'arena-cc.dll');
const rsp=['-nologo','-noconfig','-nostdlib','-t:library','-langversion:9','-nowarn:CS0649,CS0414,CS0169,CS1701,CS0162,CS0219','-define:UNITY_EDITOR;UNITY_STANDALONE_WIN','-out:"'+out+'"',...refs.map(r=>'-r:"'+r+'"'),...files.map(f=>'"'+path.resolve(f)+'"')].join('\n');
fs.writeFileSync(path.join(tmp,'arena-cc.rsp'),rsp);
const r=cp.spawnSync(E+'/NetCoreRuntime/dotnet.exe',[E+'/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll','@'+path.join(tmp,'arena-cc.rsp')],{encoding:'utf8'});
const lines=(r.stdout+r.stderr).split(/\r?\n/).filter(l=>/error/.test(l));
console.log(lines.slice(0,40).join('\n')); console.log('errors',lines.length,'exit',r.status);
