namespace YeetMacro2.Services;

public interface IRecorderService
{
    bool IsInitialized { get; }
    Task StartRecording();
    void StopRecording();
}
