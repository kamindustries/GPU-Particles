using UnityEngine;

//This game object invokes PlaneComputeShader (when attached via drag'n drop in the editor) using the PlaneBufferShader (also attached in the editor)
//to display a grid of points moving back and forth along the z axis.
namespace GPUParticles 
{
    public class GPUParticles : MonoBehaviour
    {
        // #region Public Variables

        // public Shader shader;
        public ComputeShader computeShader;
        public Material material;
        [Space(10)]
        
        [Header("Variables")]
        [Range(0,1)]
        public float Mass = 0.5f;
        [Range(0,1)]
        public float Momentum = 0.5f;
        public ColorRamp ColorByVelocity;
        public Vector4 Origin = new Vector4(0f, 0f, 0f, 1f);


        // #endregion
        

        // #region Private Variables

        private ComputeBuffer offsetBuffer;
        private ComputeBuffer colorBuffer;
        private ComputeBuffer velocityBuffer;
        private ComputeBuffer particlesBuffer;
        private int _kernel;

        private const int NumElements = 10; //float3 pos, vel, cd; float age
        private const int VertCount = 262144; //32*32*16*16 (Groups*ThreadsPerGroup)

        // #endregion


        //We initialize the buffers and the material used to draw.
        void Start()
        {
            CreateBuffers();            
            _kernel = computeShader.FindKernel("ParticleSystemKernel");

            computeShader.SetBuffer(_kernel, "offsets", offsetBuffer);
            computeShader.SetBuffer(_kernel, "velocity", velocityBuffer);
            computeShader.SetBuffer(_kernel, "colors", colorBuffer);
            computeShader.SetBuffer(_kernel, "output", particlesBuffer);

            ColorByVelocity.Setup();
            computeShader.SetTexture(_kernel, "colorByVelocity", (Texture)ColorByVelocity.texture);

            
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
            //material.SetBuffer("buf_Points", particlesBuffer);
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
                Origin.x = o.x;
                Origin.y = o.y;
                Origin.z = o.z;
            }

            computeShader.SetFloat("dt", Time.deltaTime);
            computeShader.SetFloat("mass", Mass);
            computeShader.SetFloat("momentum", Momentum);
            computeShader.SetVector("origin", Origin);

            // ColorByVelocity.Update();
            computeShader.SetTexture(_kernel, "colorByVelocity", ColorByVelocity.texture);

        }


        //To setup a ComputeBuffer we pass in the array length, as well as the size in bytes of a single element.
        //We fill the offset buffer with random numbers between 0 and 2*PI.
        private void CreateBuffers()
        {
            // Allocate
            offsetBuffer = new ComputeBuffer(VertCount, 4); //Contains a single float value (OffsetStruct)
            colorBuffer = new ComputeBuffer(VertCount, 12); //float * 3
            velocityBuffer = new ComputeBuffer(VertCount, 4 * 3);
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

            offsetBuffer.SetData(offsetValues);
            colorBuffer.SetData(colors);
            particlesBuffer.SetData(particleZeroes);

        }

        //Remember to release buffers and destroy the material when play has been stopped.
        private void ReleaseBuffers()
        {
            offsetBuffer.Release();
            particlesBuffer.Release();
            velocityBuffer.Release();
            colorBuffer.Release();

        }

    }
}