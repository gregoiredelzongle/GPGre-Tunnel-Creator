using UnityEngine;
using GPGre.TunnelCreator;

namespace GPGre.TunnelCreator.Modifiers
{
	public abstract class BaseModifier : MonoBehaviour {

		public virtual bool ApplyModifierCondition(int x, int y, int width, int height){
			return true;
		}

		public virtual Vector3 ApplyModifierPosition(Vector3 pos,int x, int y, int width, int height)
		{
			return pos;
		}
	}
}
