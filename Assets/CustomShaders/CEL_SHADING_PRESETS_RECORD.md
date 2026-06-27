# Cel Shading Presets Record

Date: 2026-06-23
Unity: 2022.3.60f1c1
Pipeline: URP 14.0.12

## Goal

Add reusable cel-shading preset options for the existing `Custom/NPR/Toon_Character_ImprovedOutline` shader so a material can move toward an anime/cel look without importing external source textures or reference assets.

The implementation controls shader parameters only. It does not replace the need for geometry, silhouettes, UVs, vertex-color normal data, or authored facial/hair/clothing line meshes when those details are required.

## Files

- `Assets/CustomShaders/ToonMaterialPreset.cs`
  - Defines `ToonMaterialPresetKind`.
  - Stores the preset names shown in the material inspector.
  - Applies all preset parameters through one tested API.
- `Assets/CustomShaders/Editor/ToonShaderGUI.cs`
  - Replaces the old hard-coded quick preset buttons with a preset dropdown.
  - Adds quick buttons for the two most useful anime/cel variants.
  - Supports applying one preset to multiple selected materials.
- `Assets/CustomShaders/Editor/ToonMaterialPresetTests.cs`
  - Verifies that the preset API writes expected material values.
  - Verifies that preset names are exposed for the inspector dropdown.

## Preset Options

`Soft Anime Cel (柔和赛璐璐)` is the closest default for the provided reference image. It keeps auto shade coloring on, uses a stable virtual key light, moderately hard shadow bands, dark but not pure-black outline color, and restrained rim/specular values.

`Hard Ink Cel (硬边墨线)` increases band hardness and outline strength. Use it when the model needs stronger line-art separation.

`High Contrast Mono (黑白高对比)` keeps the older black-and-white dramatic look but moves it into the shared preset system.

`Warm Comic Cel (暖色漫画)` keeps the older warm comic palette for materials that need a print/comic direction.

`Procedural Background Cel (程序化背景)` reduces outline width and softens shading. It is meant for simple generated background meshes, sky cards, water cards, or terrain-like shapes so the background stays behind the character visually.

## Rollback

The pre-change snapshot is stored at:

```text
_codex_backups/20260623-174434-cel-shading-presets
```

See:

```text
_codex_backups/20260623-174434-cel-shading-presets/RESTORE.md
```

## Usage

1. Select a material using `Custom/NPR/Toon_Character_ImprovedOutline`.
2. In the material inspector, open `预设选项 (Preset Options)`.
3. Choose a style from `风格预设 (Style Preset)`.
4. Click `应用预设 (Apply Preset)`.
5. Tune base color, outline width, shadow thresholds, and rim/specular values per model.

The preset application does not clear texture slots. Existing `_BaseMap` and `_OutlineWidthMap` assignments are preserved.

## Validation Notes

Unity batchmode could not be run against the live project while another Unity Editor instance had the project open. Test validation was run from a temporary project copy. Do not pass `-quit` with `-runTests`; the Unity Test Framework exits the editor after writing results.

```powershell
& 'E:\Unity\2022.3.60f1c1\Editor\Unity.exe' -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter ToonMaterialPresetTests -testResults Logs\codex-toon-preset-results.xml -logFile Logs\codex-toon-preset-tests.log
```

Verified result from the temporary copy:

```text
ToonMaterialPresetTests: 2 total, 2 passed, 0 failed
```
