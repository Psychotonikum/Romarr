using System.Collections.Generic;

namespace Romarr.Core.Annotations
{
    public interface ISelectOptionsConverter
    {
        List<SelectOption> GetSelectOptions();
    }
}
