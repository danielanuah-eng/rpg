using System;
using UnityEditor;

// Scoped to this original kit. Runtime static batching needs readable source meshes.
public sealed class DungeonAssetImport : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if(!assetPath.StartsWith("Assets/DungeonBDX/Art/Resources/DungeonBDX/Models/",StringComparison.Ordinal))return;
        var model=(ModelImporter)assetImporter;model.isReadable=true;model.importAnimation=false;
        model.importCameras=false;model.importLights=false;
    }

    // Board art is pixel art enlarged 8x: keep the pixels crisp and the colours exact.
    void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/DungeonBDX/Art/Resources/DungeonBDX/Iso/",StringComparison.Ordinal))return;
        var texture=(TextureImporter)assetImporter;texture.filterMode=UnityEngine.FilterMode.Point;
        texture.textureCompression=TextureImporterCompression.Uncompressed;texture.mipmapEnabled=true;
        texture.wrapMode=assetPath.Contains("/Frame/")?UnityEngine.TextureWrapMode.Repeat:UnityEngine.TextureWrapMode.Clamp;
    }
}
