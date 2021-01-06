using Rebirth.Characters;
using Rebirth.Network;
using System;
using System.Collections.Generic;

namespace Rebirth.Commands
{
    public class CommandCtx
    {
        public Character Character { get; }
        public Queue<string> Queue { get; }

        public CommandCtx(Character character, string[] split)
        {
            Character = character;
            Queue = new Queue<string>();

            //TODO: Phase out the Queue and use the raw string[] provided

            for (int i = 1; i < split.Length; i++)
            {
                Queue.Enqueue(split[i]);
            }
        }

        public bool Empty => Queue.Count == 0;

        public string NextString() => Queue.Dequeue();
        public int NextInt() => Convert.ToInt32(NextString());
        public bool NextBool() => Convert.ToBoolean(NextString());

        public string Remaining()
        {
            var msg = string.Empty;

            while (Queue.TryDequeue(out var s))
            {
                msg += $"{s} ";
            }

            return msg.Trim();
        }

        public void SendPacket(COutPacket packet) => Character.SendPacket(packet);
    }
}
