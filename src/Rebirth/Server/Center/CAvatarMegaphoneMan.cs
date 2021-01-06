using Rebirth.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;

namespace Rebirth.Server.Center
{
	public class CAvatarMegaphoneMan
	{
		private const int MEGAPHONE_DELAY = 2000;
		private const int MEGAPHONE_DURATION = 12000;
		private const int MEGAPHONE_QUEUE_MAX_LENGTH = 1; // 15 seconds lol (GMS-like, maybe we change in future)
														  // string pool error code: 4013 | Message: The waiting line is longer than 15 seconds. Please try using it at a later time.
		private bool _active;
		private readonly Queue<AvatarMegaphone> _queue;
		private AvatarMegaphone _activeMega;
		private DateTime _lastAvatarSent;

		public CAvatarMegaphoneMan()
		{
			_queue = new Queue<AvatarMegaphone>();
			_lastAvatarSent = DateTime.Now.AddDays(-1);
		}

		public void Update()
		{
			if (_active)
			{
				if (IsCurrentMegaphoneExpired())
				{
					RemoveActiveMegaphone();
				}

			}
			else if (CanBroadcastNextMegaphone())
			{
				BroadcastNextMegaphone();
			}
		}

		public AvatarMegaphoneResCode TryAddToQueue(AvatarMegaphone item)
		{
			if (IsQueueFull())
				return AvatarMegaphoneResCode.QueueFull;

			if (_queue.FirstOrDefault(i => i.nCharId == item.nCharId) != null)
				return AvatarMegaphoneResCode.QueueFull; // same char cant have several in the queue

			_queue.Enqueue(item);

			item.nInitialQueuePosition = _queue.Count;

			if (!_active)
			{
				Update();
				return AvatarMegaphoneResCode.Success_Now;
			}

			return AvatarMegaphoneResCode.Success_Later;
		}

		public void RemoveActiveMegaphone()
		{
			_active = false;
			_activeMega = null;
			_lastAvatarSent = DateTime.Now;
			MasterManager.CharacterPool.Broadcast(CPacket.ClearAvatarMegapone());
		}

		public void BroadcastNextMegaphone()
		{
			_active = true;
			_activeMega = _queue.Dequeue();
			_lastAvatarSent = DateTime.Now;
			MasterManager.CharacterPool.Broadcast(CPacket.SetAvatarMegaphone(_activeMega));
		}

		public bool IsCurrentMegaphoneExpired() => (DateTime.Now - _lastAvatarSent).TotalMilliseconds >= MEGAPHONE_DURATION;
		public bool CanBroadcastNextMegaphone() => !IsQueueEmpty();//&& (DateTime.Now - _lastAvatarSent).TotalMilliseconds >= MEGAPHONE_DELAY;
		public bool IsQueueFull() => _queue.Count >= MEGAPHONE_QUEUE_MAX_LENGTH;
		public bool IsQueueEmpty() => _queue.Count <= 0;
	}
}
