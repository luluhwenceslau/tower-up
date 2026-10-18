using UnityEngine;

[DisallowMultipleComponent]
public class PerfectFitEffect : MonoBehaviour
{
    private ParticleSystem particles;
    private Material particleMaterial;
    private Texture2D starTexture;

    public static void Play(Vector3 position, float width, int sortingLayerId, int sortingOrder)
    {
        GameObject effectObject = new GameObject("Perfect Fit Stardust");
        effectObject.transform.position = position;

        PerfectFitEffect effect = effectObject.AddComponent<PerfectFitEffect>();
        effect.Initialize(width, sortingLayerId, sortingOrder);
    }

    private void Initialize(float width, int sortingLayerId, int sortingOrder)
    {
        particles = gameObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.12f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.15f);
        main.startColor = Color.white;
        main.gravityModifier = -0.12f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 32;
        main.stopAction = ParticleSystemStopAction.None;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.9f, 0.35f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = new ParticleSystem.MinMaxGradient(colorGradient);

        ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        ParticleSystem.RotationOverLifetimeModule rotation = particles.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-120f, 120f);

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.06f;
        noise.frequency = 0.7f;

        CreateParticleMaterial();
        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        particleRenderer.sortingLayerID = sortingLayerId;
        particleRenderer.sortingOrder = sortingOrder;
        particleRenderer.material = particleMaterial;

        particles.Play();
        EmitOutwardStars(width);
    }

    private void EmitOutwardStars(float width)
    {
        const int particleCount = 30;
        float halfWidth = Mathf.Max(width, 0.1f) * 0.5f;
        Color paleGold = new Color(1f, 0.99f, 0.76f, 1f);
        Color brightGold = new Color(1f, 0.78f, 0.18f, 1f);

        for (int i = 0; i < particleCount; i++)
        {
            float normalizedPosition = (i + 0.5f) / particleCount;
            float x = Mathf.Lerp(-halfWidth, halfWidth, normalizedPosition);
            float direction = x < 0f ? -1f : 1f;
            var emit = new ParticleSystem.EmitParams
            {
                position = transform.position + new Vector3(x, Random.Range(-0.02f, 0.02f), 0f),
                velocity = new Vector3(direction * Random.Range(1.15f, 2.1f), Random.Range(-0.16f, 0.16f), 0f),
                startLifetime = Random.Range(0.55f, 1f),
                startSize = Random.Range(0.1f, 0.2f),
                startColor = Color.Lerp(paleGold, brightGold, Random.value),
                rotation = Random.Range(0f, 360f)
            };
            particles.Emit(emit, 1);
        }
    }

    private void CreateParticleMaterial()
    {
        starTexture = CreateStarTexture();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
            return;

        particleMaterial = new Material(shader)
        {
            hideFlags = HideFlags.HideAndDontSave,
            mainTexture = starTexture
        };
    }

    private static Texture2D CreateStarTexture()
    {
        const int size = 32;
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
            float dy = (y + 0.5f - size * 0.5f) / (size * 0.5f);
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            float angle = Mathf.Atan2(dy, dx);
            float radius = 0.2f + 0.78f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 2f)), 5f);
            byte alpha = (byte)(Mathf.Clamp01((radius - distance) * size * 0.75f) * 255f);
            pixels[y * size + x] = new Color32(255, 255, 255, alpha);
        }

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private void Update()
    {
        if (particles != null && !particles.IsAlive(true))
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (particleMaterial != null)
            Destroy(particleMaterial);
        if (starTexture != null)
            Destroy(starTexture);
    }
}
