#if ARCH_UNITY_VCONTAINER_SUPPORT
using System;
using Arch.Unity.Toolkit;
using VContainer;
using VContainer.Internal;

namespace Arch.Unity
{
	internal sealed class SystemRegistrationBuilder : RegistrationBuilder
	{
		internal SystemRegistrationBuilder(Type implementationType, ISystemRunner systemRunner) : base(implementationType, default)
		{
			this.systemRunner = systemRunner;
		}

		readonly ISystemRunner systemRunner;

		public override Registration Build()
		{
			var injector = InjectorCache.GetOrBuild(ImplementationType);

			var parameters = new object[]
			{
				injector,
				systemRunner,
				Parameters
			};

			Type type = typeof(SystemInstanceProvider<>).MakeGenericType(ImplementationType);
			var provider = (IInstanceProvider)Activator.CreateInstance(type, parameters);
			return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
		}
	}
}
#endif