// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 4054 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.PythonBinding;
using IronPython.Compiler.Ast;
using NUnit.Framework;
using PythonBinding.Tests.Utils;

namespace PythonBinding.Tests.Designer
{
	/// <summary>
	/// Tests that the constructor arguments are returned when the first argument is
	/// an array.
	/// </summary>
	[TestFixture]
	public class DeserializeConstructorStringArrayTestFixture
	{		
		string code =  "System.Windows.Forms.ListViewItem(System.Array[System.String](\r\n" +
				"    [\"a\",\r\n" +
				"    \"sa\",\r\n" +
				"    \"sa2\"]))\r\n";

		List<object> args;
		
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
			MockComponentCreator componentCreator = new MockComponentCreator();
			CallExpression callExpression = PythonParserHelper.GetCallExpression(code);			
			PythonCodeDeserializer deserializer = new PythonCodeDeserializer(componentCreator);
			args = deserializer.GetArguments(callExpression);
		}
		
		[Test]
		public void OneArgument()
		{
			Assert.AreEqual(1, args.Count);
		}

		[Test]
		public void ArgumentIsStringArray()
		{
			string[] array = new string[0];
			Assert.IsInstanceOf(array.GetType(), args[0]);
		}
	}
}
