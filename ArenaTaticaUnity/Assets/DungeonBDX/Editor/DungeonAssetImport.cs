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
}
