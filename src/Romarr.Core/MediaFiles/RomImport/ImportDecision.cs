using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport
{
    public class ImportDecision
    {
        public LocalGameFile LocalGameFile { get; private set; }
        public IEnumerable<ImportRejection> Rejections { get; private set; }

        public bool Approved => Rejections.Empty();

        public ImportDecision(LocalGameFile localRom, params ImportRejection[] rejections)
        {
            LocalGameFile = localRom;
            Rejections = rejections.ToList();
        }
    }
}
