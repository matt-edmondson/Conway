namespace MattEdmondson.Conway;

using System.Text;

internal sealed class Simulation
{
	/// <summary>
	/// Represents a cell in the simulation grid.
	/// </summary>
	internal record struct Cell(long X, long Y);

	/// <summary>
	/// The format identifier for Life 1.06 files.
	/// </summary>
	private const string LifeFormatIdentifier = "#Life 1.06";

	/// <summary>
	/// Represents the set of live cells in the simulation.
	/// </summary>
	internal HashSet<Cell> LiveCells = [];

	/// <summary>
	/// Advances the simulation by one tick, updating the state of all cells.
	/// </summary>
	/// <remarks>
	/// The simulation follows the rules of Conway's Game of Life:
	/// - Any live cell with fewer than two live neighbors dies (underpopulation).
	/// - Any live cell with two or three live neighbors lives on to the next generation.
	/// - Any live cell with more than three live neighbors dies (overpopulation).
	/// - Any dead cell with exactly three live neighbors becomes a live cell (reproduction).
	/// </remarks>
	internal void Tick()
	{
		Dictionary<Cell, bool> workingCells = [];

		// Initialize the working set with all live cells and their neighbors.
		foreach (Cell cell in LiveCells)
		{
			// Mark the current cell as alive in the working set
			workingCells[cell] = true;

			// Add all neighbors to the working set
			EnumerateNeighbors(cell, neighbor => workingCells[neighbor] = LiveCells.Contains(neighbor));
		}

		// Evaluate the next state for each cell in the working set.
		foreach (KeyValuePair<Cell, bool> kvp in workingCells.ToList())
		{
			Cell cell = kvp.Key;
			bool isAlive = kvp.Value;
			int liveNeighbors = CountLiveNeighbors(cell);
			if (isAlive)
			{
				if (liveNeighbors is < 2 or > 3)
				{
					workingCells[cell] = false;
				}
			}
			else
			{
				if (liveNeighbors == 3)
				{
					workingCells[cell] = true;
				}
			}
		}

		IEnumerable<Cell> resultingLiveCells = workingCells
			.Where(kvp => kvp.Value)
			.Select(kvp => kvp.Key);

		LiveCells = [.. resultingLiveCells];
	}

	/// <summary>
	/// Serializes the simulation state to a string in Life 1.06 format.
	/// </summary>
	/// <returns>A string containing the serialized simulation state.</returns>
	internal string Serialize()
	{
		StringBuilder sb = new();
		sb.AppendLine(LifeFormatIdentifier);
		foreach (Cell cell in LiveCells)
		{
			sb.AppendLine($"{cell.X} {cell.Y}");
		}
		return sb.ToString();
	}

	/// <summary>
	/// Deserializes the simulation state from a string in Life 1.06 format.
	/// </summary>
	/// <param name="stateString">The string containing the serialized simulation state.</param>
	/// <exception cref="FormatException">Thrown if the state string is not in a valid Life 1.06 format.</exception>
	internal void Deserialize(string stateString)
	{
		LiveCells.Clear();
		string[] lines = stateString
			.Replace("\r", "\n")
			.Split('\n', StringSplitOptions.RemoveEmptyEntries);

		if (lines.Length == 0)
		{
			throw new FormatException($"Invalid Life 1.06 format. Expected identifier: [{LifeFormatIdentifier}] Received: [empty]");
		}

		string identifier = lines[0].Trim();
		if (!IsValidFormatIdentifier(identifier))
		{
			throw new FormatException($"Invalid Life 1.06 format. Expected identifier: [{LifeFormatIdentifier}] Received: [{identifier}]");
		}

		IEnumerable<string> linesWithoutIdentifier = lines.Skip(1);
		foreach (string? line in linesWithoutIdentifier)
		{
			Cell cell = ReadCellFromLine(line);
			LiveCells.Add(cell);
		}
	}

	/// <summary>
	/// Counts the number of live neighbors for the given cell.
	/// </summary>
	/// <param name="cell">The cell for which to count live neighbors.</param>
	/// <returns>The number of live neighbors.</returns>
	private int CountLiveNeighbors(Cell cell)
	{
		int count = 0;
		EnumerateNeighbors(cell, neighbor =>
		{
			if (LiveCells.Contains(neighbor))
			{
				count++;
			}
		});
		return count;
	}

	/// <summary>
	/// Determines whether the given cell is the same as the offset (dx, dy) from itself.
	/// </summary>
	/// <param name="dx">The x offset.</param>
	/// <param name="dy">The y offset.</param>
	/// <returns>True if the offset corresponds to the cell itself; otherwise, false.</returns>
	private static bool IsSelf(long dx, long dy) => dx == 0 && dy == 0;

	/// <summary>
	/// Determines whether the given neighbor offset is valid for the specified cell.
	/// </summary>
	/// <param name="cell">The cell to check.</param>
	/// <param name="dx">The x offset of the neighbor.</param>
	/// <param name="dy">The y offset of the neighbor.</param>
	/// <returns>True if the neighbor is valid; otherwise, false.</returns>
	private static bool IsValidNeighbour(Cell cell, long dx, long dy) =>
		!IsSelf(dx, dy) &&
		!IsOutOfBounds(cell.X, dx) &&
		!IsOutOfBounds(cell.Y, dy);

	/// <summary>
	/// Determines whether the given coordinate with the specified delta is out of bounds.
	/// </summary>
	/// <param name="n">The coordinate to check.</param>
	/// <param name="delta">The delta to apply to the coordinate.</param>
	/// <returns>True if the coordinate is out of bounds; otherwise, false.</returns>
	private static bool IsOutOfBounds(long n, long delta) =>
		(delta > 0 && n == long.MaxValue) || (delta < 0 && n == long.MinValue);

	/// <summary>
	/// Enumerates all neighbors of the given cell and performs the specified action on each neighbor.
	/// </summary>
	/// <param name="cell">The cell whose neighbors to enumerate.</param>
	/// <param name="action">The action to perform on each neighbor.</param>
	/// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
	private static void EnumerateNeighbors(Cell cell, Action<Cell> action)
	{
		Ensure.NotNull(action);

		for (long dx = -1; dx <= 1; dx++)
		{
			for (long dy = -1; dy <= 1; dy++)
			{
				if (IsValidNeighbour(cell, dx, dy))
				{
					action(new Cell(cell.X + dx, cell.Y + dy));
				}
			}
		}
	}

	/// <summary>
	/// Determines whether the given line is a valid Life 1.06 format identifier.
	/// </summary>
	/// <param name="line">The line to check.</param>
	/// <returns>True if the line is a valid Life 1.06 format identifier; otherwise, false.</returns>
	private static bool IsValidFormatIdentifier(string line) => line == LifeFormatIdentifier;

	/// <summary>
	/// Reads a cell from a line in the Life 1.06 format.
	/// </summary>
	/// <param name="line">The line containing the cell coordinates.</param>
	/// <returns>The cell represented by the line.</returns>
	/// <exception cref="FormatException">Thrown if the line is not in a valid cell format.</exception>
	private static Cell ReadCellFromLine(string line)
	{
		string[] parts = line
			.Trim()
			.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 2)
		{
			throw new FormatException("Invalid cell line format.");
		}

		if (long.TryParse(parts[0], out long x) && long.TryParse(parts[1], out long y))
		{
			return new Cell(x, y);
		}

		throw new FormatException("Invalid cell line format.");
	}
}
