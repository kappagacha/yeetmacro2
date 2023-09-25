using CommunityToolkit.Mvvm.Messaging.Messages;

namespace YeetMacro2.Data.Messaging;

public class ScriptEvent
{
    public ScriptEventType Type { get; set; }
    public string Result { get; set; }
}

public enum ScriptEventType
{
    Started,
    Finished
}

// https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
public class ScriptEventMessage : ValueChangedMessage<ScriptEvent>
{
    public ScriptEventMessage(ScriptEvent evnt) : base(evnt)
    {

    }
}