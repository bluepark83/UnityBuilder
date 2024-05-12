using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BuildRunner
{
    private static string GetArgValue(string InKey)
    {
        var args = System.Environment.GetCommandLineArgs();

        for (var i = 0; i < args.Length; ++i)
        {
            if (args[i].Equals(InKey) && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
