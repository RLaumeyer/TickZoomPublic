// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 4065 $</version>
// </file>

using System;
using System.Reflection;
using System.Reflection.Emit;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using ICSharpCode.Python.Build.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using NUnit.Framework;

namespace Python.Build.Tasks.Tests
{
	/// <summary>
	/// Tests that an IOException is caught and logged.
	/// </summary>
	[TestFixture]
	public class IOErrorTestFixture
	{
		MockPythonCompiler mockCompiler;
		DummyPythonCompilerTask compiler;
		bool success;
		
		[SetUp]
		public void Init()
		{
			mockCompiler = new MockPythonCompiler();
			compiler = new DummyPythonCompilerTask(mockCompiler, @"C:\Projects\MyProject");
			compiler.TargetType = "Exe";
			compiler.OutputAssembly = "test.exe";
			
			TaskItem sourceFile = new TaskItem(@"D:\Projects\MyProject\test.py");
			compiler.Sources = new ITaskItem[] {sourceFile};
			
			mockCompiler.ThrowExceptionAtCompile = PythonOps.IOError("Could not find main file test.py");
			
			success = compiler.Execute();
		}
		
		[Test]
		public void ExecuteFailed()
		{
			Assert.IsFalse(success);
		}
		
		[Test]
		public void IsExceptionMessageLogged()
		{
			Assert.AreEqual("Could not find main file test.py", compiler.LoggedErrorMessage);
		}
	}
}
