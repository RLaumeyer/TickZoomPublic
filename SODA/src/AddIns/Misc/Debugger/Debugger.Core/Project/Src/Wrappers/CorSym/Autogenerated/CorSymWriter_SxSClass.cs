// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision: 2201 $</version>
// </file>

// This file is automatically generated - any changes will be lost

#pragma warning disable 1591

namespace Debugger.Wrappers.CorSym
{
	using System;
	
	
	public partial class CorSymWriter_SxSClass
	{
		
		private Debugger.Interop.CorSym.CorSymWriter_SxSClass wrappedObject;
		
		internal Debugger.Interop.CorSym.CorSymWriter_SxSClass WrappedObject
		{
			get
			{
				return this.wrappedObject;
			}
		}
		
		public CorSymWriter_SxSClass(Debugger.Interop.CorSym.CorSymWriter_SxSClass wrappedObject)
		{
			this.wrappedObject = wrappedObject;
			ResourceManager.TrackCOMObject(wrappedObject, typeof(CorSymWriter_SxSClass));
		}
		
		public static CorSymWriter_SxSClass Wrap(Debugger.Interop.CorSym.CorSymWriter_SxSClass objectToWrap)
		{
			if ((objectToWrap != null))
			{
				return new CorSymWriter_SxSClass(objectToWrap);
			} else
			{
				return null;
			}
		}
		
		~CorSymWriter_SxSClass()
		{
			object o = wrappedObject;
			wrappedObject = null;
			ResourceManager.ReleaseCOMObject(o, typeof(CorSymWriter_SxSClass));
		}
		
		public bool Is<T>() where T: class
		{
			System.Reflection.ConstructorInfo ctor = typeof(T).GetConstructors()[0];
			System.Type paramType = ctor.GetParameters()[0].ParameterType;
			return paramType.IsInstanceOfType(this.WrappedObject);
		}
		
		public T As<T>() where T: class
		{
			try {
				return CastTo<T>();
			} catch {
				return null;
			}
		}
		
		public T CastTo<T>() where T: class
		{
			return (T)Activator.CreateInstance(typeof(T), this.WrappedObject);
		}
		
		public static bool operator ==(CorSymWriter_SxSClass o1, CorSymWriter_SxSClass o2)
		{
			return ((object)o1 == null && (object)o2 == null) ||
			       ((object)o1 != null && (object)o2 != null && o1.WrappedObject == o2.WrappedObject);
		}
		
		public static bool operator !=(CorSymWriter_SxSClass o1, CorSymWriter_SxSClass o2)
		{
			return !(o1 == o2);
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override bool Equals(object o)
		{
			CorSymWriter_SxSClass casted = o as CorSymWriter_SxSClass;
			return (casted != null) && (casted.WrappedObject == wrappedObject);
		}
		
		
		public void Abort()
		{
			this.WrappedObject.Abort();
		}
		
		public void Close()
		{
			this.WrappedObject.Close();
		}
		
		public void CloseMethod()
		{
			this.WrappedObject.CloseMethod();
		}
		
		public void CloseNamespace()
		{
			this.WrappedObject.CloseNamespace();
		}
		
		public void CloseScope(uint endOffset)
		{
			this.WrappedObject.CloseScope(endOffset);
		}
		
		public void DefineConstant(System.IntPtr name, object value, uint cSig, ref byte signature)
		{
			this.WrappedObject.DefineConstant(name, value, cSig, ref signature);
		}
		
		public ISymUnmanagedDocumentWriter DefineDocument(System.IntPtr url, ref System.Guid language, ref System.Guid languageVendor, ref System.Guid documentType)
		{
			return ISymUnmanagedDocumentWriter.Wrap(this.WrappedObject.DefineDocument(url, ref language, ref languageVendor, ref documentType));
		}
		
		public void DefineField(uint parent, System.IntPtr name, uint attributes, uint cSig, ref byte signature, uint addrKind, uint addr1, uint addr2, uint addr3)
		{
			this.WrappedObject.DefineField(parent, name, attributes, cSig, ref signature, addrKind, addr1, addr2, addr3);
		}
		
		public void DefineGlobalVariable(System.IntPtr name, uint attributes, uint cSig, ref byte signature, uint addrKind, uint addr1, uint addr2, uint addr3)
		{
			this.WrappedObject.DefineGlobalVariable(name, attributes, cSig, ref signature, addrKind, addr1, addr2, addr3);
		}
		
		public void DefineLocalVariable(System.IntPtr name, uint attributes, uint cSig, ref byte signature, uint addrKind, uint addr1, uint addr2, uint addr3, uint startOffset, uint endOffset)
		{
			this.WrappedObject.DefineLocalVariable(name, attributes, cSig, ref signature, addrKind, addr1, addr2, addr3, startOffset, endOffset);
		}
		
		public void DefineParameter(System.IntPtr name, uint attributes, uint sequence, uint addrKind, uint addr1, uint addr2, uint addr3)
		{
			this.WrappedObject.DefineParameter(name, attributes, sequence, addrKind, addr1, addr2, addr3);
		}
		
		public void DefineSequencePoints(ISymUnmanagedDocumentWriter document, uint spCount, ref uint offsets, ref uint lines, ref uint columns, ref uint endLines, ref uint endColumns)
		{
			this.WrappedObject.DefineSequencePoints(document.WrappedObject, spCount, ref offsets, ref lines, ref columns, ref endLines, ref endColumns);
		}
		
		public void GetDebugInfo(ref uint pIDD, uint cData, out uint pcData, System.IntPtr data)
		{
			this.WrappedObject.GetDebugInfo(ref pIDD, cData, out pcData, data);
		}
		
		public void Initialize(object emitter, System.IntPtr filename, IStream pIStream, int fFullBuild)
		{
			this.WrappedObject.Initialize(emitter, filename, pIStream.WrappedObject, fFullBuild);
		}
		
		public void Initialize2(object emitter, System.IntPtr tempfilename, IStream pIStream, int fFullBuild, System.IntPtr finalfilename)
		{
			this.WrappedObject.Initialize2(emitter, tempfilename, pIStream.WrappedObject, fFullBuild, finalfilename);
		}
		
		public void OpenMethod(uint method)
		{
			this.WrappedObject.OpenMethod(method);
		}
		
		public void OpenNamespace(System.IntPtr name)
		{
			this.WrappedObject.OpenNamespace(name);
		}
		
		public uint OpenScope(uint startOffset)
		{
			return this.WrappedObject.OpenScope(startOffset);
		}
		
		public void RemapToken(uint oldToken, uint newToken)
		{
			this.WrappedObject.RemapToken(oldToken, newToken);
		}
		
		public void SetMethodSourceRange(ISymUnmanagedDocumentWriter startDoc, uint startLine, uint startColumn, ISymUnmanagedDocumentWriter endDoc, uint endLine, uint endColumn)
		{
			this.WrappedObject.SetMethodSourceRange(startDoc.WrappedObject, startLine, startColumn, endDoc.WrappedObject, endLine, endColumn);
		}
		
		public void SetScopeRange(uint scopeID, uint startOffset, uint endOffset)
		{
			this.WrappedObject.SetScopeRange(scopeID, startOffset, endOffset);
		}
		
		public void SetSymAttribute(uint parent, System.IntPtr name, uint cData, ref byte data)
		{
			this.WrappedObject.SetSymAttribute(parent, name, cData, ref data);
		}
		
		public void SetUserEntryPoint(uint entryMethod)
		{
			this.WrappedObject.SetUserEntryPoint(entryMethod);
		}
		
		public void UsingNamespace(System.IntPtr fullName)
		{
			this.WrappedObject.UsingNamespace(fullName);
		}
	}
}

#pragma warning restore 1591
