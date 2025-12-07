namespace AdventOfCode;

public sealed class Day07 : BaseDay
{
    private readonly string[] _input;

    public Day07()
    {
        _input = File.ReadAllLines(InputFilePath);
    }

    public override ValueTask<string> Solve_1() => new(CalculateResults().ToString());

    public override ValueTask<string> Solve_2() => new(CalculateResults(true).ToString());
    
    private long CalculateResults(bool countTimelines = false)
    {
        var splitCount = 0;
        
        var beams = new Dictionary<int, long> { [_input[0].IndexOf('S')] = 1 };
        
        for (var row = 1; row < _input.Length; row++)
        {
            var currentRow = _input[row].AsSpan();
            var newBeams = new Dictionary<int, long>();
            
            foreach (var (col, count) in beams)
            {
                var c = currentRow[col];

                switch (c)
                {
                    case '.':
                        newBeams[col] = newBeams.GetValueOrDefault(col) + count;
                        break;
                    case '^':
                        splitCount++;
                        newBeams[col - 1] = newBeams.GetValueOrDefault(col - 1) + count;
                        newBeams[col + 1] = newBeams.GetValueOrDefault(col + 1) + count;
                        break;
                }
            }
            
            beams = newBeams;
        }

        return countTimelines ? beams.Values.Sum() : splitCount;
    }
}