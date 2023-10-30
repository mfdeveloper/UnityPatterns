# Unity: Command line classes/methods

Use this folder/namespace `Tests.Editor.CommandLineMethods` to store static classes
with static methods that should be called from `Unity.exe/Unity.app` editor CLI.

> **WARNING:** For now, the all outputs from Unity Editor CLI will be stored in a log file: `./Logs/Editor.log`. 
> That's a "limitation" of async job executions on Unity, that don't print the results on console :( 

### TODO

Consider in the future, implement a streaming file reading of `./Logs/Editor.log`, or save your CLI outputs into to another file, 
read it and show on console !

- [Facepunch.UnityBatch](https://github.com/Facepunch/Facepunch.UnityBatch): Implementation example with a **[dotnet console app](https://learn.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code)**

### References

- [Unity Editor command line arguments](https://docs.unity3d.com/Manual/EditorCommandLineArguments.html)
- [Redirecting standard output using the -logFile parameter when in batchmode](https://forum.unity.com/threads/redirecting-standard-output-using-the-logfile-parameter-when-in-batchmode.395339)
- [Forcing output to command line when building on command line](https://forum.unity.com/threads/forcing-output-to-command-line-when-building-on-command-line.753242)
- [Redirecting Build Output to Console for Automated Builds](https://discussions.unity.com/t/redirecting-build-output-to-console-for-automated-builds/128805)

#### Testing automation

- [Running tests from the command line](https://docs.unity3d.com/Packages/com.unity.test-framework@1.3/manual/reference-command-line.html)
- [Unit testing automation with Unity/C# and Codemagic](https://blog.codemagic.io/unit-testing-automation-unity)
- [RUNNING UNITY TESTS ON A BUILD SERVER](https://andrewfray.wordpress.com/2020/09/27/running-unity-tests-on-a-build-server)