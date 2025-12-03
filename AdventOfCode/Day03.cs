namespace AdventOfCode;

public sealed class Day03 : BaseDay
{
    private readonly int[][] _input;

    public Day03()
    {
        _input = File.ReadAllLines(InputFilePath)
            .Select(line => line.Select(c => int.Parse(c.ToString())).ToArray())
            .ToArray();
    }

    public override ValueTask<string> Solve_1() => new(SumVoltage(2).ToString());

    public override ValueTask<string> Solve_2() => new(SumVoltage(12).ToString());

    private long SumVoltage(int totalDigits)
    {
        long total = 0;

        foreach (var bank in _input)
        {
            long joltage = 0;
            var nextStartIndex = 0;
            
            for (var pos = 0; pos < totalDigits; pos++)
            {
                var maxDigit = -1;

                var searchEnd = bank.Length - (totalDigits - pos - 1);

                for (var i = nextStartIndex; i < searchEnd; i++)
                {
                    if (bank[i] <= maxDigit) continue;
                    maxDigit = bank[i];
                    nextStartIndex = i + 1;
                }

                joltage = joltage * 10 + maxDigit;
            }

            total += joltage;
        }

        return total;
    }
}