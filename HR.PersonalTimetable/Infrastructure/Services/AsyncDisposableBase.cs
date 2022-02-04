using Developist.Core.Utilities;

using System;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Infrastructure.Services
{
    public class AsyncDisposableBase : DisposableBase, IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called to asynchronously dispose of managed resources.
        /// </summary>
        /// <returns>An awaitable value task representing the asynchronous operation.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (IsDisposed)
            {
                return;
            }

            await ReleaseManagedResourcesAsync().ConfigureAwait(false);

            IsDisposed = true;
        }

        /// <summary>
        /// Override to asynchronously release any managed resources held by this instance.
        /// </summary>
        /// <returns>An awaitable value task representing the asynchronous operation.</returns>
        protected virtual ValueTask ReleaseManagedResourcesAsync() => default;
    }
}
