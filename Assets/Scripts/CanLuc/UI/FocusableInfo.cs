using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Component chứa thông tin hiển thị UI cho Focusable object
	/// </summary>
	public class FocusableInfo : MonoBehaviour
	{
		[Header("Thông tin hiển thị")]
		[SerializeField] private string actionDescription = "This object will move forward when executed";
		
		[Header("UI Settings")]
		[SerializeField] private Vector2 offset = new Vector2(0, 1f);
		
		public string ActionDescription => actionDescription;
		public Vector2 Offset => offset;
	}
}
