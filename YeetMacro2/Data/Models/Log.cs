using System.Text.Json.Serialization;
namespace YeetMacro2.Data.Models;

public class ScriptLog : Log
{
    public string MacroSet { get; set; }
    public string Script { get; set; }
    public IList<Log> Logs { get; set; }
}

public class ExceptionLog : Log
{
    public IList<Log> Logs { get; set; }
}

public class ScreenCaptureLog : Log
{
    public byte[] ScreenCapture { get; set; }
}

public class Log
{
    [JsonIgnore]
    public int LogId { get; set; }
    [JsonIgnore]
    public int? ParentId { get; set; }
    public long Timestamp { get; set; }
    public string Message { get; set; }
    public LogType Type { get; set; }
    public virtual bool IsArchived { get; set; }
    [JsonIgnore]
    public virtual bool IsSelected { get; set; }
}

public enum LogType
{
    Info,
    Debug,
    Verbose,
    Error
}