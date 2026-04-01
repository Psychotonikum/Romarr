using System;
using System.Collections.Generic;
using Romarr.Core.Messaging.Commands;

namespace Romarr.Core.Games.Commands
{
    public class BulkMoveGameCommand : Command
    {
        public List<BulkMoveGame> Game { get; set; }
        public string DestinationRootFolder { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }

    public class BulkMoveGame : IEquatable<BulkMoveGame>
    {
        public int GameId { get; set; }
        public string SourcePath { get; set; }

        public bool Equals(BulkMoveGame other)
        {
            if (other == null)
            {
                return false;
            }

            return GameId.Equals(other.GameId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return GameId.Equals(((BulkMoveGame)obj).GameId);
        }

        public override int GetHashCode()
        {
            return GameId.GetHashCode();
        }
    }
}
