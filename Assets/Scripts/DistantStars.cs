using UnityEngine;

[ExecuteAlways]
public class DistantStars : MonoBehaviour
{
    [Header("Star Settings")]
    [SerializeField] private int starAmount = 1000;
    [SerializeField] private Vector2 starDistance = new Vector2(5000, 10000f);
    [SerializeField] private GameObject starPrefab;

    [Header("Seed")]
    [SerializeField] private int seed = 0;

    private Transform starsParent;

    public void GenerateStars()
    {
        Random.InitState(seed);
        ClearStars();

        starsParent = new GameObject("Distant Stars").transform;
        starsParent.parent = transform;        

        for (int i = 0; i < starAmount; i++)
        {
            Vector3 dir = Random.onUnitSphere;

            float distance = Random.Range(starDistance.x, starDistance.y);

            Vector3 pos = transform.position + dir * distance;

            GameObject star = Instantiate(starPrefab, pos, Quaternion.identity, starsParent);

            float scale = Random.Range(1f, 3f);
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
    }
}