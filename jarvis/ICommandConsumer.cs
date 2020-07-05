using System;
using System.Collections.Immutable;

namespace jarvis {
    interface ICommandConsumer {
        void Consume(String command, Object context);
        ImmutableHashSet<String> GetGrammar();
    }
}
