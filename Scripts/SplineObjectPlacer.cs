using UnityEngine;
using GPGre.QuadraticBezier;

namespace GPGre.QuadraticBezier
{
	/// <summary>
	/// Place an object on spline
	/// </summary>
	public class SplineObjectPlacer : MonoBehaviour {

		public BezierSpline spline;

		public float PositionOnSpline 
		{
			get{ return _positionOnSpline; 
			}
			set{
				PositionObjectOnSpline (value);
				_positionOnSpline = value;
			}
		}
		[SerializeField,HideInInspector]
		private float _positionOnSpline = 0;

		public bool followCurveDirection = true;

		void PositionObjectOnSpline(float newPosition)
		{
			transform.position = spline.GetPointUniform (newPosition);
			if(followCurveDirection)
				transform.rotation = Quaternion.LookRotation(spline.GetDirectionUniform(newPosition));
		}
	}
}
