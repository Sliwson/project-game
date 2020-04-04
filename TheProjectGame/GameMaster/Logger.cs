﻿using System;
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
            var layout = "${time} [${level:}] - ${ndc}${message}";

            memoryTarget = new NLog.Targets.MemoryTarget();
            memoryTarget.Layout = layout;

            var fileTarget = new NLog.Targets.FileTarget();
            fileTarget.FileName = "gm-log.txt";
            fileTarget.Layout = layout;

            config.AddRuleForAllLevels(memoryTarget);
            config.AddRuleForAllLevels(fileTarget);

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
