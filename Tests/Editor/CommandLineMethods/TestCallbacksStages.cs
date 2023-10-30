using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityPatterns.Tests.Editor.CommandLineMethods
{
    public class TestCallbacksStages : ICallbacks
    {
        public TestRunnerApi Runner { protected get; set; }
        public Dictionary<TestStatus, TestRunnerReport.TestStatusData> TestsStatusTypes { protected get; set; }

        #region Methods Overrides
        
        public void RunStarted(ITestAdaptor testsToRun) { }

        public void RunFinished(ITestResultAdaptor result)
        {
            LogTestsSummary(result);
            LogFixtures(result);

            if (Runner is not null)
            {
                Runner.UnregisterCallbacks(this);
            }
            
            if (result.TestStatus == TestStatus.Failed && Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result)
        {
            LogTestMethod(result);
        }

        #endregion

        #region Methods

        protected virtual void LogTestsSummary(ITestResultAdaptor result)
        {
            var testStatus = TestsStatusTypes.GetValueOrDefault(result.TestStatus);
                
            if (testStatus is not null)
            {
                testStatus.Result = result;
                
                // TODO: [Refactor] Change this logic here to only append "passed" tests when "fixtureSuite.PassCount > 0"
                var resultMessage = result.FailCount > 0
                    ? $"<color={testStatus.Color}>{result.FailCount} {result.ResultState.ToLower()}</color>, <color=lime>{result.PassCount} passed</color>"
                    : $"<color={testStatus.Color}>{testStatus.ResultCount} {testStatus.Description}</color>";
                
                Debug.Log($"<b><color={testStatus.Color}>[{testStatus.Title}]</color></b> {result.FullName}");

                Debug.Log($"Tests: {resultMessage}, {result.Test.TestCaseCount} total");
                Debug.Log($"Tests TIME: <b>{result.Duration} s</b>");
            }
        }

        protected virtual void LogFixtures(ITestResultAdaptor result)
        {
            ITestResultAdaptor newTestCaseResult = result;
            var foundFixtures = false;
            while (newTestCaseResult?.HasChildren == true && !foundFixtures)
            {
                var fixtures = newTestCaseResult.Children.Where(child =>
                {
                    return child.Test.IsSuite && child.ToXml().Attributes["type"] == "TestFixture";
                }).ToList();

                if (fixtures.Count > 0)
                {
                    foreach (var fixtureSuite in fixtures)
                    {
                        var fixtureTestStatus = TestsStatusTypes.GetValueOrDefault(fixtureSuite.TestStatus);

                        if (fixtureTestStatus is not null)
                        {
                            fixtureTestStatus.Result = fixtureSuite;
                            
                            // TODO: [Refactor] Change this logic here to only append "passed" tests when "fixtureSuite.PassCount > 0"
                            var resultMessage = fixtureSuite.FailCount > 0
                                ? $"<color={fixtureTestStatus.Color}>{fixtureSuite.FailCount} {fixtureSuite.ResultState.ToLower()}</color>, <color=lime>{fixtureSuite.PassCount} passed</color>"
                                : $"<color={fixtureTestStatus.Color}>{fixtureTestStatus.ResultCount} {fixtureTestStatus.Description}</color>";
                            
                            var fixtureLog =
                                $"Test Fixtures: {fixtureSuite.FullName} => {resultMessage}" +
                                $", {fixtureSuite.Test.TestCaseCount} total";

                            Debug.Log(fixtureLog);
                        }
                    }

                    foundFixtures = true;
                }

                newTestCaseResult = newTestCaseResult.Children.FirstOrDefault();
            }
        }
        
        protected virtual void LogTestMethod(ITestResultAdaptor result)
        {
            if (result.Test.IsSuite
                || result.Test.IsTestAssembly
                || result.Name.EndsWith(".dll"))
            {
                return;
            }

            if (TestsStatusTypes.TryGetValue(result.TestStatus, out var testStatus))
            {
                var testLog = $"<color={testStatus.Color}>{testStatus.IconSymbol}</color>{result.FullName}";

                if (!string.IsNullOrWhiteSpace(result.Test.Description))
                {
                    testLog += $" (description: {result.Test.Description})";
                }
                
                testLog += $" [duration: {result.Duration}]";
                
                Debug.Log(testLog);
            }
        }
        
        #endregion
    }
}
