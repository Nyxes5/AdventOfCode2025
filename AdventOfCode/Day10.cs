namespace AdventOfCode;

public sealed class Day10 : BaseDay
{
    private record Machine(bool[] TargetState, int[][] Buttons, int[] JoltageRequirements);
    
    private readonly List<Machine> _machines;

    public Day10()
    {
        _machines = File.ReadAllLines(InputFilePath).Select(ParseMachine).ToList();
    }

    public override ValueTask<string> Solve_1() => new(_machines.Sum(SolveMachine).ToString());

    public override ValueTask<string> Solve_2() => new(_machines.Sum(SolveJoltageMachine).ToString());

    private static Machine ParseMachine(string line)
    {
        var lightStart = line.IndexOf('[') + 1;
        var lightEnd = line.IndexOf(']');
        var targetState = line.Substring(lightStart, lightEnd - lightStart).Select(c => c == '#').ToArray();

        var buttons = new List<int[]>();
        var buttonStartIndex = line.IndexOf('(', lightEnd) + 1;
        while (buttonStartIndex != 0)
        {
            var buttonEndIndex = line.IndexOf(')', buttonStartIndex);
            var buttonLights = line.Substring(buttonStartIndex, buttonEndIndex - buttonStartIndex)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToArray();
            buttons.Add(buttonLights);
            buttonStartIndex = line.IndexOf('(', buttonEndIndex) + 1;
        }

        var joltageStart = line.IndexOf('{') + 1;
        var joltageEnd = line.IndexOf('}');
        var joltageRequirements = line.Substring(joltageStart, joltageEnd - joltageStart)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToArray();

        return new Machine(targetState, buttons.ToArray(), joltageRequirements);
    }

    // Example: [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
    // Matrix:
    //        B0  B1  B2  B3  B4  B5 | Target
    // L0: [  0   0   0   0   1   1  |   0   ]
    // L1: [  0   1   0   0   0   1  |   1   ]
    // L2: [  0   0   1   0   1   0  |   1   ]
    // L3: [  1   1   0   1   0   0  |   0   ]
    private static int SolveMachine(Machine machine)
    {
        var numLights = machine.TargetState.Length;
        var numButtons = machine.Buttons.Length;

        var matrix = new bool[numLights, numButtons + 1];
        
        for (var light = 0; light < numLights; light++)
        {
            for (var button = 0; button < numButtons; button++)
            {
                matrix[light, button] = machine.Buttons[button].Contains(light);
            }
            matrix[light, numButtons] = machine.TargetState[light];
        }
        
        return SolveMatrix(matrix, numLights, numButtons);
    }

    // Example: [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
    // Matrix:
    //        B0  B1  B2  B3  B4  B5 | Target
    // L0: [  0   0   0   0   1   1  |   3   ]
    // L1: [  0   1   0   0   0   1  |   5   ]
    // L2: [  0   0   1   0   1   0  |   4   ]
    // L3: [  1   1   0   1   0   0  |   7   ]
    private static int SolveJoltageMachine(Machine machine)
    {
        var numCounters = machine.JoltageRequirements.Length;
        var numButtons = machine.Buttons.Length;

        var matrix = new double[numCounters, numButtons + 1];
        
        for (var counter = 0; counter < numCounters; counter++)
        {
            for (var button = 0; button < numButtons; button++)
            {
                matrix[counter, button] = machine.Buttons[button].Contains(counter) ? 1 : 0;
            }
            matrix[counter, numButtons] = machine.JoltageRequirements[counter];
        }
        
        return SolveMatrix(matrix, numCounters, numButtons);
    }
    
    private static int SolveMatrix(bool[,] matrix, int rowCount, int colCount)
    {
        var determinedColumns = SimplifyMatrix(matrix, rowCount, colCount);
        var undeterminedColumns = GetUndeterminedColumns(determinedColumns, colCount);
        return FindMinimalButtonPressCount(matrix, colCount, determinedColumns, undeterminedColumns);
    }

    private static int SolveMatrix(double[,] matrix, int rowCount, int colCount)
    {
        var determinedColumns = SimplifyMatrix(matrix, rowCount, colCount);
        var undeterminedColumns = GetUndeterminedColumns(determinedColumns, colCount);
        return FindMinimalButtonPressCount(matrix, rowCount, colCount, determinedColumns, undeterminedColumns);
    }
    
    private static List<int> SimplifyMatrix(bool[,] matrix, int rowCount, int colCount)
    {
        var currentRow = 0;
        var determinedColumns = new List<int>();

        for (var col = 0; col < colCount && currentRow < rowCount; col++)
        {
            var bestRow = FindDeterminingRow(matrix, rowCount, col, currentRow);
            if (bestRow == -1) continue;

            determinedColumns.Add(col);
            SwapRows(matrix, colCount, currentRow, bestRow);
            EliminateColumn(matrix, rowCount, colCount, col, currentRow);
            
            currentRow++;
        }

        return determinedColumns;
    }

    private static List<int> SimplifyMatrix(double[,] matrix, int rowCount, int colCount)
    {
        var currentRow = 0;
        var determinedColumns = new List<int>();

        for (var col = 0; col < colCount && currentRow < rowCount; col++)
        {
            var bestRow = FindDeterminingRow(matrix, rowCount, col, currentRow);
            if (bestRow == -1) continue;

            determinedColumns.Add(col);
            SwapRows(matrix, colCount, currentRow, bestRow);
            NormalizeRowAtColumn(matrix, colCount, currentRow, col);
            EliminateColumn(matrix, rowCount, colCount, col, currentRow);

            currentRow++;
        }

        return determinedColumns;
    }

    private static int FindDeterminingRow(bool[,] matrix, int rowCount, int col, int startRow)
    {
        for (var row = startRow; row < rowCount; row++)
        {
            if (matrix[row, col]) return row;
        }
        return -1;
    }

    private static int FindDeterminingRow(double[,] matrix, int rowCount, int col, int startRow)
    {
        var maxAbs = 0.0;
        var bestRow = -1;

        for (var row = startRow; row < rowCount; row++)
        {
            var value = Math.Abs(matrix[row, col]);
            if (!(value > maxAbs)) continue;
            maxAbs = value;
            bestRow = row;
        }

        return bestRow;
    }

    private static void SwapRows(bool[,] matrix, int colCount, int row1, int row2)
    {
        if (row1 == row2) return;
        
        for (var c = 0; c <= colCount; c++)
        {
            (matrix[row1, c], matrix[row2, c]) = (matrix[row2, c], matrix[row1, c]);
        }
    }

    private static void SwapRows(double[,] matrix, int colCount, int row1, int row2)
    {
        if (row1 == row2) return;
        for (var col = 0; col <= colCount; col++)
        {
            (matrix[row1, col], matrix[row2, col]) = (matrix[row2, col], matrix[row1, col]);
        }
    }

    private static void NormalizeRowAtColumn(double[,] matrix, int colCount, int row, int col)
    {
        var factor = matrix[row, col];
        if (Math.Abs(factor) < 1e-12) return;
        for (var c = 0; c <= colCount; c++)
        {
            matrix[row, c] /= factor;
        }
    }

    private static void EliminateColumn(bool[,] matrix, int rowCount, int colCount, int col, int sourceRow)
    {
        for (var row = 0; row < rowCount; row++)
        {
            if (row == sourceRow || !matrix[row, col]) continue;
            
            for (var c = 0; c <= colCount; c++)
            {
                matrix[row, c] ^= matrix[sourceRow, c];
            }
        }
    }

    private static void EliminateColumn(double[,] matrix, int rowCount, int colCount, int col, int sourceRow)
    {
        for (var row = 0; row < rowCount; row++)
        {
            if (row == sourceRow) continue;
            var factor = matrix[row, col];
            if (Math.Abs(factor) < 1e-12) continue;
            for (var c = 0; c <= colCount; c++)
            {
                matrix[row, c] -= factor * matrix[sourceRow, c];
            }
        }
    }

    private static List<int> GetUndeterminedColumns(List<int> determined, int total)
    {
        var undetermined = new List<int>();
        for (var col = 0; col < total; col++)
        {
            if (!determined.Contains(col))
            {
                undetermined.Add(col);
            }
        }
        return undetermined;
    }

    private static int FindMinimalButtonPressCount(bool[,] matrix, int colCount, List<int> determinedCols, List<int> undeterminedCols)
    {
        var best = int.MaxValue;
        var undeterminedCount = undeterminedCols.Count;
        var combinations = 1 << undeterminedCount;

        for (var mask = 0; mask < combinations; mask++)
        {
            var solution = new bool[colCount];

            for (var i = 0; i < undeterminedCount; i++)
            {
                solution[undeterminedCols[i]] = (mask & (1 << i)) != 0;
            }
            
            for (var r = determinedCols.Count - 1; r >= 0; r--)
            {
                var col = determinedCols[r];
                var value = matrix[r, colCount];

                for (var c = col + 1; c < colCount; c++)
                {
                    if (matrix[r, c])
                    {
                        value ^= solution[c];
                    }
                }

                solution[col] = value;
            }

            var pressed = solution.Count(x => x);
            if (pressed < best) best = pressed;
        }

        return best;
    }

    private static int FindMinimalButtonPressCount(double[,] matrix, int rowCount, int colCount, List<int> determinedCols, List<int> undeterminedCols)
    {
        var minSum = int.MaxValue;
        if (undeterminedCols.Count == 0)
        {
            var solution = new int[colCount];
            for (var row = 0; row < determinedCols.Count; row++)
            {
                var col = determinedCols[row];
                solution[col] = (int)Math.Round(matrix[row, colCount]);
            }

            return solution.Sum();
        }

        var maxTarget = GetMaxTarget(matrix, rowCount, colCount);
        var determinedCoefficients = ExtractDeterminedCoefficients(matrix, determinedCols, undeterminedCols);
        var workingSolution = new int[colCount];
        var undeterminedValues = new int[undeterminedCols.Count];
        FindInitialSolution(matrix, colCount, determinedCols, undeterminedCols, determinedCoefficients, workingSolution, undeterminedValues, maxTarget, ref minSum);
        SearchForOptimal(matrix, colCount, determinedCols, undeterminedCols, determinedCoefficients, workingSolution, undeterminedValues, maxTarget * 2, ref minSum);
        return minSum;
    }

    private static int GetMaxTarget(double[,] matrix, int rowCount, int colCount)
    {
        double maxTarget = 0;
        for (var row = 0; row < rowCount; row++)
        {
            var value = matrix[row, colCount];
            if (value > maxTarget)
            {
                maxTarget = value;
            }
        }

        return (int)Math.Round(maxTarget);
    }

    private static double[,] ExtractDeterminedCoefficients(double[,] matrix, List<int> determinedCols, List<int> undeterminedCols)
    {
        var result = new double[determinedCols.Count, undeterminedCols.Count];

        for (var row = 0; row < determinedCols.Count; row++)
        {
            for (var i = 0; i < undeterminedCols.Count; i++)
            {
                result[row, i] = matrix[row, undeterminedCols[i]];
            }
        }

        return result;
    }

    private static void FindInitialSolution(double[,] matrix, int colCount, List<int> determinedCols, List<int> undeterminedCols,
        double[,] determinedCoefficients, int[] workingSolution, int[] undeterminedValues, int maxTarget, ref int minSum)
    {
        var costBounds = new[] { maxTarget, maxTarget * 2, maxTarget * 3, maxTarget * 5 };
        foreach (var bound in costBounds)
        {
            if (SearchInitialSolution(matrix, colCount, determinedCols, undeterminedCols, determinedCoefficients,
                    workingSolution, undeterminedValues, bound, ref minSum))
            {
                break;
            }
        }
    }

    private static bool SearchInitialSolution(double[,] matrix, int colCount, List<int> determinedCols, List<int> undeterminedCols,
        double[,] determinedCoefficients, int[] workingSolution, int[] undeterminedValues, int maxCost, ref int minSum, 
        int undeterminedIndex = 0, int currentSum = 0)
    {
        if (currentSum > maxCost) return false;
        if (undeterminedIndex == undeterminedCols.Count)
        {
            return ValidateAndStoreSolution(matrix, colCount, determinedCols, undeterminedCols, determinedCoefficients, workingSolution,
                undeterminedValues, currentSum, maxCost, ref minSum);
        }

        var maxVal = maxCost - currentSum;
        for (var val = 0; val <= maxVal; val++)
        {
            undeterminedValues[undeterminedIndex] = val;
            if (SearchInitialSolution(matrix, colCount, determinedCols, undeterminedCols, determinedCoefficients,
                    workingSolution, undeterminedValues, maxCost, ref minSum, undeterminedIndex + 1, currentSum + val))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ValidateAndStoreSolution(double[,] matrix, int colCount, List<int> determinedCols,
        List<int> undeterminedCols, double[,] determinedCoefficients, int[] workingSolution, int[] undeterminedValues,
        int currentSum, int maxCost, ref int minSum)
    {
        var totalSum = currentSum;
        for (var row = 0; row < determinedCols.Count; row++)
        {
            var dependentButton = determinedCols[row];
            var value = matrix[row, colCount];
            for (var i = 0; i < undeterminedCols.Count; i++)
            {
                value -= determinedCoefficients[row, i] * undeterminedValues[i];
            }

            var roundedValue = (int)Math.Round(value);
            if (roundedValue < 0 || Math.Abs(value - roundedValue) > 1e-12) return false;
            workingSolution[dependentButton] = roundedValue;
            totalSum += roundedValue;
            if (totalSum > maxCost) return false;
        }

        for (var i = 0; i < undeterminedCols.Count; i++)
        {
            workingSolution[undeterminedCols[i]] = undeterminedValues[i];
        }

        minSum = totalSum;
        return true;
    }

    private static void SearchForOptimal(double[,] matrix, int colCount, List<int> determinedCols,
        List<int> undeterminedCols, double[,] determinedCoefficients, int[] workingSolution, int[] undeterminedValues,
        int maxUndeterminedValue, ref int minSum, int undeterminedIndex = 0, int currentSum = 0)
    {
        if (currentSum >= minSum) return;
        if (undeterminedIndex == undeterminedCols.Count)
        {
            TryOptimalSolution(matrix, colCount, determinedCols, undeterminedCols, determinedCoefficients,
                workingSolution, undeterminedValues, currentSum, ref minSum);
        }
        else
        {
            var maxVal = Math.Min(maxUndeterminedValue, minSum - currentSum - 1);
            for (var val = 0; val <= maxVal; val++)
            {
                undeterminedValues[undeterminedIndex] = val;
                SearchForOptimal(matrix, colCount, determinedCols, undeterminedCols, determinedCoefficients, workingSolution,
                    undeterminedValues, maxUndeterminedValue, ref minSum, undeterminedIndex + 1, currentSum + val);
                if (minSum <= currentSum + val + 1) return;
            }
        }
    }

    private static void TryOptimalSolution(double[,] matrix, int colCount, List<int> determinedCols,
        List<int> undeterminedCols, double[,] determinedCoefficients, int[] workingSolution, int[] undeterminedValues,
        int currentSum, ref int minSum)
    {
        var totalSum = currentSum;
        for (var row = 0; row < determinedCols.Count; row++)
        {
            var dependentButton = determinedCols[row];
            var value = matrix[row, colCount];
            for (var i = 0; i < undeterminedCols.Count; i++)
            {
                value -= determinedCoefficients[row, i] * undeterminedValues[i];
            }

            var roundedValue = (int)Math.Round(value);
            if (roundedValue < 0 || Math.Abs(value - roundedValue) > 1e-12) return;
            workingSolution[dependentButton] = roundedValue;
            totalSum += roundedValue;
            if (totalSum >= minSum) return;
        }

        for (var i = 0; i < undeterminedCols.Count; i++)
        {
            workingSolution[undeterminedCols[i]] = undeterminedValues[i];
        }

        minSum = totalSum;
    }
}