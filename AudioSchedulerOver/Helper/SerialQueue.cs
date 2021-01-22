using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.Helper
{
    public interface ISerialQueue
    {
        Task Enqueue(Action action);
        Task<T> Enqueue<T>(Func<T> function);
        Task Enqueue(Func<Task> asyncAction);
        Task<T> Enqueue<T>(Func<Task<T>> asyncFunction);
    }

    public class SerialQueue : ISerialQueue
    {
        readonly static object _locker = new object();

        WeakReference<Task> _lastTask;

        public Task Enqueue(Action action)
        {
            return Enqueue(() =>
            {
                action();
                return true;
            });
        }

        public Task<T> Enqueue<T>(Func<T> function)
        {
            lock (_locker)
            {
                Task<T> resultTask = null;

                if (_lastTask != null && _lastTask.TryGetTarget(out Task lastTask))
                {
                    resultTask = lastTask.ContinueWith(_ => function(), TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                {
                    resultTask = Task.Run(function);
                }

                _lastTask = new WeakReference<Task>(resultTask);
                return resultTask;
            }
        }

        public Task Enqueue(Func<Task> asyncAction)
        {
            lock (_locker)
            {
                Task resultTask = null;

                if (_lastTask != null && _lastTask.TryGetTarget(out Task lastTask))
                {
                    resultTask = lastTask.ContinueWith(_ => asyncAction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                }
                else
                {
                    resultTask = Task.Run(asyncAction);
                }

                _lastTask = new WeakReference<Task>(resultTask);
                return resultTask;
            }
        }

        public Task<T> Enqueue<T>(Func<Task<T>> asyncFunction)
        {
            lock (_locker)
            {
                Task<T> resultTask = null;

                if (_lastTask != null && _lastTask.TryGetTarget(out Task lastTask))
                {
                    resultTask = lastTask.ContinueWith(_ => asyncFunction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                }
                else
                {
                    resultTask = Task.Run(asyncFunction);
                }

                _lastTask = new WeakReference<Task>(resultTask);
                return resultTask;
            }
        }
    }
}
