using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public class ProcessSceneWithReport : IProcessSceneWithReport
    {
        private IProcessSceneWithReport _processSceneWithReportImplementation;
        public int callbackOrder => _processSceneWithReportImplementation.callbackOrder;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // Debug.Log($"OnProcessScene : {scene.name}");
            // ContentBuildInterface.CalculatePlayerDependenciesForScene(scene.name, new BuildSettings(),
            //     new BuildUsageTagSet(), new BuildUsageCache());
            //
            // _processSceneWithReportImplementation.OnProcessScene(scene, report);
        }
    }
}