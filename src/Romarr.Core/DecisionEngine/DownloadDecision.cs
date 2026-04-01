using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public RemoteRom RemoteRom { get; private set; }
        public IEnumerable<DownloadRejection> Rejections { get; private set; }

        public bool Approved => !Rejections.Any();

        public bool TemporarilyRejected
        {
            get
            {
                return Rejections.Any() && Rejections.All(r => r.Type == RejectionType.Temporary);
            }
        }

        public bool Rejected
        {
            get
            {
                return Rejections.Any() && Rejections.Any(r => r.Type == RejectionType.Permanent);
            }
        }

        public DownloadDecision(RemoteRom rom, params DownloadRejection[] rejections)
        {
            RemoteRom = rom;
            Rejections = rejections.ToList();
        }

        public override string ToString()
        {
            if (Approved)
            {
                return "[OK] " + RemoteRom;
            }

            return "[Rejected " + Rejections.Count() + "]" + RemoteRom;
        }
    }
}
