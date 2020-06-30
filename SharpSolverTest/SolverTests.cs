using System;
using System.Collections.Generic;
using Numerics;
using NUnit.Framework;
using SharpSolver;
using SharpSolverTest;

public class SolverTests
{
    [SetUp] public void SetUp()
    {
    }

    [Test] public void DirectSimplex()
    {
        var x1 = Variable.Input("X1");
        var x2 = Variable.Input("X2");
        var x3 = Variable.Input("X3");
        var s1 = Variable.Slack("S1");
        var s2 = Variable.Slack("S2");
        var s3 = Variable.Slack("S3");
        
        var c1 = Constraint.LessThanOrEqualTo("C1", 8, x1.As(2), x2.As(3));
        var c2 = Constraint.LessThanOrEqualTo("C2", 10, x2.As(2), x3.As(5));
        var c3 = Constraint.LessThanOrEqualTo("C3", 15, x1.As(3), x2.As(2), x3.As(4));

        var solverIteration = new SolverIteration
        {
            ObjectiveVariables = new BigRational[] {3, 5, 4, 0, 0, 0},
            Variables = new[] {x1, x2, x3, s1, s2, s3},
            Bases = new[] {s1, s2, s3},
            BaseVariables = new BigRational[] {8, 10, 15},
            DecisionVariables = new BigRational[] {-3, -5, -4, 0, 0, 0},
            InputGrid = new[]
            {
                new BigRational[] {2, 3, 0, 1, 0, 0},
                new BigRational[] {0, 2, 5, 0, 1, 0},
                new BigRational[] {3, 2, 4, 0, 0, 1}
            }
        };

        var res = LinearModelSolver.Simplex(solverIteration, new List<Constraint>(new[] {c1, c2, c3}));

        Assert.IsTrue(res.Result == SolvedResult.Optimized);

        Assert.AreEqual(new BigRational(765,41), res.Z);
    }

    [Test] public void DirectTwoPhases()
    {
        var x1 = new Variable {Name = "X1", Type = VariableType.Input};
        var x2 = new Variable {Name = "X2", Type = VariableType.Input};
        var s1 = new Variable {Name = "S1", Type = VariableType.Slack};
        var s2 = new Variable {Name = "S2", Type = VariableType.Slack};
        var a1 = new Variable {Name = "A1", Type = VariableType.Artificial};
        var a2 = new Variable {Name = "A2", Type = VariableType.Artificial};
        var c1 = new Constraint
        {
            Name = "C1", ConstraintType = ConstraintType.GreaterThanOrEqualTo,
            Variables = new List<ValueVariable>(new[]
            {
                new ValueVariable {Variable = x1, Coefficient = 2},
                new ValueVariable {Variable = x2, Coefficient = 1},
            }),
            ConstraintValue = 4
        };
        var c2 = new Constraint
        {
            Name = "C2", ConstraintType = ConstraintType.GreaterThanOrEqualTo,
            Variables = new List<ValueVariable>(new[]
            {
                new ValueVariable {Variable = x1, Coefficient = 1},
                new ValueVariable {Variable = x2, Coefficient = 7},
            }),
            ConstraintValue = 7
        };

        var solverIteration = new SolverIteration
        {
            ObjectiveVariables = new BigRational[] {-1, -1, 0, 0, 0, 0},
            Variables = new[] {x1, x2, s1, s2, a1, a2},
            Bases = new[] {x1, x2},
            BaseVariables = new BigRational[] {4, 7},
            DecisionVariables = new BigRational[] {-3, -8, 1, 1, 0, 0},
            InputGrid = new[]
            {
                new BigRational[] {2, 1, -1, 0, 1, 0},
                new BigRational[] {1, 7, 0, -1, 0, 1},
            }
        };

        var res = LinearModelSolver.TwoPhases(solverIteration, new List<Constraint>(new[] {c1, c2}));

        Assert.AreEqual(new BigRational(-31,13), res.Z);
    }

    [Test] public void TwoPhasesFromLinearModel()
    {
        var x1 = new Variable {Name = "X1", Type = VariableType.Input};
        var x2 = new Variable {Name = "X2", Type = VariableType.Input};
        var c1 = new Constraint
        {
            Name = "C1", ConstraintType = ConstraintType.GreaterThanOrEqualTo,
            Variables = new List<ValueVariable>(new[]
            {
                new ValueVariable {Variable = x1, Coefficient = 2},
                new ValueVariable {Variable = x2, Coefficient = 1},
            }),
            ConstraintValue = 4
        };
        var c2 = new Constraint
        {
            Name = "C2", ConstraintType = ConstraintType.GreaterThanOrEqualTo,
            Variables = new List<ValueVariable>(new[]
            {
                new ValueVariable {Variable = x1, Coefficient = 1},
                new ValueVariable {Variable = x2, Coefficient = 7},
            }),
            ConstraintValue = 7
        };
        var objectiveFunction = new List<ValueVariable>(new[]
        {
            new ValueVariable {Variable = x1, Coefficient = 1},
            new ValueVariable {Variable = x2, Coefficient = 1},
        });

        var linearModel = new LinearModel
        {
            Objective = Objective.MIN,
            Variables = new List<Variable>(new[] {x1, x2}),
            Constraints = new List<Constraint>(new[] {c1, c2}),
            ObjectiveFunction = objectiveFunction
        };

        var res = LinearModelSolver.Solve(linearModel);

        Assert.AreEqual(new BigRational(31,13), res.Z);
    }

    [Test] public void TwoPhasesInfeasibleFromLinearModel()
    {
        var x1 = new Variable{Name = "X1", Type = VariableType.Input};
        var x2 = new Variable{Name = "X2", Type = VariableType.Input};
        var x3 = new Variable{Name = "X3", Type = VariableType.Input};
        var c1 = new Constraint
        {
            ConstraintType = ConstraintType.EqualTo,
            Variables = new List<ValueVariable>(new[]
            {
                x1.As(-2),
                x2.As(1),
                x3.As(3),
            }),
            ConstraintValue = 2
        };
        var c2 = new Constraint
        {
            ConstraintType = ConstraintType.EqualTo,
            Variables = new List<ValueVariable>(new[]
            {
                x1.As(2),
                x2.As(3),
                x3.As(4), 
            }),
            ConstraintValue = 1
        };
        var objectiveFunctions = new List<ValueVariable>(new[]
        {
            x1.As(1),
            x2.As(-2),
            x3.As(-3)
        });

        var linearModel = new LinearModel
        {
            Constraints = new List<Constraint>(new[] {c1, c2}),
            Objective = Objective.MIN,
            ObjectiveFunction = objectiveFunctions,
            Variables = new List<Variable>(new[] {x1, x2, x3})
        };

        var res = LinearModelSolver.Solve(linearModel);
        
        Assert.IsTrue(res.Result == SolvedResult.Infeasible);
        Assert.IsTrue(res.TwoPhasesIterations.Count == 2);
    }

    [Test] public void BuildLinearModelFromString()
    {
        var text = InputsResource.Linear1;
        Console.WriteLine(text);
        var model = LinearParser.Parse(InputsResource.Linear1);
        
        Assert.IsTrue(model.Objective == Objective.MAX);
        Assert.IsTrue(model.Variables.Count == 2);

        var x1 = model.Variables[0];
        var x2 = model.Variables[1];

        Assert.IsTrue(model.Constraints.Count == 3);

        var c1 = model.Constraints[0];
        var c2 = model.Constraints[1];
        var c3 = model.Constraints[2];

        Assert.IsTrue(c1.ConstraintValue == 4);
        Assert.IsTrue(c1.ConstraintType == ConstraintType.LessThanOrEqualTo);
        Assert.IsTrue(c1.Variables.Count == 1);
        Assert.IsTrue(c1.Variables[0].Variable == x1);
        Assert.IsTrue(c1.Variables[0].Coefficient == 1);
        
        Assert.IsTrue(c2.ConstraintValue == 12);
        Assert.IsTrue(c2.ConstraintType == ConstraintType.LessThanOrEqualTo);
        Assert.IsTrue(c2.Variables.Count == 1);
        Assert.IsTrue(c2.Variables[0].Variable == x2);
        Assert.IsTrue(c2.Variables[0].Coefficient == 2);

        Assert.IsTrue(c3.ConstraintValue == 18);
        Assert.IsTrue(c3.ConstraintType == ConstraintType.LessThanOrEqualTo);
        Assert.IsTrue(c3.Variables.Count == 2);
        Assert.IsTrue(c3.Variables[0].Variable == x1);
        Assert.IsTrue(c3.Variables[0].Coefficient == 3);
        Assert.IsTrue(c3.Variables[1].Variable == x2);
        Assert.IsTrue(c3.Variables[1].Coefficient == 2);

        var objFunc = model.ObjectiveFunction;
        
        Assert.IsTrue(objFunc[0].Variable == x1);
        Assert.IsTrue(objFunc[0].Coefficient == 3);
        Assert.IsTrue(objFunc[1].Variable == x2);
        Assert.IsTrue(objFunc[1].Coefficient == 5);
        
        Assert.Pass();
    }

    [Test] public void LinearFromString()
    {
        var inputString =@"MAX Z = 3X1 + 5X2
X1 <= 4
2X2 <= 12
3X1 + 2X2 <= 18";

        var model = LinearParser.Parse(inputString);
        var solvedModel = LinearModelSolver.Solve(model);
        
        Assert.AreEqual(SolvedResult.Optimized, solvedModel.Result);
        Assert.AreEqual(new BigRational(36,1), solvedModel.Z);
    }
    
    [Test] public void LinearFromString2()
    {
        var inputString =@"MAX Z = X1 + X2
3X1 + 2X2 <= 20
2X1 + 3X2 <= 20
X1 + 2X2 >= 2";

        var model = LinearParser.Parse(inputString);
        var solvedModel = LinearModelSolver.Solve(model);
        
        Assert.AreEqual(SolvedResult.Optimized, solvedModel.Result);
        Assert.AreEqual(new BigRational(8,1), solvedModel.Z);
    }
    
    [Test] public void LinearFromString3()
    {
        var inputString =@"MIN Z = -1X1 + 2X2 - 3X3
X1 + X2 + X3 = 6
-1X1 + X2 + 2X3 = 4
2X2 + 3X3 = 10
X3 <= 2";

        var model = LinearParser.Parse(inputString);
        var solvedModel = LinearModelSolver.Solve(model);
        
        Assert.AreEqual(SolvedResult.Optimized, solvedModel.Result);
        Assert.AreEqual(new BigRational(-4,1), solvedModel.Z);
    }
    
    [Test] public void LinearFromString4()
    {
        var inputString =@"MAX Z = 3X1 + X2 + 3X3
2X1 + X2 + X3 <= 2
X1 + 2X2 + 3X3 <= 5
2X1 + 2X2 + X3 <= 6";

        var model = LinearParser.Parse(inputString);
        var solvedModel = LinearModelSolver.Solve(model);
        
        Assert.AreEqual(SolvedResult.Optimized, solvedModel.Result);
        Assert.AreEqual(new BigRational(54,10), solvedModel.Z);
    }
    
    [Test] public void LinearFromString5()
    {
        var inputString =@"MAX Z = X1 + X2
1.5X1 + 1.5X2 <= 3";

        var model = LinearParser.Parse(inputString);
        var solvedModel = LinearModelSolver.Solve(model);
        
        Assert.AreEqual(SolvedResult.Optimized, solvedModel.Result);
        Assert.AreEqual(new BigRational(2,1), solvedModel.Z);
    }
}