using UnityEngine;
using UnityEditor;

using System.Collections;
using System;

using GPGre.TunnelCreator;

[CustomEditor(typeof(TunnelSpawner))]
public class TunnelSpawnerEditor : Editor
{
    TunnelSpawner script;

    private bool sizeEditingMode = false;
    private int selectedPoint = -1;

    void OnEnable()
    {
        script = target as TunnelSpawner;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(script.bezierSpline == null);

        if (GUILayout.Button("Bake Tunnel"))
        {
            Transform tunnel = script.transform.Find("Tunnel");
            if (tunnel != null && script.overrideExistingTunnels)
                DestroyImmediate(tunnel.gameObject);

            script.SpawnTunnelPrefabs();
        }
        if (GUILayout.Button("Generate Collider"))
        {
            Transform col = script.transform.Find("Tunnel Collider");
            if (col != null)
                DestroyImmediate(col.gameObject);

            script.CreateTunnelCollider();
        }
        EditorGUI.BeginChangeCheck();
        sizeEditingMode = GUILayout.Toggle(sizeEditingMode, "Edit Tunnel sizes", EditorStyles.miniButton);
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
        EditorGUI.EndDisabledGroup();

        if (script.bezierSpline != null && selectedPoint > script.bezierSpline.CurveCount - 1)
            selectedPoint = -1;

        if (sizeEditingMode && selectedPoint != -1)
            ShowSizeEditingInspector();
    }

    private void ShowSizeEditingInspector()
    {
        if (GUILayout.Button("Reset Sizes"))
        {
            Undo.RecordObject(target, "Reset Sizes");
            script.RegenerateControlPointSizes();
            EditorUtility.SetDirty(script);
            SceneView.RepaintAll();
        }

        EditorGUILayout.LabelField("Point n° "+selectedPoint);

        EditorGUI.BeginChangeCheck();
        float size = EditorGUILayout.FloatField("Size : ", script.GetControlPointSize(selectedPoint));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Scale Size");
            script.SetControlPointSize(selectedPoint, size);
            EditorUtility.SetDirty(script);
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        if (!sizeEditingMode || script.bezierSpline == null)
            return;

        if (script.ControlPointSizeCount() != script.bezierSpline.CurveCount+1)
            script.RegenerateControlPointSizes();

        for (int i = 0; i < script.ControlPointSizeCount(); i++)
        {
            Vector3 pos = script.transform.TransformPoint(script.bezierSpline.GetControlPoint(i * 3));
            Vector3 dir = script.bezierSpline.GetControlPointDirection(i * 3);
            float size = script.GetControlPointSize(i);
            ShowSizeHandle(i, pos, dir, size);

        }
    }

    private void ShowSizeHandle(int id, Vector3 pos, Vector3 dir, float size)
    {
        Handles.color = id == selectedPoint ? Color.yellow : Color.red;
        Handles.DrawWireDisc(pos, dir, size);

        
        EditorGUI.BeginChangeCheck();
        float scale = Handles.ScaleSlider(size, pos, Vector3.Cross(Vector3.up, dir), Quaternion.identity, 3f, 0.5f);
        if (EditorGUI.EndChangeCheck())
        {
            selectedPoint = id;
            Undo.RecordObject(target, "Scale Size");
            script.SetControlPointSize(id, scale);
            EditorUtility.SetDirty(script);
            Repaint();
        }
    }
}
