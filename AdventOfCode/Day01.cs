namespace AdventOfCode;

public sealed class Day01 : BaseDay
{
    private readonly string[] _input;

    public Day01()
    {
        _input = File.ReadAllLines(InputFilePath);
    }

    public override ValueTask<string> Solve_1() => new(CountZeros(method0X434C49434B: false).ToString());

    public override ValueTask<string> Solve_2() => new(CountZeros(method0X434C49434B: true).ToString());

    private int CountZeros(bool method0X434C49434B)
    {
        var position = 50;
        var count = 0;

        foreach (var line in _input)
        {
            var direction = line[0];
            var distance = int.Parse(line[1..]);
            var delta = direction == 'L' ? -distance : distance;
            
            var oldPosition = position;
            position = ((position + delta) % 100 + 100) % 100;
            
            switch (method0X434C49434B)
            {
                case true:
                    switch (direction)
                    {
                        case 'R':
                            count += (oldPosition + distance) / 100;
                            break;
                        case 'L':
                        {
                            if (oldPosition == 0)
                            {
                                count += distance / 100;
                            }
                            else if (distance >= oldPosition)
                            {
                                count += (distance - oldPosition) / 100 + 1;
                            }
                            break;
                        }
                    }

                    break;
                case false when position == 0:
                    count += 1;
                    break;
            }
        }

        return count;
    }
}
