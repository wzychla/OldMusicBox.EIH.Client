using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Validation
{
    /// <summary>
    /// Validation exception
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException() : base() { }

        public ValidationException(string message) : base(message) { }

        public ValidationException(string message, Exception inner) : base( message, inner ) { }
    }
}
