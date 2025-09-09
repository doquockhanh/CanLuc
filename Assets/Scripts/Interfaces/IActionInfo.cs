using UnityEngine;


public interface IActionInfo
{
	/// <summary>
	/// Mô tả hành động
	/// </summary>
	string ActionDescription { get; }

	/// <summary>
	/// Offset cho UI panel
	/// </summary>
	Vector2 Offset { get; }

	/// <summary>
	/// Lấy thông tin đầy đủ để hiển thị
	/// </summary>
	string GetFullDescription();

	string GetName();
}