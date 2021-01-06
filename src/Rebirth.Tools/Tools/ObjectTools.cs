using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Common.Tools
{
	public static class ObjectTools
	{
		// TODO untested, verify
		// From: https://stackoverflow.com/questions/6553183/check-to-see-if-a-given-object-reference-or-value-type-is-equal-to-its-default?lq=1
		public static bool IsNullOrDefault<T>(this T argument)
		{
			// deal with normal scenarios
			if (argument == null) return true;
			if (object.Equals(argument, default(T))) return true;

			// deal with non-null nullables
			Type methodType = typeof(T);
			if (Nullable.GetUnderlyingType(methodType) != null) return false;

			// deal with boxed value types
			Type argumentType = argument.GetType();
			if (argumentType.IsValueType && argumentType != methodType)
			{
				object obj = Activator.CreateInstance(argument.GetType());
				return obj.Equals(argument);
			}

			return false;
		}
	}
}
