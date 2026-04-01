using System.Diagnostics;

namespace Romarr.Core.Datastore
{
    [DebuggerDisplay("{GetType()} ID = {Id}")]
    public abstract class ModelBase
    {
        public int Id { get; set; }
    }
}
