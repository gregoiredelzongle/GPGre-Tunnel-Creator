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

        // Collider parameters
        public float colliderOffset = 0.3f;
        public float colliderPrecision = 0.2f;
        public int colliderRadiusVertexCount = 12;

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
                float t = (float)i / (float)steps;
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

        #region Tunnel Collider Methods


        public void CreateTunnelCollider()
        {
            GameObject col = new GameObject("Tunnel Collider");

            MeshFilter mesh = col.AddComponent<MeshFilter>();
            MeshCollider meshCol = col.AddComponent<MeshCollider>();

            col.transform.SetParent(transform, false);
            col.isStatic = true;

            mesh.sharedMesh = new Mesh();
            mesh.sharedMesh.name = "Tunnel Collider";
            mesh.sharedMesh.vertices = TunnelVertices();
            mesh.sharedMesh.triangles = TunnelTriangles();
            meshCol.sharedMesh = mesh.sharedMesh;
        }

        Vector3[] TunnelVertices()
        {


            float tunnelLength = bezierSpline.SplineDistance;
            int ringAmount = Mathf.RoundToInt(tunnelLength * tunnelDensity.x * colliderPrecision);

            Vector3[] vertices = new Vector3[ringAmount * colliderRadiusVertexCount];

            for (int z = 0, i = 0; z < ringAmount; z++)
            {
                float t = (float)z / (float)ringAmount;
                Vector3 ringCenter = bezierSpline.GetPointUniform(t);
                Vector3 ringDir = bezierSpline.GetDirectionUniform(t);
                float size = GetRingSize(t);

                for (int x = 0; x < colliderRadiusVertexCount; x++, i++)
                {
                    float angle = ((float)x / (float)colliderRadiusVertexCount) * 360;
                    Quaternion rot = Quaternion.LookRotation(ringDir) * Quaternion.Euler(Vector3.forward * angle);
                    vertices[i] = (rot * Vector3.up * (size + colliderOffset)) + transform.InverseTransformPoint(ringCenter);
                }
            }
            return vertices;
        }

        int[] TunnelTriangles()
        {
            float tunnelLength = bezierSpline.SplineDistance;
            int ringAmount = Mathf.RoundToInt(tunnelLength * tunnelDensity.x * colliderPrecision);

            int numTriangles = ((ringAmount - 1) * colliderRadiusVertexCount) * 6;
            int[] triangles = new int[numTriangles];

            triangles[0] = colliderRadiusVertexCount - 1;
            triangles[1] = colliderRadiusVertexCount;
            triangles[2] = 0;

            for (int t = 3, i = 0; t < numTriangles - 6; t += 6, i += 1)
            {
                triangles[t] = i + colliderRadiusVertexCount;
                triangles[t + 1] = i + 1;
                triangles[t + 2] = i;

                triangles[t + 3] = i + colliderRadiusVertexCount + 1;
                triangles[t + 4] = i + 1;
                triangles[t + 5] = i + colliderRadiusVertexCount;

            }
            triangles[triangles.Length - 3] = (colliderRadiusVertexCount * ringAmount) - 1;
            triangles[triangles.Length - 2] = colliderRadiusVertexCount * (ringAmount - 1);
            triangles[triangles.Length - 1] = (colliderRadiusVertexCount * (ringAmount - 1)) - 1;

            return triangles;
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

            controlPointSizes = new float[bezierSpline.CurveCount + 1];

            for (int i = 0; i < controlPointSizes.Length; i++)
            {
                controlPointSizes[i] = defaultRingSize;
            }
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

        #region Gizmos
        /*
        void OnDrawGizmosSelected()
        {


            if (vertices == null)
            {
                MeshCollider tunnelCol = transform.GetComponentInChildren<MeshCollider>();
                if (tunnelCol != null)
                    vertices = tunnelCol.sharedMesh.vertices;

            }

            if (vertices == null || !showGizmos)
                return;

            Gizmos.color = Color.white;

            foreach (Vector3 vert in vertices)
            {
                Gizmos.DrawSphere(transform.TransformPoint(vert), 0.1f);
            }
        }
        */
        #endregion

    }
}
