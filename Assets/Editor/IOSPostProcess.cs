#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class IOSPostProcess
{
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(
        BuildTarget buildTarget,
        string pathToBuiltProject
    )
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        AddTrackingUsageDescription(
            pathToBuiltProject
        );

        AddAppTrackingTransparencyFramework(
            pathToBuiltProject
        );
    }

    private static void AddTrackingUsageDescription(
        string pathToBuiltProject
    )
    {
        string plistPath =
            Path.Combine(
                pathToBuiltProject,
                "Info.plist"
            );

        PlistDocument plist =
            new PlistDocument();

        plist.ReadFromFile(
            plistPath
        );

        plist.root.SetString(
            "NSUserTrackingUsageDescription",
            "Your device identifier may be used to deliver and measure personalized ads."
        );

        File.WriteAllText(
            plistPath,
            plist.WriteToString()
        );
    }

    private static void AddAppTrackingTransparencyFramework(
        string pathToBuiltProject
    )
    {
        string projectPath =
            PBXProject.GetPBXProjectPath(
                pathToBuiltProject
            );

        PBXProject project =
            new PBXProject();

        project.ReadFromFile(
            projectPath
        );

        string frameworkTarget =
            project.GetUnityFrameworkTargetGuid();

        project.AddFrameworkToProject(
            frameworkTarget,
            "AppTrackingTransparency.framework",
            false
        );

        project.WriteToFile(
            projectPath
        );
    }
}

#endif