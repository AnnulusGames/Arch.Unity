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

        public override void Dispose()
        {
            base.Dispose();
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}