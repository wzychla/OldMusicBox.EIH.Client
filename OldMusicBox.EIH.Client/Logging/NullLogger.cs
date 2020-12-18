using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Logging
{
    /// <summary>
    /// Null Logger
    /// </summary>
    public class NullLogger : AbstractLogger
    {
        public override bool ShouldDebug(Event evnt)
        {
            return true;
        }

        public override void Debug(string Message)
        {
        }

        public override void Error(string Message, Exception ex)
        {
        }
    }
}
