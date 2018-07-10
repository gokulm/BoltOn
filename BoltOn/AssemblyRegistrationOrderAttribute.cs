using System;
namespace BoltOn
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyRegistrationOrderAttribute : Attribute
	{
		public int Order
		{
			get;
			private set;
		}

		public AssemblyRegistrationOrderAttribute(int order) 
		{ 
			Order = order; 
		}
	}
}
