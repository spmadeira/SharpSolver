using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Numerics;
using SharpSolver;

public static class LinearParser
{
    private static readonly Regex VarRegex = new Regex(@"(\-?\d*\.?\d*)?([X]\d+)");
    private static readonly Regex ConstraintRegex = new Regex(@"(<?>?=?=)(-?\d+)");
    private static readonly Regex WsCleaner = new Regex(@"\s+");
    
    //Parse string into a linear model.
    public static LinearModel Parse(string model)
    {
        LinearModel Model = new LinearModel();
        
        string[] cleanedInput = CleanInput(model);
        
        var declarativeLine = cleanedInput[0];
        
        if (declarativeLine.StartsWith("MAXZ="))
            Model.Objective = Objective.MAX;
        else if (declarativeLine.StartsWith("MINZ="))
            Model.Objective = Objective.MIN;
        else
            throw new InvalidInputException("Invalid or undeclared objective.", 0, cleanedInput);
        
        var decMatches = VarRegex.Matches(declarativeLine);
        
        Dictionary<string, Variable> declaredVariables = new Dictionary<string, Variable>();

        foreach (Match match in decMatches)
        {
            try
            {
                var name = match.Groups[2].Value;
                var valueMatch = match.Groups[1].Value;
                BigRational value;
                if (valueMatch == "-") value = -1;
                else
                {
                    var hasValue = decimal.TryParse(valueMatch, out var decimalValue);
                    if (!hasValue) value = 1;
                    else value = decimalValue;
                }
        
                Variable var = Variable.Input(name);
                declaredVariables[name] = var;
                ValueVariable valVar = var.As(value);
                Model.Variables.Add(var);
                Model.ObjectiveFunction.Add(valVar);
            }
            catch (Exception e)
            {
                throw new InvalidInputException("Invalid variable declaration.", 0, cleanedInput, e);
            }  
        }
            
        
        for (int i = 1; i < cleanedInput.Length; i++)
        {
            var constraintLine = cleanedInput[i];
            
            var variables = new List<ValueVariable>();
            var vars = VarRegex.Matches(constraintLine);

            foreach (Match match in vars)
            {
                try
                {
                    var name = match.Groups[2].Value;
                    var valueMatch = match.Groups[1].Value;
                    BigRational value;
                    if (valueMatch == "-") value = -1;
                    else
                    {
                        var hasValue = decimal.TryParse(valueMatch, out var decimalValue);
                        if (!hasValue) value = 1;
                        else value = decimalValue;
                    }

                    if (declaredVariables.TryGetValue(name, out var variable))
                    {
                        variables.Add(variable.As(value));
                    }
                    else
                    {
                        throw new InvalidInputException($"Undeclared variable {name}", i, cleanedInput);
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidInputException($"Invalid variable declaration.", i, cleanedInput, e);
                }
            }
            
            Constraint ct;
            try
            {
                var constraint = ConstraintRegex.Match(constraintLine);
                var constraintSign = constraint.Groups[1].Value;
                var constraintValueMatch = constraint.Groups[2].Value;
                var constraintValue = decimal.Parse(constraintValueMatch);

                switch (constraintSign)
                {
                    case ">=":
                        ct = Constraint.GreaterThanOrEqualTo($"CT{i}", constraintValue, variables);
                        break;
                    case "<=":
                        ct = Constraint.LessThanOrEqualTo($"CT{i}",constraintValue,variables);
                        break;
                    case "=":
                    case "==":
                        ct = Constraint.EqualTo($"CT{i}",constraintValue,variables);
                        break;
                    default:
                        throw new InvalidInputException("Invalid constraint value", i, cleanedInput);
                }
            }
            catch (Exception e)
            {
                throw new InvalidInputException("Invalid constraint declaration", i, cleanedInput, e);
            }
            Model.Constraints.Add(ct);
        }
        
        return Model;
    }
    
    //Separate raw string input into a cleaned string array without whitespace and newline.
    private static string[] CleanInput(string input)
    {
        var lines = new List<string>();

        var inputLines = input.ToUpper().Split('\n');

        foreach (var inputLine in inputLines)
        {
            //Remove whitespace
            var l = WsCleaner.Replace(inputLine, "");
            if (string.IsNullOrWhiteSpace(inputLine)) continue;
            lines.Add(l);
        }

        return lines.ToArray();
    }
}
