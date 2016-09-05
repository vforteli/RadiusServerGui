using log4net.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;
using log4net;
using System.Collections.Concurrent;

namespace Flexinets.Radius
{
    public class DuplicateMessageThrottleFilter : FilterSkeleton
    {
        private ConcurrentDictionary<String, String> _previousList = new ConcurrentDictionary<string, string>();
        private Int32 repeatCount = 0;

        public override void ActivateOptions()
        {
            base.ActivateOptions();
        }

        public override FilterDecision Decide(LoggingEvent loggingEvent)
        {
            if (loggingEvent.LoggerName == "LogThrottler")
            {
                return FilterDecision.Neutral;

            }
            //private Int32 repeatThreshold = (string)loggingEvent.Properties["UserId"];
            var message = loggingEvent.MessageObject.ToString();

            String previousMessage;
            if (_previousList.TryGetValue(loggingEvent.LoggerName, out previousMessage))
            {
                if (previousMessage.Equals(message))
                {
                    var _log = LogManager.GetLogger("LogThrottler");

                    _log.Info($"Previous message repeated {repeatCount} times");
                    return FilterDecision.Deny;

                }
            }


            //if (repeatCount > 0)
            //{
            //    ILog _log = LogManager.GetLogger(loggingEvent.LoggerName);
            //    _log.Info($"Previous message repeated {repeatCount} times");
            //}

            _previousList.AddOrUpdate(loggingEvent.LoggerName, message, (s, i) => message);
            return FilterDecision.Neutral;
        }
    }
}
