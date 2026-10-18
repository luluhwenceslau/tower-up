using UnityEngine;

[DisallowMultipleComponent]
public class SkyProgression : MonoBehaviour
{
    private const float ColorBlendSpeed = 0.8f;
    private static readonly int[] BlockMilestones = { 0, 20, 40, 60, 80, 100, 150, 200 };
    private static readonly Color[] SkyColors =
    {
        new Color(0.42745098f, 0.7176471f, 0.9098039f), // Dia: azul atual
        new Color(0.42745098f, 0.7176471f, 0.9098039f), // Dia até o bloco 20
        new Color(0.76f, 0.65f, 0.62f),                 // Pôr do sol suave, rosado
        new Color(0.52f, 0.47f, 0.61f),                 // Anoitecer violeta
        new Color(0.20f, 0.27f, 0.43f),                 // Noite azul
        new Color(0.075f, 0.10f, 0.22f),                // Céu estrelado
        new Color(0.018f, 0.025f, 0.065f),              // Limite da atmosfera
        new Color(0.004f, 0.006f, 0.016f)               // Espaço
    };

    private Camera targetCamera;

    public void Initialize(Camera cameraToTint)
    {
        targetCamera = cameraToTint;
    }

    public void UpdateSky(int landedBlocks)
    {
        if (targetCamera == null)
            return;

        int lastSegment = BlockMilestones.Length - 1;
        for (int i = 0; i < lastSegment; i++)
        {
            if (landedBlocks > BlockMilestones[i + 1])
                continue;

            float progress = Mathf.InverseLerp(
                BlockMilestones[i],
                BlockMilestones[i + 1],
                landedBlocks
            );
            progress = Mathf.SmoothStep(0f, 1f, progress);
            ApplySkyColor(Color.Lerp(SkyColors[i], SkyColors[i + 1], progress));
            return;
        }

        ApplySkyColor(SkyColors[lastSegment]);
    }

    private void ApplySkyColor(Color targetColor)
    {
        float blend = 1f - Mathf.Exp(-ColorBlendSpeed * Time.deltaTime);
        targetCamera.backgroundColor = Color.Lerp(targetCamera.backgroundColor, targetColor, blend);
    }
}
