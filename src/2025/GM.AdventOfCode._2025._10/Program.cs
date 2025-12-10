using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using GM.Utility;

var processes = new List<Process>
{
	new()
	{
		InputFileName = "inputExample.txt",
		ExpectedPartOne = 7,
		ExpectedPartTwo = 33,
		ReadFromFile = false
	},
	new()
	{
		InputFileName = "input.txt",
		ExpectedPartOne = 396,
		ReadFromFile = true
	}
};

foreach(var process in processes) {
	Machine.s_idCounter = 0;

	Console.WriteLine($"Start on {process.InputFileName}.");
	await process.DoWork();
	Console.WriteLine($"Finished with {process.InputFileName}.");

	Console.WriteLine($"Part One: {process.CalculatedPartOne}.");
	if(process.ExpectedPartOne != null && process.CalculatedPartOne != process.ExpectedPartOne.Value) {
		Console.WriteLine($"Wrong part one. Should be {process.ExpectedPartOne.Value}.");
	}
	Console.WriteLine($"Part Two: {process.CalculatedPartTwo}.");
	if(process.ExpectedPartTwo != null && process.CalculatedPartTwo != process.ExpectedPartTwo.Value) {
		Console.WriteLine($"Wrong part two. Should be {process.ExpectedPartTwo.Value}.");
	}

	Console.WriteLine("Press a key to continue ...");
	Console.ReadKey(true);
	Console.WriteLine();
}

class Process
{
	public required string InputFileName { get; init; }
	public long? ExpectedPartOne { get; init; }
	public long? ExpectedPartTwo { get; init; }
	public required bool ReadFromFile { get; init; }

	public long? CalculatedPartOne { get; private set; }
	public long? CalculatedPartTwo { get; private set; }

	public async Task DoWork()
	{
		string resultsFile = $"../../../results{InputFileName}.txt";

		void WriteResultToFile(int id, long buttonPresses)
		{
			using var stream = File.AppendText(resultsFile);
			stream.WriteLine($"{id}=>{buttonPresses}");
		}
		long? TryGetResultFromFile(int id)
		{
			if(File.Exists(resultsFile) == false) {
				return null;
			}
			string idString = id.ToString();
			var allLines = File.ReadAllLines(resultsFile);
			var tmp = allLines
				.Select(line => line.Split("=>"))
				.Where(x => x[0] == idString)
				.SingleOrDefault();
			return tmp == null ? null : long.Parse(tmp[1]);
		}

		string[] inputLines = await File.ReadAllLinesAsync(InputFileName);


		// The manual describes one machine per line. Each line contains a single indicator light diagram in [square brackets], one or more button wiring schematics in (parentheses), and joltage requirements in {curly braces}.
		List<Machine> machines = inputLines
			.Select(line => {
				var indicatorLightDiagramPart = line.Split('[', ']')[1];
				var buttonWiringSchematicParts = line.Split('(', ')')
					.Where((part, index) => index % 2 == 1)
					.ToList();
				var joltageRequirementParts = line.Split('{', '}')
					.Where((part, index) => index % 2 == 1)
					.ToList();
				var indicatorLightDiagrams = indicatorLightDiagramPart
					.Select(c => c == '#')
					.ToList();
				var buttonWiringSchematics = buttonWiringSchematicParts
					.Select(part => part.Split(',')
						.Select(numStr => int.Parse(numStr))
						.ToList())
					.Select(affectedPositions => new Button(affectedPositions))
					.ToList();
				var joltageRequirements = joltageRequirementParts.Single().Split(',')
					.Select(part => int.Parse(part))
					.Select(target => new Joltage(target))
					.ToList();
				return new Machine
				{
					IndicatorLightDiagrams = indicatorLightDiagrams,
					ButtonWiringSchematics = buttonWiringSchematics,
					JoltageRequirements = joltageRequirements
				};
			})
			.ToList();

		// To start a machine, its indicator lights must match those shown in the diagram, where . means off and # means on. The machine has the number of indicator lights shown, but its indicator lights are all initially off.
		// So, a button wiring schematic like (0,3,4) means that each time you push that button, the first, fourth, and fifth indicator lights would all toggle between on and off. If the indicator lights were [#.....], pushing the button would change them to be [...##.] instead.

		// part one
		{
			List<long> fewestButtonPressesPerMachine = [];

			for(int i = 0; i < machines.Count; ++i) {
				var machine = machines[i];

				// find the fewest button presses to reach the target indicator light diagram
				long fewestButtonPresses = -1;
				{
					HashSet<string> visitedStates = [];
					// (current state, buttonPressesCount)
					Queue<(List<bool> currentState, long buttonPresses)> queue = new();
					queue.Enqueue((new List<bool>(new bool[machine.IndicatorLightDiagrams!.Count]), 0));
					visitedStates.Add(string.Join(',', new bool[machine.IndicatorLightDiagrams.Count]));
					while(queue.Count > 0) {
						var (currentState, buttonPresses) = queue.Dequeue();
						if(currentState.SequenceEqual(machine.IndicatorLightDiagrams)) {
							fewestButtonPresses = buttonPresses;
							break;
						}
						foreach(var button in machine.ButtonWiringSchematics!) {
							var newState = new List<bool>(currentState);
							foreach(int position in button.AffectedPositions) {
								newState[position] = !newState[position];
							}
							string stateKey = string.Join(',', newState);
							if(visitedStates.Contains(stateKey) == false) {
								visitedStates.Add(stateKey);
								queue.Enqueue((newState, buttonPresses + 1));
							}
						}
					}
				}
				if(fewestButtonPresses < 0) {
					throw new Exception("Shouldn't happen.");
				}

				fewestButtonPressesPerMachine.Add(fewestButtonPresses);
			}

			CalculatedPartOne = fewestButtonPressesPerMachine.Sum();
		}

		Console.WriteLine($"Part One: {CalculatedPartOne}");

		// All of the machines are starting to come online! Now, it's time to worry about the joltage requirements.
		// Each machine needs to be configured to exactly the specified joltage levels to function properly. Below the buttons on each machine is a big lever that you can use to switch the buttons from configuring the indicator lights to increasing the joltage levels. (Ignore the indicator light diagrams.)
		// The machines each have a set of numeric counters tracking its joltage levels, one counter per joltage requirement. The counters are all initially set to zero.
		// The button wiring schematics are still relevant: in this new joltage configuration mode, each button now indicates which counters it affects, where 0 means the first counter, 1 means the second counter, and so on. When you push a button, each listed counter is increased by 1.

		// part two
		{
			List<long> fewestButtonPressesPerMachine = [];
			Output.InitialMachinesCount = machines.Count;
			for(int i = 0; i < machines.Count; ++i) {
				var machine = machines[i];
				// find the fewest button presses to reach the target joltage requirements
				long fewestButtonPresses;
				{
					long? fromFile = null;
					if(ReadFromFile) {
						fromFile = TryGetResultFromFile(machine.Id);
					}
					if(fromFile != null) {
						fewestButtonPresses = fromFile.Value;
					} else {
						if(Output.ProgressPartTwo) {
							Console.WriteLine();
							Console.WriteLine($"Machine {i + 1}/{machines.Count}. Id={machine.Id}.");
						}

						// init
						machine.InitializeJoltagesAndButtons();

						if(Output.DebuggingPartTwo) {
							Console.WriteLine($"Joltages: {{{string.Join(",", machine.JoltageRequirements!)}}}. Buttons: {string.Join(" ", machine.ButtonWiringSchematics!)}");
							Console.WriteLine("Start!");
						}

						fewestButtonPresses = machine.GetFewestButtonPressesForPartTwo();
						WriteResultToFile(machine.Id, fewestButtonPresses);

						if(Output.DebuggingPartTwo) {
							Console.WriteLine("End!");
							Console.WriteLine($"Machine {machine.Id}: {fewestButtonPresses} button presses.");
						}
					}
				}
				fewestButtonPressesPerMachine.Add(fewestButtonPresses);
			}
			CalculatedPartTwo = fewestButtonPressesPerMachine.Sum();
		}

		Console.WriteLine($"Part Two: {CalculatedPartTwo}");
	}
}

static class Output
{
	public static bool ProgressPartTwo = true;
	public static bool DebuggingPartTwo = false;
	public static int InitialMachinesCount;
}

class Machine
{
	public List<bool>? IndicatorLightDiagrams { get; init; }
	public List<Button>? ButtonWiringSchematics
	{
		get;
		init
		{
			field = value;
			field!.ForEach(b => b.Pressed += Button_Pressed);
		}
	}
	public List<Joltage>? JoltageRequirements { get; init; }

	public static int s_idCounter = 0;
	public int Id { get; } = ++s_idCounter;
	public Machine? Source { get; private init; }
	public List<Button> PressedButtons { get; init; } = [];

	public Machine() { }

	public Machine(Machine source)
	{
		Source = source;
		IndicatorLightDiagrams = source.IndicatorLightDiagrams;
		ButtonWiringSchematics = source.ButtonWiringSchematics!
			.Select(b => new Button(b))
			.ToList();
		JoltageRequirements = source.JoltageRequirements!
			.Select(j => new Joltage(j))
			.ToList();
		PressedButtons = [.. source.PressedButtons];
	}

	public void InitializeJoltagesAndButtons()
	{
		// link joltages and buttons
		for(int i=0;i<JoltageRequirements!.Count; ++i) {
			var joltage = JoltageRequirements[i];
			joltage.Buttons = ButtonWiringSchematics!
				.Where(button => button.AffectedPositions.Contains(i))
				.ToList();
		}
		// link buttons to joltages
		for(int i=0;i<ButtonWiringSchematics!.Count; ++i) {
			var button = ButtonWiringSchematics[i];
			button.Joltages = button.AffectedPositions
				.Select(pos => JoltageRequirements[pos])
				.ToList();
		}
	}

	public int GetFewestButtonPressesForPartTwo()
	{
		return TryGetFewestButtonPressesForPartTwo()!.Value;
	}

	private static string? s_progressMessagePrefix;
	private static Stopwatch? s_progress_stopWatch;
	private int subtractPressesCount;
	private int maxSubtract;
	private int output_buttonIndex;
	private int output_buttonCount;
	private int? TryGetFewestButtonPressesForPartTwo()
	{
		int buttonPressesCount = 0;
		while(true) {
			List<Joltage> joltagesNotAtTargetYet = JoltageRequirements!
					.Where(j => j.CanBeIncreased)
					.ToList();
			if(joltagesNotAtTargetYet.Count == 0) {
				// we are done
				break;
			}

			// start with the lowest joltage requirement
			List<Joltage> joltagesClosestToTarget;
			{
				int lowestAvailableIncreaseCount = joltagesNotAtTargetYet.Min(j => j.AvailableIncreaseCount);
				joltagesClosestToTarget = joltagesNotAtTargetYet
					.Where(j => j.AvailableIncreaseCount == lowestAvailableIncreaseCount)
					.ToList();
			}

			// select all possible buttons that we can press
			List<Button> buttonsThatCanBePressed = joltagesClosestToTarget
				.SelectMany(j => j.Buttons!)
				.Distinct()
				.ToList();

			// decide what to do
			if(buttonsThatCanBePressed.Count == 0) {
				// No buttons to press anymore, somewhere along the road it pressed the wrong buttons
				return null;
			} else if(buttonsThatCanBePressed.Count == 1) {
				// only one button, press it as many times as needed
				Button theOnlyButton = buttonsThatCanBePressed.Single();
				buttonPressesCount += theOnlyButton.AvailablePressesCount;
				theOnlyButton.Press(theOnlyButton.AvailablePressesCount);
			} else {
				// go through all the available buttons until we get to the end

				// priority order
				buttonsThatCanBePressed = buttonsThatCanBePressed
					// start with those buttons that can be pressed the least amount of times
					//.OrderBy(b => b.AvailablePressesCount)
					// start with those that affect the most number of joltages
					.OrderByDescending(b => b.Joltages!.Count)
					.ToList();

				int? pressesFurtherDown = null;
				// this subtract is stupid, could be determined more smarter
				maxSubtract = buttonsThatCanBePressed.Max(b => b.AvailablePressesCount) - 1;
				if(Id < Output.InitialMachinesCount) {
					s_progressMessagePrefix = $"Machine {Id}:";
				}
				for(subtractPressesCount = 0; ; ++subtractPressesCount) {
					bool skippedAll = true;
					output_buttonCount = buttonsThatCanBePressed.Count;
					for(int i = 0; i < buttonsThatCanBePressed.Count; ++i) {
						output_buttonIndex = i + 1;
						var buttonThatCanBePressed = buttonsThatCanBePressed[i];

						int pressCount = buttonThatCanBePressed.AvailablePressesCount - subtractPressesCount;
						if(pressCount <= 0) {
							continue;
						}
						skippedAll = false;

						if(Output.ProgressPartTwo) {
							// only output initial machines
							if(Id < Output.InitialMachinesCount) {
								s_progressMessagePrefix += $" (Subtract={subtractPressesCount}/{maxSubtract}. Button={output_buttonIndex}/{output_buttonCount}.)";
								Console.WriteLine(s_progressMessagePrefix);
								s_progress_stopWatch = new();
								s_progress_stopWatch.Start();
							} else {
								if(s_progress_stopWatch!.ElapsedMilliseconds >= 5000) {
									Console.WriteLine($"{s_progressMessagePrefix} Path={GetOutputPath_AsProgress()}. (Subtract={subtractPressesCount}/{maxSubtract}. Button={output_buttonIndex}/{output_buttonCount}.)");
									s_progress_stopWatch.Restart();
								}
							}
						}

						var machineCopy = new Machine(this);
						machineCopy.InitializeJoltagesAndButtons();

						// press this button in the machine copy
						var buttonInTheCopy = machineCopy.ButtonWiringSchematics!.Single(b => b.Source == buttonThatCanBePressed);
						buttonPressesCount += pressCount;
						buttonInTheCopy.Press(pressCount);

						// check if it's still possible to resolve the problem from here onward
						if(machineCopy.JoltageRequirements!.Any(j =>
							j.CanBeIncreased
							&& j.Buttons!.Count == 0)
							) {
							// there are uncompleted joltages without buttons to press them
							continue;
						}

						// continue further down
						pressesFurtherDown = machineCopy.TryGetFewestButtonPressesForPartTwo();
						if(pressesFurtherDown != null) {
							// let's assume that this is the best option
							break;
						}
					}
					if(pressesFurtherDown != null) {
						break;
					}
					if(skippedAll) {
						break;
					}
				}
				if(pressesFurtherDown == null) {
					return null;
				}

				buttonPressesCount += pressesFurtherDown.Value;
				break;
			}
		}

		return buttonPressesCount;
	}

	private void Button_Pressed(object? sender, int count)
	{
		var pressedButton = (Button)sender!;

		PressedButtons.Add(pressedButton);

		if(Output.DebuggingPartTwo) {
			Console.WriteLine($"Joltages: {{{string.Join(",", JoltageRequirements!)}}}. Path {GetOutputPath_AsTrace()}: Buttons: {string.Join(" ", PressedButtons.Reverse<Button>().Select(b => $"{b.PressesCount}x{b}"))}");
		}

		if(pressedButton.AffectsAMaxedOutJoltage) {
			for(int i = ButtonWiringSchematics!.Count - 1; i >= 0; --i) {
				var button = ButtonWiringSchematics[i];
				if(button.AffectsAMaxedOutJoltage) {
					// remove this button because it shouldn't be pressed anymore
					ButtonWiringSchematics!.RemoveAt(i);
					button.Joltages!.ForEach(j => j.Buttons!.Remove(button));
				}
			}
		}
	}

	private string GetOutputPath_AsTrace()
	{
		List<Machine> path = GetPathToHere();
		return string.Join("=>", path.Select(m => m.Id));
	}

	private string GetOutputPath_AsProgress()
	{
		List<Machine> path = GetPathToHere();
		return string.Join("=>", path.Select(m => $"(S={m.subtractPressesCount}/{m.maxSubtract}. B={m.output_buttonIndex}/{m.output_buttonCount}.)"));
	}

	private List<Machine> GetPathToHere()
	{
		List<Machine> path = [];
		Machine current = this;
		while(current != null) {
			path.Add(current);
			current = current.Source!;
		}
		path.Reverse();
		return path;
	}
}

public class Button(List<int> affectedPositions)
{
	public event EventHandler<int>? Pressed;

	public List<int> AffectedPositions { get; init; } = affectedPositions;

	public int PressesCount { get; private set; } = 0;
	public Button? Source { get; private init; }

	/// <summary>
	/// Joltages that this button affects.
	/// </summary>
	public List<Joltage>? Joltages { get; set; }

	public Button(Button source) : this(source.AffectedPositions)
	{
		Source = source;
	}

	public override string ToString() => $"({string.Join(",",AffectedPositions)})";

	public int AvailablePressesCount => Joltages!.Min(j => j.AvailableIncreaseCount);

	public void Press(int count = 1)
	{
		Joltages!.ForEach(j => j.Current+=count);
		PressesCount += count;
		Pressed?.Invoke(this, count);
	}

	public bool AffectsAMaxedOutJoltage => Joltages!.Any(j => j.CanBeIncreased == false);
}

public class Joltage(int targetJoltage)
{
	public int Target { get; init; } = targetJoltage;

	public int Current { get; set; } = 0;

	/// <summary>
	/// Buttons that affect this joltage.
	/// </summary>
	public List<Button>? Buttons { get; set; }

	public Joltage(Joltage source) : this(source.Target)
	{
		Current = source.Current;
	}

	public override string ToString() => $"{AvailableIncreaseCount}";

	public bool CanBeIncreased => Current < Target;
	public int AvailableIncreaseCount => Target - Current;
}
