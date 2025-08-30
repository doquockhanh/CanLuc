using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Interface chung cho các component chứa thông tin hiển thị UI
	/// </summary>
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
	}
}
