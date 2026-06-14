using UnityEngine;

namespace CubeShift.Core
{
    /// <summary>
    /// Handles procedural runtime particle systems for Cube Shift game juice.
    /// All particles are configured dynamically in code without prefab dependencies.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class JuiceEffectsManager : MonoBehaviour
    {
        public static JuiceEffectsManager Instance { get; private set; }

        private Material particleMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject go = new GameObject("JuiceEffectsManager");
            Instance = go.AddComponent<JuiceEffectsManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Find or create a default unlit particle material
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            if (particleShader != null)
            {
                particleMaterial = new Material(particleShader);
            }
        }

        public void SpawnLandingDust(Vector3 position)
        {
            GameObject particleObj = new GameObject("LandingDustEffect");
            particleObj.transform.position = position + Vector3.down * 0.4f;

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(ps, particleMaterial, new Color(0.7f, 0.7f, 0.7f, 0.5f), 12, 0.45f, 0.5f, 0.12f);

            var main = ps.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 0.35f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // Face up

            ps.Play();
            Destroy(particleObj, 1f);
        }

        public void SpawnTileBreakParticles(Vector3 position)
        {
            GameObject particleObj = new GameObject("TileBreakEffect");
            particleObj.transform.position = position;

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(ps, particleMaterial, new Color(0.9f, 0.22f, 0.18f, 0.95f), 28, 0.75f, 3f, 0.2f);

            var main = ps.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.gravityModifier = 1.6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            ps.Play();
            Destroy(particleObj, 1.5f);
        }

        public void SpawnButtonPressParticles(Vector3 position, Color color)
        {
            GameObject particleObj = new GameObject("ButtonPressEffect");
            particleObj.transform.position = position + Vector3.up * 0.15f;

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(ps, particleMaterial, color, 16, 0.6f, 2f, 0.15f);

            var main = ps.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.22f;
            shape.rotation = new Vector3(90f, 0f, 0f); // Lay flat on the floor

            ps.Play();
            Destroy(particleObj, 1.2f);
        }

        public void SpawnVictoryConfetti()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                return;
            }

            Color[] colors = new Color[] 
            { 
                new Color(1f, 0.2f, 0.2f),
                new Color(0.2f, 1f, 0.3f), 
                new Color(0.2f, 0.4f, 1f), 
                new Color(1f, 0.9f, 0.15f), 
                new Color(0.7f, 0.2f, 1f) 
            };

            for (int i = 0; i < 5; i++)
            {
                GameObject particleObj = new GameObject("VictoryConfetti_" + i);
                
                // Spread nicely across the viewport top
                Vector3 screenPos = new Vector3(0.15f + i * 0.175f, 0.95f, 4.5f);
                Vector3 worldPos = mainCam.ViewportToWorldPoint(screenPos);
                particleObj.transform.position = worldPos;

                ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
                Color selectedColor = colors[i % colors.Length];
                ConfigureParticleSystem(ps, particleMaterial, selectedColor, 40, 3.2f, 1.5f, 0.16f);

                var main = ps.main;
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
                main.gravityModifier = 0.65f;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 25f;
                shape.radius = 0.4f;
                shape.rotation = new Vector3(90f, 0f, 0f); // Point downwards

                var limit = ps.limitVelocityOverLifetime;
                limit.enabled = true;
                limit.drag = 0.22f;

                ps.Play();
                Destroy(particleObj, 4f);
            }
        }

        private void ConfigureParticleSystem(ParticleSystem ps, Material mat, Color color, int count, float lifetime, float startSpeed, float startSize)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
            if (psr != null && mat != null)
            {
                psr.sharedMaterial = mat;
            }

            var main = ps.main;
            main.duration = 0.1f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = color;
            main.maxParticles = count;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            var burst = new ParticleSystem.Burst(0.0f, (short)count);
            emission.SetBursts(new ParticleSystem.Burst[] { burst });
        }
    }
}
