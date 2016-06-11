using UnityEngine;
using UnityEditor;
using GPGre.QuadraticBezier;

[CustomEditor(typeof(SplineObjectPlacer))]
public class SplineObjectPlacerInspector : Editor {

	SplineObjectPlacer script;

	void OnEnable()
	{
		script = target as SplineObjectPlacer;
	}

	override public void OnInspectorGUI()
	{
		script.spline = (BezierSpline)EditorGUILayout.ObjectField (script.spline, typeof(BezierSpline), true);

		if (script.spline == null)
			return;

		EditorGUI.BeginChangeCheck ();
		float positionValue = EditorGUILayout.Slider(script.PositionOnSpline,0,1);
		if (EditorGUI.EndChangeCheck ()) {
			script.PositionOnSpline = positionValue;
		}

		EditorGUI.BeginChangeCheck ();
		script.followCurveDirection = EditorGUILayout.ToggleLeft("Follow Curve Direction",script.followCurveDirection);
		if (EditorGUI.EndChangeCheck ()) {
			script.PositionOnSpline = positionValue;
		}


	}
}
