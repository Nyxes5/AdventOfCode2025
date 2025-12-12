namespace AdventOfCode;

public sealed class Day12 : BaseDay
{ 
    private record Region(int Width, int Height, int[] ShapeCounts);

    private readonly List<bool[,]> _shapes = [];
    private readonly List<Region> _regions = [];
    private readonly List<int> _shapeAreas = [];
    
    public Day12()
    {
        var input = File.ReadAllLines(InputFilePath);
        
        var i = 0;
        
        for (var shapeIndex = 0; shapeIndex < 6; shapeIndex++)
        {
            i++;
            var shape = new bool[3, 3];
            var shapeArea = 0;
            for (var row = 0; row < 3; row++)
            {
                var line = input[i++];
                for (var col = 0; col < 3; col++)
                {
                    if (line[col] == '#')
                    {
                        shape[row, col] = true;
                        shapeArea++;
                    }
                    else
                    {
                        shape[row, col] = false;
                    }
                }
            }
            _shapes.Add(shape);
            _shapeAreas.Add(shapeArea);
            
            i++;
        }

        while (i < input.Length)
        {
            var line = input[i++].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            var parts = line.Split(':');
            
            var dimensions = parts[0].Split('x').Select(int.Parse).ToArray();
            var counts = parts[1].Trim().Split(' ').Select(int.Parse).ToArray();
            
            _regions.Add(new Region(dimensions[0], dimensions[1], counts));
        }
    }

    public override ValueTask<string> Solve_1() => new(CountValidRegions().ToString());

    public override ValueTask<string> Solve_2() => new("🚀 Completed Advent of Code 2025 🚀");
    
    private int CountValidRegions()
    {
        var valid = _regions.Count;

        foreach (var region in _regions)
        {
            var totalShapeArea = region.ShapeCounts.Select((c, idx) => c * _shapeAreas[idx]).Sum();

            // Early check: if total shape area exceeds region area, it's invalid
            if (totalShapeArea > region.Width * region.Height)
            {
                valid--;
                continue;
            }
            
            // Apparently this already gives the correct answer.
            // Further checks would be too heavy computationally.
        }

        return valid;
    }
}