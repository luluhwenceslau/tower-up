using UnityEngine;

public class TowerPresentation : MonoBehaviour
{
    private const float CameraLookAheadInBlockHeights = 0.6f;

    [Header("Câmera")]
    public Camera cameraTransform;
    [Min(0.01f)] public float cameraSmoothTime = 0.3f;

    [Header("Distância do próximo bloco")]
    public Transform spawnPoint;
    [Min(0f)] public float spawnClearanceInBlockHeights = 1.15f;
    [Min(0.01f)] public float spawnPointSmoothTime = 0.18f;

    [Header("Cabo")]
    public Transform cableRig;
    public Transform cableVisual;
    public float swaySpeed = 1.2f;
    public float swayAmount = 0.15f;
    public float swayIncreasePerBlock = 0.04f;

    private Vector3 cameraVelocity;
    private float spawnPointVelocityY;
    private float cableTopLocalY;
    private float cableSpriteHeight = 1f;
    private Vector3 cableBaseScale;

    private void Start()
    {
        if (cableVisual != null)
        {
            SpriteRenderer cableRenderer = cableVisual.GetComponent<SpriteRenderer>();
            cableSpriteHeight = cableRenderer != null ? cableRenderer.size.y : 1f;
            cableBaseScale = cableVisual.localScale;
            cableTopLocalY = cableVisual.localPosition.y +
                (cableSpriteHeight * Mathf.Abs(cableBaseScale.y) * 0.5f);
        }
    }

    public void UpdatePresentation(int landedBlocks, Transform highestBlock)
    {
        FollowTower(landedBlocks, highestBlock);
        SwayCable(landedBlocks);
        KeepSpawnPointAboveTower(highestBlock, landedBlocks == 1);
        UpdateCableLength();
    }

    private void UpdateCableLength()
    {
        if (cableRig == null || cableVisual == null || spawnPoint == null)
            return;

        float cableBottomLocalY = cableRig.InverseTransformPoint(spawnPoint.position).y;
        float cableLength = Mathf.Max(0.05f, cableTopLocalY - cableBottomLocalY);

        Vector3 scale = cableBaseScale;
        scale.y = cableLength / cableSpriteHeight;
        cableVisual.localScale = scale;

        Vector3 position = cableVisual.localPosition;
        position.y = (cableTopLocalY + cableBottomLocalY) * 0.5f;
        cableVisual.localPosition = position;
    }

    private void KeepSpawnPointAboveTower(Transform highestBlock, bool snapToTarget)
    {
        if (spawnPoint == null || highestBlock == null)
            return;

        Collider2D towerCollider = highestBlock.GetComponent<Collider2D>();
        if (towerCollider == null)
            return;

        float blockHeight = GetNominalBlockHeight(highestBlock, towerCollider);
        float minimumGap = blockHeight * spawnClearanceInBlockHeights;
        float hangingBlockHalfHeight = blockHeight * 0.5f;
        float targetY = towerCollider.bounds.max.y + minimumGap + hangingBlockHalfHeight;

        Vector3 spawnPosition = spawnPoint.position;
        if (snapToTarget)
        {
            spawnPosition.y = targetY;
            spawnPointVelocityY = 0f;
        }
        else
        {
            spawnPosition.y = Mathf.SmoothDamp(
                spawnPosition.y,
                targetY,
                ref spawnPointVelocityY,
                spawnPointSmoothTime
            );
        }
        spawnPoint.position = spawnPosition;
    }

    public bool IsSpawnPointAtTarget(Transform highestBlock, float tolerance)
    {
        if (spawnPoint == null || highestBlock == null)
            return true;

        Collider2D towerCollider = highestBlock.GetComponent<Collider2D>();
        if (towerCollider == null)
            return true;

        float blockHeight = GetNominalBlockHeight(highestBlock, towerCollider);
        float targetY = towerCollider.bounds.max.y +
            (blockHeight * (spawnClearanceInBlockHeights + 0.5f));
        return Mathf.Abs(spawnPoint.position.y - targetY) <= tolerance;
    }

    private float GetNominalBlockHeight(Transform block, Collider2D blockCollider)
    {
        BoxCollider2D boxCollider = blockCollider as BoxCollider2D;
        if (boxCollider != null)
            return boxCollider.size.y * Mathf.Abs(block.lossyScale.y);

        return blockCollider.bounds.size.y;
    }

    private void FollowTower(int landedBlocks, Transform highestBlock)
    {
        if (cameraTransform == null || landedBlocks < 2 || highestBlock == null)
            return;

        Collider2D towerCollider = highestBlock.GetComponent<Collider2D>();
        if (towerCollider == null)
            return;

        float targetY = highestBlock.position.y +
            (GetNominalBlockHeight(highestBlock, towerCollider) * CameraLookAheadInBlockHeights);
        Vector3 targetPosition = new Vector3(
            cameraTransform.transform.position.x,
            targetY,
            cameraTransform.transform.position.z
        );

        cameraTransform.transform.position = Vector3.SmoothDamp(
            cameraTransform.transform.position,
            targetPosition,
            ref cameraVelocity,
            cameraSmoothTime
        );
    }

    private void SwayCable(int landedBlocks)
    {
        if (cableRig == null)
            return;

        if (landedBlocks <= 2)
        {
            cableRig.localPosition = Vector3.zero;
            return;
        }

        float amplitude = swayAmount + ((landedBlocks - 2) * swayIncreasePerBlock);
        float sway = Mathf.Sin(Time.time * swaySpeed) * amplitude;
        cableRig.localPosition = new Vector3(sway, 0f, 0f);
    }
}
