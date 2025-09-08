using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Gameplay.Focus
{
	/// <summary>
	/// Base class providing a default implementation of IFocusable.
	/// Handles simple material color highlight on focus.
	/// Also centralizes focus selection responsibility away from FocusManager.
	/// </summary>
	public abstract class FocusableBase : MonoBehaviour, IFocusable
	{
		[Header("Focus Visuals")]
		[SerializeField] protected static Color focusColor = new Color(0, 1, 0.92f, 1);
		[SerializeField] protected static Color normalColor = new Color(0, 0.6f, 0.2f, 1);

		protected Renderer cachedRenderer;

		// Global focus state
		public static FocusableBase Current { get; private set; }
		public static GameObject CurrentGameObject => Current != null ? Current.gameObject : null;
		public static System.Action<FocusableBase, FocusableBase> OnFocusChanged; // (previous, current)

		protected virtual void Awake()
		{
			cachedRenderer = GetComponentInChildren<Renderer>();
		}

		void OnMouseDown()
		{
			// Ignore clicks on UI
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
			SetFocus(this);
		}

		public static void ClearFocus()
		{
			SetFocus((FocusableBase)null);
		}

		public static void SetFocus(GameObject target)
		{
			if (target == null)
			{
				SetFocus((FocusableBase)null);
				return;
			}
			var focusable = target.GetComponentInParent<FocusableBase>();
			SetFocus(focusable);
		}

		private static void SetFocus(FocusableBase target)
		{
			if (ReferenceEquals(Current, target)) return;

			GameObject previousGO = Current != null ? Current.gameObject : null;
			IFocusable[] previousListeners = null;
			if (Current != null)
			{
				previousListeners = Current.GetComponentsInChildren<IFocusable>(true);
			}

			var previous = Current;
			Current = target;
			IFocusable[] newListeners = null;
			GameObject nextGO = null;
			if (Current != null)
			{
				nextGO = Current.gameObject;
				newListeners = Current.GetComponentsInChildren<IFocusable>(true);
			}

			// Notify previous
			if (previousListeners != null)
			{
				for (int i = 0; i < previousListeners.Length; i++)
				{
					previousListeners[i].OnDefocused(nextGO);
				}
			}

			// Notify new
			if (newListeners != null)
			{
				for (int i = 0; i < newListeners.Length; i++)
				{
					newListeners[i].OnFocused(previousGO);
				}
			}

			// Notify subscribers
			OnFocusChanged?.Invoke(previous, Current);
		}

		public virtual void OnFocused(GameObject previous)
		{
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = focusColor;
			}
		}

		public virtual void OnDefocused(GameObject next)
		{
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = normalColor;
			}
		}
	}
}



