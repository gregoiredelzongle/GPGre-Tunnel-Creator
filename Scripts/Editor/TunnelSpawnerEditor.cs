//
// GPGre Tunnel Creator - Tunnel Creation Tool
//
// Copyright (C) 2016 Gregoire Delzongle
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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

        if (GUILayout.Button("Populate Tunnel With Prefabs"))
        {
			if (script.overrideExistingTunnels) {
				Transform tunnel = script.transform.Find ("Tunnel");
				if (tunnel != null)
					DestroyImmediate (tunnel.gameObject);
			}
            script.SpawnTunnelPrefabs();
        }
        if (GUILayout.Button("Generate Collider"))
        {
			if (script.overrideExistingTunnels) {
				Transform col = script.transform.Find ("Tunnel Collider");
				if (col != null)
					DestroyImmediate (col.gameObject);
			}

            script.CreateTunnelCollider();
        }
		if (GUILayout.Button("Generate Mesh"))
		{
			if (script.overrideExistingTunnels) {
				Transform col = script.transform.Find ("Tunnel Mesh");
				if (col != null)
					DestroyImmediate (col.gameObject);
			}
			script.CreateTunnelMesh ();

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
