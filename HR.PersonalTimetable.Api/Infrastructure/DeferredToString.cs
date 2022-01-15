using System;

namespace HR.PersonalTimetable.Api.Infrastructure
{
    internal class DeferredToString
    {
        private readonly Lazy<string> initializer;
        public DeferredToString(Func<string> toString) => initializer = new Lazy<string>(toString);
        public override string ToString() => initializer.Value;
    }
}
