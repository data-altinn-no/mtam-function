using CloudNative.CloudEvents;
using MtamFunction.Models;

namespace MtamFunction.Services;

public interface IMtamService
{
    void Create(MtamSourceMessage message);
    IEnumerable<MtamSourceMessage> Get(DateTime? fromDate);
    MtamSourceMessage? GetById(string id);
    
    void AddCloudEvent(CloudEvent cloudEvent);
    IEnumerable<CloudEvent> GetCloudEvents();
}

public class MtamService(IMtamStorage mtamStorage) : IMtamService
{
    public void Create(MtamSourceMessage message)
    {
        mtamStorage.AddSourceMessage(message);
    }

    public IEnumerable<MtamSourceMessage> Get(DateTime? fromDate)
    {
        var messages = mtamStorage.GetSourceMessages().ToList();
        return fromDate is null ? 
            messages : 
            messages.Where(m => m.Timestamp >= fromDate);
    }

    public MtamSourceMessage? GetById(string id)
    {
        var messages = mtamStorage.GetSourceMessages().ToList();
        return messages.FirstOrDefault(m => m.Id == id);
    }

    public void AddCloudEvent(CloudEvent cloudEvent)
    {
        mtamStorage.AddCloudEvent(cloudEvent);
    }

    public IEnumerable<CloudEvent> GetCloudEvents()
    {
        return mtamStorage.GetCloudEvents().ToList();
    }
}