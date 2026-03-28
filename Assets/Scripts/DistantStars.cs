using UnityEngine;

[ExecuteAlways]
public class DistantStars : MonoBehaviour
{
    [Header("Star Settings")]
    [SerializeField] private int starAmount = 1000;
    [SerializeField] private Vector2 starDistance = new Vector2(1500, 3000f);
    [SerializeField] private GameObject starPrefab;

    [Header("Seed")]
    [SerializeField] private int seed = 0;

    [SerializeField] private Vector3 localPos;

    private Transform starsParent;

    private void LateUpdate()
    {
        if (Camera.main != null)
            transform.position = Camera.main.transform.position;
    }

    public void GenerateStars()
    {
        UnityEngine.Random.InitState(seed);
        ClearStars();
        Debug.Log("Generating stars with seed: " + seed);

        starsParent = new GameObject("Distant Stars").transform;
        starsParent.parent = transform;        

        for (int i = 0; i < starAmount; i++)
        {
            Vector3 dir = UnityEngine.Random.onUnitSphere;
            float distance = UnityEngine.Random.Range(starDistance.x, starDistance.y);

            localPos = dir * distance;

            GameObject star = Instantiate(starPrefab, starsParent);

            star.transform.localPosition = localPos;

            float scale = UnityEngine.Random.Range(0.03f, 0.3f);
            star.transform.localScale = Vector3.one * scale;
        }
    }


    private void ClearStars()
    {
        Transform existing = transform.Find("Distant Stars");

        if (existing != null)
        {
            if (Application.isPlaying)
                Destroy(existing.gameObject);
            else
                DestroyImmediate(existing.gameObject);
        }

        Debug.Log("Cleared existing stars.");
    }
}