using UnityEngine;
using GPGre.TunnelCreator.Modifiers;
namespace GPGre.TunnelCreator.Modifiers
{
	[RequireComponent(typeof(TunnelSpawner))]
	public class TextureMaskModifier : BaseModifier {

		public Texture2D textureMask;

		[Range(0,1)]
		public float falloff = 0.5f;

		public override bool ApplyModifierCondition(int x, int y, int width, int height){

			if (textureMask == null) {
				return true;
			}

			float u = (float)x / (float)width;
			float v = (float)y / (float)height;

			return textureMask.GetPixelBilinear (u, v).grayscale >= falloff;
		}
	}
}
