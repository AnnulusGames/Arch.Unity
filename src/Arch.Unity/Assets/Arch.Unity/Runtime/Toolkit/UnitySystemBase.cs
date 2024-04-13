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

        internal bool TryInitialize()
        {
            if (isInitialized) return false;

            Initialize();

            isInitialized = true;
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}