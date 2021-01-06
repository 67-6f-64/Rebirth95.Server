using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WzTools.Objects;

namespace Rebirth.Provider.WzToolsEx
{
	public static class WzPropEx
	{
		public static object GetImgPropVal(this WzProperty prop, Type type, string attributeName)
		{
			var tc = Type.GetTypeCode(type);

			switch (tc)
			{
				case TypeCode.Boolean:
					return prop.GetInt32(attributeName) != 0;
				case TypeCode.Byte:
					return prop.GetInt8(attributeName);
				case TypeCode.Int16:
					return prop.GetInt16(attributeName);
				case TypeCode.Int32:
					if (prop.GetAllChildren().Count > 0) return 0;

					return prop.GetInt32(attributeName);
				case TypeCode.Int64:
					return prop.GetInt64(attributeName);
				case TypeCode.String:
					return prop.GetString(attributeName);

				case TypeCode.Object:
					switch (type.Name)
					{
						case "Point":
							{
								if (prop[attributeName] is WzVector2D vector)
								{
									return new Point(vector.X, vector.Y);
								}
								else
								{
									return new Point();
								}
							}
					}
					break;
				case TypeCode.Double:
					return Convert.ToDouble(prop.Get(attributeName));
			}

			throw new InvalidOperationException();
		}
	}
}
