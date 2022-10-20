using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    public static class GetAwaiterExtension_TargetTypeName
    {
        public static TaskAwaiterFor_TargetTypeName GetAwaiter(this TargetType value)
        {
            return new TaskAwaiterFor_TargetTypeName(value);
        }

        public readonly struct TaskAwaiterFor_TargetTypeName : ICriticalNotifyCompletion, INotifyCompletion
        {
            private readonly TargetType _value;

            public bool IsCompleted { get; } = true;

            public TaskAwaiterFor_TargetTypeName(TargetType value)
            {
                _value = value;
            }

            public TargetType GetResult()
            {
                return _value;
            }

            public void OnCompleted(Action continuation)
            {
                continuation();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                continuation();
            }
        }
    }
}