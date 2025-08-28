using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Component that stores and exposes an accumulated force value per object.
	/// </summary>
	public class ForceAccumulator : MonoBehaviour
	{
		[Header("Force Accumulation")]
		[SerializeField] private float maxForce = 100f;
		[SerializeField] private float accumulationPerSecond = 25f;
		[SerializeField] private float currentForce = 0f;

		public float CurrentForce => currentForce;
		public float MaxForce => maxForce;

		/// <summary>
		/// Increase force over time. Call while input is held.
		/// </summary>
		/// <param name="deltaTime">Usually Time.deltaTime</param>
		public void Accumulate(float deltaTime)
		{
			if (maxForce <= 0f || accumulationPerSecond <= 0f) return;
			currentForce = Mathf.Min(maxForce, currentForce + accumulationPerSecond * deltaTime);
		}

		/// <summary>
		/// Manually add force.
		/// </summary>
		public void Add(float amount)
		{
			if (amount <= 0f) return;
			currentForce = Mathf.Min(maxForce, currentForce + amount);
		}

		/// <summary>
		/// Consume and reset accumulated force, returning the value.
		/// </summary>
		public float Consume()
		{
			float value = currentForce;
			currentForce = 0f;
			return value;
		}

		public void ResetForce()
		{
			currentForce = 0f;
		}
	}
}


