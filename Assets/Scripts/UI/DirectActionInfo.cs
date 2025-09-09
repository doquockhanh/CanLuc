using UnityEngine;


public class DirectActionInfo : MonoBehaviour, IActionInfo
{
	[Header("Thông tin hiển thị")]
	[SerializeField] private string actionDescription = "Direct Action Object - Hold Space to rotate, A/D to move";

	[Header("UI Settings")]
	[SerializeField] private Vector2 offset = new Vector2(0, 1f);

	[Header("Input Hints")]
	[SerializeField] private string rotationHint = "Hold SPACE to rotate child";
	[SerializeField] private string movementHint = "Press A/D to move left/right";

	public string ActionDescription => actionDescription;
	public Vector2 Offset => offset;
	public string RotationHint => rotationHint;
	public string MovementHint => movementHint;

	/// <summary>
	/// Lấy thông tin đầy đủ bao gồm cả hints
	/// </summary>
	public string GetFullDescription()
	{
		return $"{actionDescription}\n{rotationHint}\n{movementHint}";
	}
}
