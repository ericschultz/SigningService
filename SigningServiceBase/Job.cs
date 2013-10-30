using System;

namespace SigningServiceBase
{
    public class Job
    {
        public Action PrefixedAction { get; set; }
        public Action Action { get; set; }
        public Action FailedAction { get; set; }
        public Action PostFixedAction { get; set; }
    }
}