using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace GameMaster
{
    public class GMLogger
    {
        private Logger logger =  LogManager.GetLogger("Logger");
        private NLog.Targets.MemoryTarget memoryTarget;
        private int logsRead = 0;

        public GMLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            memoryTarget = new NLog.Targets.MemoryTarget();
            memoryTarget.Layout = "${time} [${level:}] - ${message}";
            config.AddRuleForAllLevels(memoryTarget);
            LogManager.Configuration = config;
        }

        public Logger Get()
        {
            return logger;
        }

        public List<string> GetPendingLogs()
        {
            var logs = new List<string>(memoryTarget.Logs);
            logs = logs.TakeLast(logs.Count - logsRead).ToList();
            logsRead += logs.Count;
            return logs;
        }
    }
}
