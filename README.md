# NPR Shader Package Export

This folder is a portable copy of the NPR / toon shader work from the Unity project at:

`F:\Unity_Project\Graphic\Graphic`

## Contents

- `Assets/CustomShaders/`
  - Toon shaders, HLSL include files, screen-space outline shader, debug visualization shader, post-process shader, renderer feature scripts, editor GUI scripts, presets, and shader documentation.
- `Assets/Settings/`
  - `URP-HighFidelity.asset`
  - `URP-HighFidelity-Renderer.asset`
  - These preserve the renderer setup that references the NPR post-process renderer feature.
- `Assets/Materials/`
  - Sample materials that reference the custom shaders.
- `Assets/Scripts/`
  - `ShowcaseRotator.cs`, a small optional scene/demo helper.

Unity `.meta` files are intentionally included to preserve asset GUIDs when importing this package into another Unity project.

## Not Included

- `Library/`
- `_codex_backups/`
- Unity MCP package files
- Full sample scene assets, models, screenshots, and generated Unity cache files

## Expected Environment

- Unity 2022.3.x
- Universal Render Pipeline (URP) 14.x

If importing into another URP project, copy this folder's `Assets` contents into the target project and then verify that the active URP renderer uses the included renderer asset or has the NPR style renderer feature added manually.
