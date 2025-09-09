using UnityEngine;

public class FocusableInfo : MonoBehaviour, IActionInfo
{
	[Header("Thông tin hiển thị")]
	[SerializeField] private string objectName = "abc";
	[SerializeField] private string actionDescription = "This object will move forward when executed";

	[Header("UI Settings")]
	[SerializeField] private Vector2 offset = new Vector2(0, 1f);

	public string ActionDescription => actionDescription;
	public Vector2 Offset => offset;

	public string GetFullDescription()
	{
		return actionDescription;
	}

	public string GetName()
	{
		return objectName;
	}
}
