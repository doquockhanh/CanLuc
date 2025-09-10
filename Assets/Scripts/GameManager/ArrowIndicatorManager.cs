using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns an arrow indicator above every object implementing IFocusable and keeps it updated.
/// Accepts a Sprite for the arrow image and applies a gentle up-down bobbing.
/// </summary>
public class ArrowIndicatorManager : MonoBehaviour
{
	[Header("Indicator Visual")]
	[SerializeField] private GameObject arrowSprite;
	[SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

	[Header("Bobbing")]
	[SerializeField] private float bobAmplitude = 0.25f;
	[SerializeField] private float bobSpeed = 2.0f;

	[Header("Auto Refresh")]
	[SerializeField] private bool autoRefresh = true;
	[SerializeField] private float refreshIntervalSeconds = 2.0f;

	private readonly Dictionary<MonoBehaviour, ArrowIndicatorFollower> focusableToIndicator = new Dictionary<MonoBehaviour, ArrowIndicatorFollower>();
	private float refreshTimer;

	private void OnEnable()
	{
		RefreshIndicators();
	}

	private void OnDisable()
	{
		ClearAllIndicators();
	}

	private void Update()
	{
		if (!autoRefresh) return;
		refreshTimer += Time.unscaledDeltaTime;
		if (refreshTimer >= refreshIntervalSeconds)
		{
			refreshTimer = 0f;
			RefreshIndicators();
		}
	}

	/// <summary>
	/// Finds all objects implementing IFocusable and ensures each has an indicator.
	/// Removes indicators for objects that no longer exist.
	/// </summary>
	public void RefreshIndicators()
	{
		RemoveDestroyedEntries();

		// Find all MonoBehaviours that implement IFocusable
		var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < allBehaviours.Length; i++)
		{
			MonoBehaviour behaviour = allBehaviours[i];
			if (behaviour == null) continue;
			if (behaviour is IFocusable)
			{
				if (!focusableToIndicator.ContainsKey(behaviour))
				{
					CreateIndicator(behaviour);
				}
			}
		}
	}

	private void RemoveDestroyedEntries()
	{
		// Collect keys to remove to avoid modifying dictionary during iteration
		List<MonoBehaviour> toRemove = null;
		foreach (var kv in focusableToIndicator)
		{
			if (kv.Key == null || kv.Value == null)
			{
				(toRemove ??= new List<MonoBehaviour>()).Add(kv.Key);
			}
		}
		if (toRemove != null)
		{
			for (int i = 0; i < toRemove.Count; i++)
			{
				var key = toRemove[i];
				if (key != null && focusableToIndicator.TryGetValue(key, out var follower) && follower != null)
				{
					Destroy(follower.gameObject);
				}
				focusableToIndicator.Remove(key);
			}
		}
	}

	private void CreateIndicator(MonoBehaviour focusableBehaviour)
	{
		if (focusableBehaviour == null) return;
		if (focusableBehaviour.transform == null) return;

		GameObject go = Instantiate(arrowSprite, focusableBehaviour.transform);
		var follower = go.AddComponent<ArrowIndicatorFollower>();
		follower.Initialize(focusableBehaviour.transform, worldOffset, bobAmplitude, bobSpeed);

		focusableToIndicator.Add(focusableBehaviour, follower);
	}

	private void ClearAllIndicators()
	{
		foreach (var kv in focusableToIndicator)
		{
			if (kv.Value != null)
			{
				Destroy(kv.Value.gameObject);
			}
		}
		focusableToIndicator.Clear();
	}
}


