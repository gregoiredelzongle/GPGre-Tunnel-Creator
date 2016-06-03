using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace GPGre.TunnelCreator
{
    public class RingSpawner : MonoBehaviour
    {

        public int amount = 10;
        public float size = 3f;
        public Vector3 rotationOffset;

        public GameObject ringPartPrefab;
        public bool isPrefabsStatic = false;


        // Use this for initialization
        void Start()
        {
            //CreateRing();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CreateRing(float noiseZ, float noiseSize, float noiseAmount)
        {
            for (int i = 0; i < amount; i++)
            {
                float angle = ((float)i / (float)amount) * 360;
                float noise = Mathf.PerlinNoise(i * noiseSize, noiseZ) * noiseAmount;
                CreateRingPart(angle, noise);
            }
        }

        void CreateRingPart(float angle, float noise)
        {
            Quaternion rot = Quaternion.Euler(Vector3.forward * angle);
            Vector3 dir = rot * Vector3.up * (size + noise);
#if UNITY_EDITOR
            GameObject ringPart = PrefabUtility.InstantiatePrefab(ringPartPrefab) as GameObject;

#else
        GameObject ringPart = Instantiate<GameObject>(ringPartPrefab);
#endif

            if (isPrefabsStatic)
            {
                ringPart.isStatic = true;
                foreach (Transform child in ringPart.transform)
                    child.gameObject.isStatic = true;
            }
            ringPart.transform.SetParent(transform, false);
            ringPart.transform.localRotation = rot * Quaternion.Euler(rotationOffset + Vector3.up * noise * 360);
            ringPart.transform.localPosition = dir;

        }
    }
}
