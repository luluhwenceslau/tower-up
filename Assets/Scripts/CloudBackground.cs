using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CloudBackground : MonoBehaviour
{
    private const int MaximumClouds = 2;
    private const int StartAtBlock = 10;
    private const int FrequentAtBlock = 20;
    private const float FirstSpawnDelayMin = 1f;
    private const float FirstSpawnDelayMax = 3f;
    private const float SlowSpawnInterval = 16f;
    private const float FrequentSpawnInterval = 9f;
    private const int CloudSortingOrder = -10;
    private const float CloudAlpha = 0.82f;
    private const float MinimumCloudWidth = 1.25f;
    private const float MaximumCloudWidth = 2.65f;
    private static readonly float[] CloudLaneOffsets = { -2.3f, -1.15f, 0f, 1.15f, 2.3f };

    private readonly List<CloudInstance> clouds = new List<CloudInstance>();
    private Sprite[] cloudSprites;
    private Camera targetCamera;
    private float nextSpawnTime;
    private float sceneDirection;
    private bool hasReachedCloudAltitude;
    private int previousSpriteIndex = -1;
    private int spawnedCloudCount;

    private sealed class CloudInstance
    {
        public GameObject gameObject;
        public SpriteRenderer renderer;
        public float direction;
        public float speed;
        public float verticalOffset;
    }

    public void Initialize(Sprite[] sprites, Camera cameraToFollow)
    {
        cloudSprites = sprites;
        targetCamera = cameraToFollow;
        sceneDirection = Random.value < 0.5f ? -1f : 1f;
    }

    public void UpdateClouds(int landedBlocks)
    {
        if (targetCamera == null || cloudSprites == null || cloudSprites.Length == 0)
            return;

        if (landedBlocks < StartAtBlock)
        {
            hasReachedCloudAltitude = false;
            DeactivateAllClouds();
            return;
        }

        if (!hasReachedCloudAltitude)
        {
            hasReachedCloudAltitude = true;
            nextSpawnTime = Time.time + Random.Range(FirstSpawnDelayMin, FirstSpawnDelayMax);
        }

        MoveClouds();

        if (Time.time >= nextSpawnTime)
        {
            if (TrySpawnCloud(landedBlocks))
            {
                float progress = Mathf.InverseLerp(StartAtBlock, FrequentAtBlock, landedBlocks);
                float interval = Mathf.Lerp(SlowSpawnInterval, FrequentSpawnInterval, progress);
                nextSpawnTime = Time.time + Random.Range(interval * 0.8f, interval * 1.2f);
            }
            else
            {
                nextSpawnTime = Time.time + 2f;
            }
        }
    }

    private void MoveClouds()
    {
        float halfViewWidth = targetCamera.orthographicSize * targetCamera.aspect;
        float cameraY = targetCamera.transform.position.y;

        for (int i = 0; i < clouds.Count; i++)
        {
            CloudInstance cloud = clouds[i];
            if (!cloud.gameObject.activeSelf)
                continue;

            Transform cloudTransform = cloud.gameObject.transform;
            Vector3 position = cloudTransform.position;
            position.x += cloud.direction * cloud.speed * Time.deltaTime;
            position.y = cameraY + cloud.verticalOffset;
            cloudTransform.position = position;

            float halfCloudWidth = cloud.renderer.bounds.extents.x;
            if ((cloud.direction > 0f && position.x - halfCloudWidth > halfViewWidth + 0.5f)
                || (cloud.direction < 0f && position.x + halfCloudWidth < -halfViewWidth - 0.5f))
            {
                cloud.gameObject.SetActive(false);
            }
        }
    }

    private bool TrySpawnCloud(int landedBlocks)
    {
        CloudInstance cloud = GetAvailableCloud();
        if (cloud == null)
            return false;

        int spriteIndex = GetNextSpriteIndex();
        Sprite sprite = cloudSprites[spriteIndex];
        if (sprite == null || sprite.bounds.size.x <= 0f)
            return false;

        bool distantIntroCloud = spawnedCloudCount < 2;
        cloud.renderer.sprite = sprite;
        float targetWidth = distantIntroCloud
            ? Random.Range(1.1f, 1.55f)
            : Random.Range(MinimumCloudWidth, MaximumCloudWidth);
        float scale = targetWidth / sprite.bounds.size.x;
        cloud.gameObject.transform.localScale = new Vector3(scale, scale, 1f);
        cloud.renderer.color = new Color(1f, 1f, 1f, distantIntroCloud ? 0.68f : CloudAlpha);
        int laneIndex = Random.Range(0, CloudLaneOffsets.Length);
        cloud.direction = sceneDirection;
        cloud.speed = Random.Range(0.25f, 0.45f);
        cloud.verticalOffset = distantIntroCloud
            ? Mathf.Max(0f, targetCamera.orthographicSize - 0.8f)
            : CloudLaneOffsets[laneIndex] + Random.Range(-0.16f, 0.16f);

        float halfViewWidth = targetCamera.orthographicSize * targetCamera.aspect;
        float halfCloudWidth = targetWidth * 0.5f;
        float startX = cloud.direction > 0f
            ? -halfViewWidth - halfCloudWidth - 0.5f
            : halfViewWidth + halfCloudWidth + 0.5f;
        cloud.gameObject.transform.position = new Vector3(
            startX,
            targetCamera.transform.position.y + cloud.verticalOffset,
            distantIntroCloud ? 20f : 5f
        );
        cloud.gameObject.SetActive(true);
        spawnedCloudCount++;
        return true;
    }

    private CloudInstance GetAvailableCloud()
    {
        for (int i = 0; i < clouds.Count; i++)
        {
            if (!clouds[i].gameObject.activeSelf)
                return clouds[i];
        }

        if (clouds.Count >= MaximumClouds)
            return null;

        var cloudObject = new GameObject("Background Cloud");
        cloudObject.transform.SetParent(transform, false);
        var cloud = new CloudInstance
        {
            gameObject = cloudObject,
            renderer = cloudObject.AddComponent<SpriteRenderer>()
        };
        cloud.renderer.sortingOrder = CloudSortingOrder;
        cloudObject.SetActive(false);
        clouds.Add(cloud);
        return cloud;
    }

    private int GetNextSpriteIndex()
    {
        if (cloudSprites.Length <= 1)
            return 0;

        int offset = Random.Range(1, cloudSprites.Length);
        previousSpriteIndex = (previousSpriteIndex + offset) % cloudSprites.Length;
        return previousSpriteIndex;
    }

    private void DeactivateAllClouds()
    {
        for (int i = 0; i < clouds.Count; i++)
            clouds[i].gameObject.SetActive(false);
    }
}
