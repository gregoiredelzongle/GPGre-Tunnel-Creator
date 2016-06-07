//
// GPGre Tunnel Creator - Tunnel Creation Tool
//
// Copyright (C) 2016 Gregoire Delzongle
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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
