public interface IForceAction
{
	/// <summary>
	/// Execute the action using provided force value.
	/// </summary>
	/// <param name="force">Accumulated force value.</param>
	void Execute(float force);
}


