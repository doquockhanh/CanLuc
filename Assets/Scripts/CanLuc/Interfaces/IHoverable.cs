using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Implement on any component attached to a hoverable GameObject.
	/// </summary>
	public interface IHoverable
	{
		/// <summary>
		/// Called when mouse enters this object.
		/// </summary>
		void OnMouseEnter();

		/// <summary>
		/// Called when mouse exits this object.
		/// </summary>
		void OnMouseExit();
	}
}
