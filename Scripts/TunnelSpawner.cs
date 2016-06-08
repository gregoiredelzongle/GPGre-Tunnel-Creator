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
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using GPGre.QuadraticBezier;
using GPGre.TunnelCreator.Modifiers;

namespace GPGre.TunnelCreator
{
    /// <summary>
    /// Tunnel creation tool along quadratic Bezier Splines
    /// </summary>
    public class TunnelSpawner : MonoBehaviour
    {
        #region Prefab Parameters

        [Header("Prefab Parameters")]
        // Prefab to be instantiated along the spline in a tunnel fashion
        public GameObject tunnelPrefab;

        // Prefab rotation offset
        public Vector3 rotationOffset;

        #endregion

        #region Tunnel Parameters

        [Header("Tunnel Parameters")]
        // Spline used to generate the tunnel
        public BezierSpline bezierSpline;

		//Tunnel Default Material
		public Material defaultMaterial;

        // Prefab spawn density
        public Vector2 tunnelDensity = Vector2.one;

        // Default ring size
        public float defaultRingSize = 2.5f;

        // Perlin noise parameters
        [Range(0,1)]
        public float noiseSize = 0.5f;
        public float noiseAmount = 0.3f;

        #endregion

        #region Bake Parameters

        [Header("Bake Parameters")]
        // Do we have to create a new gameObject or replacing the old one
        public bool overrideExistingTunnels = true;

		[Header("Mesh Parameters")]
        // Collider parameters
        public float meshInnerOffset = 0.3f;
        public float meshPrecision = 0.2f;
        public int meshRadiusVertexCount = 12;

        #endregion

        #region Private Variables

        // Hold every Curve Point size
        [SerializeField]
        [HideInInspector]
        private float[] controlPointSizes;

        #endregion

        #region Spawn Prefabs Methods

        /// <summary>
        /// Create a new tunnel gameobject using set parameters
        /// </summary>
        public void SpawnTunnelPrefabs()
        {
            GameObject tunnel = new GameObject("Tunnel");

            tunnel.transform.SetParent(transform, false);
            float tunnelLength = bezierSpline.SplineDistance;
            int steps = Mathf.RoundToInt(tunnelLength * tunnelDensity.x);
            for (int i = 0; i < steps; i++)
            {
				float t = Mathf.InverseLerp (0, steps - 1, i);
                Vector3 pos = bezierSpline.GetPointUniform(t);
                Vector3 dir = bezierSpline.GetDirectionUniform(t);
                float size = GetRingSize(t);
                float noiseZ = i * noiseSize;
				CreateRing(tunnel, pos, dir, noiseZ, i, steps, size);
            }
        }

        /// <summary>
        /// Create a ring of prefabs
        /// </summary>
		private void CreateRing(GameObject parent, Vector3 pos, Vector3 dir, float noiseZ, int index, int steps, float radius)
        {
            GameObject ring = new GameObject("Ring " + index.ToString());

            // Set ring Transform values
            ring.transform.SetParent(parent.transform, false);
            ring.transform.position = pos;
            ring.transform.rotation = Quaternion.LookRotation(dir);

            // Set values
            int amount = Mathf.RoundToInt(Mathf.PI * radius * tunnelDensity.y);

            //Create ring
			for (int i = 0; i < amount; i++)
			{
				float angle = ((float)i / (float)amount) * 360;
				float noise = Mathf.PerlinNoise(i * noiseSize, noiseZ) * noiseAmount;
				if(ApplyModifiersCondition(index,i,steps,amount))
					SpawnPrefabAroundParent(ring.transform, index, i, steps, amount, angle, radius, noise);
			}
        }

		void SpawnPrefabAroundParent(Transform parent, int x, int y, int width, int height, float angle, float distance, float noise)
		{
			// Calculate prefab position
			Quaternion rot = Quaternion.Euler(Vector3.forward * angle);
			Vector3 localPosition = rot * Vector3.up * (distance + noise);
			localPosition = ApplyModifiersPrefabPosition (localPosition, x, y, width, height);

			// Instantiate Prefab
			#if UNITY_EDITOR
			GameObject go = PrefabUtility.InstantiatePrefab(tunnelPrefab) as GameObject;
			#else
			GameObject go = Instantiate<GameObject>(ringPartPrefab);
			#endif

			// Set prefab Transform values
			go.transform.SetParent(parent, false);
			go.transform.localRotation = rot * Quaternion.Euler(rotationOffset + Vector3.up * noise * 360);
			go.transform.localPosition = localPosition;

		}
        #endregion

        #region Tunnel Mesh Methods

		public void CreateTunnelMesh()
		{
			GameObject go = new GameObject("Tunnel Mesh");

			MeshFilter meshFilter = go.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();

			go.transform.SetParent(transform, false);

			Mesh mesh = new Mesh();

			mesh.name = go.name;

			// Mesh Generation Parameters
			mesh.vertices  = TunnelVertices();
			mesh.triangles = TunnelTriangles();
			mesh.uv = TunnelUV ();
			mesh.RecalculateNormals ();

			meshRenderer.material = defaultMaterial;

			meshFilter.sharedMesh = mesh;
		}

        public void CreateTunnelCollider()
        {
            GameObject col = new GameObject("Tunnel Collider");

            MeshCollider meshCol = col.AddComponent<MeshCollider>();

            col.transform.SetParent(transform, false);

			meshCol.sharedMesh = new Mesh();
			meshCol.sharedMesh.name = "Tunnel Collider";
			meshCol.sharedMesh.Clear ();
			meshCol.sharedMesh.vertices = TunnelVertices();
			meshCol.sharedMesh.triangles = TunnelTriangles();
        }

        Vector3[] TunnelVertices()
        {


            float tunnelLength = bezierSpline.SplineDistance;
            int ringAmount = Mathf.RoundToInt(tunnelLength * tunnelDensity.x * meshPrecision);
            Vector3[] vertices = new Vector3[ringAmount * meshRadiusVertexCount];

            for (int z = 0, i = 0; z < ringAmount; z++)
            {
				float t = Mathf.InverseLerp (0, ringAmount-1, z);
                Vector3 ringCenter = bezierSpline.GetPointUniform(t);
                Vector3 ringDir = bezierSpline.GetDirectionUniform(t);

				if (controlPointSizes == null)
					RegenerateControlPointSizes ();
				
                float size = GetRingSize(t);

                for (int x = 0; x < meshRadiusVertexCount; x++, i++)
                {
					float angle = Mathf.InverseLerp(0,meshRadiusVertexCount-1,x) * 360;
                    Quaternion rot = Quaternion.LookRotation(ringDir) * Quaternion.Euler(Vector3.forward * angle);
                    vertices[i] = (rot * Vector3.up * (size + meshInnerOffset)) + transform.InverseTransformPoint(ringCenter);
                }
            }
            return vertices;
        }

        int[] TunnelTriangles()
        {
            float tunnelLength = bezierSpline.SplineDistance;
            int ringAmount = Mathf.RoundToInt(tunnelLength * tunnelDensity.x * meshPrecision);

            int numTriangles = ((ringAmount - 1) * meshRadiusVertexCount) * 2;
            int[] triangles = new int[numTriangles*3];

			triangles[0] = meshRadiusVertexCount - 1;
			triangles[1] = meshRadiusVertexCount;
            triangles[2] = 0;

			for (int t = 3, i = 0; t < triangles.Length-6; t += 6, i += 1)
            {
				triangles[t] = i + meshRadiusVertexCount;
                triangles[t + 1] = i + 1;
                triangles[t + 2] = i;

				triangles[t + 3] = i + meshRadiusVertexCount + 1;
                triangles[t + 4] = i + 1;
				triangles[t + 5] = i + meshRadiusVertexCount;

            }
			triangles[triangles.Length - 3] = (meshRadiusVertexCount * ringAmount) - 1;
			triangles[triangles.Length - 2] = meshRadiusVertexCount * (ringAmount - 1);
			triangles[triangles.Length - 1] = (meshRadiusVertexCount * (ringAmount - 1)) - 1;

            return triangles;
        }

		Vector2[] TunnelUV()
		{
			float tunnelLength = bezierSpline.SplineDistance;
			int ringAmount = Mathf.RoundToInt(tunnelLength * tunnelDensity.x * meshPrecision);

			Vector2[] uv = new Vector2[ringAmount * meshRadiusVertexCount];

			for (int z = 0, i = 0; z < ringAmount; z++)
			{
				for (int x = 0; x < meshRadiusVertexCount; x+=2, i+=2)
				{
					if (z % 2 == 0) {
						uv [i] = new Vector2 (0, 1);
						if(i%meshRadiusVertexCount != meshRadiusVertexCount-1)
							uv [i + 1] = new Vector2 (0, 0);

					} else {
						uv [i] = new Vector2 (1, 1);
						if(i%meshRadiusVertexCount != meshRadiusVertexCount-1)
							uv [i + 1] = new Vector2 (1, 0);
					}

				}
			}
			return uv;
		}

        #endregion

        #region Tunnel Size Modifiers

        public float GetRingSize(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = controlPointSizes.Length - 2;
            }
            else
            {
                t = Mathf.Clamp01(t) * (ControlPointSizeCount() - 1);
                i = (int)t;
                t -= i;
            }

            return Mathf.Lerp(controlPointSizes[i], controlPointSizes[i + 1], t);
        }

        public float GetControlPointSize(int i)
        {
            return controlPointSizes[i];
        }

        public void SetControlPointSize(int i, float size)
        {
            controlPointSizes[i] = size;
        }

        public void RegenerateControlPointSizes()
        {
            if (bezierSpline == null)
            {
                controlPointSizes = null;
                return;
            }

            float[] newControlPointSizes = new float[bezierSpline.CurveCount + 1];

            for (int i = 0; i < newControlPointSizes.Length; i++)
            {
				if(i<controlPointSizes.Length)
					newControlPointSizes[i] = controlPointSizes[i];
				else
                	newControlPointSizes[i] = defaultRingSize;
            }
			controlPointSizes = newControlPointSizes;
        }

        public int ControlPointSizeCount()
        {
            if (controlPointSizes == null)
                RegenerateControlPointSizes();
            return controlPointSizes.Length;
        }

        #endregion

		#region Modifiers Functions

		bool ApplyModifiersCondition(int x, int y, int width, int height)
		{
			BaseModifier[] modifiers = GetComponents<BaseModifier> ();

			for (int i = 0; i < modifiers.Length; i++) {
				if (!modifiers [i].ApplyModifierCondition (x, y, width, height))
					return false;
			}
			return true;
		}

		Vector3 ApplyModifiersPrefabPosition(Vector3 localPos,int x, int y, int width, int height)
		{
			BaseModifier[] modifiers = GetComponents<BaseModifier> ();

			for (int i = 0; i < modifiers.Length; i++) {
				localPos = modifiers [i].ApplyModifierPosition (localPos, x, y, width, height);
			}
			return localPos;
		}

		#endregion

		/*
        #region Gizmos
        
        void OnDrawGizmosSelected()
        {

            if (vertices == null)
                return;

            Gizmos.color = Color.red;

            foreach (Vector3 vert in vertices)
            {
                Gizmos.DrawSphere(transform.TransformPoint(vert), 0.1f);
            }
        }
        
        #endregion
        */

    }
}
