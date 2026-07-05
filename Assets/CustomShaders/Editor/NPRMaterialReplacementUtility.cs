using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class NPRMaterialReplacementOptions
{
    public bool IncludeInactive { get; set; } = true;
    public bool CreateMaterialAssets { get; set; } = true;
    public bool UseUndo { get; set; } = true;
    public bool ProcessOutlineNormals { get; set; } = true;

    public static NPRMaterialReplacementOptions CreateInMemoryForTests()
    {
        return new NPRMaterialReplacementOptions
        {
            IncludeInactive = true,
            CreateMaterialAssets = false,
            UseUndo = false,
            ProcessOutlineNormals = false,
        };
    }
}

public sealed class NPRMaterialReplacementResult
{
    public int RendererCount { get; internal set; }
    public int MaterialSlotCount { get; internal set; }
    public int CreatedMaterialCount { get; internal set; }
    public int ReusedMaterialCount { get; internal set; }
    public int ExplicitReplacementCount { get; internal set; }
    public int SkippedMaterialCount { get; internal set; }
    public int ProcessedNormalsCount { get; internal set; }
}

public sealed class NPRMaterialManagementOptions
{
    public bool IncludeInactive { get; set; } = true;
    public bool UseUndo { get; set; } = true;
    public bool ProcessOutlineNormals { get; set; } = true;

    public static NPRMaterialManagementOptions CreateInMemoryForTests()
    {
        return new NPRMaterialManagementOptions
        {
            IncludeInactive = true,
            UseUndo = false,
            ProcessOutlineNormals = false,
        };
    }
}

public sealed class NPRMaterialManagementResult
{
    public int RendererCount { get; internal set; }
    public int MaterialSlotCount { get; internal set; }
    public int ManagedMaterialCount { get; internal set; }
    public int ReusedMaterialSlotCount { get; internal set; }
    public int SkippedMaterialCount { get; internal set; }
    public int ProcessedNormalsCount { get; internal set; }
    public int CreatedMaterialCount { get; internal set; }
}

public static class NPRMaterialReplacementUtility
{
    private static readonly string[] SourceBaseTextureProperties =
    {
        "_BaseMap",
        "_MainTex",
        "_BaseTexture",
    };

    private static readonly string[] SourceBaseColorProperties =
    {
        "_BaseColor",
        "_Color",
        "_MainColor",
    };

    public static NPRMaterialReplacementResult ReplaceMaterials(
        GameObject root,
        NPRMaterialReplacementProfile profile,
        NPRMaterialReplacementOptions options = null)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        options ??= new NPRMaterialReplacementOptions();

        Shader targetShader = profile.ResolveTargetShader();
        if (targetShader == null)
        {
            throw new InvalidOperationException($"Could not find NPR shader '{NPRMaterialReplacementProfile.DefaultToonShaderName}'.");
        }

        NPRMaterialReplacementResult result = new NPRMaterialReplacementResult();
        Dictionary<Material, Material> generatedBySource = new Dictionary<Material, Material>();
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(options.IncludeInactive);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                continue;
            }

            bool changed = false;
            result.RendererCount++;
            result.MaterialSlotCount += materials.Length;

            for (int i = 0; i < materials.Length; i++)
            {
                Material sourceMaterial = materials[i];
                if (sourceMaterial == null)
                {
                    result.SkippedMaterialCount++;
                    continue;
                }

                if (profile.TryResolveRule(sourceMaterial, out Material explicitReplacement, out bool skip))
                {
                    if (skip)
                    {
                        result.SkippedMaterialCount++;
                        continue;
                    }

                    if (explicitReplacement != null)
                    {
                        materials[i] = explicitReplacement;
                        changed = true;
                        result.ExplicitReplacementCount++;
                        continue;
                    }
                }

                if (generatedBySource.TryGetValue(sourceMaterial, out Material generatedMaterial))
                {
                    materials[i] = generatedMaterial;
                    changed = true;
                    result.ReusedMaterialCount++;
                    continue;
                }

                generatedMaterial = CreateReplacementMaterial(sourceMaterial, targetShader, profile, options);
                generatedBySource.Add(sourceMaterial, generatedMaterial);
                materials[i] = generatedMaterial;
                changed = true;
                result.CreatedMaterialCount++;
            }

            if (changed)
            {
                if (options.UseUndo)
                {
                    Undo.RecordObject(renderer, "Apply NPR material replacement");
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }

            if (options.ProcessOutlineNormals && profile.ProcessMeshFilterNormals && TryProcessOutlineNormals(renderer, profile, options))
            {
                result.ProcessedNormalsCount++;
            }
        }

        if (options.CreateMaterialAssets && result.CreatedMaterialCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return result;
    }

    public static NPRMaterialManagementResult UpdateExistingNPRMaterials(
        GameObject root,
        NPRMaterialReplacementProfile profile,
        NPRMaterialManagementOptions options = null)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        options ??= new NPRMaterialManagementOptions();

        Shader targetShader = profile.ResolveTargetShader();
        if (targetShader == null)
        {
            throw new InvalidOperationException($"Could not find NPR shader '{NPRMaterialReplacementProfile.DefaultToonShaderName}'.");
        }

        NPRMaterialManagementResult result = new NPRMaterialManagementResult();
        HashSet<Material> managedMaterials = new HashSet<Material>();
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(options.IncludeInactive);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                continue;
            }

            bool rendererUsesNPR = false;
            result.RendererCount++;
            result.MaterialSlotCount += materials.Length;

            foreach (Material material in materials)
            {
                if (!IsTargetNPRMaterial(material, targetShader))
                {
                    result.SkippedMaterialCount++;
                    continue;
                }

                rendererUsesNPR = true;

                if (!managedMaterials.Add(material))
                {
                    result.ReusedMaterialSlotCount++;
                    continue;
                }

                MaterialSurfaceIdentity surfaceIdentity = MaterialSurfaceIdentity.Capture(material, profile);

                if (options.UseUndo)
                {
                    Undo.RecordObject(material, "Update NPR material from profile");
                }

                profile.ApplyStyleTo(material);
                surfaceIdentity.RestoreTo(material, profile);
                EditorUtility.SetDirty(material);
                result.ManagedMaterialCount++;
            }

            if (rendererUsesNPR && options.ProcessOutlineNormals && profile.ProcessMeshFilterNormals && TryProcessOutlineNormals(renderer, profile, new NPRMaterialReplacementOptions
            {
                IncludeInactive = options.IncludeInactive,
                CreateMaterialAssets = false,
                UseUndo = options.UseUndo,
                ProcessOutlineNormals = options.ProcessOutlineNormals,
            }))
            {
                result.ProcessedNormalsCount++;
            }
        }

        if (result.ManagedMaterialCount > 0)
        {
            AssetDatabase.SaveAssets();
        }

        return result;
    }

    public static NPRMaterialReplacementScan Scan(GameObject root, bool includeInactive = true)
    {
        if (root == null)
        {
            return NPRMaterialReplacementScan.Empty;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive);
        HashSet<Material> uniqueMaterials = new HashSet<Material>();
        int rendererCount = 0;
        int slotCount = 0;

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                continue;
            }

            rendererCount++;
            slotCount += materials.Length;

            foreach (Material material in materials)
            {
                if (material != null)
                {
                    uniqueMaterials.Add(material);
                }
            }
        }

        return new NPRMaterialReplacementScan(rendererCount, slotCount, uniqueMaterials.Count);
    }

    private static bool IsTargetNPRMaterial(Material material, Shader targetShader)
    {
        return material != null && material.shader != null && material.shader.name == targetShader.name;
    }

    private static Material CreateReplacementMaterial(
        Material sourceMaterial,
        Shader targetShader,
        NPRMaterialReplacementProfile profile,
        NPRMaterialReplacementOptions options)
    {
        Material replacement = new Material(targetShader)
        {
            name = $"{SanitizeMaterialName(sourceMaterial.name)}_NPR",
        };

        profile.ApplyStyleTo(replacement);
        CopySourceSurfaceIdentity(sourceMaterial, replacement, profile);

        if (options.CreateMaterialAssets)
        {
            EnsureAssetFolder(profile.OutputFolder);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{profile.OutputFolder}/{replacement.name}.mat");
            AssetDatabase.CreateAsset(replacement, assetPath);
        }

        return replacement;
    }

    private static void CopySourceSurfaceIdentity(
        Material sourceMaterial,
        Material replacement,
        NPRMaterialReplacementProfile profile)
    {
        if (profile.PreserveBaseColor && TryGetFirstColor(sourceMaterial, SourceBaseColorProperties, out Color sourceColor))
        {
            SetColorIfPresent(replacement, "_BaseColor", sourceColor);
        }

        if (profile.PreserveBaseMap && TryGetFirstTexture(sourceMaterial, SourceBaseTextureProperties, out Texture sourceTexture, out string sourceTextureProperty))
        {
            SetTextureIfPresent(replacement, "_BaseMap", sourceTexture);

            if (profile.PreserveTextureScaleAndOffset)
            {
                replacement.SetTextureScale("_BaseMap", sourceMaterial.GetTextureScale(sourceTextureProperty));
                replacement.SetTextureOffset("_BaseMap", sourceMaterial.GetTextureOffset(sourceTextureProperty));
            }
        }

        CopyTextureIfBothMaterialsSupport(sourceMaterial, replacement, "_ShadeMap");
        CopyTextureIfBothMaterialsSupport(sourceMaterial, replacement, "_ControlMap");
        CopyTextureIfBothMaterialsSupport(sourceMaterial, replacement, "_RampTexture");
        CopyTextureIfBothMaterialsSupport(sourceMaterial, replacement, "_OutlineWidthMap");
    }

    private static bool TryProcessOutlineNormals(
        Renderer renderer,
        NPRMaterialReplacementProfile profile,
        NPRMaterialReplacementOptions options)
    {
        if (renderer is not MeshRenderer)
        {
            return false;
        }

        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return false;
        }

        NPR_NormalsSmoother smoother = renderer.GetComponent<NPR_NormalsSmoother>();
        if (smoother == null)
        {
            smoother = renderer.gameObject.AddComponent<NPR_NormalsSmoother>();
            if (options.UseUndo)
            {
                Undo.RegisterCreatedObjectUndo(smoother, "Add NPR normals smoother");
            }
        }
        else if (options.UseUndo)
        {
            Undo.RecordObject(smoother, "Configure NPR normals smoother");
        }

        smoother.storeInVertexColors = true;
        smoother.vertexColorFormat = profile.VertexColorFormat;
        smoother.debugMode = profile.DebugNormalsProcessing;
        smoother.ProcessMesh();
        EditorUtility.SetDirty(smoother);
        return true;
    }

    private static bool TryGetFirstTexture(Material material, string[] propertyNames, out Texture texture, out string propertyName)
    {
        for (int i = 0; i < propertyNames.Length; i++)
        {
            propertyName = propertyNames[i];
            if (material.HasProperty(propertyName))
            {
                texture = material.GetTexture(propertyName);
                if (texture != null)
                {
                    return true;
                }
            }
        }

        texture = null;
        propertyName = null;
        return false;
    }

    private static bool TryGetFirstColor(Material material, string[] propertyNames, out Color color)
    {
        for (int i = 0; i < propertyNames.Length; i++)
        {
            string propertyName = propertyNames[i];
            if (material.HasProperty(propertyName))
            {
                color = material.GetColor(propertyName);
                return true;
            }
        }

        color = Color.white;
        return false;
    }

    private static void CopyTextureIfBothMaterialsSupport(Material source, Material target, string propertyName)
    {
        if (!source.HasProperty(propertyName) || !target.HasProperty(propertyName))
        {
            return;
        }

        Texture texture = source.GetTexture(propertyName);
        if (texture == null)
        {
            return;
        }

        target.SetTexture(propertyName, texture);
        target.SetTextureScale(propertyName, source.GetTextureScale(propertyName));
        target.SetTextureOffset(propertyName, source.GetTextureOffset(propertyName));
    }

    private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, texture);
        }
    }

    private static void SetColorIfPresent(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, color);
        }
    }

    private static void EnsureAssetFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/');
        if (parts.Length == 0 || parts[0] != "Assets")
        {
            throw new InvalidOperationException("Material output folder must be under Assets.");
        }

        string current = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static string SanitizeMaterialName(string materialName)
    {
        string safeName = string.IsNullOrWhiteSpace(materialName) ? "Material" : materialName;
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            safeName = safeName.Replace(invalidChar, '_');
        }

        return safeName;
    }

    private readonly struct MaterialSurfaceIdentity
    {
        private readonly bool hasBaseColor;
        private readonly Color baseColor;
        private readonly bool hasBaseMap;
        private readonly Texture baseMap;
        private readonly Vector2 baseMapScale;
        private readonly Vector2 baseMapOffset;

        private MaterialSurfaceIdentity(
            bool hasBaseColor,
            Color baseColor,
            bool hasBaseMap,
            Texture baseMap,
            Vector2 baseMapScale,
            Vector2 baseMapOffset)
        {
            this.hasBaseColor = hasBaseColor;
            this.baseColor = baseColor;
            this.hasBaseMap = hasBaseMap;
            this.baseMap = baseMap;
            this.baseMapScale = baseMapScale;
            this.baseMapOffset = baseMapOffset;
        }

        public static MaterialSurfaceIdentity Capture(Material material, NPRMaterialReplacementProfile profile)
        {
            bool capturedColor = profile.PreserveBaseColor && material.HasProperty("_BaseColor");
            bool capturedMap = profile.PreserveBaseMap && material.HasProperty("_BaseMap");
            Texture texture = capturedMap ? material.GetTexture("_BaseMap") : null;
            Vector2 scale = capturedMap ? material.GetTextureScale("_BaseMap") : Vector2.one;
            Vector2 offset = capturedMap ? material.GetTextureOffset("_BaseMap") : Vector2.zero;

            return new MaterialSurfaceIdentity(
                capturedColor,
                capturedColor ? material.GetColor("_BaseColor") : Color.white,
                capturedMap,
                texture,
                scale,
                offset);
        }

        public void RestoreTo(Material material, NPRMaterialReplacementProfile profile)
        {
            if (hasBaseColor && profile.PreserveBaseColor && material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }

            if (hasBaseMap && profile.PreserveBaseMap && material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", baseMap);

                if (profile.PreserveTextureScaleAndOffset)
                {
                    material.SetTextureScale("_BaseMap", baseMapScale);
                    material.SetTextureOffset("_BaseMap", baseMapOffset);
                }
            }
        }
    }
}

public readonly struct NPRMaterialReplacementScan
{
    public static readonly NPRMaterialReplacementScan Empty = new NPRMaterialReplacementScan(0, 0, 0);

    public NPRMaterialReplacementScan(int rendererCount, int materialSlotCount, int uniqueMaterialCount)
    {
        RendererCount = rendererCount;
        MaterialSlotCount = materialSlotCount;
        UniqueMaterialCount = uniqueMaterialCount;
    }

    public int RendererCount { get; }
    public int MaterialSlotCount { get; }
    public int UniqueMaterialCount { get; }
}
