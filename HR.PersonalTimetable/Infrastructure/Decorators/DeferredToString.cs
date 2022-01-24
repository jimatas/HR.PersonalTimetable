using System;

namespace HR.PersonalTimetable.Infrastructure.Decorators
{
    internal class DeferredToString
    {
        private readonly Lazy<string> initializer;
        public DeferredToString(Func<string> toString) => initializer = new Lazy<string>(toString);
        public override string ToString() => initializer.Value;
    }
}
