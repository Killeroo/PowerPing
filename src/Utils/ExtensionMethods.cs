/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2024 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing 
 *************************************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPing
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Waits for a task to complete and returns its value. Similar to calling task.Result which
        /// will also wait if it needs to, but this method allows for use of a cancellation token.
        /// </summary>
        /// <param name="task">The task to wait on.</param>
        /// <param name="cancellationToken">The cancellation token to signal if the wait should end early.</param>
        /// <returns>The task's result.</returns>
        public static T WaitForResult<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            try {
                task.Wait(cancellationToken);
            } catch (AggregateException ex) when (ex.InnerExceptions.Count == 1) {
                // Wait wraps everything in an AggregateException, so we'll unwrap it here to give the
                // caller the orignal exception. There should only be one InnerException since we only
                // waited on one task, but the validation is just to be safe.
                throw ex.InnerExceptions[0];
            }
            return task.Result;
        }
    }
}
