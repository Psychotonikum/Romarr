using System;
using System.Collections.Generic;
using Romarr.Core.Indexers;
using Romarr.Core.Validation;

namespace Romarr.Core.Test.IndexerTests
{
    public class TestIndexerSettings : IIndexerSettings
    {
        public RomarrValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public string BaseUrl { get; set; }

        public IEnumerable<int> MultiLanguages { get; set; }
        public IEnumerable<int> FailDownloads { get; set; }
    }
}
