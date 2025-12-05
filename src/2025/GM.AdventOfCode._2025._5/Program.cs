//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/

string[] inputLines = await File.ReadAllLinesAsync(inputFileName);
// It consists of a list of fresh ingredient ID ranges, a blank line, and a list of available ingredient IDs.
string[] freshIdRangeTexts = inputLines.TakeWhile(x => string.IsNullOrWhiteSpace(x) == false).ToArray();
string[] availableIngredientIdTexts = inputLines.Skip(freshIdRangeTexts.Length + 1).ToArray();

List<(long, long)> freshIdRanges = freshIdRangeTexts
	.Select(x => (long.Parse(x.Split('-')[0]), long.Parse(x.Split('-')[1])))
	.OrderBy(x => x.Item1)
	.ToList();
long[] availableIngredientIds = availableIngredientIdTexts
	.Select(x => long.Parse(x))
	.ToArray();

// The fresh ID ranges are inclusive: the range 3-5 means that ingredient IDs 3, 4, and 5 are all fresh. The ranges can also overlap; an ingredient ID is fresh if it is in any range.

int freshCountPartOne = 0;
foreach(long availableIngredientId in availableIngredientIds) {
	if(IsFresh(availableIngredientId, freshIdRanges)) {
		++freshCountPartOne;
	}
}

long allFreshCount = 0;
long position = -1;
// ranges are already ordered by range start
foreach(var freshRange in freshIdRanges) {
	long startCountAt = Math.Max(position + 1, freshRange.Item1);
	if(freshRange.Item2 > position) {
		allFreshCount += freshRange.Item2 - startCountAt + 1;
		position = freshRange.Item2;
	}
}

// 726
Console.WriteLine($"Part One: {freshCountPartOne}");
// 354226555270043
Console.WriteLine($"Part Two: {allFreshCount}");

static bool IsFresh(long ingredientId, List<(long, long)> freshRanges)
{
	foreach(var freshRange in freshRanges) {
		if(ingredientId >= freshRange.Item1
			&& ingredientId <= freshRange.Item2
			) {
			return true;
		}
	}
	return false;
}
