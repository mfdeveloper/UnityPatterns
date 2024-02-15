using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityPatterns.Tests.Editor.CommandLineMethods
{
    public static class TestRunnerReport
    {
        private const string TAG = nameof(TestRunnerReport);
        
        private static TestRunnerApi runner;
        private static TestMode testMode = TestMode.EditMode;
        private static TestCallbacksStages callbacksStages;

        private static Lazy<Dictionary<TestStatus, TestStatusData>> testsStatusTypes;

        public class TestStatusData
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Color { get; set; }
            public string IconSymbol { get; set; }
            public int ResultCount { get; protected set; }

            public ITestResultAdaptor Result
            {
                get => result;
                set
                {
                    result = value;
                    ResultCount = result.TestStatus switch
                    {
                        TestStatus.Passed => result.PassCount,
                        TestStatus.Failed => result.FailCount,
                        _ => result.SkipCount
                    };
                    Description = result.ResultState.ToLower();
                }
            }

            private ITestResultAdaptor result;
        }
        
        /// <summary>
        ///  Lazy initialize <see cref="testsStatusTypes"/>, <see cref="Dictionary{TKey,TValue}"/> values in a static constructor 
        /// </summary>
        /// <remarks>
        /// <b> References </b>
        /// <ul>
        ///     <li>
        ///         <a href="https://josef.codes/enumeration-class-in-c-sharp-using-records">Enumeration class in C# using records (Lazy class)</a>
        ///     </li>
        ///     <li>
        ///         <a href="https://learn.microsoft.com/en-us/dotnet/framework/performance/lazy-initialization">C#: Lazy Initialization</a>
        ///     </li>
        /// </ul>
        /// </remarks>
        static TestRunnerReport()
        {
            testsStatusTypes = new Lazy<Dictionary<TestStatus, TestStatusData>>(() =>
                new Dictionary<TestStatus, TestStatusData>
                {
                    {
                        TestStatus.Passed, new TestStatusData
                        {
                            Title = "PASS",
                            Color = "lime",
                            // ✓ => checkmark console
                            // \u2713
                            IconSymbol = "\u2713"
                        }
                    },
                    {
                        TestStatus.Failed, new TestStatusData
                        {
                            Title = "FAIL",
                            Color = "red",
                            // ⨯ => cross error console
                            // &#x2A2F, \u2a2f
                            IconSymbol = "\u2a2f"
                        }
                    }
                });
        }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            if (runner is not null && callbacksStages is not null)
            {
                runner.UnregisterCallbacks(callbacksStages);
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void RunTests()
        {
            // CLI extra arguments
            SetupArguments();

            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter
            {
                testMode = testMode
            };
            
            Debug.Log($"[{TAG}] Selected test mode: {testMode}");

            callbacksStages = new TestCallbacksStages
            {
                Runner = runner,
                TestsStatusTypes = testsStatusTypes.Value
            };
            runner.RegisterCallbacks(callbacksStages);
            runner.Execute(new ExecutionSettings(filter));
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private static void SetupArguments()
        {
            var testPlatform = Environment.GetEnvironmentVariable("UNITY_TEST_MODE", EnvironmentVariableTarget.Machine);

            if (string.IsNullOrWhiteSpace(testPlatform))
            {
                var cliArgs = Environment.GetCommandLineArgs();
                
                int previousIndex = Array.IndexOf(cliArgs, "-testPlatform");
                if (previousIndex != -1)
                {
                    testPlatform = cliArgs[previousIndex + 1];
                }
            }
            
            testMode = testPlatform?.ToLower() switch
            {
                "playmode" => TestMode.PlayMode,
                "editmode" => TestMode.EditMode,
                _ => TestMode.EditMode
            };
        }
    }
}
