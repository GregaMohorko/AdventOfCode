//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/


string[] inputLines = await File.ReadAllLinesAsync(inputFileName);
if(inputLines.Any(x => x.Contains("^^"))) {
	throw new Exception("Unhandled edge case ^^.");
}
if(inputLines.Any(x => x[0] == '^')) {
	throw new Exception("Unhandled edge case ^ at start of line.");
}
if(inputLines.Any(x => x.Last() == '^')) {
	throw new Exception("Unhandled edge case ^ at end of line.");
}


int start = inputLines[0].IndexOf('S');

const bool outputPartOne = false;

// part one
int splitCountPartOne = 0;
List<int> beamsPartOne = [];
int yPartOne = 0;
{
	OutputPartOne();
	beamsPartOne.Add(start);
	yPartOne = 1;
	OutputPartOne();
	while(++yPartOne < inputLines.Length - 1) {
		for(int i = beamsPartOne.Count - 1; i >= 0; --i) {
			int x = beamsPartOne[i];
			// if a tachyon beam encounters a splitter (^), the beam is stopped
			if(inputLines[yPartOne][x] == '^') {
				// instead, a new tachyon beam continues from the immediate left and from the immediate right of the splitter.
				// split
				++splitCountPartOne;
				// right
				if(i == beamsPartOne.Count - 1
					// check that there is not already a beam there
					|| beamsPartOne[i + 1] != x + 1) {
					beamsPartOne.Insert(i + 1, x + 1);
				}
				// left
				if(i == 0
					// check that there is not already a beam there
					|| beamsPartOne[i - 1] != x - 1) {
					beamsPartOne[i] = x - 1;
				} else {
					beamsPartOne.RemoveAt(i);
				}
			}
		}
		OutputPartOne();
	}
	OutputPartOne();
}

// part two
long possiblePathsPartTwo = 0;
{
	var beamsPartTwo = new List<(int X, long Count)>
	{
		(start, 1)
	};

	int yPartTwo = 1;
	while(++yPartTwo < inputLines.Length - 1) {
		for(int i = beamsPartTwo.Count - 1; i >= 0; --i) {
			int x = beamsPartTwo[i].X;
			// if a tachyon beam encounters a splitter (^), the beam is stopped
			if(inputLines[yPartTwo][x] == '^') {
				// split
				// right
				// check that there is not already a beam there
				if(i < beamsPartTwo.Count - 1
					&& beamsPartTwo[i + 1].X == x + 1) {
					// add 1
					beamsPartTwo[i + 1] = (x + 1, beamsPartTwo[i+1].Count + beamsPartTwo[i].Count);
				} else {
					// start new
					beamsPartTwo.Insert(i + 1, (x + 1, beamsPartTwo[i].Count));
				}
				// left
				if(i > 0
					// check that there is not already a beam there
					&& beamsPartTwo[i - 1].X == x - 1) {
					beamsPartTwo[i - 1] = (x - 1, beamsPartTwo[i-1].Count + beamsPartTwo[i].Count);
					beamsPartTwo.RemoveAt(i);
				} else {
					// continue
					beamsPartTwo[i] = (x - 1, beamsPartTwo[i].Count);
				}
			}
		}
	}
	possiblePathsPartTwo = beamsPartTwo.Select(x => x.Count).Sum();
}

// 1635
Console.WriteLine($"Part One: {splitCountPartOne}");
// 58097428661390
Console.WriteLine($"Part Two: {possiblePathsPartTwo}");


void OutputPartOne()
{
	if(outputPartOne) {
		for(int i = 0; i < inputLines[yPartOne].Length; ++i) {
			if(beamsPartOne.Contains(i)) {
				Console.Write('|');
			} else {
				Console.Write(inputLines[yPartOne][i]);
			}
		}
		Console.WriteLine();
	}
}
