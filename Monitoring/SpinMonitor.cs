using System;
using System.Threading;

namespace PubComp.Aspects.Monitoring
{   
    /// <summary>
    /// A thread re-enterable synchronization monitor based on a spin-lock (busy wait)
    /// </summary>
    /// <remarks>
    /// This synchronization method causes busy waiting and should therefore only be used for short-running code.
    /// 
    /// This synchronization method should only be used for synchronizing
    /// short-running, non-blocking, non-I/O methods that do not wait for other threads.
    ///
    /// This synchronization method is not detectable using standard deadlock detection
    /// and therefore should only be used for synchronizing code that does not include other synchronization methods
    /// e.g. locks, Monitors, Mutexes, Semaphores, Signals, await.
    /// 
    /// This class is thread safe.
    /// </remarks>
    public class SpinMonitor
    {
        private long threadId;
        private long entries;

        private bool IsZeroOrCurrentThreadId(long value, long currentThreadId)
        {
            // Value will equal 0L if ownership was newly obtained,
            // value will equal currentThreadId if resource was previously obtained by thread (if entry depth > 1),
            // otherwise, value will equal to threadId of the other thread that obtained ownership
            return (value == 0L || value == currentThreadId);
        }

        /// <summary>
        /// Enter the monitor
        /// </summary>
        /// <remarks>
        /// InMonitor() is preferable for usage.
        /// Exit() must be called, preferably in a finally statement</remarks>
        /// This method is thread safe.
        /// <example>
        /// spinMonitor.Enter();
        /// try
        /// {
        ///     MyMethod(a, b, c);
        /// }
        /// finally
        /// {
        ///     spinMonitor.Exit();
        /// }
        /// </example>
        public void Enter()
        {
            long currentThreadId = Thread.CurrentThread.ManagedThreadId;

            // Wait until this thread obtains ownership
            while (!IsZeroOrCurrentThreadId(Interlocked.CompareExchange(ref threadId, currentThreadId, 0), currentThreadId))
            {
            }

            // Increase entry count (for current thread)
            Interlocked.Increment(ref entries);
        }

        /// <summary>
        /// Exit the monitor
        /// </summary>
        /// <remarks>
        /// InMonitor() is preferable for usage.
        /// Call after Enter(), preferable in a finally statement
        /// This method is thread safe.
        /// </remarks>
        /// <example>
        /// spinMonitor.Enter();
        /// try
        /// {
        ///     MyMethod(a, b, c);
        /// }
        /// finally
        /// {
        ///     spinMonitor.Exit();
        /// }
        /// </example>
        public void Exit()
        {
            // Decrease entry count (for current thread)
            if (Interlocked.Decrement(ref entries) <= 0)
            {
                Interlocked.Exchange(ref entries, 0);

                // If thread has exited all entries, then remove ownership
                Interlocked.Exchange(ref threadId, 0);

                // Other threads can no obtain ownership
            }
        }

        /// <summary>
        /// Performs an action within the monitor
        /// </summary>
        /// <param name="action"></param>
        /// <remarks>This method is thread safe</remarks>
        /// <example>
        /// spinMonitor.InMonitor(() => myMethod(a, b, c));
        /// </example>
        public void InMonitor(Action action)
        {
            Enter();
            try
            {
                action();
            }
            finally
            {
                Exit();
            }
        }

        /// <summary>
        /// Performs a function within the monitor
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns>The result of the function</returns>
        /// <remarks>This method is thread safe</remarks>
        /// <example>
        /// var result = spinMonitor.InMonitor(() => myMethod(a, b, c));
        /// </example>
        public TResult InMonitor<TResult>(Func<TResult> func)
        {
            Enter();
            try
            {
                return func();
            }
            finally
            {
                Exit();
            }
        }
    }
}
