namespace MattEdmondson.Conway;

/// <summary>
/// Provides an entry point for running Conway's Game of Life simulation.
/// </summary>
public static class Conway
{
	/// <summary>
	/// Entry point for the Conway's Game of Life simulation.
	/// </summary>
	public static void Main()
	{
		string initialState = ReadInitialState();
		string finalState = Run(initialState, 10);
		Console.WriteLine(finalState);
	}

	/// <summary>
	/// Reads the initial state of the simulation from standard input as a string in Life 1.06 format.
	/// </summary>
	/// <returns>A string containing the initial state in Life 1.06 format.</returns>
	private static string ReadInitialState() =>
		Console.In.ReadToEnd().Trim();

	/// <summary>
	/// Runs the Conway's Game of Life simulation with the given initial state and outputs the final state to standard output.
	/// </summary>
	/// <param name="initialState">The initial state of the simulation in Life 1.06 format.</param>
	/// <param name="ticks">The number of ticks to run the simulation for.</param>
	/// <returns>The final state of the simulation in Life 1.06 format.</returns>
	private static string Run(string initialState, int ticks)
	{
		Simulation simulation = new();
		simulation.Deserialize(initialState);
		for (int i = 0; i < ticks; i++)
		{
			simulation.Tick();
		}
		return simulation.Serialize();
	}
}
