namespace AdventOfCode;

public sealed class Day08 : BaseDay
{
    private readonly (int x, int y, int z)[] _junctionBoxes;
    private readonly List<(double distance, int boxIndex1, int boxIndex2)> _sortedConnections = [];

    public Day08()
    {
        var lines = File.ReadAllLines(InputFilePath);
        _junctionBoxes = new (int x, int y, int z)[lines.Length];
        
        for (var i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            _junctionBoxes[i] = (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }
        
        for (var i = 0; i < _junctionBoxes.Length; i++)
        {
            for (var j = i + 1; j < _junctionBoxes.Length; j++)
            {
                var dx = (long)_junctionBoxes[i].x - _junctionBoxes[j].x;
                var dy = (long)_junctionBoxes[i].y - _junctionBoxes[j].y;
                var dz = (long)_junctionBoxes[i].z - _junctionBoxes[j].z;
                var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                _sortedConnections.Add((distance, i, j));
            }
        }
        _sortedConnections.Sort((a, b) => a.distance.CompareTo(b.distance));
    }

    public override ValueTask<string> Solve_1() => new(Connect1000JunctionBoxes().ToString());

    public override ValueTask<string> Solve_2() => new(ConnectAllJunctionBoxes().ToString());
    
    private long Connect1000JunctionBoxes()
    {
        var unionFind = new UnionFind(_junctionBoxes.Length);
        
        var connectionLimit = Math.Min(1000, _sortedConnections.Count);
        for (var k = 0; k < connectionLimit; k++)
        {
            var (_, boxIndex1, boxIndex2) = _sortedConnections[k];
            unionFind.ConnectJunctionBoxes(boxIndex1, boxIndex2);
        }
        
        var circuitSizes = unionFind.GetCircuitSizes();
        
        var topThreeSizes = circuitSizes.OrderByDescending(size => size).Take(3).ToList();
        return (long)topThreeSizes[0] * topThreeSizes[1] * topThreeSizes[2];
    }
    
    private long ConnectAllJunctionBoxes()
    {
        var unionFind = new UnionFind(_junctionBoxes.Length);
        
        foreach (var (_, boxIndex1, boxIndex2) in _sortedConnections)
        {
            if (unionFind.AreInSameCircuit(boxIndex1, boxIndex2)) continue;
            unionFind.ConnectJunctionBoxes(boxIndex1, boxIndex2);
                
            if (unionFind.CircuitCount == 1)
            {
                return (long)_junctionBoxes[boxIndex1].x * _junctionBoxes[boxIndex2].x;
            }
        }
        
        return 0; // Should never reach here if input is valid
    }
    
    private sealed class UnionFind
    {
        private readonly int[] _parent;
        private readonly int[] _rank;
        
        public int CircuitCount { get; private set; }

        public UnionFind(int size)
        {
            _parent = new int[size];
            _rank = new int[size];
            CircuitCount = size;
            
            for (var i = 0; i < size; i++)
            {
                _parent[i] = i;
            }
        }

        private int FindRoot(int node)
        {
            if (_parent[node] != node)
            {
                _parent[node] = FindRoot(_parent[node]);
            }
            return _parent[node];
        }

        public void ConnectJunctionBoxes(int node1, int node2)
        {
            var root1 = FindRoot(node1);
            var root2 = FindRoot(node2);
            
            if (root1 == root2) return;
            
            if (_rank[root1] < _rank[root2])
            {
                _parent[root1] = root2;
            }
            else if (_rank[root1] > _rank[root2])
            {
                _parent[root2] = root1;
            }
            else
            {
                _parent[root2] = root1;
                _rank[root1]++;
            }
            
            CircuitCount--;
        }

        public bool AreInSameCircuit(int node1, int node2)
        {
            return FindRoot(node1) == FindRoot(node2);
        }

        public List<int> GetCircuitSizes()
        {
            var circuitSizes = new Dictionary<int, int>();
            for (var i = 0; i < _parent.Length; i++)
            {
                var root = FindRoot(i);
                circuitSizes[root] = circuitSizes.GetValueOrDefault(root, 0) + 1;
            }
            return circuitSizes.Values.ToList();
        }
    }
}

