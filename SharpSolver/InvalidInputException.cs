using System;

namespace SharpSolver
{
    public class InvalidInputException : Exception
    {
        public int ErrorLine { get; set; }
        public string[] ParsedInput { get; set; }
        
        public InvalidInputException(string errorMessage, int errorLine, string[] parsedInput) : base(errorMessage)
        {
            ErrorLine = errorLine;
            ParsedInput = parsedInput;
        }

        public InvalidInputException(string errorMessage, int errorLine, string[] parsedInput, Exception inner) : base(errorMessage, inner)
        {
            ErrorLine = errorLine;
            ParsedInput = parsedInput;
        }
    }
}