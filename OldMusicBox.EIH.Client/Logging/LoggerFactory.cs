using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Logging
{
    /// <summary>
    /// Logging Local Factory with injectable provider
    /// </summary>
    public class LoggerFactory
    {
        private static Func<Type, AbstractLogger> _loggerProvider;

        static LoggerFactory()
        {
            _loggerProvider = (type) => new NullLogger();
        }

        public static void SetProvider( Func<Type, AbstractLogger> loggerProvider )
        {
            if ( loggerProvider != null )
            {
                _loggerProvider = loggerProvider;
            }
            else
            {
                _loggerProvider = (type) => new NullLogger();
            }
        }

        public AbstractLogger For(Type type)
        {
            if (_loggerProvider == null) throw new ArgumentNullException();

            return _loggerProvider(type);
        }

        public AbstractLogger For(object o)
        {
            if (_loggerProvider == null) throw new ArgumentNullException();
            if ( o == null ) throw new ArgumentNullException();

            return _loggerProvider(o.GetType());
        }
    }
}
