using Xunit;

public class SimpleXunitSink : Serilog.Core.ILogEventSink
{
    private readonly ITestOutputHelper _output;
    public SimpleXunitSink(ITestOutputHelper output) => _output = output;

    public void Emit(Serilog.Events.LogEvent logEvent)
    {
        _output.WriteLine(logEvent.RenderMessage());
    }
}