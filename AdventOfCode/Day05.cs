namespace AdventOfCode;

public sealed class Day05 : BaseDay
{
    private readonly List<(long Start, long End)> _ranges;
    private readonly string[] _ingredientIds;

    public Day05()
    {
        var lines = File.ReadAllLines(InputFilePath);
        _ranges = [];
        _ingredientIds = [];

        var i = 0;
        // Parse ranges
        while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
        {
            var parts = lines[i].Split('-');
            _ranges.Add((long.Parse(parts[0]), long.Parse(parts[1])));
            i++;
        }

        i++;
        
        _ingredientIds = lines.Skip(i).ToArray();
    }

    public override ValueTask<string> Solve_1() => new(CountFreshIds().ToString());

    public override ValueTask<string> Solve_2() => new(CountAllFreshIds().ToString());

    private int CountFreshIds()
    {
        return _ingredientIds.Count(IsFresh);
    }

    private bool IsFresh(string idStr)
    {
        var id = long.Parse(idStr);
        foreach (var (start, end) in _ranges)
        {
            if (id >= start && id <= end)
            {
                return true;
            }
        }
        return false;
    }

    private long CountAllFreshIds()
    {
        var mergedRanges = MergeRanges();
        return mergedRanges.Sum(range => range.End - range.Start + 1);
    }
    
    private List<(long Start, long End)> MergeRanges()
    {
        var sortedRanges = _ranges.OrderBy(r => r.Start).ToList();
        var merged = new List<(long Start, long End)> { sortedRanges[0] };
        
        foreach (var (start, end) in sortedRanges.Skip(1))
        {
            var lastMerged = merged[^1];
            
            if (start <= lastMerged.End + 1)
            {
                merged[^1] = (lastMerged.Start, Math.Max(lastMerged.End, end));
            }
            else
            {
                merged.Add((start, end));
            }
        }
        
        return merged;
    }
}