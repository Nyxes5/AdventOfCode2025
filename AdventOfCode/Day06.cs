namespace AdventOfCode;

public sealed class Day06 : BaseDay
{
    private readonly string[] _input;
    private readonly string[] _dataLines;

    public Day06()
    {
        _input = File.ReadAllLines(InputFilePath);
        _dataLines = _input[..^1];
    }

    public override ValueTask<string> Solve_1() => new(Solve(ProcessBlockLeftToRight).ToString());

    public override ValueTask<string> Solve_2() => new(Solve(ProcessBlockRightToLeft).ToString());
    
    private long Solve(Func<int, int, long> processBlock)
    {
        var maxLength = _input.Max(l => l.Length);
        long grandTotal = 0;
        int? blockStart = null;
        
        // Scan columns to find blocks
        for (var col = 0; col < maxLength; col++)
        {
            var hasOperator = _input[^1][col] is '*' or '+';
            var hasData = _dataLines.Any(line => col < line.Length && line[col] != ' ');
            
            if (hasOperator || hasData)
            {
                blockStart ??= col;
            }
            else if (blockStart != null)
            {
                grandTotal += processBlock(blockStart.Value, col - 1);
                blockStart = null;
            }
        }
        
        // Process last block
        if (blockStart != null)
        {
            grandTotal += processBlock(blockStart.Value, maxLength - 1);
        }
        
        return grandTotal;
    }

    private long ProcessBlockLeftToRight(int start, int end)
    {   
        var op = _input[^1][start];
        var neutralElement = op == '*' ? 1L : 0L;
        
        var result = _dataLines
            .Select(line => line.Substring(start, end - start + 1).Trim())
            .Select(long.Parse)
            .Aggregate(neutralElement, (current, num) => _input[^1][start] == '*' ? current * num : current + num);

        return result;
    }

    private long ProcessBlockRightToLeft(int start, int end)
    {
        var op = _input[^1][start];
        var neutralElement = op == '*' ? 1L : 0L;
        
        var result = neutralElement;
        for (var col = end; col >= start; col--)
        {
            var digitChars = _dataLines
                .Where(line => col < line.Length && line[col] != ' ')
                .Select(line => line[col])
                .ToArray();

            var num = long.Parse(digitChars);

            result = op == '*' ? result * num : result + num;
        }
        
        return result;
    }
}