using UnityEditor;
using UnityEngine;

namespace TwilightGrove
{
    public class ForestTextureImporter : AssetPostprocessor
    {
        public override uint GetVersion() { return 4; }

        void OnPreprocessTexture()
        {
            if(assetPath != "Assets/Resources/Forest/OakBarkV1.png" &&
               assetPath != "Assets/Resources/Forest/OakLeavesV1.png" &&
               assetPath != "Assets/Resources/Forest/GrassGroundV1.png" &&
               assetPath != "Assets/Resources/Forest/ForestPathV1.png" &&
               assetPath != "Assets/Resources/Forest/SpruceNeedlesV1.png" &&
               assetPath != "Assets/Resources/Forest/ForestStoneV1.png" &&
               assetPath != "Assets/Resources/Forest/ForestMossV1.png") return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.textureShape=TextureImporterShape.Texture2D;
            importer.sRGBTexture=true;
            importer.alphaSource=TextureImporterAlphaSource.None;
            importer.mipmapEnabled=true;
            importer.maxTextureSize=512;
            importer.wrapMode=TextureWrapMode.Repeat;
            importer.filterMode=FilterMode.Trilinear;
            importer.textureCompression=TextureImporterCompression.Compressed;
        }
    }
}
