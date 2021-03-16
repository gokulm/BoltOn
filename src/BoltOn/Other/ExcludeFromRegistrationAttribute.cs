using System;
namespace BoltOn.Other
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class ExcludeFromRegistrationAttribute : Attribute
	{
	}
}
