using System;
using System.Threading;
using UnityEngine;

namespace UnityPatterns.Extensions
{
    public static class TaskExts
    {
        public static bool IsCanceled(
            this CancellationToken cancellationToken, 
            string logTag = nameof(TaskExts),
            Action<CancellationToken> onCancel = null    
        )
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"[{logTag}] Get async asset was CANCELED");
                }
                
                onCancel?.Invoke(cancellationToken);

                return true;
            }

            return false;
        }
    }
}