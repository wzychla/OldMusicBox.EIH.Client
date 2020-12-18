using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Logging
{
    /// <summary>
    /// Internal logging
    /// </summary>
    public abstract class AbstractLogger
    {
        public abstract bool ShouldDebug(Event evnt);

        public abstract void Debug(string Message);


        public void Debug(Event evnt, string Message)
        {
            if ( this.ShouldDebug( evnt ) )
            {
                this.Debug(string.Format( "{0}: {1}", evnt, Message));
            }
        }
        public abstract void Error(string Message, Exception ex);
    }
}
