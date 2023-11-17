using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityPatterns.YieldInstructions
{
    public class WaitAsyncOperation<T> : CustomYieldInstruction
    {
        private Func<bool> predicate;
        public AsyncOperationHandle<T> OpHandle { get; set; }

        public override bool keepWaiting => predicate();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public WaitAsyncOperation(AsyncOperationHandle<T> opHandle)
        {
            OpHandle = opHandle;
            predicate = AwaitWhile;
        }

        public WaitAsyncOperation()
        {
            predicate = AwaitWhile;
        }

        public bool AwaitWhile()
        {
            return !OpHandle.IsDone && OpHandle.Status != AsyncOperationStatus.Succeeded;
        }
    }
}
