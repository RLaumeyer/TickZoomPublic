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
	
	
	public partial class IStream
	{
		
		private Debugger.Interop.CorSym.IStream wrappedObject;
		
		internal Debugger.Interop.CorSym.IStream WrappedObject
		{
			get
			{
				return this.wrappedObject;
			}
		}
		
		public IStream(Debugger.Interop.CorSym.IStream wrappedObject)
		{
			this.wrappedObject = wrappedObject;
			ResourceManager.TrackCOMObject(wrappedObject, typeof(IStream));
		}
		
		public static IStream Wrap(Debugger.Interop.CorSym.IStream objectToWrap)
		{
			if ((objectToWrap != null))
			{
				return new IStream(objectToWrap);
			} else
			{
				return null;
			}
		}
		
		~IStream()
		{
			object o = wrappedObject;
			wrappedObject = null;
			ResourceManager.ReleaseCOMObject(o, typeof(IStream));
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
		
		public static bool operator ==(IStream o1, IStream o2)
		{
			return ((object)o1 == null && (object)o2 == null) ||
			       ((object)o1 != null && (object)o2 != null && o1.WrappedObject == o2.WrappedObject);
		}
		
		public static bool operator !=(IStream o1, IStream o2)
		{
			return !(o1 == o2);
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override bool Equals(object o)
		{
			IStream casted = o as IStream;
			return (casted != null) && (casted.WrappedObject == wrappedObject);
		}
		
		
		public uint RemoteRead(out byte pv, uint cb)
		{
			uint pcbRead;
			this.WrappedObject.RemoteRead(out pv, cb, out pcbRead);
			return pcbRead;
		}
		
		public uint RemoteWrite(ref byte pv, uint cb)
		{
			uint pcbWritten;
			this.WrappedObject.RemoteWrite(ref pv, cb, out pcbWritten);
			return pcbWritten;
		}
		
		public Debugger.Interop.CorSym._ULARGE_INTEGER RemoteSeek(Debugger.Interop.CorSym._LARGE_INTEGER dlibMove, uint dwOrigin)
		{
			Debugger.Interop.CorSym._ULARGE_INTEGER plibNewPosition;
			this.WrappedObject.RemoteSeek(dlibMove, dwOrigin, out plibNewPosition);
			return plibNewPosition;
		}
		
		public void SetSize(Debugger.Interop.CorSym._ULARGE_INTEGER libNewSize)
		{
			this.WrappedObject.SetSize(libNewSize);
		}
		
		public Debugger.Interop.CorSym._ULARGE_INTEGER RemoteCopyTo(IStream pstm, Debugger.Interop.CorSym._ULARGE_INTEGER cb, out Debugger.Interop.CorSym._ULARGE_INTEGER pcbRead)
		{
			Debugger.Interop.CorSym._ULARGE_INTEGER pcbWritten;
			this.WrappedObject.RemoteCopyTo(pstm.WrappedObject, cb, out pcbRead, out pcbWritten);
			return pcbWritten;
		}
		
		public void Commit(uint grfCommitFlags)
		{
			this.WrappedObject.Commit(grfCommitFlags);
		}
		
		public void Revert()
		{
			this.WrappedObject.Revert();
		}
		
		public void LockRegion(Debugger.Interop.CorSym._ULARGE_INTEGER libOffset, Debugger.Interop.CorSym._ULARGE_INTEGER cb, uint dwLockType)
		{
			this.WrappedObject.LockRegion(libOffset, cb, dwLockType);
		}
		
		public void UnlockRegion(Debugger.Interop.CorSym._ULARGE_INTEGER libOffset, Debugger.Interop.CorSym._ULARGE_INTEGER cb, uint dwLockType)
		{
			this.WrappedObject.UnlockRegion(libOffset, cb, dwLockType);
		}
		
		public void Stat(out Debugger.Interop.CorSym.tagSTATSTG pstatstg, uint grfStatFlag)
		{
			this.WrappedObject.Stat(out pstatstg, grfStatFlag);
		}
		
		public IStream Clone()
		{
			IStream ppstm;
			Debugger.Interop.CorSym.IStream out_ppstm;
			this.WrappedObject.Clone(out out_ppstm);
			ppstm = IStream.Wrap(out_ppstm);
			return ppstm;
		}
	}
}

#pragma warning restore 1591
