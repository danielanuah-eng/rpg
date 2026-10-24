using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// PixelLab native cells are exported as strips, preserving scale and cell-center registration.
// This importer owns only assets listed by this delivery; Unity generates and maintains GUIDs.
public static class ArenaSomnaxGuardaImport
{
 const string Root="Assets/ArenaTatica/Resources/";
 const string Manifest=Root+"Manifestos/somnax-guarda-vozes.json";
 [Serializable] sealed class Delivery { public Entry[] entries; public string[] audio; }
 [Serializable] sealed class Entry { public string resource; public int frames,frameSize; public float[] unityPivot; public float pixelsPerUnit; }
 static Delivery Read() => File.Exists(Manifest)?JsonUtility.FromJson<Delivery>(File.ReadAllText(Manifest)):new Delivery{entries=Array.Empty<Entry>()};
 public static bool IsManaged(string path) => Read().entries.Any(e=>Root+e.resource+".png"==path.Replace('\\','/'));
 [MenuItem("Arena Tática/Importar Somnax guarda e vozes")]
 public static void Apply()
 {
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  var factory=new SpriteDataProviderFactories();factory.Init();int changed=0;
  foreach(var e in Read().entries)
  {
   string path=Root+e.resource+".png";
   var importer=AssetImporter.GetAtPath(path) as TextureImporter;
   if(!importer)throw new InvalidOperationException("Sem textura nativa: "+path);
   var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(path));
   bool dimensions=e.frames>0&&e.frameSize>0&&image.width<=8192&&image.height<=8192&&image.width==(long)e.frameSize*e.frames&&image.height==e.frameSize;UnityEngine.Object.DestroyImmediate(image);
   if(!dimensions)throw new InvalidOperationException("Dimensões inválidas: "+path);
   var pivot=new Vector2(e.unityPivot[0],e.unityPivot[1]);var mode=e.frames>1?SpriteImportMode.Multiple:SpriteImportMode.Single;
   var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
   var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
   var old=provider.GetSpriteRects();string name=Path.GetFileNameWithoutExtension(path);
   bool same=importer.textureType==TextureImporterType.Sprite&&importer.spriteImportMode==mode&&importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&importer.alphaIsTransparency&&importer.textureCompression==TextureImporterCompression.Uncompressed&&importer.spritePixelsPerUnit==e.pixelsPerUnit&&importer.maxTextureSize==8192&&settings.spriteMeshType==SpriteMeshType.Tight&&importer.spritePivot==pivot;
   if(mode==SpriteImportMode.Multiple)same&=old.Length==e.frames&&old.All(x=>x.pivot==pivot&&x.rect.height==e.frameSize)&&old.Select(x=>x.name).SequenceEqual(Enumerable.Range(0,e.frames).Select(i=>name+"_"+i));
   if(same)continue;
   importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=mode;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
   importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=e.pixelsPerUnit;importer.maxTextureSize=8192;
   importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spriteMeshType=SpriteMeshType.Tight;importer.SetTextureSettings(settings);importer.spritePivot=pivot;
   if(mode==SpriteImportMode.Multiple)
   {
    provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();var ids=old.ToDictionary(x=>x.name,x=>x.spriteID);
    var rects=Enumerable.Range(0,e.frames).Select(i=>{string n=name+"_"+i;return new SpriteRect{name=n,rect=new Rect(i*e.frameSize,0,e.frameSize,e.frameSize),pivot=pivot,alignment=SpriteAlignment.Custom,spriteID=ids.TryGetValue(n,out var id)?id:GUID.Generate()};}).ToArray();
    provider.SetSpriteRects(rects);
    var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();if(names!=null)names.SetNameFileIdPairs(rects.Select(x=>new SpriteNameFileIdPair(x.name,x.spriteID)));
    provider.Apply();
   }
   importer.SaveAndReimport();changed++;
  }
  foreach(string resource in Read().audio??Array.Empty<string>()) {
   if(!resource.StartsWith("Audio/Vozes/")||resource.Contains(".."))throw new InvalidOperationException("Recurso de voz inválido: "+resource);
   string file=Root+resource+".wav";
   var audio=AssetImporter.GetAtPath(file.Replace('\\','/')) as AudioImporter;if(!audio)throw new InvalidOperationException("Sem áudio: "+file);
   var settings=audio.defaultSampleSettings;
   if(audio.forceToMono&&settings.preloadAudioData&&settings.loadType==AudioClipLoadType.DecompressOnLoad&&settings.compressionFormat==AudioCompressionFormat.PCM&&settings.sampleRateSetting==AudioSampleRateSetting.PreserveSampleRate)continue;
   settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
   audio.forceToMono=true;settings.preloadAudioData=true;audio.defaultSampleSettings=settings;audio.SaveAndReimport();
  }
  AssetDatabase.SaveAssets();Debug.Log("SOMNAX_GUARDA_IMPORT_OK entries="+Read().entries.Length+" changed="+changed);
 }
}
