using GM.Utility;

//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/

string[] inputLines = await File.ReadAllLinesAsync(inputFileName);

// read numbers
List<List<long>> inputLinesNumbers = new();
for(int i = 0; i < inputLines.Length - 1; ++i) {
	var lineNumbers = ReadLine(inputLines[i]);
	inputLinesNumbers.Add(lineNumbers);
}
if(inputLinesNumbers.AllSame(x => x.Count) == false) {
	throw new Exception("Not all input lines numbers are same length.");
}
int problemCount = inputLinesNumbers[0].Count;
// read signs
List<char> inputSigns = GetAllElements(inputLines.Last()).Select(x => x.Single()).ToList();
if(inputSigns.Count != problemCount) {
	throw new Exception("Sign count doesn't match with numbers count.");
}

// PART ONE
// horizontal to vertical
List<List<long>> problemNumbersPartOne = new(problemCount);
for(int i = 0; i < problemCount; ++i) {
	problemNumbersPartOne.Add(new List<long>(inputLinesNumbers.Count));
	for(int j = 0; j < inputLinesNumbers.Count; ++j) {
		problemNumbersPartOne[i].Add(inputLinesNumbers[j][i]);
	}
}
long grandTotalPartOne = 0;
for(int i = 0; i < problemCount; ++i) {
	long answer = Calculate(problemNumbersPartOne[i], inputSigns[i]);
	grandTotalPartOne += answer;
}

// PART TWO
List<List<long>> problemNumbersPartTwo = new(problemNumbersPartOne.Count);
for(int i = 0; i < problemCount; ++i) {
	problemNumbersPartTwo.Add([]);
}
// flip input lines
var numberInputLines = inputLines.Take(inputLines.Length - 1).ToList();
if(numberInputLines.AllSame(x => x.Length) == false) {
	throw new Exception("Not all number input lines are same length.");
}
int problemIndex = problemCount - 1;
for(int x = numberInputLines[0].Length - 1; x >= 0; --x) {
	string newNumberText = string.Empty;
	for(int y = 0; y < numberInputLines.Count; ++y) {
		char character = numberInputLines[y][x];
		if(character != ' ') {
			newNumberText += character;
		}
	}
	if(newNumberText.Length == 0) {
		// empty column
		--problemIndex;
		if(problemIndex < 0) {
			throw new Exception("-1");
		}
	} else {
		long newNumber = long.Parse(newNumberText);
		problemNumbersPartTwo[problemIndex].Add(newNumber);
	}
}
long grandTotalPartTwo = 0;
for(int i = 0; i < problemCount; ++i) {
	long answer = Calculate(problemNumbersPartTwo[i], inputSigns[i]);
	grandTotalPartTwo += answer;
}


// 6169101504608
Console.WriteLine($"Part One: {grandTotalPartOne}");
// 10442199710797
Console.WriteLine($"Part Two: {grandTotalPartTwo}");

static long Calculate(List<long> numbers, char sign)
{
	return sign switch
	{
		'+' => numbers.Aggregate(0L, (result, x) => result + x),
		'*' => numbers.Aggregate(1L, (result, x) => result * x),
		_ => throw new Exception($"Sign '{sign}'."),
	};
}

static List<long> ReadLine(string line)
{
	var lineElements = GetAllElements(line);
	return lineElements.Select(element => long.Parse(element)).ToList();
}

static List<string> GetAllElements(string inputLine)
{
	return inputLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
}
