using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model.Logout
{
    /// <summary>
    /// Logout request exception
    /// </summary>
    public class LogoutRequestException : Exception
    {
        public LogoutRequestException() : base() { }

        public LogoutRequestException(string message) : base(message) { }

        public LogoutRequestException(string message, Exception inner) : base(message, inner) { }
    }
}
