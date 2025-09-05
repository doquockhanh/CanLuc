using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Helper to trigger the currently focused object's IForceAction using its accumulated force.
	/// Attach this to any object and call TriggerCurrent() from UI button or key binding.
	/// </summary>
	public class FocusInputRelay : MonoBehaviour
	{
		[SerializeField] private FocusManager focusManager;

		void Awake()
		{
			if (focusManager == null)
			{
				focusManager = FindFirstObjectByType<FocusManager>();
			}
		}

		public void TriggerCurrent()
		{
			if (focusManager == null) return;
			GameObject focused = focusManager.GetCurrentFocus();
			if (focused == null) return;

			float force = focusManager.ConsumeCurrentForce();
			if (force <= 0f) return;

			var actions = focused.GetComponentsInChildren<IForceAction>(true);
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].Execute(force);
			}
		}
	}
}


