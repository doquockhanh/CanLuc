using UnityEngine;
using UnityEngine.UI;


public class ForceAccumulator : MonoBehaviour
{
	[Header("Force Accumulation")]
	[SerializeField] private float maxForce = 100f;
	[SerializeField] private float accumulationPerSecond = 25f;
	[SerializeField] private float currentForce = 0f;
	[SerializeField] private GameObject forceBar;
	[SerializeField] private Image forceImage;

	public float CurrentForce => currentForce;
	public float MaxForce => maxForce;

	private void Start()
	{
		UpdateForceDisplay();
	}

	private void Update()
	{
		UpdateForceDisplay();
	}

	/// <summary>
	/// Cập nhật hiển thị force bar và force image
	/// </summary>
	private void UpdateForceDisplay()
	{
		// Ẩn/hiện forceBar dựa trên currentForce
		if (forceBar != null)
		{
			forceBar.SetActive(currentForce > 0f);
		}

		// Cập nhật kích thước forceImage dựa trên tỷ lệ currentForce/maxForce
		if (forceImage != null && maxForce > 0f)
		{
			float fillAmount = currentForce / maxForce;
			forceImage.fillAmount = Mathf.Clamp01(fillAmount);
		}
	}

	/// <summary>
	/// Increase force over time. Call while input is held.
	/// </summary>
	/// <param name="deltaTime">Usually Time.deltaTime</param>
	public void Accumulate(float deltaTime)
	{
		if (maxForce <= 0f || accumulationPerSecond <= 0f) return;
		currentForce = Mathf.Min(maxForce, currentForce + accumulationPerSecond * deltaTime);
		UpdateForceDisplay();
	}

	/// <summary>
	/// Manually add force.
	/// </summary>
	public void Add(float amount)
	{
		if (amount <= 0f) return;
		currentForce = Mathf.Min(maxForce, currentForce + amount);
		UpdateForceDisplay();
	}

	/// <summary>
	/// Consume and reset accumulated force, returning the value.
	/// </summary>
	public float Consume()
	{
		float value = currentForce;
		currentForce = 0f;
		UpdateForceDisplay();
		return value;
	}

	public void ResetForce()
	{
		currentForce = 0f;
		UpdateForceDisplay();
	}
}


