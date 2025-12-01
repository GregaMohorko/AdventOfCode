//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/

const int START = 50;
const int MIN = 0;
const int MAX = 99;
const int LENGTH = MAX - MIN + 1;

string[] inputLines = await File.ReadAllLinesAsync(inputFileName);

// password: the number of times the dial is left pointing at 0 after any rotation in the sequence.
int stoppedAtMinPosTotalCount = 0;
int wasAtMinPosTotalCount = 0;
int position = START;
for(int i = 0; i < inputLines.Length; ++i) {
	string line = inputLines[i];
	if(string.IsNullOrWhiteSpace(line)) {
		Console.WriteLine($"Line {(i + 1)}/{inputLines.Length} is empty.");
		continue;
	}

	char directionChar = line[0];
	int distance = int.Parse(line[1..]);

	// move
	int startPosition = position;
	if(directionChar == 'L') {
		// left
		position -= distance;
	} else if(directionChar == 'R') {
		// right
		position += distance;
	} else {
		throw new InvalidDataException($"Direction '{directionChar}' is not recognized.");
	}

	string movementDescription = $"Start at {startPosition}. Move {line}";

	// imitate circle rotation
	int rotationCount = 0;
	while(position < MIN) {
		position += LENGTH;
		++rotationCount;
		movementDescription += " AROUND";
	}
	while(position > MAX) {
		position -= LENGTH;
		++rotationCount;
		movementDescription += " AROUND";
	}

	movementDescription += $" to {position}.";

	int wasAtMinPosInThisMovement = rotationCount;
	if(rotationCount > 0) {
		if(startPosition == MIN
			&& directionChar == 'L'
			) {
			// don't count the start rotation to the left if it started at MIN
			--wasAtMinPosInThisMovement;
		}
	}

	if(position == MIN) {
		++stoppedAtMinPosTotalCount;
		if(directionChar == 'L') {
			// only count the left direction, because the right direction was already counted when rotating
			++wasAtMinPosInThisMovement;
		}
	}

	wasAtMinPosTotalCount += wasAtMinPosInThisMovement;

	if(wasAtMinPosInThisMovement > 0) {
		movementDescription += $" Was at MIN {wasAtMinPosInThisMovement} times. Total count now {wasAtMinPosTotalCount}.";
	}

	Console.WriteLine($"Line {(i + 1)}/{inputLines.Length}: {movementDescription}");
}

// 1135
Console.WriteLine($"Part One: Password: {stoppedAtMinPosTotalCount}");
// 6558
Console.WriteLine($"Part Two: Password: {wasAtMinPosTotalCount}");
