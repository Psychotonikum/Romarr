using NLog;
using Romarr.Common.Instrumentation;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.Parser
{
    public static class ValidateParsedRomInfo
    {
        private static readonly Logger Logger = RomarrLogger.GetLogger(typeof(ValidateParsedRomInfo));

        public static bool ValidateForGameType(ParsedRomInfo parsedRomInfo, Game game, bool warnIfInvalid = true)
        {
            return true;
        }
    }
}
