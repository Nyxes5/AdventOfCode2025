namespace AdventOfCode;

public sealed class Day04 : BaseDay
{
    private readonly char[][] _input;
    private readonly int _rows;
    private readonly int _cols;
    
    private static ReadOnlySpan<int> DirectionsX => [-1, 1, 0, 0, -1, -1, 1, 1];
    private static ReadOnlySpan<int> DirectionsY => [0, 0, -1, 1, -1, 1, -1, 1];

    public Day04()
    {
        var lines = File.ReadAllLines(InputFilePath);
        _rows = lines.Length;
        _cols = lines[0].Length;
        _input = new char[_rows][];
        for (var i = 0; i < _rows; i++)
        {
            _input[i] = lines[i].ToCharArray();
        }
    }

    public override ValueTask<string> Solve_1() => new(AmountOfFreeRolls().ToString());

    public override ValueTask<string> Solve_2() => new(TotalRemovableRolls().ToString());

    private int CountAdjacentRolls(char[][] grid, int row, int col)
    {
        if (grid[row][col] != '@') return 0;
        
        var count = 0;
        var dx = DirectionsX;
        var dy = DirectionsY;
        
        for (var d = 0; d < 8; d++)
        {
            var ni = row + dx[d];
            var nj = col + dy[d];
            
            if (ni >= 0 && ni < _rows && nj >= 0 && nj < _cols && grid[ni][nj] == '@')
            {
                count++;
            }
        }
        
        return count;
    }

    private int AmountOfFreeRolls()
    {
        var count = 0;
        
        for (var i = 0; i < _rows; i++)
        {
            for (var j = 0; j < _cols; j++)
            {
                if (_input[i][j] != '@' || CountAdjacentRolls(_input, i, j) >= 4) continue;
                count++;
            }
        }
        
        return count;
    }

    private int TotalRemovableRolls()
    {
        var grid = new char[_rows][];
        for (var i = 0; i < _rows; i++)
        {
            grid[i] = (char[])_input[i].Clone();
        }
        
        var totalRemoved = 0;
        var removedInIteration = true;
        
        while (removedInIteration)
        {
            removedInIteration = false;
            
            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _cols; j++)
                {
                    if (grid[i][j] != '@' || CountAdjacentRolls(grid, i, j) >= 4) continue;
                    
                    grid[i][j] = '.';
                    totalRemoved++;
                    removedInIteration = true;
                }
            }
        }
        
        return totalRemoved;
    }
}