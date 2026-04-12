namespace YeetMacro2.Services;

public interface IRecorderService
{
    bool IsInitialized { get; }
    Task<bool> StartRecording();
    void StopRecording();
    void SetForegroundServiceReady();
}
