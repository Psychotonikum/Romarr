using Romarr.Core.Datastore;

namespace Romarr.Core.ThingiProvider
{
    public interface IProviderRepository<TProvider> : IBasicRepository<TProvider>
        where TProvider : ModelBase, new()
    {
// void DeleteImplementations(string implementation);
    }
}
