﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 5343 $</version>
// </file>

using System;
using ICSharpCode.RubyBinding;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Dom;
using NUnit.Framework;
using RubyBinding.Tests.Utils;

namespace RubyBinding.Tests.Designer
{
	/// <summary>
	/// Tests the RubyDesignerGenerator uses the text editor properties for indentation when
	/// inserting an event handler into the document.
	/// </summary>
	[TestFixture]
	public class InsertEventHandlerWithSpaceIndentTestFixture : InsertEventHandlerTestFixtureBase
	{
		public override void AfterSetUpFixture()
		{
			textEditorProperties.ConvertTabsToSpaces = true;
			textEditorProperties.IndentationSize = 4;
			MockEventDescriptor mockEventDescriptor = new MockEventDescriptor("Click");
			insertedEventHandler = generator.InsertComponentEvent(null, mockEventDescriptor, "button1_click", String.Empty, out file, out position);
		}
		
		[Test]
		public void ExpectedCodeAfterEventHandlerInserted()
		{
			string expectedCode = 
				"require \"System.Windows.Forms\"\r\n" +
				"\r\n" +
				"class MainForm < Form\r\n" +
				"    def initialize()\r\n" +
				"        self.InitializeComponents()\r\n" +
				"    end\r\n" +
				"    \r\n" +
				"    def InitializeComponents()\r\n" +
				"        @button1 = System::Windows::Forms::Button.new()\r\n" +
				"        self.Controls.Add(@button1)\r\n" +
				"    end\r\n" +
				"\r\n" +
				"    def button1_click(sender, e)\r\n" +
				"        \r\n" +
				"    end\r\n" +
				"end";
			Assert.AreEqual(expectedCode, viewContent.DesignerCodeFileContent, viewContent.DesignerCodeFileContent);
		}
		
		/// <summary>
		/// Note that the text editor code already has the
		/// statement: 
		/// 
		/// "self._button1.Click += button1_click"
		/// 
		/// This is generated by the form designer framework and not
		/// by the designer generator.
		/// </summary>
		protected override string GetTextEditorCode()
		{
			return "require \"System.Windows.Forms\"\r\n" +
					"\r\n" +
					"class MainForm < Form\r\n" +
					"    def initialize()\r\n" +
					"        self.InitializeComponents()\r\n" +
					"    end\r\n" +
					"    \r\n" +
					"    def InitializeComponents()\r\n" +
					"        @button1 = System::Windows::Forms::Button.new()\r\n" +
					"        self.Controls.Add(@button1)\r\n" +
					"    end\r\n" +
					"end";
		}
	}
}
