/* 

Description:
    GPU Particles using compute shaders in Unity.
    Built with Unity 2017.2

*/

using UnityEngine;

namespace GPUParticles 
{
    public class GPUParticles : MonoBehaviour
    {
        // #region Public Variables

        // public Shader shader;
        public ComputeShader computeShader;
        public Material material;
        
        [Space(10)]
        [Header("Particles")]
        public Vector3 Origin = new Vector3(0f, 0f, 0f);
        public Vector2 Mass = new Vector2(0.5f, 0.5f);
        public Vector2 Momentum = new Vector2(0.5f, 0.5f);
        public Vector2 Lifespan = new Vector2(5f, 5f);
        [Range(0,VertCount)]
        public int Emission = 65000;
        public Color StartColor;
        public ColorRamp ColorByLife;
        public ColorRampRange ColorByVelocity;
        public int PreWarmFrames = 0;

        [Space(10)]
        [Header("Noise")]
        public Vector3 NoiseAmplitude = new Vector3(1f,1f,1f);
        public Vector3 NoiseScale = new Vector3(1f,1f,1f);
        public Vector3 NoiseOffset = new Vector3(0f,0f,0f);


        // #endregion
        
        // #region Private Variables

        private ComputeBuffer particlesBuffer;
        private int _kernel;

        private const int NumElements = 14; //float4 cd; float3 pos, vel; float age, lifespan, mass, momentum
        private const int VertCount = 262144; //32*32*16*16 (Groups*ThreadsPerGroup)

        // #endregion

        //We initialize the buffers and the material used to draw.
        void Start()
        {
            CreateBuffers();            
            _kernel = computeShader.FindKernel("ParticleSystemKernel");

            computeShader.SetBuffer(_kernel, "output", particlesBuffer);

            ColorByLife.Setup();
            ColorByVelocity.Setup();
            computeShader.SetTexture(_kernel, "colorByLife", (Texture)ColorByLife.Texture);
            computeShader.SetTexture(_kernel, "colorByVelocity", (Texture)ColorByVelocity.Texture);

            UpdateUniforms();

            // Prewarm the system
            if (PreWarmFrames > 0) {
                for (int i = 0; i < PreWarmFrames; i++) {
                    Dispatch();
                }
            }
            
        }

        void Update() 
        {
            UpdateUniforms();
        }

        //this just draws the "mesh" as a set of points
        void OnRenderObject() 
        {
            Dispatch();

            material.SetPass(0);
            material.SetBuffer("dataBuffer", particlesBuffer);
            Graphics.DrawProcedural(MeshTopology.Points, VertCount);
        }


        void OnDisable()
        {
            ReleaseBuffers();
        }

        //We dispatch 32x32x1 groups of threads of our CSMain kernel.
        private void Dispatch()
        {
            computeShader.Dispatch(_kernel, 32, 32, 1);
        }

        private void UpdateUniforms() 
        {
            if (Input.GetMouseButton(0)){
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 o = ray.origin + (ray.direction * 20f);
                Origin = o;
            }

            computeShader.SetFloat("dt", Time.deltaTime);
            computeShader.SetVector("origin", Origin);
            computeShader.SetVector("massNew", Mass);
            computeShader.SetVector("momentumNew", Momentum);
            computeShader.SetVector("lifespanNew", Lifespan);
            computeShader.SetInt("emission", Emission);
            computeShader.SetVector("startColor", StartColor);
            computeShader.SetFloat("velocityColorRange", ColorByVelocity.Range);
            computeShader.SetVector("noiseAmplitude", NoiseAmplitude);
            computeShader.SetVector("noiseScale", NoiseScale);
            computeShader.SetVector("noiseOffset", NoiseOffset);


        }


        //To setup a ComputeBuffer we pass in the array length, as well as the size in bytes of a single element.
        //We fill the offset buffer with random numbers between 0 and 2*PI.
        private void CreateBuffers()
        {
            // Allocate
            particlesBuffer = new ComputeBuffer(VertCount, 4 * NumElements); //float3 pos, vel, cd; float age
            

            // Initialize
            float[] offsetValues = new float[VertCount];
            Vector3[] colors = new Vector3[VertCount];
            float[] particleZeroes = new float[VertCount * NumElements];

            for (int i = 0; i < VertCount; i++)
            {
                offsetValues[i] = Random.value * 2 * Mathf.PI;

                colors[i][0] = Random.value;
                colors[i][1] = Random.value;
                colors[i][2] = Random.value;

                for (int j = 0; j < NumElements; j++) {
                    particleZeroes[(i*NumElements)+j] = 0f;
                }
            }

            particlesBuffer.SetData(particleZeroes);

        }

        //Remember to release buffers and destroy the material when play has been stopped.
        private void ReleaseBuffers()
        {
            particlesBuffer.Release();

        }

    }
}