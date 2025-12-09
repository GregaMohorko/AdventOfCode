//*
const string inputFileName = "input.txt";
/*/
const string inputFileName = "inputExample.txt";
//*/


string[] inputLines = await File.ReadAllLinesAsync(inputFileName);


List<(long, long)> redTiles = inputLines
	.Select(x => x.Split(','))
	.Select(x => x.Select(y => long.Parse(y)).ToList())
	.Select(x => (x[0], x[1]))
	.ToList();

const bool outputDebuggingPartOne = false;

// part one
long partOne;
{
	long largestArea = -1;
	for(int i=0; i<redTiles.Count; ++i) {
		var tile1 = redTiles[i];
		for(int j = 0; j < redTiles.Count; ++j) {
			if(i == j) {
				continue;
			}
			var tile2 = redTiles[j];
			long area = GetArea(tile1, tile2);
			if(area > largestArea) {
				largestArea = area;
				if(outputDebuggingPartOne) {
#pragma warning disable CS0162 // Unreachable code detected
					Console.WriteLine($"Part One: New largest area: {largestArea} from tiles {tile1} and {tile2}");
#pragma warning restore CS0162 // Unreachable code detected
				}
			}
		}
	}
	partOne = largestArea;
}

const bool outputDebuggingPartTwo = true;
const bool outputResultPartTwo = true;

// part two
long partTwo;
{
	// check for any edge cases where the area is concave
	List<(long, long)> theWrap;
	//List<(long, long)> greenOrRedTiles;
	{
		// the wrap
		if(outputDebuggingPartTwo) {
			Console.WriteLine("Beginning to create the wrap ...");
		}
		theWrap = [];
		theWrap.Add(redTiles[0]);
		(long, long) lastRedTile = redTiles[0];
		for(int i = 1; i < redTiles.Count; ++i) {
			var redTile = redTiles[i];
			if(redTile.Item1 == lastRedTile.Item1) {
				// same x
				if(redTile.Item2 > lastRedTile.Item2) {
					// going down
					for(long y=lastRedTile.Item2 + 1;y <= redTile.Item2; ++y) {
						theWrap.Add((redTile.Item1, y));
					}
				} else if(redTile.Item2 < lastRedTile.Item2) {
					// going up
					for(long y = lastRedTile.Item2 - 1; y >= redTile.Item2; --y) {
						theWrap.Add((redTile.Item1, y));
					}
				} else {
					throw new Exception("Same y?");
				}
			} else if(redTile.Item2 == lastRedTile.Item2) {
				// same y
				if(redTile.Item1 > lastRedTile.Item1) {
					// going right
					for(long x = lastRedTile.Item1 + 1; x <= redTile.Item1; ++x) {
						theWrap.Add((x, redTile.Item2));
					}
				} else if(redTile.Item1 < lastRedTile.Item1) {
					// going left
					for(long x = lastRedTile.Item1 - 1; x >= redTile.Item1; --x) {
						theWrap.Add((x, redTile.Item2));
					}
				} else {
					throw new Exception("Same x?");
				}
			} else {
				throw new Exception("Where to go?");
			}

			lastRedTile = redTile;
		}
		if(outputDebuggingPartTwo) {
			Console.WriteLine("Wrap created.");
		}
	}

	long wrapMinX = theWrap.Min(x => x.Item1);
	long wrapMaxX = theWrap.Max(x => x.Item1);
	long wrapMinY = theWrap.Min(x => x.Item2);
	long wrapMaxY = theWrap.Max(x => x.Item2);
	Dictionary<long, Dictionary<long, bool>> theWrapDictionary = theWrap
		.GroupBy(point => point.Item1)
		.ToDictionary(g => g.Key, g => g.ToDictionary(gg => gg.Item2, gg => true));

	long largestArea = -1;
	Dictionary<long, Dictionary<long, Dictionary<long, Dictionary<long, bool>>>> alreadyCheckedAreas = [];
	for(int i = 1; i < redTiles.Count; ++i) {
		var tile1 = redTiles[i];
		for(int j = 0; j < i; ++j) {
			var tile2 = redTiles[j];
			long area = GetArea(tile1, tile2);
			if(area > largestArea) {
				// check if it's inside of green tiles
				bool isInsideOfGreenTiles;
				{
					long xLeft = Math.Min(tile1.Item1, tile2.Item1);
					long xRight = Math.Max(tile1.Item1, tile2.Item1);
					long yTop = Math.Min(tile1.Item2, tile2.Item2);
					long yBottom = Math.Max(tile1.Item2, tile2.Item2);

					isInsideOfGreenTiles = true;

					if(alreadyCheckedAreas.TryGetValue(xLeft, out var xLeftDict) == false) {
						xLeftDict = [];
						alreadyCheckedAreas.Add(xLeft, xLeftDict);
					}
					if(xLeftDict.TryGetValue(xRight, out var xRightDict) == false) {
						xRightDict = [];
						xLeftDict.Add(xRight, xRightDict);
					}
					if(xRightDict.TryGetValue(yTop, out var yTopDict) == false) {
						yTopDict = [];
						xRightDict.Add(yTop, yTopDict);
					}
					if(yTopDict.ContainsKey(yBottom)) {
						// already checked this area
						isInsideOfGreenTiles = false;
					} else {
						yTopDict.Add(yBottom, true);
					}

					if(outputDebuggingPartTwo) {
						Console.WriteLine($"i={i + 1}/{redTiles.Count}. j={j + 1}/{i}. Checking if area x=[{xLeft},{xRight}], y=[{yTop},{yBottom}] is inside of the wrap ...");
					}

					if(isInsideOfGreenTiles) {
						List<List<(long, long)>> allBorders = [];
						// top/bottom border
						foreach(long y in new long[] { yTop, yBottom }) {
							List<(long, long)> horizontalBorder = [];
							for(long x = xLeft; x <= xRight; ++x) {
								horizontalBorder.Add((x, y));
							}
							allBorders.Add(horizontalBorder);
						}
						// left/right border
						foreach(long x in new long[] { xLeft, xRight }) {
							List<(long, long)> verticalBorder = [];
							for(long y = yTop + 1; y < yBottom; ++y) {
								verticalBorder.Add((x, y));
							}
							allBorders.Add(verticalBorder);
						}

						// go through all the points
						foreach(var border in allBorders) {
							bool? wasLastOnWrap = null;
							foreach(var point in border) {
								bool isOnWrap = IsOnWrap(point, theWrapDictionary);
								if(wasLastOnWrap != null
									&& isOnWrap == wasLastOnWrap.Value
									) {
									// don't need to check, should be the same
									continue;
								}
								wasLastOnWrap = isOnWrap;
								if(isOnWrap == false) {
									if(IsInsideTheWrap(
										pointNotOnWrap: point,
										theWrapDictionary: theWrapDictionary,
										useShortestDirection: false,
										wrapMinX: wrapMinX,
										wrapMaxX: wrapMaxX,
										wrapMinY: wrapMinY,
										wrapMaxY: wrapMaxY
										) == false
										) {
										isInsideOfGreenTiles = false;
										break;
									}
								}
							}
							if(isInsideOfGreenTiles == false) {
								break;
							}
						}
					}
				}

				if(isInsideOfGreenTiles) {
					largestArea = area;
					if(outputResultPartTwo) {
						Console.WriteLine($"i={i + 1}/{redTiles.Count}. j={j + 1}/{i}. Part Two: New largest area: {largestArea} from tiles {tile1} and {tile2}");
					}
				} else {
					if(outputDebuggingPartTwo) {
						Console.WriteLine($"i={i+1}/{redTiles.Count}. j={j+1}/{i}. Nope.");
					}
				}
			}
		}
	}
	partTwo = largestArea;
}



// 4776487744
Console.WriteLine($"Part One: {partOne}");
// 1560299548
Console.WriteLine($"Part Two: {partTwo}");

static long GetArea((long, long) tile1, (long, long) tile2)
{
	long dx = Math.Abs(tile1.Item1 - tile2.Item1) + 1;
	long dy = Math.Abs(tile1.Item2 - tile2.Item2) + 1;
	return dx * dy;
}

static bool IsOnWrap(
	(long, long) point,
	Dictionary<long, Dictionary<long, bool>> theWrapDictionary
	)
{
	if(theWrapDictionary.TryGetValue(point.Item1, out var xDictionary)) {
		if(xDictionary.ContainsKey(point.Item2)) {
			return true;
		}
	}
	return false;
}

static bool IsInsideTheWrap(
	(long, long) pointNotOnWrap,
	Dictionary<long, Dictionary<long, bool>> theWrapDictionary,
	bool useShortestDirection,
	long wrapMinX,
	long wrapMaxX,
	long wrapMinY,
	long wrapMaxY
	)
{
	bool isClosestTop, isClosestBottom, isClosestLeft, isClosestRight;
	{
		if(useShortestDirection == false) {
			// always go right
			isClosestRight = true;
			isClosestTop = isClosestBottom = isClosestLeft = false;
		} else {
			long distanceTop = pointNotOnWrap.Item2 - wrapMinY;
			long distanceBottom = wrapMaxY - pointNotOnWrap.Item2;
			long distanceLeft = pointNotOnWrap.Item1 - wrapMinX;
			long distanceRight = wrapMaxX - pointNotOnWrap.Item1;
			long min = Math.Min(distanceTop, Math.Min(distanceBottom, Math.Min(distanceLeft, distanceRight)));
			if(distanceTop == min) {
				isClosestTop = true;
				isClosestBottom = isClosestLeft = isClosestRight = false;
			} else if(distanceBottom == min) {
				isClosestBottom = true;
				isClosestTop = isClosestLeft = isClosestRight = false;
			} else if(distanceLeft == min) {
				isClosestLeft = true;
				isClosestTop = isClosestBottom = isClosestRight = false;
			} else {
				isClosestRight = true;
				isClosestTop = isClosestBottom = isClosestLeft = false;
			}
		}
	}

	List<(long, long)> allPoints = [];
	if(isClosestRight) {
		for(long x = pointNotOnWrap.Item1; x <= wrapMaxX; ++x) {
			allPoints.Add((x, pointNotOnWrap.Item2));
		}
	}else if(isClosestLeft) {
		for(long x = pointNotOnWrap.Item1; x >= wrapMinX; --x) {
			allPoints.Add((x, pointNotOnWrap.Item2));
		}
	}else if(isClosestTop) {
		for(long y = pointNotOnWrap.Item2; y >= wrapMinY; --y) {
			allPoints.Add((pointNotOnWrap.Item1, y));
		}
	} else {
		// closest right
		for(long y = pointNotOnWrap.Item2; y <= wrapMaxY; ++y) {
			allPoints.Add((pointNotOnWrap.Item1, y));
		}
	}

	int wrapIntersectionCount = 0;
	bool wasLastOneOnWrap = false;
	foreach((long x, long y) in allPoints) {
		// check if it's on the wrap
		if(IsOnWrap((x, y), theWrapDictionary)) {
			// on the wrap
			if(wasLastOneOnWrap) {
				// two in a row on the wrap
				// means it's on the wrap border
				// since we know that we didn't start on the wrap border (we checked for that above)
				// it's outside
				return false;
			}
			wasLastOneOnWrap = true;
			++wrapIntersectionCount;
		} else {
			// not on the wrap
			wasLastOneOnWrap = false;
		}
	}
	// odd count means it's inside
	bool countIsOdd = wrapIntersectionCount % 2 == 1;
	if(countIsOdd) {
		return true;
	} else {
		return false;
	}
}
