// ColorRamp is a gradient property with an associated Texture2D for passing to shaders
// Currently set to RGBAHalf

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUParticles 
{
	[System.Serializable]
	public class ColorRamp {

		public Gradient Gradient;

		private Texture2D texture;
		public Texture2D Texture 
		{
			get
			{
				return texture;
			}
		}
		private const int width = 1024;
		

		public void Setup() {
			texture = new Texture2D(width, 1, TextureFormat.RGBAFloat, false);
			texture.filterMode = FilterMode.Point;
			
			Update();
		}

		public void Update() {
			Color [] tempArray = new Color[width];
			for (int i = 0; i < 1024; i++) {
				float time = (float)i/(float)width;
				tempArray[i] = Gradient.Evaluate(time);
			}
			texture.SetPixels(tempArray);
			texture.Apply();
		}

	}
}