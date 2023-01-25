using System;
using System.Diagnostics;
using System.Reflection;

namespace UE3ShaderCachePatcher;

public class BindingException : Exception
{
    public BindingException(string message)
        : base(message)
    {
    }
}

public sealed class BindingListener : DefaultTraceListener
{
    private string Callstack { get; set; }
    private string DateTime { get; set; }
    private int InformationPropertyCount { get; set; }
    private bool IsFirstWrite { get; set; }
    private string LogicalOperationStack { get; set; }
    private string Message { get; set; }
    private string ProcessId { get; set; }
    private string ThreadId { get; set; }
    private string Timestamp { get; set; }

    public BindingListener(TraceOptions options)
    {
        IsFirstWrite = true;
        Callstack = "";
        DateTime = "";
        InformationPropertyCount = 0;
        LogicalOperationStack = "";
        Message = "";
        ProcessId = "";
        ThreadId = "";
        Timestamp = "";

        PresentationTraceSources.Refresh();
        PresentationTraceSources.DataBindingSource.Listeners.Add(this);
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
        TraceOutputOptions = options;
        DetermineInformationPropertyCount();
    }

    private void DetermineInformationPropertyCount()
    {
        foreach (TraceOptions traceOptionValue in Enum.GetValues(typeof(TraceOptions)))
        {
            if (traceOptionValue != TraceOptions.None)
            {
                InformationPropertyCount += GetTraceOptionEnabled(traceOptionValue);
            }
        }
    }

    private int GetTraceOptionEnabled(TraceOptions option)
    {
        return (TraceOutputOptions & option) == option ? 1 : 0;
    }

    public override void WriteLine(string? message)
    {
        message ??= "";

        if (IsFirstWrite)
        {
            Message = message;
            IsFirstWrite = false;
        }
        else
        {
            var propertyInformation = message.Split(new[] { "=" }, StringSplitOptions.None);

            if (propertyInformation.Length == 1)
            {
                LogicalOperationStack = propertyInformation[0];
            }
            else
            {
                GetType().GetProperty(propertyInformation[0],
                        BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(this, propertyInformation[1], null);
            }

            InformationPropertyCount--;
        }

        Flush();

        if (InformationPropertyCount == 0)
        {
            PresentationTraceSources.DataBindingSource.Listeners.Remove(this);
            var msg = $"{Message}\n " +
                      $"{Callstack}\n\n " +
                      $"DateTime: {DateTime}\n " +
                      $"LogicalOperationStack: {LogicalOperationStack}\n " +
                      $"ProcessId: {ProcessId}\n " +
                      $"ThreadId: {ThreadId}\n " +
                      $"TimeStamp: {Timestamp}";
            throw new BindingException(msg);
        }
    }
}
