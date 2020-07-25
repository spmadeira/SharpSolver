using System;
using System.Collections.Generic;
using System.Linq;
using Numerics;
using SharpSolver;

public static class LinearModelSolver {
    
    //Generate SolvedModel from LinearModel
    public static SolvedModel Solve(LinearModel linearModel)
    {
        var firstIteration = BuildFirstIteration(linearModel);

        SolvedModel res;
        
        if (firstIteration.Variables.Any(var => var.Type == VariableType.Artificial))
            res = TwoPhases(firstIteration, linearModel.Constraints);
        else
            res = Simplex(firstIteration, linearModel.Constraints);

        if (linearModel.Objective == Objective.MIN && res.Z != BigRational.Zero) //BigRational.Zero * -1 throws exception
            res.Z *= -1;

        res.ObjectiveOrientation = linearModel.Objective;
        res.Objective = linearModel.ObjectiveFunction.ToList();
        
        return res;
    }

    //Run simplex algorithm starting from an initial SolverIteration
    public static SolvedModel Simplex(SolverIteration initialIteration, List<Constraint> constraints)
    {
        var listIterations = new List<SolverIteration>(new []{initialIteration});
        SolverIteration currentIteration = initialIteration;
        
        while (true)
        {
            if (currentIteration.DecisionVariables.All(dv => dv >= 0))
            {
                var model = new SolvedModel
                {
                    SimplexIterations = listIterations, 
                    Constraints = constraints.ToList(),
                    Variables = currentIteration.Variables.ToList(),
                    Z = currentIteration.Z
                };

                //Initialize dir for all vars
                foreach (var variable in currentIteration.Variables)
                {
                    model.ObjectiveResult[variable] = BigRational.Zero;
                }

                for (int i = 0; i < currentIteration.Bases.Length; i++)
                {
                    model.ObjectiveResult[currentIteration.Bases[i]] = currentIteration.BaseVariables[i];
                }

                model.Result = 
                    constraints.All(c => ConstraintPasses(c, model.ObjectiveResult)) ? 
                        SolvedResult.Optimized : 
                        SolvedResult.Infeasible;

                return model;
            }

            try
            {
                currentIteration = ComputeIteration(currentIteration);
                listIterations.Add(currentIteration);
            }
            catch (UnboundedException)
            {
                return new SolvedModel
                {
                    Variables = initialIteration.Variables.ToList(),
                    Constraints = constraints.ToList(),
                    SimplexIterations = listIterations,
                    Result = SolvedResult.Unbounded
                };
            }
        }
    }
    
    //Run two-phases simplex algorithm starting from an initial SolverIteration
    public static SolvedModel TwoPhases(SolverIteration initialIteration, List<Constraint> constraints)
    {
        var twoPhasesInitialIteration = initialIteration.Copy();
        int artificialCutoff = -1;
        for (int i = 0; i < initialIteration.Variables.Length; i++)
        {
            if (initialIteration.Variables[i].Type == VariableType.Artificial)
            {
                artificialCutoff = i;
                break;
            }
        }
        
        if (artificialCutoff == -1)
            throw new ArgumentException();

        var twoPhasesObjective = Enumerable.Repeat<BigRational>(
            BigRational.One, twoPhasesInitialIteration.Variables.Length).ToArray();

        for (int i = 0; i < artificialCutoff; i++)
        {
            twoPhasesObjective[i] = 0;
        }

        twoPhasesInitialIteration.ObjectiveVariables = twoPhasesObjective;

        var twoPhaseBases = twoPhasesInitialIteration
            .Variables.Skip(
                twoPhasesInitialIteration.Variables.Length - twoPhasesInitialIteration.Bases.Length)
            .ToArray();
        
        twoPhasesInitialIteration.Bases = twoPhaseBases;
        
        for (int i = 0; i < twoPhasesInitialIteration.Variables.Length; i++)
        {
            BigRational Zj = BigRational.Zero;

            for (int j = 0; j < twoPhasesInitialIteration.Bases.Length; j++)
            {
                var rowVariable = twoPhasesInitialIteration.Bases[j];
                var rowObjectiveIndex = Array.IndexOf(twoPhasesInitialIteration.Variables, rowVariable);
                var rowObjective = twoPhasesInitialIteration.ObjectiveVariables[rowObjectiveIndex];
                Zj += twoPhasesInitialIteration.InputGrid[j][i] * rowObjective;
            }

            twoPhasesInitialIteration.DecisionVariables[i] = twoPhasesInitialIteration.ObjectiveVariables[i] - Zj;
        }
        
        twoPhasesInitialIteration.Z = 0;
        for (var i = 0; i < twoPhasesInitialIteration.Bases.Length; i++)
        {
            var baseVar = twoPhasesInitialIteration.Bases[i];
            var columnIndex = Array.IndexOf(twoPhasesInitialIteration.Variables, baseVar);
            twoPhasesInitialIteration.Z += twoPhasesInitialIteration.BaseVariables[i] * twoPhasesInitialIteration.ObjectiveVariables[columnIndex];
        }
        
        var solvedTwoPhases = Simplex(twoPhasesInitialIteration, constraints);

        solvedTwoPhases.TwoPhasesIterations = solvedTwoPhases.SimplexIterations;
        solvedTwoPhases.SimplexIterations = new List<SolverIteration>();
        
        if (solvedTwoPhases.Result != SolvedResult.Optimized)
        {
            return solvedTwoPhases;
        }
        if (solvedTwoPhases.Z != 0)
        {
            solvedTwoPhases.Result = SolvedResult.Unbounded;
            return solvedTwoPhases;
        }
        var finalIteration = solvedTwoPhases.TwoPhasesIterations.Last();

        for (int i = 0; i < finalIteration.Bases.Length; i++)
        {
            if (finalIteration.Bases[i].Type == VariableType.Artificial
                && finalIteration.BaseVariables[i] > 0)
            {
                solvedTwoPhases.Result = SolvedResult.Infeasible;
                return solvedTwoPhases;
            }
        }
        
        finalIteration = RemoveIndexes(
            finalIteration.Copy(), artificialCutoff);

        finalIteration.ObjectiveVariables = initialIteration.ObjectiveVariables.Take(artificialCutoff).ToArray();
        finalIteration.Z = 0;
        for (var i = 0; i < finalIteration.Bases.Length; i++)
        {
            var baseVar = finalIteration.Bases[i];
            var columnIndex = Array.IndexOf(finalIteration.Variables, baseVar);
            if (columnIndex == -1) finalIteration.Z += 0;
            else finalIteration.Z += finalIteration.BaseVariables[i] * finalIteration.ObjectiveVariables[columnIndex];
        }

        for (int i = 0; i < finalIteration.Variables.Length; i++)
        {
            var zj = -GetColumn(finalIteration.InputGrid, i).Sum();
            var cj = finalIteration.ObjectiveVariables[i];
            finalIteration.DecisionVariables[i] =
                -GetColumn(finalIteration.InputGrid, i).Sum() - finalIteration.ObjectiveVariables[i];
        }

        var simplexResult =  Simplex(finalIteration, constraints);
        simplexResult.TwoPhasesIterations = solvedTwoPhases.TwoPhasesIterations;
        return simplexResult;
    }

    //Compute SolverIteration from preceding iteration
    private static SolverIteration ComputeIteration(SolverIteration previous)
    {
        SolverIteration iteration = previous.Copy();
        
        var column = IncomingVariable(iteration.DecisionVariables);

        var row = LeavingVariable(GetColumn(iteration.InputGrid, column),
            iteration.BaseVariables);

        var variable = iteration.InputGrid[row][column];
        var rowArray = iteration.InputGrid[row];
        for (int i = 0; i < rowArray.Length; i++)
        {
            rowArray[i] /= variable;
        }
        
        iteration.BaseVariables[row] /= variable;
        
        for (int i = 0; i < iteration.InputGrid.Length; i++)
        {
            if(i == row) continue;
            var value = iteration.InputGrid[i][column];
            for (int j = 0; j < iteration.InputGrid[i].Length; j++)
            {
                iteration.InputGrid[i][j] -= value * iteration.InputGrid[row][j];
            }
            iteration.BaseVariables[i] -= value * iteration.BaseVariables[row];
        }

        var val = iteration.DecisionVariables[column];
        for (int i = 0; i < iteration.DecisionVariables.Length; i++)
        {
            iteration.DecisionVariables[i] -=
                val * iteration.InputGrid[row][i];
        }

        iteration.Bases[row] = iteration.Variables[column];
        
        iteration.Z = 0;
        for (var i = 0; i < iteration.Bases.Length; i++)
        {
            var baseVar = iteration.Bases[i];
            iteration.Z += GetVariableObjective(baseVar, iteration) * iteration.BaseVariables[i];
        }

        previous.SelectedGridPositions = (row, column);
        
        return iteration;
    }
    
    //Get incoming variable from decision row
    private static int IncomingVariable(BigRational[] decision)
    {
        var lowestIndex = 0;
        for (int i = 1; i < decision.Length; i++)
        {
            if (decision[i] < decision[lowestIndex])
                lowestIndex = i;
        }
        return lowestIndex;
    }

    //Get leaving variable from variable row
    private static int LeavingVariable(BigRational[] column, BigRational[] bases)
    {
        bool unbounded = true;
        var lowestIndex = -1;

        BigRational GetRatio(int r)
        {
            if (r < 0) return decimal.MaxValue;
            var element = column[r];
            if (element <= 0) return decimal.MaxValue;
            unbounded = false;
            var baseVal = bases[r];
            return baseVal / element;
        }
        
        for (int i = 0; i < column.Length; i++)
        {
            if (GetRatio(i) < GetRatio(lowestIndex))
                lowestIndex = i;
        }
        
        if (unbounded) throw new UnboundedException();
        return lowestIndex;
    }

    //Get grid column from grid and column index
    private static BigRational[] GetColumn(BigRational[][] grid, int columnIndex)
    {
        BigRational[] column = new BigRational[grid.Length];
        for (int i = 0; i < grid.Length; i++)
        {
            column[i] = grid[i][columnIndex];
        }
        return column;
    }
    
    //Get objective value of variable from current iteration
    private static BigRational GetVariableObjective(Variable variable, SolverIteration iteration)
    {
        var columnIndex = Array.IndexOf(iteration.Variables, variable);
        if (columnIndex == -1) return 0;
        else return iteration.ObjectiveVariables[columnIndex];
    }
    
    //Check if constraint passes from objective result
    private static bool ConstraintPasses(Constraint constraint, Dictionary<Variable, BigRational> variables)
    {
        BigRational total = BigRational.Zero;

        foreach (var variable in constraint.Variables)
        {
            total += variables[variable.Variable] * variable.Coefficient;
        }

        switch (constraint.ConstraintType)
        {
            case ConstraintType.LessThanOrEqualTo:
                return total <= constraint.ConstraintValue;
            case ConstraintType.GreaterThanOrEqualTo:
                return total >= constraint.ConstraintValue;
            case ConstraintType.EqualTo:
                return total == constraint.ConstraintValue;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //Remove artificial variable indexes after two-phases finishes
    public static SolverIteration RemoveIndexes(SolverIteration iteration, int cutoffIndex)
    {
        var cutIteration = iteration.Copy();

        var variables = iteration.Variables.Take(cutoffIndex).ToArray();
        var objectiveVariables = iteration.ObjectiveVariables.Take(cutoffIndex)
            .ToArray();
        var decisionVariables = iteration.DecisionVariables.Take(cutoffIndex)
            .ToArray();
        var inputGrid = new List<BigRational[]>();
        
        for (int i = 0; i < iteration.InputGrid.Length; i++)
        {
            inputGrid.Add(iteration.InputGrid[i].Take(cutoffIndex).ToArray());
        }

        cutIteration.Variables = variables;
        cutIteration.ObjectiveVariables = objectiveVariables;
        cutIteration.DecisionVariables = decisionVariables;
        cutIteration.InputGrid = inputGrid.ToArray();
        return cutIteration;
    }

    //Build first SolverIteration from LinearModel
    public static SolverIteration BuildFirstIteration(LinearModel model)
    {
        var variables = model.Variables.ToList();
        var constraints = model.Constraints.Select(c => c.Copy()).ToList();
        var objectives = model.ObjectiveFunction
            .Select(vv => vv.Coefficient).ToList();

        if (model.Objective == Objective.MIN)
            objectives = objectives.Select(o => o * -1).ToList();
        
        var inputGrid = new List<List<BigRational>>();

        int slackCount = 0;
        int artificialCount = 0;
        
        foreach (var constraint in constraints)
        {
            if (constraint.ConstraintValue < 0)
            {
                switch (constraint.ConstraintType)
                {
                    case ConstraintType.LessThanOrEqualTo:
                        constraint.ConstraintType = ConstraintType.GreaterThanOrEqualTo;
                        break;
                    case ConstraintType.GreaterThanOrEqualTo:
                        constraint.ConstraintType = ConstraintType.LessThanOrEqualTo;
                        break;
                    case ConstraintType.EqualTo:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                for (int i = 0; i < constraint.Variables.Count; i++)
                {
                    constraint.Variables[i].Coefficient *= -1;
                }

                constraint.ConstraintValue *= -1;
            }

            switch (constraint.ConstraintType)
            {
                case ConstraintType.LessThanOrEqualTo:
                {
                    var slack = new Variable{Name = $"S{++slackCount}", Type = VariableType.Slack};
                    variables.Add(slack);
                    objectives.Add(0);
                    constraint.Variables.Add(new ValueVariable{Variable = slack, Coefficient = 1});
                    break;
                }
                case ConstraintType.GreaterThanOrEqualTo:
                {
                    var slack = new Variable{Name = $"S{++slackCount}", Type = VariableType.Slack};
                    var artificial = new Variable {Name = $"A{++artificialCount}", Type = VariableType.Artificial};
                    variables.Add(slack);
                    objectives.Add(BigRational.Zero);
                    variables.Add(artificial);
                    objectives.Add(BigRational.Zero);
                    constraint.Variables.Add(new ValueVariable{Variable = slack, Coefficient = -1});
                    constraint.Variables.Add(new ValueVariable{Variable = artificial, Coefficient = 1});
                    break;
                }
                case ConstraintType.EqualTo:
                {
                    var artificial = new Variable {Name = $"A{++artificialCount}", Type = VariableType.Artificial};
                    variables.Add(artificial);
                    objectives.Add(0);
                    constraint.Variables.Add(new ValueVariable{Variable = artificial, Coefficient = 1});
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        variables.Sort((v1,v2) => v1.Type-v2.Type);

        foreach (var constraint in constraints)
        {
            foreach (var missingVariable in variables.Except(constraint.Variables.Select(vv => vv.Variable)))
            {
                constraint.Variables.Add(missingVariable.As(0));
            }
        }
        
        foreach (var constraint in constraints)
        {
            constraint.Variables.Sort((vv1, vv2) => 
                variables.IndexOf(vv1.Variable) - variables.IndexOf(vv2.Variable));
        }
        
        inputGrid = constraints
            .Select(c => c.Variables
                .Select(v => v.Coefficient).ToList())
            .ToList();
        
        var firstIteration = new SolverIteration
        {
            Variables = variables.ToArray(),
            Z = BigRational.Zero,
            ObjectiveVariables = objectives.ToArray(),
            DecisionVariables = objectives.Select(o => o*-1).ToArray(),
            BaseVariables = constraints.Select(c => c.ConstraintValue).ToArray(),
            Bases = variables.Skip(variables.Count-constraints.Count).ToArray(),
            InputGrid = inputGrid.Select(r => r.ToArray()).ToArray()
        };

        return firstIteration;
    }
    
    //Thrown is model is unbounded
    class UnboundedException : Exception {}
}
