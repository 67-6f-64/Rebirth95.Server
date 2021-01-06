using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;
using Rebirth.Provider.Attribute;
using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.Reactor
{
	/// <summary>
	/// This contains the data that is shared between all reactors of the same ID.
	/// </summary>
	public sealed class ReactorTemplate : AbstractTemplate
	{
		private int _statecount;
		public int StateCount
		{
			get => _statecount;
			set => _statecount = !Locked ? value : throw new TemplateAccessException();
		}

		private int _hitdelay;
		public int HitDelay
		{
			get => _hitdelay;
			set => _hitdelay = !Locked ? value : throw new TemplateAccessException();
		}

		private bool _move;
		[ProviderProperty("info/move")]
		public bool Move
		{
			get => _move;
			set => _move = !Locked ? value : throw new TemplateAccessException();
		}

		private int _moveonce;
		[ProviderProperty("info/moveOnce")]
		public int MoveOnce
		{
			get => _moveonce;
			set => _moveonce = !Locked ? value : throw new TemplateAccessException();
		}

		private int _movedelay;
		[ProviderProperty("info/moveDelay")]
		public int MoveDelay
		{
			get => _movedelay;
			set => _movedelay = !Locked ? value : throw new TemplateAccessException();
		}

		private int _requiredhitcount;
		[ProviderProperty("info/hitCount")]
		public int RequiredHitCount
		{
			get => _requiredhitcount;
			set => _requiredhitcount = !Locked ? value : throw new TemplateAccessException();
		}

		private bool _removeinfieldset;
		[ProviderProperty("info/removeInFieldSet")]
		public bool RemoveInFieldSet
		{
			get => _removeinfieldset;
			set => _removeinfieldset = !Locked ? value : throw new TemplateAccessException();
		}

		private string _action;
		[ProviderProperty("action")]
		public string Action
		{
			get => _action;
			set => _action = !Locked ? value ?? "" : throw new TemplateAccessException();
		}

		[ProviderList]
		public List<StateInfo> StateInfoList { get; set; }

		//[ProviderProperty(""), ProviderList]
		public List<ActionInfo> ActionInfoList { get; set; } // TODO

		public ReactorTemplate(int templateId)
			: base(templateId)
		{
			StateInfoList = new List<StateInfo>();
			ActionInfoList = new List<ActionInfo>();

			Action = "";
		}

		public override void LockTemplate()
		{
			foreach (var item in StateInfoList)
			{
				item.LockTemplate();
			}

			foreach (var item in ActionInfoList)
			{
				item.LockTemplate();
			}

			base.LockTemplate();
		}

		[ProviderClass]
		public class StateInfo
		{
			[JsonIgnore]
			public bool Locked { get; private set; }
			public void LockTemplate() => Locked = true;

			private int _hitDelay;
			public int HitDelay
			{
				get => _hitDelay;
				set => _hitDelay = !Locked ? value : throw new TemplateAccessException();
			}

			private int _timeOut;
			[ProviderProperty("timeOut")]
			public int TimeOut
			{
				get => _timeOut;
				set => _timeOut = !Locked ? value : throw new TemplateAccessException();
			}

			[ProviderProperty(""), ProviderList]
			public List<EventInfo> EventInfos { get; set; } = new List<EventInfo>();


			[ProviderClass("event")] // Reactor.wz/0000.img/x/event
			public sealed class EventInfo
			{
				[JsonIgnore]
				public bool Locked { get; private set; }
				public void LockTemplate() => Locked = true;

				private int _eventType;
				[ProviderProperty("")]
				public int EventType
				{
					get => _eventType;
					set => _eventType = !Locked ? value : throw new TemplateAccessException();
				}

				private int _hitDelay;
				[ProviderProperty("")]
				public int HitDelay
				{
					get => _hitDelay;
					set => _hitDelay = !Locked ? value : throw new TemplateAccessException();
				}

				private int _nextState;
				[ProviderProperty("")]
				public int NextState
				{
					get => _nextState;
					set => _nextState = !Locked ? value : throw new TemplateAccessException();
				}

				private Point _lt;
				[ProviderProperty("")]
				public Point LT
				{
					get => _lt;
					set => _lt = !Locked ? value : throw new TemplateAccessException();
				}

				private Point _rb;
				[ProviderProperty("")]
				public Point RB
				{
					get => _rb;
					set => _rb = !Locked ? value : throw new TemplateAccessException();
				}
			}
		}

		[ProviderClass]
		public sealed class ActionInfo // TODO
		{
			[JsonIgnore]
			public bool Locked { get; private set; }
			public void LockTemplate() => Locked = true;

			[ProviderProperty("")]
			public int State { get; set; }
			[ProviderProperty("")]
			public int StateType { get; set; }
			[ProviderProperty("")]
			public int Prob { get; set; }
			[ProviderProperty("")]
			public int Period { get; set; }
			[ProviderProperty("")]
			public string Message { get; set; }
		}
	}
}
