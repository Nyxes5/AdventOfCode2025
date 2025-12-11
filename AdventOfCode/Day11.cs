namespace AdventOfCode;

public sealed class Day11 : BaseDay
{
    private readonly Dictionary<string, List<string>> _devices = new();
    private readonly Dictionary<(string, string), long> _cache = new();

    public Day11()
    {
        var input = File.ReadAllLines(InputFilePath);
        foreach (var line in input)
        {
            var parts = line.Split(": ");
            var device = parts[0];
            var outputs = parts[1].Split(' ').ToList();
            _devices[device] = outputs;
        }
    }

    public override ValueTask<string> Solve_1() => new(CountPaths("you", "out").ToString());

    public override ValueTask<string> Solve_2() => new(CountPaths("svr", "out", ["dac", "fft"]).ToString());
    
    private long CountPaths(string current, string end, HashSet<string> requiredNodes = null, HashSet<string> visited = null)
    {
        visited ??= [];
        requiredNodes ??= [];
        
        if (current == end)
        {
            return requiredNodes.Count == 0 ? 1 : 0;
        }
        
        if (!_devices.TryGetValue(current, out var outputs)) return 0;

        var cacheKey = (current, string.Join(",", requiredNodes.OrderBy(x => x)));
        
        if (_cache.TryGetValue(cacheKey, out var cached)) return cached;

        visited.Add(current);

        var newRequiredNodes = requiredNodes.Contains(current)
            ? [..requiredNodes.Where(n => n != current)]
            : requiredNodes;
        
        var pathCount = outputs
            .Where(output => !visited.Contains(output))
            .Sum(output => CountPaths(output, end, newRequiredNodes, visited));
        
        visited.Remove(current);
        
        _cache[cacheKey] = pathCount;
        
        return pathCount;
    }
}