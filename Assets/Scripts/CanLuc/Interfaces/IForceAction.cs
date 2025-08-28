using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Implement behavior that executes using accumulated force data.

	/// </summary>
	public interface IForceAction
	{
		/// <summary>
		/// Execute the action using provided force value.
		/// </summary>
		/// <param name="force">Accumulated force value.</param>
		void Execute(float force);
	}
}


