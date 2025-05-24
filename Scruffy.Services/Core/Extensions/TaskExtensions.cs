using System.Runtime.CompilerServices;

namespace Scruffy.Services.Core.Extensions;

/// <summary>
/// <see cref="Task"/>  extension methods
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Marking that there is no need to wait for the task
    /// </summary>
    /// <param name="task">Task</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forget(this Task task)
    {
    }
}