using Newtonsoft.Json;

namespace Rebirth.Provider.Template
{
	/// <summary>
	/// Serves as the base class for all template game data objects.
	/// </summary>
	public abstract class AbstractTemplate
	{
		/// <summary>
		/// Indicates whether the object has been locked or not.
		/// </summary>
		[JsonIgnore]
		protected bool Locked { get; private set; }

		/// <summary>
		/// Locks template to prevent further modification.
		/// </summary>
		public virtual void LockTemplate() => Locked = true;

		/// <summary>
		/// ID for this object to be keyed against.
		/// </summary>
		[JsonProperty]
		public int TemplateId { get; }
		protected AbstractTemplate(int templateId) => TemplateId = templateId;
	}
}
