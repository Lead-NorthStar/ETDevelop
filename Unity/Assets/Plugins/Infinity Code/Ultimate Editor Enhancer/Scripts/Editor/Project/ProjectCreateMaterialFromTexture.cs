﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectCreateMaterialFromTexture
    {
        static ProjectCreateMaterialFromTexture()
        {
            ProjectItemDrawer.Register("CREATE_MATERIAL_FROM_TEXTURE", DrawButton, 10);
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectCreateMaterial) return;
            if (!item.hovered) return;
            Object asset = item.asset;
            if (asset == null) return;
            if (!(asset is Texture2D)) return;
        
            Rect r = item.rect;
            r.xMin = r.xMax - 18;
            r.height = 16;

            item.rect.xMax -= 18;

            Texture icon = EditorIconContents.material.image;
            string tooltip = "Create Material From Texture";

            ButtonEvent be = GUILayoutUtils.Button(r, TempContent.Get(icon, tooltip), GUIStyle.none);
            if (be == ButtonEvent.click)
            {
                Event e = Event.current;
                if (e.button == 0)
                {
                    Selection.activeObject = asset;
                    Material material = new Material(Shader.Find("Standard"));
                    material.mainTexture = asset as Texture2D;
                    ProjectWindowUtil.CreateAsset(material, asset.name + ".mat");
                }
            }
        }
    }
}