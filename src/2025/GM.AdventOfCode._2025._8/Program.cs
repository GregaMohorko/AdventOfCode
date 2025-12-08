//*
const string inputFileName = "input.txt";
const int partOneMaxConnections = 1000;
/*/
const string inputFileName = "inputExample.txt";
const int partOneMaxConnections = 10;
//*/


string[] inputLines = await File.ReadAllLinesAsync(inputFileName);

List<(int, int, int)> junctionBoxes = inputLines
	.Select(x => x.Split(','))
	.Select(x => x.Select(y => int.Parse(y)).ToList())
	.Select(x => (x[0], x[1], x[2]))
	.ToList();



// part one
int partOne = -1;
long partTwo = -1;
{
	bool outputDebugging = false;
	bool outputProgress = true;
	// (circuitId, pos)
	List<(int, (int, int, int))> junctionBoxesAndCircuits = junctionBoxes
		.Select((x, i) => (i, x))
		.ToList();
	var circuitCounts = junctionBoxesAndCircuits
		.ToDictionary(keySelector: x => x.Item1, elementSelector: x => 1);
	int connectionsMade = 0;
	Dictionary<int, List<int>> usedPairs = [];
	while(true) {
		// find closest two (who are not already in the same circuit)
		int closest_i = -1;
		int closest_j = -1;
		double closestDistance = double.MaxValue;
		for(int i = junctionBoxesAndCircuits.Count - 1; i >= 0; --i) {
			int circuit1 = junctionBoxesAndCircuits[i].Item1;
			var p1 = junctionBoxesAndCircuits[i].Item2;
			if(usedPairs.TryGetValue(i, out var usedConnectionsWith_i) == false) {
				usedConnectionsWith_i = [];
				usedPairs.Add(i, usedConnectionsWith_i);
			}
			for(int j = junctionBoxesAndCircuits.Count - 1; j >= 0; --j) {
				if(j == i) {
					continue;
				}
				int circuit2 = junctionBoxesAndCircuits[j].Item1;
				if(partOne == -1) {
					if(usedConnectionsWith_i.Contains(j)) {
						// already used connection
						continue;
					}
				} else {
					// let's speed things up for part two and ignore connections which are already in the same circuit
					if(circuit1 == circuit2) {
						continue;
					}
				}
				var p2 = junctionBoxesAndCircuits[j].Item2;
				double distance = GetDistance(p1, p2);
				if(distance < closestDistance) {
					closestDistance = distance;
					closest_i = i;
					closest_j = j;
				}
			}
		}
		usedPairs[closest_i].Add(closest_j);
		usedPairs[closest_j].Add(closest_i);
		// join them into the same circuit
		int circuitToUse = junctionBoxesAndCircuits[closest_i].Item1;
		int circuitToJoin = junctionBoxesAndCircuits[closest_j].Item1;
		if(circuitToUse != circuitToJoin) {
			if(outputDebugging) {
				Console.WriteLine($"Joining ({junctionBoxesAndCircuits[closest_i].Item2.Item1},{junctionBoxesAndCircuits[closest_i].Item2.Item2}, {junctionBoxesAndCircuits[closest_i].Item2.Item3}) and ({junctionBoxesAndCircuits[closest_j].Item2.Item1},{junctionBoxesAndCircuits[closest_j].Item2.Item2},{junctionBoxesAndCircuits[closest_j].Item2.Item3}).");
			}

			for(int i = junctionBoxesAndCircuits.Count - 1; i >= 0; --i) {
				int circuit = junctionBoxesAndCircuits[i].Item1;
				if(circuit != circuitToUse && circuit == circuitToJoin) {
					junctionBoxesAndCircuits[i] = (circuitToUse, junctionBoxesAndCircuits[i].Item2);
				}
			}
			circuitCounts[circuitToUse] += circuitCounts[circuitToJoin];
			circuitCounts.Remove(circuitToJoin);
		} else {
			if(outputDebugging) {
				Console.WriteLine($"({junctionBoxesAndCircuits[closest_i].Item2.Item1},{junctionBoxesAndCircuits[closest_i].Item2.Item2}, {junctionBoxesAndCircuits[closest_i].Item2.Item3}) and ({junctionBoxesAndCircuits[closest_j].Item2.Item1},{junctionBoxesAndCircuits[closest_j].Item2.Item2},{junctionBoxesAndCircuits[closest_j].Item2.Item3}) are already in the same circuit.");
			}
		}
		++connectionsMade;

		if(outputProgress) {
			Console.WriteLine($"Connections made: {connectionsMade}. Circuit count: {circuitCounts.Count}.");
		}

		if(connectionsMade == partOneMaxConnections) {
			// multiply three largest circuits
			var threeLargest = circuitCounts.Values
				.OrderByDescending(x => x)
				.Take(3)
				.ToList();
			partOne = threeLargest.Aggregate(1, (total, x) => total * x);
		}

		if(circuitCounts.Count == 1) {
			long x1 = junctionBoxesAndCircuits[closest_i].Item2.Item1;
			long x2 = junctionBoxesAndCircuits[closest_j].Item2.Item1;
			partTwo =  x1 * x2;
			break;
		}
	}
}

// 97384
Console.WriteLine($"Part One: {partOne}");
// 9003685096
Console.WriteLine($"Part Two: {partTwo}");

static double GetDistance((int, int, int) p1, (int, int, int) p2)
{
	return Math.Sqrt(Math.Pow(p1.Item1 - p2.Item1,2) + Math.Pow(p1.Item2-p2.Item2, 2) + Math.Pow(p1.Item3-p2.Item3, 2));
}
