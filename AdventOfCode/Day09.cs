using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AdventOfCode;

public sealed class Day09 : BaseDay
{
    private readonly List<(int X, int Y)> _redTiles = [];

    public Day09()
    {
        var input = File.ReadAllLines(InputFilePath);
        foreach (var line in input)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var parts = line.Split(',');
            _redTiles.Add((int.Parse(parts[0]), int.Parse(parts[1])));
        }
    }

    public override ValueTask<string> Solve_1() => new(FindLargestRectangleArea().ToString());

    public override ValueTask<string> Solve_2() => new(FindLargestRectangleAreaWithConstraint().ToString());
    
    private long FindLargestRectangleArea()
    {
        long maxArea = 0;
        
        for (var i = 0; i < _redTiles.Count; i++)
        {
            for (var j = i + 1; j < _redTiles.Count; j++)
            {
                var tile1 = _redTiles[i];
                var tile2 = _redTiles[j];
                
                long width = Math.Abs(tile2.X - tile1.X) + 1;
                long height = Math.Abs(tile2.Y - tile1.Y) + 1;
                var area = width * height;
                
                maxArea = Math.Max(maxArea, area);
            }
        }
        
        return maxArea;
    }

    private long FindLargestRectangleAreaWithConstraint()
    {
        var horizRanges = BuildHorizontalRangesFromPolygonEdges(out var minY, out var maxY);
        var tilesToCheck = FilterTilesToValidYLevels(horizRanges);

        return FindLargestValidRectangleInParallel(tilesToCheck, horizRanges, minY, maxY);
    }

    private Dictionary<int, (int Start, int End)[]> BuildHorizontalRangesFromPolygonEdges(out int minY, out int maxY)
    {
        var horizRanges = new Dictionary<int, List<(int Start, int End)>>();

        minY = int.MaxValue;
        maxY = int.MinValue;

        for (var i = 0; i < _redTiles.Count; i++)
        {
            var (currentX, currentY) = _redTiles[i];
            var nextVertex = _redTiles[(i + 1) % _redTiles.Count];

            if (currentX == nextVertex.X)
            {
                var yMin = Math.Min(currentY, nextVertex.Y);
                var yMax = Math.Max(currentY, nextVertex.Y);

                for (var y = yMin + 1; y < yMax; y++)
                {
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;

                    ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(horizRanges, y, out _);
                    list ??= [];
                    list.Add((currentX, currentX));
                }
            }
            else
            {
                var xMin = Math.Min(currentX, nextVertex.X);
                var xMax = Math.Max(currentX, nextVertex.X);

                if (currentY < minY) minY = currentY;
                if (currentY > maxY) maxY = currentY;

                ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(horizRanges, currentY, out _);
                list ??= [];
                list.Add((xMin, xMax));
            }
        }

        var result = new Dictionary<int, (int Start, int End)[]>(horizRanges.Count);

        foreach (var (y, ranges) in horizRanges)
        {
            ranges.Sort((r1, r2) => r1.Start.CompareTo(r2.Start));

            var merged = new List<(int Start, int End)>(ranges.Count);
            var current = ranges[0];

            for (var i = 1; i < ranges.Count; i++)
            {
                var next = ranges[i];
                if (next.Start <= current.End + 1)
                {
                    current = (current.Start, Math.Max(current.End, next.End));
                }
                else
                {
                    merged.Add(current);
                    current = next;
                }
            }

            merged.Add(current);
            result[y] = merged.ToArray();
        }
        
        return result;
    }

    private (int X, int Y)[] FilterTilesToValidYLevels(Dictionary<int, (int Start, int End)[]> horizRanges)
    {
        var result = new List<(int X, int Y)>(_redTiles.Count);
        result.AddRange(_redTiles.Where(t => horizRanges.ContainsKey(t.Y)));

        result.Sort((a, b) =>
        {
            var yCmp = a.Y.CompareTo(b.Y);
            return yCmp != 0 ? yCmp : a.X.CompareTo(b.X);
        });

        return result.ToArray();
    }

    private static long FindLargestValidRectangleInParallel((int X, int Y)[] tilesToCheck, 
        Dictionary<int, (int Start, int End)[]> horizRanges, int minY, int maxY)
    {
        var partitioner = Partitioner.Create(0, tilesToCheck.Length);
        long globalMax = 0;

        Parallel.ForEach(partitioner, range =>
        {
            var localMaxArea = FindLargestRectangleInRange(tilesToCheck, range.Item1, range.Item2,
                horizRanges, minY, maxY);

            if (localMaxArea <= 0)
                return;

            long current;
            do
            {
                current = Volatile.Read(ref globalMax);
                if (localMaxArea <= current)
                    break;
            }
            while (Interlocked.CompareExchange(ref globalMax, localMaxArea, current) != current);
        });

        return globalMax;
    }

    private static long FindLargestRectangleInRange((int X, int Y)[] tiles, int startIndex, int endIndex,
        Dictionary<int, (int Start, int End)[]> horizRanges, int minY, int maxY)
    {
        long localMaxArea = 0;
        var tilesLength = tiles.Length;

        for (var i = startIndex; i < endIndex; i++)
        {
            var (c1X, c1Y) = tiles[i];

            for (var j = i + 1; j < tilesLength; j++)
            {
                var (c2X, c2Y) = tiles[j];

                var rectMinX = c1X < c2X ? c1X : c2X;
                var rectMaxX = c1X > c2X ? c1X : c2X;
                var rectMinY = c1Y < c2Y ? c1Y : c2Y;
                var rectMaxY = c1Y > c2Y ? c1Y : c2Y;

                if (rectMinY < minY || rectMaxY > maxY)
                    continue;

                var area = (rectMaxX - rectMinX + 1L) * (rectMaxY - rectMinY + 1L);
                if (area <= localMaxArea)
                    continue;

                if (IsRectangleInsidePolygon(horizRanges, rectMinX, rectMinY, rectMaxX, rectMaxY))
                    localMaxArea = area;
            }
        }

        return localMaxArea;
    }

    private static bool IsRectangleInsidePolygon(Dictionary<int, (int Start, int End)[]> horizRanges,
        int minX, int minY, int maxX, int maxY)
    {
        for (var y = minY; y <= maxY; y++)
        {
            if (!IsHorizontalLineInsidePolygon(horizRanges, y, minX, maxX))
                return false;
        }
        return true;
    }

    private static bool IsHorizontalLineInsidePolygon(Dictionary<int, (int Start, int End)[]> horizRanges,
        int y, int minX, int maxX)
    {
        if (!horizRanges.TryGetValue(y, out var ranges))
            return false;
        
        if (minX < ranges[0].Start)
            return false;
        
        var rangeIndex = FindRangeContainingPoint(ranges, minX);
        if (rangeIndex == -1)
            return false;
        
        var rangeEnd = ranges[rangeIndex].End;
        
        if (maxX <= rangeEnd)
            return true;
        
        if (rangeIndex % 2 == 1)
            return false;
        
        return rangeIndex + 1 < ranges.Length && ranges[rangeIndex + 1].End >= maxX;
    }

    private static int FindRangeContainingPoint((int Start, int End)[] ranges, int x)
    {
        var left = 0;
        var right = ranges.Length - 1;
        var result = -1;
        
        while (left <= right)
        {
            var mid = (left + right) >> 1;
            if (ranges[mid].Start <= x)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        
        return result;
    }
}