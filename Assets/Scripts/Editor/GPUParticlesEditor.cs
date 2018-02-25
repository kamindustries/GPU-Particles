using UnityEditor;
using UnityEngine;
using System.Collections;

namespace GPU_Particles {

	[CustomEditor(typeof(GPUParticles))]
	[CanEditMultipleObjects]
	public class GPUParticlesEditor : Editor {

        SerializedProperty Compute;
        SerializedProperty Material;

        SerializedProperty Mass;
        SerializedProperty Momentum;
        SerializedProperty Lifespan;
        SerializedProperty InitialSpeed;

        SerializedProperty Emission;

        SerializedProperty StartColor;
        SerializedProperty ColorByLife;
        SerializedProperty ColorByVelocity;
        SerializedProperty PreWarmFrames;

        SerializedProperty NoiseAmplitude;
        SerializedProperty NoiseScale;
        SerializedProperty NoiseOffset;

		string[] _emitterVelocity = new string[] {"Rigid body", "Transform"};
		int _emitterVelocityIdx = 0;

		void OnEnable() 
		{
			Compute = serializedObject.FindProperty("Compute");
			Material = serializedObject.FindProperty("Material");
			
			Mass = serializedObject.FindProperty("Mass");
			Momentum = serializedObject.FindProperty("Momentum");
			Lifespan = serializedObject.FindProperty("Lifespan");
			InitialSpeed = serializedObject.FindProperty("InitialSpeed");

			Emission = serializedObject.FindProperty("Emission");

			StartColor = serializedObject.FindProperty("StartColor");
			ColorByLife = serializedObject.FindProperty("ColorByLife");
			ColorByVelocity = serializedObject.FindProperty("ColorByVelocity");
			PreWarmFrames = serializedObject.FindProperty("PreWarmFrames");

			NoiseAmplitude = serializedObject.FindProperty("NoiseAmplitude");
			NoiseScale = serializedObject.FindProperty("NoiseScale");
			NoiseOffset = serializedObject.FindProperty("NoiseOffset");
			

		}

		public override void OnInspectorGUI()
		{
			var particles = target as GPUParticles;

			EditorGUILayout.PropertyField(Compute);
			EditorGUILayout.PropertyField(Material);

			EditorGUILayout.PropertyField(Mass);
			EditorGUILayout.PropertyField(Momentum);
			EditorGUILayout.PropertyField(Lifespan);
			EditorGUILayout.PropertyField(InitialSpeed);
			_emitterVelocityIdx = particles.EmitterVelocity;
			_emitterVelocityIdx = EditorGUILayout.Popup("Emitter Velocity", _emitterVelocityIdx, _emitterVelocity);
			particles.EmitterVelocity = _emitterVelocityIdx;

			EditorGUILayout.PropertyField(Emission);

			EditorGUILayout.PropertyField(StartColor);
			
			// Interactive gradient editor
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(ColorByLife, true);
			EditorGUILayout.PropertyField(ColorByVelocity, true);
			if (EditorGUI.EndChangeCheck()) {
				particles.ColorByLife.Update();
				particles.ColorByVelocity.Update();
			}

			EditorGUILayout.PropertyField(PreWarmFrames);

			EditorGUILayout.PropertyField(NoiseAmplitude);
			EditorGUILayout.PropertyField(NoiseScale);
			EditorGUILayout.PropertyField(NoiseOffset);
			
			EditorUtility.SetDirty(particles);

			serializedObject.ApplyModifiedProperties();

		}

	}
}

