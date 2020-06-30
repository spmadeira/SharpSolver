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

You can generate a `SolvedModel` from a `LinearModel` by calling the static `Solve` from `LinearModelSolver`

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