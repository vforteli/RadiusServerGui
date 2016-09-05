using Log4Slack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;

namespace Flexinets.Radius
{
    public class SlackThrottlingAppender : Log4Slack.SlackAppender
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            base.Append(loggingEvent);
        }
    }
}
