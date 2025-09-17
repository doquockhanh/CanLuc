using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ActionBase : MonoBehaviour, IFocusable, IGamePhaseAware
{
	[Header("Focus Visuals")]
	[SerializeField] protected static Color focusColor = new Color(0, 1, 0.92f, 1);
	[SerializeField] protected static Color normalColor = new Color(0, 0.6f, 0.2f, 1);

	[Header("Input Settings")]
	[SerializeField] protected KeyCode accumulateKey = KeyCode.Space;
	[SerializeField] protected bool canAccumulateForce = true;

	protected Renderer cachedRenderer;
	protected ForceAccumulator forceAccumulator;
	protected bool isFocused = false;
	protected bool hasAccumulatedForce = false;
	protected bool accumulationFinalized = false; // khóa sau lần tích lực đầu tiên

	// Global focus state
	public static ActionBase Current { get; private set; }
	public static GameObject CurrentGameObject => Current != null ? Current.gameObject : null;
	public static System.Action<ActionBase, ActionBase> OnFocusChanged; // (previous, current)

	protected virtual void Awake()
	{
		cachedRenderer = GetComponentInChildren<Renderer>();
		forceAccumulator = GetComponent<ForceAccumulator>();
		
		// Cấu hình số thanh lực dựa trên các Action đính kèm
		ConfigureRequiredForceBars();
		
		// Đăng ký với GameManager để lắng nghe GamePhase changes
		if (GameManager.Instance != null)
		{
			GameManager.Instance.RegisterGamePhaseAwareComponent(this);
		}
	}

	private void ConfigureRequiredForceBars()
	{
		if (forceAccumulator == null) return;
		int bars = 1;
		var multiActions = GetComponentsInChildren<IMultiForceAction>(true);
		for (int i = 0; i < multiActions.Length; i++)
		{
			if (multiActions[i] != null)
			{
				bars = Mathf.Max(bars, Mathf.Max(1, multiActions[i].ForceBarCount));
			}
		}
		forceAccumulator.SetRequiredBars(bars);
	}

	protected virtual void OnDestroy()
	{
		// Hủy đăng ký khi component bị destroy
		if (GameManager.Instance != null)
		{
			GameManager.Instance.UnregisterGamePhaseAwareComponent(this);
		}
	}

	protected virtual void Update()
	{
		// Chỉ xử lý input khi đang được focus và ở Prepare phase
		if (isFocused && GameManager.Instance != null && GameManager.Instance.IsInPreparePhase())
		{
			HandleInput();
		}

		// Unfocus bằng chuột phải khi không click lên UI
		if (Input.GetMouseButtonDown(1))
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
			ClearFocus();
		}
	}

	protected virtual void HandleInput()
	{
		if (!canAccumulateForce || forceAccumulator == null) return;
		if (accumulationFinalized) return; // chỉ cho phép tích lực 1 lần

		// Xử lý tích lực khi Space được nhấn/giữ
		if (Input.GetKeyDown(accumulateKey))
		{
			OnAccumulateStart();
		}

		if (Input.GetKey(accumulateKey))
		{
			OnAccumulateHold();
		}

		if (Input.GetKeyUp(accumulateKey))
		{
			OnAccumulateEnd();
		}
	}

	protected virtual void OnAccumulateStart()
	{
		if (accumulationFinalized) return;
		hasAccumulatedForce = true;
		// Có thể thêm audio feedback ở đây
	}

	protected virtual void OnAccumulateHold()
	{
		if (forceAccumulator != null && !accumulationFinalized)
		{
			forceAccumulator.Accumulate(Time.deltaTime);
		}
	}

	protected virtual void OnAccumulateEnd()
	{
		if (forceAccumulator != null)
		{
			// Mỗi lần nhả phím sẽ chốt một thanh lực
			forceAccumulator.FinalizeCurrentBar();
			// Chỉ khóa khi đã đủ số thanh yêu cầu
			accumulationFinalized = forceAccumulator.HasCompletedAllBars;
		}
	}

	void OnMouseDown()
	{
		// Ignore clicks on UI
		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
		SetFocus(this);
	}

	public static void ClearFocus()
	{
		SetFocus((ActionBase)null);
	}

	public static void SetFocus(GameObject target)
	{
		if (target == null)
		{
			SetFocus((ActionBase)null);
			return;
		}
		var focusable = target.GetComponentInParent<ActionBase>();
		SetFocus(focusable);
	}

	private static void SetFocus(ActionBase target)
	{
		if (ReferenceEquals(Current, target)) return;

		GameObject previousGO = Current != null ? Current.gameObject : null;
		IFocusable[] previousListeners = null;
		if (Current != null)
		{
			previousListeners = Current.GetComponentsInChildren<IFocusable>(true);
		}

		var previous = Current;
		Current = target;
		IFocusable[] newListeners = null;
		GameObject nextGO = null;
		if (Current != null)
		{
			nextGO = Current.gameObject;
			newListeners = Current.GetComponentsInChildren<IFocusable>(true);
		}

		// Notify previous
		if (previousListeners != null)
		{
			for (int i = 0; i < previousListeners.Length; i++)
			{
				previousListeners[i].OnDefocused(nextGO);
			}
		}

		// Notify new
		if (newListeners != null)
		{
			for (int i = 0; i < newListeners.Length; i++)
			{
				newListeners[i].OnFocused(previousGO);
			}
		}

		// Notify subscribers
		OnFocusChanged?.Invoke(previous, Current);
	}

	public virtual void OnFocused(GameObject previous)
	{
		isFocused = true;
		if (cachedRenderer != null)
		{
			cachedRenderer.material.color = focusColor;
		}
	}

	public virtual void OnDefocused(GameObject next)
	{
		isFocused = false;
		if (cachedRenderer != null)
		{
			cachedRenderer.material.color = normalColor;
		}
	}

	#region IGamePhaseAware Implementation

	public virtual void OnPreparePhaseStarted()
	{
		// Reset trạng thái khi quay về Prepare phase
		hasAccumulatedForce = false;
		accumulationFinalized = false;
		if (forceAccumulator != null)
		{
			forceAccumulator.ResetForce();
		}
	}

	public virtual void OnBattlePhaseStarted()
	{
		// Tự động execute khi chuyển sang Battle phase nếu đã tích lực
		if (hasAccumulatedForce && forceAccumulator != null)
		{
			ExecuteAccumulatedForce();
		}
	}

	public virtual void OnPhaseChanged(GamePhase newPhase)
	{
		// Có thể thêm logic chung khi phase thay đổi
	}

	#endregion

	protected virtual void ExecuteAccumulatedForce()
	{
		if (forceAccumulator == null) return;

		// Lấy cả tổng và mảng thanh để hỗ trợ cả 2 kiểu Action
		float totalForce = forceAccumulator.CurrentForce;
		float[] forces = forceAccumulator.ConsumeAllBars();
		if (totalForce <= 0f) return;

		// Execute các action đa-thanh trước
		var multiActions = GetComponentsInChildren<IMultiForceAction>(true);
		for (int i = 0; i < multiActions.Length; i++)
		{
			multiActions[i].Execute(forces);
		}

		// Execute các action đơn-thanh dùng tổng lực
		var actions = GetComponentsInChildren<IForceAction>(true);
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].Execute(totalForce);
		}
	}

	#region Public API for UI

	public void UIAccumulateStart()
	{
		OnAccumulateStart();
	}

	public void UIAccumulateHold()
	{
		OnAccumulateHold();
	}

	public void UIAccumulateEnd()
	{
		OnAccumulateEnd();
	}

	#endregion
}


