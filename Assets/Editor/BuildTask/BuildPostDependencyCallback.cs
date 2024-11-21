using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;

namespace Editor.BuildTask
{
    public class BuildPostDependencyCallback : IBuildTask
    {
        public ReturnCode Run()
        {
            Debug.Log("BuildPostDependencyCallback");

            return ReturnCode.Success;
        }

        public int Version { get { return 2; } }
    }
}