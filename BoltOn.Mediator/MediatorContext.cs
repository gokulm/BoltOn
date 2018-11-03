using System;
using System.Collections.Generic;
using System.Transactions;

namespace BoltOn.Mediator
{
    public class MediatorContext
    {
        public IsolationLevel DefaultIsolationLevel { get; set; }
        public IsolationLevel DefaultCommandIsolationLevel { get; set; }
        public IsolationLevel DefaultQueryIsolationLevel { get; set; }
		public TimeSpan? DefaultTransactionTimeout { get; set; }
    }
}
