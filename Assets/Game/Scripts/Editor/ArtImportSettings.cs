using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Keeps the art folder importing the way a phone build needs it.
///
/// New textures pick the rules up automatically. The menu item re-applies them
/// to everything, which is what you run after dropping in a batch of art or
/// changing a rule here.
///
/// The two rules that matter most: sprites that go into an atlas stay
/// uncompressed at source, because the atlas is what gets compressed, and
/// nothing generates mipmaps, because nothing is ever seen at a distance.
/// </summary>
public class ArtImportSettings : AssetPostprocessor
{
    private const string ArtFolder = "Assets/Game/Art";
    private const int AtlasMaxSize = 2048;
    private const TextureImporterFormat MobileFormat = TextureImporterFormat.ASTC_6x6;

    private static readonly string[] MobilePlatforms = { "Android", "iPhone" };

    /// <summary>Applies the rules to art the first time it is imported.</summary>
    private void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;

        if (!assetPath.StartsWith(ArtFolder, System.StringComparison.Ordinal) || !importer.importSettingsMissing)
        {
            return;
        }

        ApplySpriteRules(importer, isAtlased: false);
    }

    [MenuItem("Tools/Word Game/Apply Mobile Art Settings")]
    public static void ApplyToAllArt()
    {
        HashSet<string> atlased = CollectAtlasedTextures();
        int changed = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                continue;
            }

            ApplySpriteRules(importer, atlased.Contains(path));
            importer.SaveAndReimport();
            changed++;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
        {
            ApplyAtlasRules(AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid)));
            changed++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"ArtImportSettings: updated {changed} assets ({atlased.Count} textures are atlased).");
    }

    private static void ApplySpriteRules(TextureImporter importer, bool isAtlased)
    {
        importer.textureType = TextureImporterType.Sprite;
        // The sprite mode is an authoring decision: several sheets here are set
        // to Multiple with named sub-sprites, and forcing Single throws those
        // away along with every reference to them.
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;

        // Nothing on the board uses the sprite outline for physics; the tile has
        // its own box collider.
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings);

        if (isAtlased)
        {
            // The atlas compresses the packed result, so the source has to stay
            // lossless or the artefacts are baked in twice.
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            foreach (string platform in MobilePlatforms)
            {
                TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings(platform);
                platformSettings.overridden = false;
                importer.SetPlatformTextureSettings(platformSettings);
            }

            return;
        }

        importer.textureCompression = TextureImporterCompression.Compressed;

        foreach (string platform in MobilePlatforms)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings(platform);
            platformSettings.overridden = true;
            platformSettings.format = MobileFormat;
            platformSettings.maxTextureSize = AtlasMaxSize;
            platformSettings.compressionQuality = 50;
            importer.SetPlatformTextureSettings(platformSettings);
        }
    }

    private static void ApplyAtlasRules(SpriteAtlas atlas)
    {
        if (atlas == null)
        {
            return;
        }

        atlas.SetIncludeInBuild(true);

        SpriteAtlasPackingSettings packing = atlas.GetPackingSettings();
        packing.padding = 4;
        // Rotation and tight packing save atlas space but break sliced and tiled
        // UI sprites, which is most of what this atlas holds.
        packing.enableRotation = false;
        packing.enableTightPacking = false;
        atlas.SetPackingSettings(packing);

        SpriteAtlasTextureSettings texture = atlas.GetTextureSettings();
        texture.generateMipMaps = false;
        texture.readable = false;
        texture.filterMode = FilterMode.Bilinear;
        atlas.SetTextureSettings(texture);

        foreach (string platform in MobilePlatforms)
        {
            TextureImporterPlatformSettings platformSettings = atlas.GetPlatformSettings(platform);
            platformSettings.overridden = true;
            platformSettings.format = MobileFormat;
            platformSettings.maxTextureSize = AtlasMaxSize;
            platformSettings.compressionQuality = 50;
            atlas.SetPlatformSettings(platformSettings);
        }

        EditorUtility.SetDirty(atlas);
    }

    /// <summary>Every texture that some atlas already packs, directly or through its folder.</summary>
    private static HashSet<string> CollectAtlasedTextures()
    {
        var atlased = new HashSet<string>();

        foreach (string guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid));
            if (atlas == null)
            {
                continue;
            }

            foreach (Object packable in atlas.GetPackables())
            {
                string path = AssetDatabase.GetAssetPath(packable);

                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (string textureGuid in AssetDatabase.FindAssets("t:Texture2D", new[] { path }))
                    {
                        atlased.Add(AssetDatabase.GUIDToAssetPath(textureGuid));
                    }
                }
                else
                {
                    atlased.Add(path);
                }
            }
        }

        return atlased;
    }
}
