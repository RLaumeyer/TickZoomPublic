﻿#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

using TickZoom.Api;

namespace TickZoom.MBTFIX
{
	public class MBTProvider : Provider
	{
		MBTFIXProvider fixProvider = new MBTFIXProvider();
		MBTQuotesProvider quotesProvider = new MBTQuotesProvider();
		
		public void SendEvent( Receiver receiver, SymbolInfo symbol, int eventType, object eventDetail) {
			switch( (EventType) eventType) {
				case EventType.Connect:
					quotesProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					fixProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					break;
				case EventType.Disconnect:
					quotesProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					fixProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					break;
				case EventType.StartSymbol:
					quotesProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					fixProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					break;
				case EventType.StopSymbol:
					quotesProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					fixProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					break;
				case EventType.PositionChange:
					fixProvider.SendEvent(receiver,symbol,eventType,eventDetail);
					break;
				case EventType.Terminate:
					Dispose();
					break; 
				default:
					throw new ApplicationException("Unexpected event type: " + (EventType) eventType);
			}
		}
		
	 	private volatile bool isDisposed = false;
	    public void Dispose() 
	    {
	        Dispose(true);
	        GC.SuppressFinalize(this);      
	    }
	
	    protected virtual void Dispose(bool disposing)
	    {
       		if( !isDisposed) {
	            isDisposed = true;   
	            if (disposing) {
	            	fixProvider.Dispose();
	            	quotesProvider.Dispose();
	            }
    		}
	    }
	}
}