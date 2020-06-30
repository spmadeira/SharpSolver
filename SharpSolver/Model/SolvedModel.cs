using System;
using System.Collections.Generic;
using System.Linq;
using Numerics;
using SharpSolver;

public class SolvedModel
{
    public List<Variable> Variables = new List<Variable>();
    public List<Constraint> Constraints = new List<Constraint>();
    public List<SolverIteration> TwoPhasesIterations { get; set; } = new List<SolverIteration>();
    public List<SolverIteration> SimplexIterations { get; set; } = new List<SolverIteration>();
    public Objective ObjectiveOrientation { get; set; }
    public List<ValueVariable> Objective { get; set; } = new List<ValueVariable>();
    public Dictionary<Variable, BigRational> ObjectiveResult { get; set; } = new Dictionary<Variable, BigRational>();
    public BigRational Z;
    public SolvedResult Result;
}

public class SolverIteration
{
    public Variable[] Variables;
    public BigRational Z = 0f;
    public BigRational[] ObjectiveVariables;
    public BigRational[] BaseVariables;
    public BigRational[] DecisionVariables;
    public Variable[] Bases;
    public BigRational[][] InputGrid;
    public (int x, int y) SelectedGridPositions = (-1, -1);

    public SolverIteration Copy()
    {
        return new SolverIteration
        {
            Variables = Variables.ToArray(),
            Z = Z,
            ObjectiveVariables = ObjectiveVariables.ToArray(),
            BaseVariables = BaseVariables.ToArray(),
            DecisionVariables = DecisionVariables.ToArray(),
            Bases = Bases.ToArray(),
            InputGrid = InputGrid.Select(row => row.ToArray()).ToArray()
        };
    }
}

public enum SolvedResult
{
    Optimized,
    Unbounded,
    Infeasible
}
