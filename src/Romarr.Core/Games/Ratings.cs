using Romarr.Core.Datastore;

namespace Romarr.Core.Games
{
    public class Ratings : IEmbeddedDocument
    {
        public int Votes { get; set; }
        public decimal Value { get; set; }
    }
}
