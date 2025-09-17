using UnityEngine;

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

	public void Accumulate(float deltaTime)
	{
		if (maxForce <= 0f || accumulationPerSecond <= 0f) return;
		EnsureArraySized();
		if (activeBarIndex < 0 || activeBarIndex >= barForces.Length) return;
		barForces[activeBarIndex] = Mathf.Min(maxForce, barForces[activeBarIndex] + accumulationPerSecond * deltaTime);
	}

	public void Add(float amount)
	{
		if (amount <= 0f) return;
		EnsureArraySized();
		if (activeBarIndex < 0 || activeBarIndex >= barForces.Length) return;
		barForces[activeBarIndex] = Mathf.Min(maxForce, barForces[activeBarIndex] + amount);
	}

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
	}

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
	}

	public void SetRequiredBars(int count)
	{
		int newCount = Mathf.Max(1, count);
		if (newCount == requiredBars && barForces != null && barForces.Length == newCount) return;
		requiredBars = newCount;
		barForces = new float[requiredBars];
		activeBarIndex = 0;
		completedBarsCount = 0;
	}

	private void EnsureArraySized()
	{
		if (barForces == null || barForces.Length != Mathf.Max(1, requiredBars))
		{
			barForces = new float[Mathf.Max(1, requiredBars)];
			activeBarIndex = Mathf.Clamp(activeBarIndex, 0, barForces.Length - 1);
		}
	}

	// UI/Panel có thể gọi các accessor này để render
	public float[] GetBarForces()
	{
		EnsureArraySized();
		float[] clone = new float[barForces.Length];
		for (int i = 0; i < barForces.Length; i++) clone[i] = barForces[i];
		return clone;
	}
}


