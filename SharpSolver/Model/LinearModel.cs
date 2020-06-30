using System;
using System.Collections.Generic;
using System.Linq;
using Numerics;

namespace SharpSolver
{
    public class LinearModel
    {
        public Objective Objective { get; set; }
        public List<Variable> Variables { get; set; } = new List<Variable>();
        public List<Constraint> Constraints { get; set; } = new List<Constraint>();
        public List<ValueVariable> ObjectiveFunction { get; set; } = new List<ValueVariable>();
    }

    #region Data

    public class Variable
    {
        public string Name;
        public VariableType Type;

        public ValueVariable As(BigRational coef)
        {
            return new ValueVariable {Variable = this, Coefficient = coef};
        }

        public static Variable Input(string name)
        {
            return new Variable {Name = name, Type = VariableType.Input};
        }
        
        public static Variable Slack(string name)
        {
            return new Variable {Name = name, Type = VariableType.Slack};
        }
        
        public static Variable Artificial(string name)
        {
            return new Variable {Name = name, Type = VariableType.Artificial};
        }
    }

    public enum VariableType
    {
        Input,
        Slack,
        Artificial
    }
    
    public class ValueVariable
    {
        public BigRational Coefficient { get; set; }
        public Variable Variable { get; set; }
    }

    public class Constraint
    {
        public string Name;
        public ConstraintType ConstraintType;
        public BigRational ConstraintValue;

        public List<ValueVariable> Variables = new List<ValueVariable>();

        public Constraint Copy()
        {
            return new Constraint
            {
                Name = Name,
                ConstraintType = ConstraintType,
                ConstraintValue = ConstraintValue,
                Variables = Variables.ToList(),
            };
        }

        public static Constraint LessThanOrEqualTo(string name, BigRational value, params ValueVariable[] vars)
        {
            return new Constraint
            {
                Name = name,
                ConstraintType = ConstraintType.LessThanOrEqualTo,
                ConstraintValue = value,
                Variables = vars.ToList()
            };
        }
        
        public static Constraint LessThanOrEqualTo(string name, BigRational value, IEnumerable<ValueVariable> vars)
        {
            return new Constraint
            {
                Name = name,
                ConstraintType = ConstraintType.LessThanOrEqualTo,
                ConstraintValue = value,
                Variables = vars.ToList()
            };
        }
        
        public static Constraint GreaterThanOrEqualTo(string name, BigRational value, params ValueVariable[] vars)
        {
            return new Constraint
            {
                Name = name,
                ConstraintType = ConstraintType.GreaterThanOrEqualTo,
                ConstraintValue = value,
                Variables = vars.ToList()
            };
        }
        
        public static Constraint GreaterThanOrEqualTo(string name, BigRational value, IEnumerable<ValueVariable> vars)
        {
            return new Constraint
            {
                Name = name,
                ConstraintType = ConstraintType.GreaterThanOrEqualTo,
                ConstraintValue = value,
                Variables = vars.ToList()
            };
        }
        
        public static Constraint EqualTo(string name, BigRational value, params ValueVariable[] vars)
        {
            return new Constraint
            {
                Name = name,
                ConstraintType = ConstraintType.EqualTo,
                ConstraintValue = value,
                Variables = vars.ToList()
            };
        }
        
        public static Constraint EqualTo(string name, BigRational value, IEnumerable<ValueVariable> vars)
        {
            return new Constraint
            {
                Name = name,
                ConstraintType = ConstraintType.EqualTo,
                ConstraintValue = value,
                Variables = vars.ToList()
            };
        }
    }

    public enum ConstraintType
    {
        LessThanOrEqualTo,
        GreaterThanOrEqualTo,
        EqualTo
    }

    public enum Objective
    {
        MAX,
        MIN
    }

    #endregion
}