# VIP Cab : Lith Harbor (Doesn't Exist?)

job_id = ctx.Character.Stats.nJob
cost = 1000 * 1 if job_id == 0 else 10

ctx.Say("Hi there! This cab is for VIP customers only. Instead of just taking you to different towns like the regular cabs, we offer a much better service worthy of VIP class. It's a bit pricey, but... for only 10,000 mesos, we'll take you safely to the \r\n#bAnt Tunnels#k.")

message = "We have a special 90% off discount for beginners. The Ant Tunnel is located deep inside the dungeon that's placed at the center of the Victoria Island, where the 24 Hr Mobile Store is. Would you like to go there for #b1,000 mesos#k?" if job_id == 0 else "The regular fee applies for all non-beginners. The Ant Tunnel is located deep inside the dungeon that's placed at the center of the Victoria Island, where the 24 Hr Mobile Store is. Would you like to go there for #b10,000 mesos#k?"

response = ctx.AskYesNo(message)

if response:
    if ctx.Character.Stats.nMoney < cost:
        ctx.Say("It looks like you don't have enough mesos. Sorry but you won't be able to use this without them.")
    else:
        ctx.Character.Modify.GainMeso(-1 * cost)
        # ctx.Character.Action.SetField(105070001) Ant Tunnel 

def close():
    ctx.Say("This town also has a lot to offer. Find us if and when you feel the need to go to the Ant Tunnel Park.")