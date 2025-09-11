using UnityEngine;
using UnityEngine.UI;


public class ForceAccumulator : MonoBehaviour
{
	[Header("Force Accumulation")]
	[SerializeField] private float maxForce = 100f;
	[SerializeField] private float accumulationPerSecond = 25f;
	[SerializeField] private int requiredBars = 1;

	// Internal state per bar
	[SerializeField] private float[] barForces = new float[1];
	[SerializeField] private int activeBarIndex = 0;
	[SerializeField] private int completedBarsCount = 0;
	[SerializeField] private GameObject forceBar;
	[SerializeField] private Image forceImage;

	public float CurrentForce
	{
		get
		{
			float sum = 0f;
			if (barForces != null)
			{
				for (int i = 0; i < barForces.Length; i++) sum += barForces[i];
			}
			return sum;
		}
	}
	public float MaxForce => maxForce;
	public int RequiredBars => requiredBars;
	public int ActiveBarIndex => activeBarIndex;
	public int CompletedBarsCount => completedBarsCount;
	public bool HasCompletedAllBars => completedBarsCount >= Mathf.Max(1, requiredBars);

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
		// Ẩn/hiện forceBar dựa trên tổng currentForce
		if (forceBar != null)
		{
			forceBar.SetActive(CurrentForce > 0f);
		}

		// Cập nhật kích thước forceImage dựa trên tỷ lệ của thanh hiện tại
		if (forceImage != null && maxForce > 0f)
		{
			float currentBarValue = 0f;
			if (barForces != null && activeBarIndex >= 0 && activeBarIndex < barForces.Length)
			{
				currentBarValue = barForces[activeBarIndex];
			}
			float fillAmount = currentBarValue / maxForce;
			forceImage.fillAmount = Mathf.Clamp01(fillAmount);
		}
	}

	/// <summary>
	/// Increase force over time for the current active bar. Call while input is held.
	/// </summary>
	/// <param name="deltaTime">Usually Time.deltaTime</param>
	public void Accumulate(float deltaTime)
	{
		if (maxForce <= 0f || accumulationPerSecond <= 0f) return;
		EnsureArraySized();
		if (activeBarIndex < 0 || activeBarIndex >= barForces.Length) return;
		barForces[activeBarIndex] = Mathf.Min(maxForce, barForces[activeBarIndex] + accumulationPerSecond * deltaTime);
		UpdateForceDisplay();
	}

	/// <summary>
	/// Manually add force to the current active bar.
	/// </summary>
	public void Add(float amount)
	{
		if (amount <= 0f) return;
		EnsureArraySized();
		if (activeBarIndex < 0 || activeBarIndex >= barForces.Length) return;
		barForces[activeBarIndex] = Mathf.Min(maxForce, barForces[activeBarIndex] + amount);
		UpdateForceDisplay();
	}

	/// <summary>
	/// Finalize the current bar and advance to the next bar if available.
	/// </summary>
	public void FinalizeCurrentBar()
	{
		EnsureArraySized();
		// Mark current bar as completed (even if value is small)
		if (completedBarsCount < requiredBars)
		{
			completedBarsCount = Mathf.Max(completedBarsCount, activeBarIndex + 1);
		}
		// Move to next bar if any
		if (activeBarIndex < requiredBars - 1)
		{
			activeBarIndex++;
		}
		UpdateForceDisplay();
	}

	/// <summary>
	/// Consume as a single total (sum of all bars) and reset all bars.
	/// Backward-compatible with old single-bar behavior.
	/// </summary>
	public float Consume()
	{
		float sum = 0f;
		if (barForces != null)
		{
			for (int i = 0; i < barForces.Length; i++) sum += barForces[i];
		}
		ResetForce();
		return sum;
	}

	/// <summary>
	/// Consume and return all bars as an array (length = RequiredBars), then reset all.
	/// </summary>
	public float[] ConsumeAllBars()
	{
		EnsureArraySized();
		float[] result = new float[requiredBars];
		for (int i = 0; i < requiredBars; i++) result[i] = barForces[i];
		ResetForce();
		return result;
	}

	public void ResetForce()
	{
		EnsureArraySized();
		for (int i = 0; i < barForces.Length; i++) barForces[i] = 0f;
		activeBarIndex = 0;
		completedBarsCount = 0;
		UpdateForceDisplay();
	}

	/// <summary>
	/// Configure how many bars are required by the attached actions.
	/// </summary>
	public void SetRequiredBars(int count)
	{
		int newCount = Mathf.Max(1, count);
		if (newCount == requiredBars && barForces != null && barForces.Length == newCount) return;
		requiredBars = newCount;
		barForces = new float[requiredBars];
		activeBarIndex = 0;
		completedBarsCount = 0;
		UpdateForceDisplay();
	}

	private void EnsureArraySized()
	{
		if (barForces == null || barForces.Length != Mathf.Max(1, requiredBars))
		{
			barForces = new float[Mathf.Max(1, requiredBars)];
			activeBarIndex = Mathf.Clamp(activeBarIndex, 0, barForces.Length - 1);
		}
	}
}


