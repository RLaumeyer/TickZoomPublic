﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 4054 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.UnitTesting;
using NUnit.Framework;
using UnitTesting.Tests.Utils;

namespace UnitTesting.Tests.Tree
{
	/// <summary>
	/// Creates a test tree view with only one test class node.
	/// </summary>
	[TestFixture]
	public class OneTestClassTestFixture
	{
		ExtTreeNode rootNode;
		TreeNodeCollection nodes;
		DummyParserServiceTestTreeView dummyTreeView;
		TestTreeView treeView;
		MSBuildBasedProject project;
		MockProjectContent projectContent;
		MockClass testClass;
		TreeNodeCollection rootChildNodes;
		ExtTreeNode rootNamespaceNode;
		TreeNodeCollection rootNamespaceChildNodes;
		ExtTreeNode testsNamespaceNode;
		TreeNodeCollection testsNamespaceChildNodes;
		ExtTreeNode testFixtureNode;
		MockMethod testMethod;
		TreeNodeCollection testFixtureChildNodes;
		ExtTreeNode testNode;
		ReferenceProjectItem nunitFrameworkReferenceItem;
		
		[SetUp]
		public void SetUp()
		{
			// Create a project to display in the test tree view.
			project = new MockCSharpProject();
			project.Name = "TestProject";
			nunitFrameworkReferenceItem = new ReferenceProjectItem(project);
			nunitFrameworkReferenceItem.Include = "NUnit.Framework";
			ProjectService.AddProjectItem(project, nunitFrameworkReferenceItem);
			List<IProject> projects = new List<IProject>();
			projects.Add(project);
			
			// Add second non-test project.
			projects.Add(new MockCSharpProject());
			
			// Add a test class with a TestFixture attributes.
			projectContent = new MockProjectContent();
			projectContent.Language = LanguageProperties.None;
			testClass = new MockClass("RootNamespace.Tests.MyTestFixture");
			testClass.Namespace = "RootNamespace.Tests";
			testClass.Attributes.Add(new MockAttribute("TestFixture"));
			testClass.ProjectContent = projectContent;
			projectContent.Classes.Add(testClass);
			
			// Add two methods to the test class only
			// one of which has test attributes.
			testMethod = new MockMethod("NameExists");
			testMethod.Attributes.Add(new MockAttribute("Test"));
			testMethod.DeclaringType = testClass;
			testClass.Methods.Add(testMethod);
			testClass.Methods.Add(new MockMethod());
					
			// Add a second class that has no test fixture attribute.
			MockClass nonTestClass = new MockClass();
			nonTestClass.ProjectContent = projectContent;
			projectContent.Classes.Add(nonTestClass);
			
			// Init mock project content to be returned.
			dummyTreeView = new DummyParserServiceTestTreeView();
			dummyTreeView.ProjectContentForProject = projectContent;
			
			// Load the projects into the test tree view.
			treeView = dummyTreeView as TestTreeView;	
			treeView.AddProjects(projects);
			nodes = treeView.Nodes;
			rootNode = (ExtTreeNode)treeView.Nodes[0];
			
			// Expand the root node so any child nodes are
			// lazily created.
			rootNode.Expanding();
			rootChildNodes = rootNode.Nodes;
			rootNamespaceNode = (ExtTreeNode)rootNode.Nodes[0];
			
			// Expand the first namespace node.
			rootNamespaceNode.Expanding();
			rootNamespaceChildNodes = rootNamespaceNode.Nodes;
			testsNamespaceNode = (ExtTreeNode)rootNamespaceNode.Nodes[0];
		
			// Expand the tests namespace node.
			testsNamespaceNode.Expanding();
			testsNamespaceChildNodes = testsNamespaceNode.Nodes;
			testFixtureNode = (ExtTreeNode)testsNamespaceNode.Nodes[0];
		
			// Expand the test node.
			testFixtureNode.Expanding();
			testFixtureChildNodes = testFixtureNode.Nodes;
			testNode = (ExtTreeNode)testFixtureChildNodes[0];
		}
		
		[TearDown]
		public void TearDown()
		{
			if (treeView != null) {
				treeView.Dispose();
			}
		}
		
		[Test]
		public void OneRootNode()
		{
			Assert.AreEqual(1, nodes.Count);
		}
		
		[Test]
		public void RootNodeText()
		{
			Assert.AreEqual(project.Name, rootNode.Text);
		}
		
		[Test]
		public void RootNodeIsTestProjectNode()
		{
			Assert.IsInstanceOf(typeof(TestProjectTreeNode), rootNode);
		}
		
		[Test]
		public void SelectedProjectWhenNoNodeSelected()
		{
			treeView.SelectedNode = null;
			Assert.IsNull(treeView.SelectedProject);
		}
		
		[Test]
		public void SelectedNamespaceWhenNoNodeSelected()
		{
			treeView.SelectedNode = null;
			Assert.IsNull(treeView.SelectedNamespace);
		}
		
		[Test]
		public void SelectedRootNamespaceNode()
		{
			treeView.SelectedNode = rootNamespaceNode;
			Assert.AreEqual("RootNamespace", treeView.SelectedNamespace);
		}
		
		[Test]
		public void SelectedTestsNamespaceNode()
		{
			treeView.SelectedNode = testsNamespaceNode;
			Assert.AreEqual("RootNamespace.Tests", treeView.SelectedNamespace);
		}
		
		[Test]
		public void SelectedProject()
		{
			treeView.SelectedNode = rootNode;
			Assert.AreSame(project, treeView.SelectedProject);
		}
		
		[Test]
		public void SelectedClass()
		{
			Assert.IsNull(treeView.SelectedClass);
		}
		
		[Test]
		public void SelectedMethod()
		{
			Assert.IsNull(treeView.SelectedMethod);
		}
		
		[Test]
		public void RootNodeImageIndex()
		{
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)rootNode.ImageIndex);
		}
		
		[Test]
		public void RootNodeHasOneChild()
		{
			Assert.AreEqual(1, rootChildNodes.Count);
		}
		
		[Test]
		public void RootNamespaceNodeText()
		{
			Assert.AreEqual("RootNamespace", rootNamespaceNode.Text);
		}
		
		[Test]
		public void RootNamespaceNodeFullNamespace()
		{
			TestNamespaceTreeNode node = (TestNamespaceTreeNode)rootNamespaceNode;
			Assert.AreEqual("RootNamespace", node.FullNamespace);
		}
		
		[Test]
		public void RootNamespaceNodeIsTestNamespaceNode()
		{
			Assert.IsInstanceOf(typeof(TestNamespaceTreeNode), rootNamespaceNode);
		}
		
		[Test]
		public void RootNamespaceNodeHasOneChild()
		{
			Assert.AreEqual(1, rootNamespaceChildNodes.Count);
		}
		
		[Test]
		public void TestsNamespaceNodeText()
		{
			Assert.AreEqual("Tests", testsNamespaceNode.Text);
		}
		
		[Test]
		public void TestsNamespaceNodeFullNamespace()
		{
			TestNamespaceTreeNode node = (TestNamespaceTreeNode)testsNamespaceNode;
			Assert.AreEqual("RootNamespace.Tests", node.FullNamespace);
		}
		
		[Test]
		public void TestsNamespaceNodeIsNamespaceNode()
		{
			Assert.IsInstanceOf(typeof(TestNamespaceTreeNode), testsNamespaceNode);
		}
		
		[Test]
		public void TestsNamespaceNodeHasOneChild()
		{
			Assert.AreEqual(1, testsNamespaceChildNodes.Count);
		}
		
		[Test]
		public void TestFixtureNodeText()
		{
			Assert.AreEqual("MyTestFixture", testFixtureNode.Text);
		}
		
		[Test]
		public void TestFixtureNodeIsTestClassNode()
		{
			Assert.IsInstanceOf(typeof(TestClassTreeNode), testFixtureNode);
		}
		
		[Test]
		public void SelectedProjectWhenClassProject()
		{
			treeView.SelectedNode = testFixtureNode;
			Assert.AreSame(project, treeView.SelectedProject);
		}
		
		[Test]
		public void SelectedClassWhenClassSelected()
		{
			treeView.SelectedNode = testFixtureNode;
			Assert.AreSame(testClass, treeView.SelectedClass);
		}
		
		[Test]
		public void SelectedTestProjectWhenClassSelected()
		{
			treeView.SelectedNode = testFixtureNode;
			TestProject testProject = ((TestProjectTreeNode)rootNode).TestProject;
			Assert.AreSame(testProject, treeView.SelectedTestProject);
		}
		
		[Test]
		public void TestFixtureHasOneChildNode()
		{
			Assert.AreEqual(1, testFixtureChildNodes.Count);
		}
		
		[Test]
		public void TestMethodIsTestMethodNode()
		{
			Assert.IsInstanceOf(typeof(TestMethodTreeNode), testNode);
		}
		
		[Test]
		public void TestMethodNodeText()
		{
			Assert.AreEqual("NameExists", testNode.Text);
		}
		
		[Test]
		public void SelectedClassWhenMethodSelected()
		{
			treeView.SelectedNode = testNode;
			Assert.AreSame(testClass, treeView.SelectedClass);
		}
		
		[Test]
		public void SelectedMethodWhenMethodSelected()
		{
			treeView.SelectedNode = testNode;
			Assert.AreSame(testMethod, treeView.SelectedMethod);
		}
		
		[Test]
		public void TestMethodFails()
		{
			TestResult result = new TestResult("RootNamespace.Tests.MyTestFixture.NameExists");
			result.IsFailure = true;
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			projectNode.TestProject.UpdateTestResult(result);
			
			Assert.AreEqual(TestTreeViewImageListIndex.TestFailed, (TestTreeViewImageListIndex)testNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestFailed, (TestTreeViewImageListIndex)testFixtureNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestFailed, (TestTreeViewImageListIndex)testsNamespaceNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestFailed, (TestTreeViewImageListIndex)rootNamespaceNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestFailed, (TestTreeViewImageListIndex)rootNode.ImageIndex);
		}
		
		[Test]
		public void TestMethodPasses()
		{
			TestResult result = new TestResult("RootNamespace.Tests.MyTestFixture.NameExists");
			result.IsSuccess = true;
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			projectNode.TestProject.UpdateTestResult(result);
			
			Assert.AreEqual(TestTreeViewImageListIndex.TestPassed, (TestTreeViewImageListIndex)testNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestPassed, (TestTreeViewImageListIndex)testFixtureNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestPassed, (TestTreeViewImageListIndex)testsNamespaceNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestPassed, (TestTreeViewImageListIndex)rootNamespaceNode.ImageIndex);
		}
		
		[Test]
		public void TestMethodIgnored()
		{
			TestResult result = new TestResult("RootNamespace.Tests.MyTestFixture.NameExists");
			result.IsIgnored = true;
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			projectNode.TestProject.UpdateTestResult(result);
			
			Assert.AreEqual(TestTreeViewImageListIndex.TestIgnored, (TestTreeViewImageListIndex)testNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestIgnored, (TestTreeViewImageListIndex)testFixtureNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestIgnored, (TestTreeViewImageListIndex)testFixtureNode.SelectedImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestIgnored, (TestTreeViewImageListIndex)rootNamespaceNode.ImageIndex);
		}
		
		[Test]
		public void TestMethodResultSetBackToNoneAfterReset()
		{
			TestMethodIgnored();
			
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			projectNode.TestProject.ResetTestResults();
			
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)testNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)testFixtureNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)rootNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)testsNamespaceNode.ImageIndex);
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)rootNamespaceNode.ImageIndex);
		}
		
		/// <summary>
		/// Tests that a new method node is added to the test class node.
		/// </summary>
		[Test]
		public void NewMethodAdded()
		{
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			TestClass testClass = projectNode.TestProject.TestClasses["RootNamespace.Tests.MyTestFixture"];		
			
			MockMethod method = new MockMethod("NewMethod");
			method.DeclaringType = testClass.Class;
			method.Attributes.Add(new MockAttribute("Test"));
			testClass.TestMethods.Add(new TestMethod(method));
			
			ExtTreeNode newMethodNode = null;
			foreach (ExtTreeNode node in testFixtureNode.Nodes) {
				if (node.Text == "NewMethod") {
					newMethodNode = node;
					break;
				}
			}

			Assert.AreEqual(2, testFixtureNode.Nodes.Count);
			Assert.IsNotNull(newMethodNode);
			Assert.IsInstanceOf(typeof(TestMethodTreeNode), newMethodNode);
		}
		
		/// <summary>
		/// Tests that when a test method is removed from a test class the
		/// TestClassTreeNode updates itself and removes the test method.
		/// </summary>
		[Test]
		public void MethodRemoved()
		{
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			TestClass testClass = projectNode.TestProject.TestClasses["RootNamespace.Tests.MyTestFixture"];		
			
			TestMethodTreeNode methodNode = (TestMethodTreeNode)testFixtureNode.Nodes[0];
			TestMethod testMethod = testClass.TestMethods[0];
			testClass.TestMethods.Remove(testMethod);
			
			Assert.AreEqual(0, testFixtureNode.Nodes.Count);
			Assert.IsTrue(methodNode.IsDisposed);
			
			// Make sure the TestMethod.Dispose call removes all
			// event handlers by changing the TestMethod's test 
			// result and seeing if the test method node is
			// affected even though we have removed it from the tree.
			
			// Make sure the test method result is not already a failure.
			testMethod.Result = TestResultType.None;
			testMethod.Result = TestResultType.Failure;
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, 
				(TestTreeViewImageListIndex)methodNode.ImageIndex,
				"Disposed TestMethodTreeNode was affected by TestMethod result change");
		}
		
		/// <summary>
		/// Tests that when a test class is removed the removed class
		/// node is not changed when one of the class's methods is removed.
		/// The test makes sure that the TestClassTreeNode.Dispose
		/// removes the TestMethods.TestMethodRemoved event handler.
		/// </summary>
		[Test]
		public void ClassRemoved()
		{
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			TestClass testClass = projectNode.TestProject.TestClasses["RootNamespace.Tests.MyTestFixture"];		
			TestMethodTreeNode methodNode = (TestMethodTreeNode)testFixtureNode.Nodes[0];
			
			testFixtureNode.Expanding();
			
			// Sanity check - test fixture node should have one method node.
			Assert.AreEqual(1, testFixtureNode.Nodes.Count);
			
			projectNode.TestProject.TestClasses.Remove(testClass);
			
			// Method node should be disposed when parent class
			// node is disposed.
			Assert.IsTrue(methodNode.IsDisposed);
			
			// Make sure the TestClass.Dispose call removes all
			// event handlers.
			testClass.TestMethods.RemoveAt(0);
			
			Assert.AreEqual(1, testFixtureNode.Nodes.Count,
				"Should still have one child node.");
		}
		
		/// <summary>
		/// The test project tree node should be removed when it no longer
		/// references the NUnit.Framework assembly.
		/// </summary>
		[Test]
		public void TestProjectNUnitReferenceRemoved()
		{
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			ProjectService.RemoveProjectItem(projectNode.TestProject.Project, nunitFrameworkReferenceItem);
			
			treeView.ProjectReferencesChanged(projectNode.TestProject.Project);
			
			Assert.AreEqual(0, treeView.Nodes.Count);
		}
		
		[Test]
		public void UnknownProjectHasReferenceRemoved()
		{
			IProject project = new MockCSharpProject();
			treeView.ProjectReferencesChanged(project);
			
			Assert.AreEqual(1, treeView.Nodes.Count);
		}
		
		[Test]
		public void UnknownProjectHasNUnitReferenceAdded()
		{
			project = new MockCSharpProject();
			project.Name = "NewProject";
			nunitFrameworkReferenceItem = new ReferenceProjectItem(project);
			nunitFrameworkReferenceItem.Include = "NUnit.Framework";
			ProjectService.AddProjectItem(project, nunitFrameworkReferenceItem);
			
			treeView.ProjectReferencesChanged(project);
			
			ExtTreeNode newProjectNode = null;
			foreach (ExtTreeNode node in treeView.Nodes) {
				if (node.Text == "NewProject") {
					newProjectNode = node;
					break;
				}
			}
			
			Assert.AreEqual(2, treeView.Nodes.Count);
			Assert.IsNotNull(newProjectNode);
		}
		
		[Test]
		public void ProjectRemoved()
		{
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;
			treeView.RemoveProject(projectNode.TestProject.Project);
			
			Assert.AreEqual(0, treeView.Nodes.Count);
		}
		
		[Test]
		public void OwnerStateWhenProjectNodeSelected()
		{
			treeView.SelectedNode = rootNode;
			Assert.AreEqual(TestTreeView.TestTreeViewState.None, (TestTreeView.TestTreeViewState)treeView.InternalState);
		}
		
		[Test]
		public void OwnerStateWhenTestClassNodeSelected()
		{
			treeView.SelectedNode = testFixtureNode;
			Assert.AreEqual(TestTreeView.TestTreeViewState.SourceCodeItemSelected, (TestTreeView.TestTreeViewState)treeView.InternalState);
		}
		
		[Test]
		public void OwnerStateWhenTestMethodNodeSelected()
		{
			treeView.SelectedNode = testNode;
			Assert.AreEqual(TestTreeView.TestTreeViewState.SourceCodeItemSelected, (TestTreeView.TestTreeViewState)treeView.InternalState);
		}
		
		/// <summary>
		/// Tests that the TestProject.TestClasses.TestClassRemoved
		/// event handler is removed when the TestTreeNamespaceNode
		/// is disposed.
		/// </summary>
		[Test]
		public void RemoveClassAfterDisposingTestsNamespaceNode()
		{
			testsNamespaceNode.Dispose();
			testsNamespaceNode.Remove();
			
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;

			// Make sure the tests namespace node child nodes are 
			// unaffected when the test class is removed.
			Assert.AreEqual(1, testsNamespaceNode.Nodes.Count);
			projectNode.TestProject.TestClasses.Remove("RootNamespace.Tests.MyTestFixture");
			Assert.AreEqual(1, testsNamespaceNode.Nodes.Count);
		}
		
		/// <summary>
		/// Tests that the TestNamespaceTreeNode.TestClassCollection.TestClassRemoved
		/// event handler is removed when the TestTreeNamespaceNode
		/// is disposed.
		/// </summary>
		[Test]
		public void ChangeTestClassResultAfterDisposingTestsNamespaceNode()
		{
			testsNamespaceNode.Dispose();
			testsNamespaceNode.Remove();
			
			TestProjectTreeNode projectNode = (TestProjectTreeNode)rootNode;

			// Make sure the tests namespace node image index is  
			// unaffected when the test class test result is changed.
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)testsNamespaceNode.ImageIndex);
			TestClass testClass = projectNode.TestProject.TestClasses["RootNamespace.Tests.MyTestFixture"];
			testClass.Result = TestResultType.Failure;
			Assert.AreEqual(TestTreeViewImageListIndex.TestNotRun, (TestTreeViewImageListIndex)testsNamespaceNode.ImageIndex);
		}
	}
}
