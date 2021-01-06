
namespace Rebirth.Provider.Exception
{
	/// <summary>
	/// Is usually thrown when an attempt to modify a locked template field has occurred.
	/// </summary>
	public class TemplateAccessException : System.Exception
	{
		public override string Message { get; }

		public TemplateAccessException() => Message = "Attempted to modify locked template field.";
		public TemplateAccessException(string exceptionMessage) => Message = exceptionMessage;
	}
}