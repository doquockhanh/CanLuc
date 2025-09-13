using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ForceBarsPanel : MonoBehaviour
{
	[Header("Layout")]
	[SerializeField] private RectTransform barsContainer;

	[Header("Prefabs")]
	[SerializeField] private GameObject barTemplatePrefab;
	[SerializeField] private string defaultTemplateResourcePath = "Prefabs/ForceBarTemplate";

	private readonly List<Image> barFills = new List<Image>();
	private ForceAccumulator observedAccumulator;

	void Awake()
	{
		// Tự tìm container nếu không gán
		if (barsContainer == null)
		{
			barsContainer = GetComponent<RectTransform>();
		}
		// Tự load template nếu không gán
		if (barTemplatePrefab == null && !string.IsNullOrEmpty(defaultTemplateResourcePath))
		{
			barTemplatePrefab = Resources.Load<GameObject>(defaultTemplateResourcePath);
		}
	}

	void OnEnable()
	{
		FocusableBase.OnFocusChanged += HandleFocusChanged;
		// Initialize theo focus hiện tại nếu có
		if (FocusableBase.Current != null)
		{
			HandleFocusChanged(null, FocusableBase.Current);
		}
		else
		{
			BindAccumulator(null);
		}
	}

	void OnDisable()
	{
		FocusableBase.OnFocusChanged -= HandleFocusChanged;
		BindAccumulator(null);
	}

	private void HandleFocusChanged(FocusableBase previous, FocusableBase current)
	{
		ForceAccumulator accumulator = null;
		if (current != null)
		{
			accumulator = current.GetComponent<ForceAccumulator>();
		}
		BindAccumulator(accumulator);
	}

	private void BindAccumulator(ForceAccumulator accumulator)
	{
		observedAccumulator = accumulator;
		RebuildBars();
		UpdateVisibility(observedAccumulator != null);
	}

	private void RebuildBars()
	{
		// Clear children
		for (int i = barsContainer.childCount - 1; i >= 0; i--)
		{
			var child = barsContainer.GetChild(i);
			child?.SetParent(null);
			Destroy(child.gameObject);
		}
		barFills.Clear();

		if (observedAccumulator == null || barTemplatePrefab == null || barsContainer == null)
		{
			return;
		}

		int barCount = Mathf.Max(1, observedAccumulator.RequiredBars);
		for (int i = 0; i < barCount; i++)
		{
			GameObject bar = Instantiate(barTemplatePrefab, barsContainer);
			bar.name = $"ForceBar_{i + 1}";
			Image fill = FindFillImage(bar);
			if (fill != null)
			{
				barFills.Add(fill);
			}
		}
	}

	private Image FindFillImage(GameObject bar)
	{
		// Tìm Image có kiểu Filled (thường là phần fill của thanh)
		var images = bar.GetComponentsInChildren<Image>(true);
		for (int i = 0; i < images.Length; i++)
		{
			if (images[i].type == Image.Type.Filled)
				return images[i];
		}
		// fallback: lấy Image con đầu tiên
		return images != null && images.Length > 0 ? images[0] : null;
	}

	void Update()
	{
		if (observedAccumulator == null)
		{
			UpdateVisibility(false);
			return;
		}
		float max = Mathf.Max(0.0001f, observedAccumulator.MaxForce);
		float[] values = observedAccumulator.GetBarForces();
		int activeIndex = Mathf.Clamp(observedAccumulator.ActiveBarIndex, 0, values.Length - 1);
		int required = Mathf.Max(1, observedAccumulator.RequiredBars);

		// Bảo đảm số thanh khớp khi requiredBars thay đổi động
		if (barFills.Count != required)
		{
			RebuildBars();
		}

		int count = Mathf.Min(barFills.Count, values.Length);
		for (int i = 0; i < count; i++)
		{
			Image fill = barFills[i];
			if (fill == null) continue;
			float v = Mathf.Clamp01(values[i] / max);
			fill.fillAmount = v;
		}
	}

	private void UpdateVisibility(bool visible)
	{
		if (barsContainer != null)
		{
			barsContainer.gameObject.SetActive(visible);
		}
	}
}


