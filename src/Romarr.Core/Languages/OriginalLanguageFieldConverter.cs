using System.Collections.Generic;
using System.Linq;
using Romarr.Core.Annotations;

namespace Romarr.Core.Languages
{
    public class OriginalLanguageFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return Language.All
                .Where(l => l.Id >= 0)
                .OrderBy(l => l.Id > 0).ThenBy(l => l.Name)
                .ToList()
                .ConvertAll(v => new SelectOption { Value = v.Id, Name = v.Name });
        }
    }
}
