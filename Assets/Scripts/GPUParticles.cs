/* 
Description:
    GPU Particles using compute shaders in Unity.
    Built with Unity 2017.2

TODO:
    * Rotation
    * Rotation over lifetime
    * Rotation by speed
    * Velocity over lifetime
    * Gravity Modifier
    * Limit velocity over lifetime
    * Size
    * Size over lifetime
    * Emission types:
        - Sphere
        - Disk
        - Line
        - Mesh
        - Skinned mesh renderer
    * Rendering modes:
        - Sprite
        - Trails
        - Instanced
    * Optimization:
        - Precompute random fields
        - Precompute noise at lower resolution?
    * Switch to DispatchIndirect to control kernel size dynamically
    * Depth buffer collision

*/

using UnityEngine;

namespace GPU_Particles 
{
    public class GPUParticles : MonoBehaviour
    {
        #region Public Variables
        public ComputeShader Compute;
        public Material Material;
        
        [Space(10)]
        [Header("Particles")]
        public Vector2 Mass = new Vector2(0.5f, 0.5f);
        public Vector2 Momentum = new Vector2(0.95f, 0.95f);
        public Vector2 Lifespan = new Vector2(5f, 5f);
        public int PreWarmFrames = 0;

        [Header("Velocity")]
        public float InheritVelocity = 0f;
        public int EmitterVelocity = 0;
        public float GravityModifier = 0f;

        [Header("Shape")]
        [Range(0,VertCount)]
        public int Emission = 65000;
        public Vector3 EmissionSize = new Vector3(1f,1f,1f);
        public float InitialSpeed = 0f;

        [Header("Color")]
        public Color StartColor = new Color(1f,1f,1f,1f);
        public ColorRamp ColorByLife;
        public ColorRampRange ColorByVelocity;

        [Header("Noise")]
        public Vector3 NoiseAmplitude = new Vector3(0f,0f,0f);
        public Vector3 NoiseScale = new Vector3(1f,1f,1f);
        public Vector3 NoiseOffset = new Vector3(0f,0f,0f);
        public Gradient TestGradient;
        #endregion
        
        #region Private Variables
        private ComputeBuffer particlesBuffer;
        private int _kernel;
        private Vector3 origin;
        private Vector3 initialVelocityDir;
        private Vector3 prevPos;
        
        private const int NumElements = 14; //float4 cd; float3 pos, vel; float age, lifespan, mass, momentum
        // private const int VertCount = 262144; //32*32*16*16 (Groups*ThreadsPerGroup)
        private const int VertCount = 1048576; //64*64*16*16 (Groups*ThreadsPerGroup)
        #endregion

        //We initialize the buffers and the material used to draw.
        void Start()
        {
            CreateBuffers();            
            _kernel = Compute.FindKernel("ParticleSystemKernel");

            Compute.SetBuffer(_kernel, "output", particlesBuffer);

            ColorByLife.Setup();
            ColorByVelocity.Setup();
            Compute.SetTexture(_kernel, "_colorByLife", (Texture)ColorByLife.Texture);
            Compute.SetTexture(_kernel, "_colorByVelocity", (Texture)ColorByVelocity.Texture);

            UpdateUniforms();

            // Prewarm the system
            if (PreWarmFrames > 0) {
                for (int i = 0; i < PreWarmFrames; i++) {
                    Dispatch();
                }
            }
            
        }

        void FixedUpdate() {
            UpdateUniforms();
            Dispatch();
            
        }

        // Dispatch the kernel and draw points
        void OnRenderObject() 
        {
            Material.SetBuffer("dataBuffer", particlesBuffer);
            Material.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, VertCount);
        }


        void OnDisable()
        {
            ReleaseBuffers();
        }

        //We dispatch 32x32x1 groups of threads of our CSMain kernel.
        private void Dispatch()
        {
            // Compute.Dispatch(_kernel, 32, 32, 1);
            Compute.Dispatch(_kernel, 64, 64, 1);

        }

        private void UpdateUniforms() 
        {
            prevPos = transform.position;

            // Follow mouse cursor
            if (Input.GetMouseButton(0)){
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 o = ray.origin + (ray.direction * 20f);
                transform.position = o;
            }

            // Handle where to take the initial velocity direction from
            // 0 = Rigidbody, 1 = Transform
            switch(EmitterVelocity) {
                case 0:
                    if (transform.parent != null) {
                        if (transform.parent.gameObject.GetComponent<Rigidbody>() != null) {
                            initialVelocityDir = transform.parent.gameObject.GetComponent<Rigidbody>().velocity;
                        }
                    }
                    else {
                        initialVelocityDir = Vector3.zero;
                    }
                    break;
                case 1:
                    initialVelocityDir = transform.position-prevPos;
                    break;
            }

            origin = transform.position;
            

            Compute.SetFloat("dt", Time.deltaTime);
            Compute.SetVector("origin", origin);
            Compute.SetVector("massNew", Mass);
            Compute.SetVector("momentumNew", Momentum);
            Compute.SetVector("lifespanNew", Lifespan);
            Compute.SetFloat("inheritVelocityMult", InheritVelocity);
            Compute.SetVector("initialVelocityDir", initialVelocityDir);
            Compute.SetVector("gravityIn", Physics.gravity);
            Compute.SetFloat("gravityModifier", GravityModifier);
            Compute.SetInt("emission", Emission);
            Compute.SetVector("emissionSize", EmissionSize);
            Compute.SetFloat("initialSpeed", InitialSpeed);
            Compute.SetVector("startColor", StartColor);
            Compute.SetFloat("velocityColorRange", ColorByVelocity.Range);
            Compute.SetVector("noiseAmplitude", NoiseAmplitude);
            Compute.SetVector("noiseScale", NoiseScale);
            Compute.SetVector("noiseOffset", NoiseOffset);

        }


        // Create and initialize compute shader buffers
        private void CreateBuffers()
        {
            // Allocate
            particlesBuffer = new ComputeBuffer(VertCount, 4 * NumElements); //float3 pos, vel, cd; float age
            
            // Initialize
            float[] particleZeroes = new float[VertCount * NumElements];

            for (int i = 0; i < VertCount; i++)
            {
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