using Developist.Core.Utilities;

using System;
using System.Threading;

namespace HR.PersonalCalendar.Extensions
{
    /// <summary>
    /// Syntactic sugar for <see cref="SemaphoreSlim"/>.
    /// </summary>
    public static class SemaphoreSlimExtensions
    {
        /// <summary>
        /// Allows the semaphore to be used in a using statement.
        /// </summary>
        /// <param name="semaphore"></param>
        /// <returns></returns>
        public static IDisposable WaitAndRelease(this SemaphoreSlim semaphore)
        {
            semaphore.Wait();
            return new SemaphoreSlimReleaser(semaphore);
        }

        private class SemaphoreSlimReleaser : DisposableBase
        {
            private readonly SemaphoreSlim semaphore;
            public SemaphoreSlimReleaser(SemaphoreSlim semaphore) => this.semaphore = semaphore;
            protected override void ReleaseManagedResources()
            {
                semaphore.Release();
                base.ReleaseManagedResources();
            }
        }
    }
}
