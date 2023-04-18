using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Technologai;

public class Ability
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    /*
    public virtual async Task Process(Information information, List<Information> context)
    {
        var collated = await Collate(context);


        var collected = await Collect(context);

        if (await Assess(collated))
        {

        }

        if (await Assess(collated))
        {
            await Execute(collated);
        }
        else
        {
            
        }
        
        
        await Spawn(information);
    }

    public virtual async Task Process(List<Information> forwardInformation, List<Information> context)
    {
        var collated = await Collate(context);

        if (await Assess(collated))
        {

        }

        if (await Assess(collated))
        {
            await Execute(collated);
        }
        else
        {

        }
        await Spawn(information);
    }

    public abstract Task<bool> Assess(Information information);
    public abstract Task<Information> Execute(Information information);
    public abstract Task<Information> Collate(List<Information> information);
    public abstract Task<Information> Spawn(List<Information> information);
    */
}
