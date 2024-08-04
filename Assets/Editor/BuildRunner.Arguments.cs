using System.Collections.Generic;

public partial class BuildRunner
{
    private static string GetArgValue(IReadOnlyList<string> arguments, string InKey)
    {
        for (var i = 0; i < arguments.Count; ++i)
        {
            if (arguments[i].Equals(InKey) && arguments.Count > i + 1)
            {
                return arguments[i + 1];
            }
        }

        return null;
    }
}
