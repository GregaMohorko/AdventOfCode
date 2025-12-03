//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/

string[] banks = await File.ReadAllLinesAsync(inputFileName);

var allMaximumJoltagesPartOne = new List<long>();
var allMaximumJoltagesPartTwo = new List<long>();
foreach(string bank in banks) {
	long maxJoltagePartOne = GetMaxJoltage(bank, 2);
	allMaximumJoltagesPartOne.Add(maxJoltagePartOne);
	long maxJoltagePartTwo = GetMaxJoltage(bank, 12);
	allMaximumJoltagesPartTwo.Add(maxJoltagePartTwo);
}

// The total output joltage is the sum of the maximum joltage from each bank
long sumOfAllJoltagesPartOne = allMaximumJoltagesPartOne.Sum();
long sumOfAllJoltagesPartTwo = allMaximumJoltagesPartTwo.Sum();

// 17383
Console.WriteLine($"Part One: Sum of all joltages: {sumOfAllJoltagesPartOne}");
// 172601598658203
Console.WriteLine($"Part Two: Sum of all joltages: {sumOfAllJoltagesPartTwo}");


static long GetMaxJoltage(string bank, int maxBatteriesCount, bool output = true)
{
	if(maxBatteriesCount == 1) {
		return bank
			.Select(c => int.Parse(c.ToString()))
			.Max();
	}
	string selectableBankPart = bank[..(bank.Length - maxBatteriesCount + 1)];
	(int Digit, int Position) largestDigitInSelectableBankPart = selectableBankPart
		.Select((c, position) => (int.Parse(c.ToString()), position))
		.MaxBy(x => x.Item1);
	string innerBank = bank[(largestDigitInSelectableBankPart.Position + 1)..];
	long innerMax = GetMaxJoltage(innerBank, maxBatteriesCount - 1, false);
	return long.Parse(largestDigitInSelectableBankPart.Digit.ToString() + innerMax.ToString());
}
