using UnityEngine;
using GPGre.TunnelCreator.Modifiers;
namespace GPGre.TunnelCreator.Modifiers
{
	[RequireComponent(typeof(TunnelSpawner))]
	public class TextureHeightModifier : BaseModifier {

		public Texture2D textureHeight;

		public float amount = 1f;

		public override Vector3 ApplyModifierPosition(Vector3 pos,int x, int y, int width, int height)
		{
			if (textureHeight == null) {
				return pos;
			}

			float u = (float)x / (float)width;
			float v = (float)y / (float)height;

			float textureAmount = textureHeight.GetPixelBilinear (u, v).grayscale;

			return pos + pos.normalized * textureAmount * amount;
		}
	}
}
