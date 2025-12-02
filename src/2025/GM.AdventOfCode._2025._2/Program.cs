//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/

// parse
List<(long, long)> ranges;
{
	string inputText = await File.ReadAllTextAsync(inputFileName);
	List<string> rangeTexts = inputText
		.Split(',')
		.Select(x => x.Trim())
		.ToList();
	ranges = rangeTexts
		.Select(x => x.Split('-').Select(x => x.Trim()).ToList())
		.Select(x => (long.Parse(x[0]), long.Parse(x[1])))
		.ToList();
}

// invalid IDs:
// sequence of digits repeated twice
// leading zeros

var invalidIdsPartOne = new List<long>();
var invalidIdsPartTwo = new List<long>();
foreach((long start, long end) in ranges) {
	for(long i = start; i <= end; ++i) {
		if(ConsistsOfTwoRepetitiveSequences(i)) {
			invalidIdsPartOne.Add(i);
		}
		if(ConsistsOfManyRepetitiveSequences(i)) {
			invalidIdsPartTwo.Add(i);
		}
	}
}

long sumOfInvalidIdsPartOne = invalidIdsPartOne.Sum();
long sumOfInvalidIdsPartTwo = invalidIdsPartTwo.Sum();

// 55916882972
Console.WriteLine($"Sum of invalid IDs: Part One: {sumOfInvalidIdsPartOne}.");
// 76169125915
Console.WriteLine($"Sum of invalid IDs: Part Two: {sumOfInvalidIdsPartTwo}.");

static bool ConsistsOfManyRepetitiveSequences(long number)
{
	string numberText = number.ToString();
	for(int i = numberText.Length / 2; i >= 1; --i) {
		if(numberText.Length % i != 0) {
			// doesn't add up
			continue;
		}
		string firstPart = numberText[..i];
		bool allPartsSame = true;
		for(int j = i; j < numberText.Length; j += i) {
			string otherPart = numberText.Substring(j, i);
			if(firstPart != otherPart) {
				allPartsSame = false;
			}
		}
		if(allPartsSame) {
			return true;
		}
	}
	return false;
}

static bool ConsistsOfTwoRepetitiveSequences(long number)
{
	string numberText = number.ToString();
	if(numberText.Length % 2 == 1) {
		// odd length
		return false;
	}
	string firstPart = numberText[..(numberText.Length / 2)];
	string secondPart = numberText[(numberText.Length / 2)..];

	return firstPart == secondPart;
}
