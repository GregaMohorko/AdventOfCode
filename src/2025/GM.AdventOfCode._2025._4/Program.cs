//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/

string[] allLines = await File.ReadAllLinesAsync(inputFileName);
if(allLines.All(line => line.Length != allLines[0].Length)) {
	throw new Exception("Not all lines have the same length.");
}
bool[][] grid = new bool[allLines.Length][];
for(int y = 0; y < allLines.Length; ++y) {
	grid[y] = new bool[allLines[y].Length];
	for(int x = 0; x < allLines[y].Length; ++x) {
		grid[y][x] = allLines[y][x] == '@';
	}
}

int totalRollsThatCanBeAccessedPartOne = 0;
for(int y = 0; y < grid.Length; ++y) {
	for(int x = 0; x < grid[y].Length; ++x) {
		if(grid[y][x]) {
			int adjacentRolls = GetAdjacentRollsCount(grid, y, x, 1);
			if(adjacentRolls < 4) {
				++totalRollsThatCanBeAccessedPartOne;
			}
		}
	}
}

int totalRollsThatCanBeRemovedPartTwo = 0;
bool[][] removingGrid;
bool anythingRemoved;
do {
	// copy array
	removingGrid = new bool[grid.Length][];
	for(int i = grid.Length - 1; i >= 0; --i) {
		removingGrid[i] = new bool[grid[i].Length];
		Array.Copy(grid[i], removingGrid[i], grid[i].Length);
	}

	anythingRemoved = false;
	for(int y = 0; y < grid.Length; ++y) {
		for(int x = 0; x < grid[y].Length; ++x) {
			if(grid[y][x]) {
				int adjacentRolls = GetAdjacentRollsCount(grid, y, x, 1);
				if(adjacentRolls < 4) {
					++totalRollsThatCanBeRemovedPartTwo;
					removingGrid[y][x] = false;
					anythingRemoved = true;
				}
			}
		}
	}

	grid = removingGrid;
} while(anythingRemoved);

// 1457
Console.WriteLine($"Part One: {totalRollsThatCanBeAccessedPartOne}");
// 8310
Console.WriteLine($"Part Two: {totalRollsThatCanBeRemovedPartTwo}");


static int GetAdjacentRollsCount(bool[][] grid, int y, int x, int maxDistance)
{
	int adjacentRollsCount = 0;

	// top and top diagonals
	{
		int startY = Math.Max(0, y - maxDistance);
		int endY = Math.Min(grid.Length - 1, y + maxDistance);
		int startX = Math.Max(0, x - maxDistance);
		for(int y2 = startY; y2 <= endY; ++y2) {
			int endX = Math.Min(grid[y].Length - 1, x + maxDistance);
			for(int x2 = startX; x2 <= endX; ++x2) {
				if(y2 == y && x2 == x) {
					// skip the origin position
					continue;
				}
				if(grid[y2][x2]) {
					++adjacentRollsCount;
				}
			}
		}
	}

	return adjacentRollsCount;
}
