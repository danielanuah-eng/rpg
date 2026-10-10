
const readInput=async(kind,key)=>{
 const cmd="& 'C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' -c \"import json,sys; from pathlib import Path; print(json.dumps(json.loads(Path('coordenacao/arena-pixelai/entregas/personagens-novos-2026-10-10/inputs-locais.json').read_text())[sys.argv[1]][sys.argv[2]]))\" "+kind+" "+key;
 let r=await tools.exec_command({cmd,login:false,yield_time_ms:1000,max_output_tokens:18000}); let out=r.output;
 while(r.session_id){r=await tools.write_stdin({session_id:r.session_id,chars:"",yield_time_ms:1000,max_output_tokens:18000});out+=r.output;}
 if(r.exit_code!==0)throw new Error("readInput failed "+key);return JSON.parse(out);
};
const save=async()=>{
 const state={characters:load("characters"),damian:load("damianStates"),special:load("specialStates"),animationPlans:load("animationPlans"),shopPlans:load("shopPlans"),ship:load("shipEditResult"),shipPlans:load("shipPlans")};
 await tools.apply_patch("*** Begin Patch\n*** Add File: "+load("deliveryDir")+"/estado-atual.json\n+"+JSON.stringify(state,null,2).replace(/\n/g,"\n+")+"\n*** End Patch");
};
const jobs=await tools.mcp__pixellab__list_jobs({include_recent:false});
const jt=jobs.content.filter(x=>x.type==="text").map(x=>x.text).join("\n");
let free=10-(Number(jt.match(/^(\d+) jobs:/)?.[1])||0),launched=0;
const shops=load("shopPlans")||[];
for(const p of shops){if(!free)break;if(p.job_id)continue;
 const data=await readInput("icons",p.id);
 const r=p.mode==="pixminimax"?await tools.mcp__pixellab__animate_image_pixminimax({first_frame_base64:data,last_frame_base64:data,description:p.action,frame_count:4,no_background:true,subject_description:"a small brass telescope shop icon",initial_pose:"stationary brass telescope icon",direction:"south",view:"high top-down"}):await tools.mcp__pixellab__animate_image({first_frame_base64:data,last_frame_base64:data,action:p.action,frame_count:4,no_background:true});
 const tx=r.content.filter(x=>x.type==="text").map(x=>x.text).join("\n");
 const jid=tx.match(/^(?:job_id|id): ([\da-f-]+)/m)?.[1];
 if(!r.isError&&jid){p.job_id=jid;p.asset_id=tx.match(/^asset_id: ([\da-f-]+)/m)?.[1];free--;launched++;store("shopPlans",shops);text({queued:"loja/"+p.id,job_id:jid});await save();}
 else{text({error:p.id,result:tx.slice(0,400)});break;}
}
const ships=load("shipPlans")||[];
for(const p of ships){if(!free)break;if(p.job_id)continue;
 const url="https://api.pixellab.ai/mcp/images/5234edf2-2d26-4f73-8045-c3449d5830d7/download";
 const args={first_frame_url:url,action:p.action,frame_count:p.frames,no_background:true};
 if(p.emptyEnd)args.last_frame_base64=await readInput("blank","ship");else args.last_frame_url=url;
 const r=await tools.mcp__pixellab__animate_image(args),tx=r.content.filter(x=>x.type==="text").map(x=>x.text).join("\n");
 const jid=tx.match(/^(?:job_id|id): ([\da-f-]+)/m)?.[1];
 if(!r.isError&&jid){p.job_id=jid;p.asset_id=tx.match(/^asset_id: ([\da-f-]+)/m)?.[1];free--;launched++;store("shipPlans",ships);text({queued:"navio/"+p.name,job_id:jid});await save();}
 else{text({error:"navio/"+p.name,result:tx.slice(0,400)});break;}
}
const plans=load("animationPlans");
for(const p of plans){
 if(!free)break;
 const actor=p.stateOwner ? (p.stateOwner==="damian"?load("damianStates")[p.stateKey]:load("specialStates")[p.stateKey]):load("characters").find(x=>x.id===p.asset);
 if(!actor?.ready)continue;
 const target=p.depState?(p.depOwner==="damian"?load("damianStates")[p.depState]:load("specialStates")[p.depState]):null;
 if(p.depState&&!target?.ready)continue;
 p.character_id=actor.character_id;
 const pending=p.directions.filter(x=>!p.requested.includes(x));if(!pending.length)continue;
 const dirs=pending.slice(0,(p.depState || p.emptyStart || p.returnToBase && p.mode!=="template")?1:free);
 const args={character_id:p.character_id,animation_name:p.name,directions:dirs,frame_count:p.frames,keep_first_frame:true};
 if(p.group_id)args.animation_group_id=p.group_id;
 if(p.mode==="template"){args.mode="template";args.template_animation_id=p.template_animation_id;}
 else{args.mode="v3";args.action_description=p.action;args.ai_freedom=10;}
 if(target){
  if(p.key==="gryvro-takeoff")args.custom_start_frame_base64=await readInput("padding",dirs[0]);
  else args.custom_start_frame_base64=await readInput("rotation",actor.character_id+"_"+dirs[0]);
  args.end_frame_base64=await readInput("rotation",target.character_id+"_"+dirs[0]);
 }
 if(p.emptyStart){args.custom_start_frame_base64=await readInput("blank",String(p.emptyStart));args.end_frame_url=actor.rotations[dirs[0]];}
 else if(!target&&p.mode!=="template"&&p.returnToBase){args.end_frame_url=actor.rotations[dirs[0]];}
 const r=await tools.mcp__pixellab__animate_character(args),tx=r.content.filter(x=>x.type==="text").map(x=>x.text).join("\n");
 const gid=tx.match(/^group: ([\da-f-]+)/m)?.[1];
 if(!r.isError&&gid){p.group_id=gid;p.requested.push(...dirs);free-=dirs.length;launched+=dirs.length;store("animationPlans",plans);text({queued:p.key,dirs,group:gid});await save();}
 else{text({error:p.key,result:tx.slice(0,450)});break;}
}
await save();
text({launched,pendingDirections:plans.reduce((n,p)=>n+p.directions.length-p.requested.length,0),pendingIcons:shops.filter(p=>!p.job_id).length});

