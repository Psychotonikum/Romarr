using System.IO;
using System.Text;

namespace Romarr.Common;

public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}
