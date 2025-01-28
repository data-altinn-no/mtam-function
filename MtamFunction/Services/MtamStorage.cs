using System.Collections.Concurrent;
using CloudNative.CloudEvents;
using MtamFunction.Models;

namespace MtamFunction.Services;

public interface IMtamStorage
{
    IEnumerable<MtamSourceMessage> GetSourceMessages();
    void AddSourceMessage(MtamSourceMessage message);
    
    IEnumerable<CloudEvent> GetCloudEvents();
    void AddCloudEvent(CloudEvent cloudEvent);
}

public class MtamStorage : IMtamStorage
{
    private readonly ConcurrentBag<MtamSourceMessage> _sourceMessages = [];
    private readonly ConcurrentBag<CloudEvent> _cloudEvents = [];
    
    public IEnumerable<MtamSourceMessage> GetSourceMessages() => _sourceMessages;
    
    public void AddSourceMessage(MtamSourceMessage message) => _sourceMessages.Add(message);
    
    public IEnumerable<CloudEvent> GetCloudEvents() => _cloudEvents;
    
    public void AddCloudEvent(CloudEvent cloudEvent) => _cloudEvents.Add(cloudEvent);
}