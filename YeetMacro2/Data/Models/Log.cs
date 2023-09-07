using System.Text.Json.Serialization;
namespace YeetMacro2.Data.Models;

public class LogGroup
{
    [JsonIgnore]
    public int LogGroupId { get; set; }
    public long Timestamp { get; set; }
    public string MacroSet { get; set; }
    public string Script { get; set; }
    public string Stack { get; set; }
    public ICollection<Log> Logs { get; set; }
}

public class Log
{
    [JsonIgnore]
    public int LogId { get; set; }
    public long Timestamp { get; set; }
    public LogType Type { get; set; }
    public string Message { get; set; }
    public string Stack { get; set; }
}

public enum LogType
{
    Info,
    Debug,
    Verbose,
    Error
}