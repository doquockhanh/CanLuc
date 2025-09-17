using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ForceBarsPanel : MonoBehaviour
{
	[Header("Layout")]
	[SerializeField] private RectTransform barsContainer;
	[SerializeField] private Button accumulateButton;

	[Header("Prefabs")]
	[SerializeField] private GameObject barTemplatePrefab;
	[SerializeField] private string defaultTemplateResourcePath = "Prefabs/ForceBarTemplate";

	[Header("Button Settings")]
	[SerializeField] private float holdThreshold = 0.1f; // Thời gian tối thiểu để bắt đầu tích lực (giây)

	private readonly List<Image> barFills = new List<Image>();
	private ForceAccumulator observedAccumulator;
	private ActionBase currentFocusable;
	private bool isButtonPressed = false;
	private bool isAccumulating = false;
	private float holdStartTime = 0f;

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
		// Setup button events
		SetupAccumulateButton();
	}

	void OnEnable()
	{
		ActionBase.OnFocusChanged += HandleFocusChanged;
		// Initialize theo focus hiện tại nếu có
		if (ActionBase.Current != null)
		{
			HandleFocusChanged(null, ActionBase.Current);
		}
		else
		{
			BindAccumulator(null);
		}
	}

	void OnDisable()
	{
		ActionBase.OnFocusChanged -= HandleFocusChanged;
		BindAccumulator(null);
	}

	private void HandleFocusChanged(ActionBase previous, ActionBase current)
	{
		currentFocusable = current;
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

		// Xử lý hold button
		HandleButtonHold();

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
		if (accumulateButton != null)
		{
			accumulateButton.gameObject.SetActive(visible);
		}
	}

	private void SetupAccumulateButton()
	{
		if (accumulateButton == null) return;

		// Setup hold events với EventTrigger
		SetupButtonHoldEvents();
	}

	private void SetupButtonHoldEvents()
	{
		if (accumulateButton == null) return;

		// Thêm EventTrigger nếu chưa có
		var eventTrigger = accumulateButton.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = accumulateButton.gameObject.AddComponent<EventTrigger>();
		}

		// Clear existing entries
		eventTrigger.triggers.Clear();

		// PointerDown - bắt đầu hold
		var pointerDownEntry = new EventTrigger.Entry();
		pointerDownEntry.eventID = EventTriggerType.PointerDown;
		pointerDownEntry.callback.AddListener((data) => { OnButtonPointerDown(); });
		eventTrigger.triggers.Add(pointerDownEntry);

		// PointerUp - kết thúc hold
		var pointerUpEntry = new EventTrigger.Entry();
		pointerUpEntry.eventID = EventTriggerType.PointerUp;
		pointerUpEntry.callback.AddListener((data) => { OnButtonPointerUp(); });
		eventTrigger.triggers.Add(pointerUpEntry);

		// PointerExit - kết thúc hold khi chuột rời khỏi button
		var pointerExitEntry = new EventTrigger.Entry();
		pointerExitEntry.eventID = EventTriggerType.PointerExit;
		pointerExitEntry.callback.AddListener((data) => { OnButtonPointerExit(); });
		eventTrigger.triggers.Add(pointerExitEntry);
	}

	private void HandleButtonHold()
	{
		if (!isButtonPressed) return;

		// Kiểm tra nếu đã hold đủ thời gian threshold
		if (!isAccumulating && Time.time - holdStartTime >= holdThreshold)
		{
			OnAccumulateStart();
		}

		// Nếu đang tích lực thì tiếp tục tích
		if (isAccumulating)
		{
			OnAccumulateHold();
		}
	}

	private void OnButtonPointerDown()
	{
		if (currentFocusable == null || observedAccumulator == null) return;
		if (GameManager.Instance != null && !GameManager.Instance.IsInPreparePhase()) return;

		// Bắt đầu hold
		isButtonPressed = true;
		holdStartTime = Time.time;
	}

	private void OnButtonPointerUp()
	{
		if (!isButtonPressed) return;

		// Kết thúc hold
		isButtonPressed = false;
		if (isAccumulating)
		{
			OnAccumulateEnd();
		}
	}

	private void OnButtonPointerExit()
	{
		if (!isButtonPressed) return;

		// Kết thúc hold khi chuột rời khỏi button
		isButtonPressed = false;
		if (isAccumulating)
		{
			OnAccumulateEnd();
		}
	}

	private void OnAccumulateStart()
	{
		if (isAccumulating) return;
		isAccumulating = true;
		
		// Gọi phương thức của ActionBase để đảm bảo logic nhất quán
		if (currentFocusable != null)
		{
			currentFocusable.UIAccumulateStart();
		}
		
		// Visual feedback cho button
		if (accumulateButton != null)
		{
			var colors = accumulateButton.colors;
			colors.normalColor = Color.yellow;
			accumulateButton.colors = colors;
		}
	}

	private void OnAccumulateHold()
	{
		if (!isAccumulating) return;
		
		// Gọi phương thức của ActionBase để đảm bảo logic nhất quán
		if (currentFocusable != null)
		{
			currentFocusable.UIAccumulateHold();
		}
	}

	private void OnAccumulateEnd()
	{
		if (!isAccumulating) return;
		isAccumulating = false;
		
		// Gọi phương thức của ActionBase để đảm bảo logic nhất quán
		if (currentFocusable != null)
		{
			currentFocusable.UIAccumulateEnd();
		}

		// Reset button color
		if (accumulateButton != null)
		{
			var colors = accumulateButton.colors;
			colors.normalColor = Color.white;
			accumulateButton.colors = colors;
		}
	}
}


