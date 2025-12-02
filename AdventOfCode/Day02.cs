namespace AdventOfCode;

public sealed class Day02 : BaseDay
{
    private readonly string[] _input;

    public Day02()
    {
        _input = File.ReadAllText(InputFilePath).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public override ValueTask<string> Solve_1() => new(SumInvalidIds().ToString());

    public override ValueTask<string> Solve_2() => new(SumInvalidIds(true).ToString());

    private long SumInvalidIds(bool anySubString = false)
    {
        var sum = 0L;
        
        foreach (var range in _input)
        {
            var parts = range.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var min = long.Parse(parts[0]);
            var max = long.Parse(parts[1]);

            for (var id = min; id <= max; id++)
            {
                if (IsInvalidId(id, anySubString))
                {
                    sum += id;
                }
            }
        }
        return sum;
    }

    private static bool IsInvalidId(long id, bool anySubString)
    {
        var idSpan = id.ToString().AsSpan();
        var length = idSpan.Length;
        
        if (!anySubString)
        {
            // Part 1: Check two halves if they are identical
            if (length % 2 != 0)
                return false;

            var halfLength = length / 2;
            return idSpan[..halfLength].SequenceEqual(idSpan[halfLength..]);
        }
        
        // Part 2: Check if the number repeats any substring pattern
        for (var patternLength = 1; patternLength <= length / 2; patternLength++)
        {
            if (length % patternLength != 0)
                continue;
            
            var isValid = true;
            
            for (var i = patternLength; i < length; i += patternLength)
            {
                if (idSpan.Slice(i, patternLength).SequenceEqual(idSpan[..patternLength])) continue;
                isValid = false;
                break;
            }
            
            if (isValid)
                return true;
        }
        
        return false;
    }
}