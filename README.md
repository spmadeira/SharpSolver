# SharpSolver

Simple linear expression solver built using C#.
Project SharpSolver implements solver logic, SharpSolverUI and SharpSolverUI.Avalonia implement the solver application GUI.

This application uses:
- [BigRational](https://github.com/AdamWhiteHat/BigRational) for full precision calculations.
- [NUnit](https://github.com/nunit/nunit) for unit testing.
- [WPF](https://github.com/dotnet/wpf) for Windows UI.
- [Avalonia](https://github.com/AvaloniaUI/Avalonia) for cross-platform GTK-based UI.

## Using the Standalone solver library

You can import only the SharpSolver functionality library by importing the SharpSolver NuGet package by running `dotnet add package SharpSolver`.

### Solving by Linear Model

You can generate a `SolvedModel` from a `LinearModel` by calling the static `Solve` method from `LinearModelSolver`.

Eg.
```csharp
SolvedModel solvedModel = LinearModelSolver.Solve(inputModel);
```

### Solving by string input
You can generate a `LinearModel` from a string input by using the `LinearParser` class. It expects a strictly defined model with an objective declaration (Min Z / Max Z) on the first line and a series of constraints defined on each new line after that.

```csharp
string inputString = @"MAX Z = 3X1 + 5X2
X1 <= 4
2X2 <= 12
3X1 + 2X2 <= 18";

LinearModel linearModel = LinearParser.Parse(inputString);
SolvedModel solvedModel = LinearModelSolver.Solve(model);

//Outputs "Result: 36"
Console.WriteLine($"Result: {solvedModel.Z.ToFractionString()}")
```

You may also write a custom wrapper that receives a string and returns a `LinearModel` if the default `LinearParser` is not sufficient.

### Creating a Linear Model

A `LinearModel` is composed of:

- An `Objective` (Maximize, Minimze)
- A list of `Variables`
- A list of `Constraints`
- An objecive function (a list of `ValueVariables`)

The following is an example of how to build a `LinearModel` equivalent to the previous example's string.

```csharp
Variable X1 = Variable.Input("X1");
Variable X2 = Variable.Input("X2");

Constraint C1 = Constraint.LessThanOrEqualTo("C1", 4, X1.As(1));
Constraint C2 = Constraint.LessThanOrEqualTo("C2", 12, X2.As(2));
Constraint C3 = Constraint.LessThanOrEqualTo("C3", 18, X1.As(3),X2.As(2));
//Alternatively, receiving an IEnumerable<ValueVariable>
//Constraint C3 = Constraint.LessThanOrEqualTo("C3", 18, new []{X1.As(3), X2.As(2)});

List<ValueVariable> ObjectiveFunction = new []{X1.As(3), X2.As(5)}.ToList();
LinearModel linearModel = new LinearModel{
    Objective = Objective.MAX,
    Variables = new[]{X1,X2}.ToList(),
    Constraints = new[]{C1,C2,C3}.ToList(),
    ObjectiveFunction = ObjectiveFunction
};

SolvedModel solvedModel = LinearModelSolver.Solve(linearModel);

//Outputs "Result: 36"
Console.WriteLine($"Result: {solvedModel.Z.ToFractionString()}");
```
