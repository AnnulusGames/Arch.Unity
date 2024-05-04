using System.Threading;
using Arch.Core;
using Arch.System;

namespace Arch.Unity.Toolkit
{
    public abstract class UnitySystemBase : BaseSystem<World, SystemState>
    {
        protected UnitySystemBase(World world) : base(world) { }

        readonly CancellationTokenSource cancellationTokenSource = new();
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        bool isInitialized;
        bool isDisposed;

        internal bool TryInitialize()
        {
            if (isInitialized) return false;

            try
            {
                Initialize();
                return true;
            }
            finally
            {
                isInitialized = true;
            }
        }

        public override void Dispose()
        {
            if (isDisposed) return;

            try
            {
                base.Dispose();
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            finally
            {
                isDisposed = true;
            }
        }
    }
}