using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Gắn vào GameObject chỉ huy. Khi đối tượng này đang được focus,
	/// nhấn phím tích lũy (accumulateKey) sẽ yêu cầu FocusManager
	/// thực thi toàn bộ các đối tượng đã được đăng ký (registry).
	/// Đây là component đánh dấu (marker), không chứa logic riêng.
	/// </summary>
	public class CommanderExecuteAction : MonoBehaviour {}
}




