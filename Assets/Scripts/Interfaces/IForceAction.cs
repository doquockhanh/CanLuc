public interface IForceAction
{
	/// <summary>
	/// Execute the action using provided force value.
	/// </summary>
	/// <param name="force">Accumulated force value.</param>
	void Execute(float force);
}


/// <summary>
/// Optional interface for actions that require multiple force bars.
/// Implement this to receive an array of forces and to declare how many bars are needed.
/// </summary>
public interface IMultiForceAction
{
    /// <summary>
    /// Number of force bars required for this action.
    /// </summary>
    int ForceBarCount { get; }

    /// <summary>
    /// Execute using multiple force bars. The length of forces will be equal to ForceBarCount.
    /// </summary>
    /// <param name="forces">Array of accumulated forces, in the order they were filled.</param>
    void Execute(float[] forces);
}


