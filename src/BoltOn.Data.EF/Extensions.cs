using System;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public static class Extensions
	{
		public static BootstrapperOptions BoltOnEFModule(this BootstrapperOptions bootstrapperOptions)
		{
			bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return bootstrapperOptions;
		}

		/// <summary>
		/// Add all the configurations that are in the same namespace as type T
		/// </summary>
		public static ModelBuilder ApplyConfigurationsFromNamespaceOfType<T>(this ModelBuilder modelBuilder)
		{
			var assembly = typeof(T).Assembly;
			var typeNamespace = typeof(T).Namespace;
			var entityTypeConfigurationType = typeof(IEntityTypeConfiguration<>);
			var mappings = (from t in assembly.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType && t.Namespace == typeNamespace && !t.IsAbstract && !t.IsInterface &&
									 entityTypeConfigurationType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { MappingClass = t, MappedEntity = i.GetGenericArguments()[0] }).ToList();
			var applyConfigurationMethod = (from m in typeof(ModelBuilder).GetMethods()
											where m.Name == nameof(modelBuilder.ApplyConfiguration)
												 && m.IsGenericMethod
												 && m.GetParameters().Any(p => p.ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
											select m).First();

			foreach (var mapping in mappings)
			{
				var genericMethod = applyConfigurationMethod.MakeGenericMethod(mapping.MappedEntity);
				genericMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(mapping.MappingClass) });
			}
			return modelBuilder;
		}
	}
}
