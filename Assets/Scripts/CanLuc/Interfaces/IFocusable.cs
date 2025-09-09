using UnityEngine;


public interface IFocusable
{
	/// <summary>
	/// Called by FocusManager when this object becomes the current focus.
	/// </summary>
	/// <param name="previous">Previously focused object, if any.</param>
	void OnFocused(GameObject previous);

	/// <summary>
	/// Called by FocusManager when this object is no longer the current focus.
	/// </summary>
	/// <param name="next">Next focused object, if any.</param>
	void OnDefocused(GameObject next);
}


