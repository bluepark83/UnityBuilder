using UnityEditor.Build.Reporting;
using UnityEngine;

public partial class BuildRunner
{
    private static void ReportBuildSummary(BuildReport InReport)
    {
        var summary = InReport.summary;

        switch (summary.result)
        {
            case BuildResult.Succeeded:
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                break;
            case BuildResult.Failed:
                Debug.Log("Build failed");
                break;
        }
    }
}
