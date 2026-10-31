using System;

namespace PieceOfCake.Core
{
    /// <summary>Thrown when level data is malformed. The message names the stage and the problem.</summary>
    public sealed class LevelFormatException : Exception
    {
        public LevelFormatException(string message) : base(message)
        {
        }
    }
}
