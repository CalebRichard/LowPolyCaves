using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InfiniteTerrain))]
public class TerrainEditor : Editor {

    InfiniteTerrain m_terrain;
    Editor m_Editor;

    public override void OnInspectorGUI() {
        using (var check = new EditorGUI.ChangeCheckScope()) {

            base.OnInspectorGUI();

            if (check.changed) {

                //m_terrain.UpdateBlocks();
            }
        }

        DrawSettingsEditor(m_terrain.TerrainSettings, m_terrain.OnSettingsUpdated, ref m_terrain.foldout, ref m_Editor);
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor) {

        if (settings != null) {

            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            using var check = new EditorGUI.ChangeCheckScope();

            if (foldout) {

                CreateCachedEditor(settings, null, ref editor);
                m_Editor.OnInspectorGUI();

                if (check.changed)
                    onSettingsUpdated?.Invoke();
            }
        }
    }

    private void OnEnable() {

        m_terrain = (InfiniteTerrain)target;
    }
}
