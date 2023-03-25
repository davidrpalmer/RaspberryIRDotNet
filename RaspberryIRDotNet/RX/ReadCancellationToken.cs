using System;
using System.Threading;

namespace RaspberryIRDotNet.RX
{
    public class ReadCancellationToken
    {
        private int _refCount = 0;

        private readonly object _locker = new object();

        private readonly object _waitLocker = new object();

        private ManualResetEventSlim _zeroRefWaiter;

        /// <summary>
        /// Event raised when cancellation is requested. Note that if cancellation has already been requested then this won't be raised.
        /// </summary>
        internal event EventHandler CancellationRequested;

        public bool IsCancellationRequested { get; private set; } // Can read without a lock, but must hold _locker to set.

        public ReadCancellationToken()
        {
        }

        public ReadCancellationToken(TimeSpan cancelAfter)
        {
            CancelAfter(cancelAfter);
        }

        /// <param name="wait">Automatically call <see cref="Wait"/> after requesting cancellation.</param>
        public void Cancel(bool wait = false)
        {
            bool raiseEvent;
            lock (_locker)
            {
                raiseEvent = !IsCancellationRequested; // Only raise the event for the first cancel.
                IsCancellationRequested = true;
            }
            if (raiseEvent)
            {
                CancellationRequested?.Invoke(this, EventArgs.Empty);
            }

            if (wait)
            {
                Wait();
            }
        }

        public void CancelAfter(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }
            System.Threading.Tasks.Task.Delay(timeout).ContinueWith((t) => Cancel(wait: false));
        }

        /// <summary>
        /// Wait until the operation has been cancelled. Depending on usage, this may guarantee that the LIRC device has been closed, or maybe only that the read operation has ended.
        /// This is only valid when <see cref="IsCancellationRequested"/> is TRUE.
        /// </summary>
        /// <exception cref="InvalidOperationException">The method was called before cancellation was requested.</exception>
        public void Wait()
        {
            if (!IsCancellationRequested)
            {
                throw new InvalidOperationException("Cannot wait until cancellation has been requested.");
            }

            lock (_waitLocker)
            {
                if (_refCount <= 0)
                {
                    return;
                }

                _zeroRefWaiter.Wait(); // Note only one waiter will actually wait here, any others will wait at the lock.
            }
        }

        /// <returns>
        /// TRUE if a reference was added, FALSE if cancellation has been requested.
        /// </returns>
        internal bool AddReference()
        {
            lock (_locker)
            {
                if (IsCancellationRequested)
                {
                    return false;
                }

                if (_refCount == 0)
                {
                    lock (_waitLocker)
                    {
                        _zeroRefWaiter = new ManualResetEventSlim();
                        _refCount++;
                    }
                }
                else
                {
                    _refCount++;
                }
            }

            return true;
        }

        internal void ReleaseReference()
        {
            lock (_locker)
            {
                if (_refCount <= 0)
                {
                    throw new InvalidOperationException("Too many releases.");
                }

                if (_refCount == 1)
                {
                    _zeroRefWaiter.Set(); // Set this first so everyone waiting now and in the future gets released first, so we can get the _waitLocker ourselves.

                    lock (_waitLocker)
                    {
                        _refCount = 0;
                        _zeroRefWaiter.Dispose();
                        _zeroRefWaiter = null;
                    }
                }
                else
                {
                    _refCount--;
                }
            }
        }
    }
}
