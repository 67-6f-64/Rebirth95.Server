# Default script.
# Can't call npc script functions in non-npc scripts

message = f"[Script] [{ctx.Script.SpeakerTemplateID}] [{ctx.ScriptName}] I have not been coded yet."

def default_script():
    return ctx.Say(message, False, False)


if ctx.IsNpcScript:
    default_script()
else:
    ctx.SystemMessage(message)

ctx.Dispose()