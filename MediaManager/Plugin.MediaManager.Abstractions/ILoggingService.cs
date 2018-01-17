using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.MediaManager.Abstractions
{
    public interface ILoggingService
    {
        void Debug(string msg);
        void Warn(string msg, Exception ex = null);
        void Info(string msg);
        void Error(string msg, Exception ex);
    }
}
