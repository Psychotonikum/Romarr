using System.Collections.Generic;
using RestSharp;
using Romarr.Api.V3.Roms;

namespace Romarr.Integration.Test.Client
{
    public class GameFileClient : ClientBase<RomResource>
    {
        public GameFileClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "rom")
        {
        }

        public List<RomResource> GetGameFilesInSeries(int gameId)
        {
            var request = BuildRequest("?gameId=" + gameId.ToString());
            return Get<List<RomResource>>(request);
        }

        public RomResource SetMonitored(RomResource rom)
        {
            var request = BuildRequest(rom.Id.ToString());
            request.AddJsonBody(rom);
            return Put<RomResource>(request);
        }
    }
}
