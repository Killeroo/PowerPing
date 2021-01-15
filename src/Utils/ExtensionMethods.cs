/*
MIT License - PowerPing 

Copyright (c) 2021 Matthew Carney

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
