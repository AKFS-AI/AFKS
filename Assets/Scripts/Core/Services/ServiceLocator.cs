using System;
using System.Collections.Generic;

namespace AFKS.Core.Services
{
	/// <summary>
	/// 서비스 등록/조회 단일 진입점. Core 상주 오브젝트 초기화 시 등록한다.
	/// </summary>
	public static class ServiceLocator
	{
		private static readonly Dictionary<Type, object> typeToService = new Dictionary<Type, object>();

		public static void Register<TService>(TService instance) where TService : class
		{
			var type = typeof(TService);
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (typeToService.ContainsKey(type))
			{
				typeToService[type] = instance;
				return;
			}
			typeToService.Add(type, instance);
		}

		public static TService Get<TService>() where TService : class
		{
			var type = typeof(TService);
			if (typeToService.TryGetValue(type, out var service))
			{
				return (TService)service;
			}
			throw new InvalidOperationException($"Service not registered: {type.Name}");
		}

		public static bool TryGet<TService>(out TService instance) where TService : class
		{
			if (typeToService.TryGetValue(typeof(TService), out var service))
			{
				instance = (TService)service;
				return true;
			}
			instance = null;
			return false;
		}

		public static void Reset()
		{
			typeToService.Clear();
		}
	}
}
