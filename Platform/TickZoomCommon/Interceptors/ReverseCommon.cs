#region Copyright
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
using System.Diagnostics;
using System.Drawing;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	public class ReverseCommon : StrategySupport
	{
		[Diagram(AttributeExclude=true)]
		public class InternalOrders {
			public LogicalOrder buyMarket;
			public LogicalOrder sellMarket;
			public LogicalOrder buyStop;
			public LogicalOrder sellStop;
			public LogicalOrder buyLimit;
			public LogicalOrder sellLimit;
		}
		private InternalOrders orders = new InternalOrders();
		
		private bool enableWrongSideOrders = false;
		private bool isNextBar = false;
		
		public ReverseCommon(Strategy strategy) : base(strategy) {
		}
		
		public void OnInitialize()
		{
			if( IsDebug) Log.Debug("OnInitialize()");
			Strategy.Drawing.Color = Color.Black;
			orders.buyMarket = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyMarket.Type = OrderType.BuyMarket;
			orders.buyMarket.TradeDirection = TradeDirection.Reverse;
			orders.sellMarket = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellMarket.Type = OrderType.SellMarket;
			orders.sellMarket.TradeDirection = TradeDirection.Reverse;
			orders.buyStop = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyStop.Type = OrderType.BuyStop;
			orders.buyStop.TradeDirection = TradeDirection.Reverse;
			orders.sellStop = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellStop.Type = OrderType.SellStop;
			orders.sellStop.TradeDirection = TradeDirection.Reverse;
			orders.buyLimit = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyLimit.Type = OrderType.BuyLimit;
			orders.buyLimit.TradeDirection = TradeDirection.Reverse;
			orders.sellLimit = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellLimit.Type = OrderType.SellLimit;
			orders.sellLimit.TradeDirection = TradeDirection.Reverse;
			Strategy.AddOrder( orders.buyMarket);
			Strategy.AddOrder( orders.sellMarket);
			Strategy.AddOrder( orders.buyStop);
			Strategy.AddOrder( orders.sellStop);
			Strategy.AddOrder( orders.buyLimit);
			Strategy.AddOrder( orders.sellLimit);
		}
		
	        public void CancelOrders()
	        {
	        	orders.buyMarket.Status = OrderStatus.AutoCancel;
	            orders.sellMarket.Status = OrderStatus.AutoCancel;
	        	orders.buyStop.Status = OrderStatus.AutoCancel;
	            orders.sellStop.Status = OrderStatus.AutoCancel;
	            orders.buyLimit.Status = OrderStatus.AutoCancel;
	            orders.sellLimit.Status = OrderStatus.AutoCancel;
	        }
		
			private void LogEntry(string description) {
				if( Strategy.Chart.IsDynamicUpdate) {
		        		if( IsNotice) Log.Notice("Bar="+Strategy.Chart.DisplayBars.CurrentBar+", " + description);
				} else {
		        		if( IsDebug) Log.Debug("Bar="+Strategy.Chart.DisplayBars.CurrentBar+", " + description);
				}
			}
		
	        #region Properties		
	        public void SellMarket() {
	        	SellMarket(1);
	        }
	        
	        public void SellMarket( double lots) {
	        	if( Strategy.Position.IsShort) {
	        		throw new ApplicationException("Cannot sell when reversing from a short position.");
	        	}
	        	orders.sellMarket.Price = 0;
	        	orders.sellMarket.Position = (int) lots;
	        	if( isNextBar) {
	        	orders.sellMarket.Status = OrderStatus.NextBar;
	        	} else {
	        		orders.sellMarket.Status = OrderStatus.Active;
	        	}
	        }
	        
	        public void BuyMarket() {
	        	BuyMarket( 1);
	        }
	        
	        public void BuyMarket(double lots) {
	        	if( Strategy.Position.IsLong) {
	        		throw new ApplicationException("Cannot buy when reversing from a long position.");
	        	}
	        	orders.buyMarket.Price = 0;
	        	orders.buyMarket.Position = (int) lots;
	        	if( isNextBar) {
	        		orders.buyMarket.Status = OrderStatus.NextBar;
	        	} else {
	        		orders.buyMarket.Status = OrderStatus.Active;
	        	}
	        }
	        
	        public void BuyLimit( double price) {
	        	BuyLimit( price, 1);
	        }
	        	
	        /// <summary>
	        /// Create a active buy limit order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	
	        public void BuyLimit( double price, double lots) {
	        	orders.buyLimit.Price = price;
	        	orders.buyLimit.Position = (int) lots;
	        	if( isNextBar) {
	        	orders.buyLimit.Status = OrderStatus.NextBar;
	        	} else {
	        		orders.buyLimit.Status = OrderStatus.Active;
	        	}
			}
	        
	        public void SellLimit( double price) {
	        	SellLimit( price, 1);
	        }
	        	
	        /// <summary>
	        /// Create a active sell limit order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	
	        public void SellLimit( double price, double lots) {
	        	orders.sellLimit.Price = price;
	        	orders.sellLimit.Position = (int) lots;
	        	if( isNextBar) {
	        	orders.sellLimit.Status = OrderStatus.NextBar;
	        	} else {
	        		orders.sellLimit.Status = OrderStatus.Active;
	        	}
			}
	        
	        public void BuyStop( double price) {
	        	BuyStop( price, 1);
	        }
	        
	        /// <summary>
	        /// Create a active buy stop order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	
	        public void BuyStop( double price, double lots) {
	        	orders.buyStop.Price = price;
	        	orders.buyStop.Position = (int) lots;
	        	if( isNextBar) {
	        	orders.buyStop.Status = OrderStatus.NextBar;
	        	} else {
	        		orders.buyStop.Status = OrderStatus.Active;
	        	}
			}
	
	        public void SellStop( double price) {
	        	SellStop( price, 1);
	        }
	        
	        /// <summary>
	        /// Create a active sell stop order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	        
	        public void SellStop( double price, double lots) {
	        	orders.sellStop.Price = price;
	        	orders.sellStop.Position = (int) lots;
	        	if( isNextBar) {
	        	orders.sellStop.Status = OrderStatus.NextBar;
	        	} else {
	        		orders.sellStop.Status = OrderStatus.Active;
	        	}
	        }
	        
		#endregion
	
		public override string ToString()
		{
			return Strategy.FullName;
		}
		
		public bool EnableWrongSideOrders {
			get { return enableWrongSideOrders; }
			set { enableWrongSideOrders = value; }
		}
	
		public bool HasBuyOrder {
			get {
				return orders.buyStop.IsActive || orders.buyStop.IsNextBar || 
					orders.buyLimit.IsActive || orders.buyLimit.IsNextBar ||
					orders.buyMarket.IsActive || orders.buyMarket.IsNextBar;
			}
		}
		
		public bool HasSellOrder {
			get {
				return orders.sellStop.IsActive || orders.sellStop.IsNextBar || 
					orders.sellLimit.IsActive || orders.sellLimit.IsNextBar || 
					orders.sellMarket.IsActive || orders.sellMarket.IsNextBar;
			}
		}
		
		internal InternalOrders Orders {
			get { return orders; }
			set { orders = value; }
		}
		
		internal bool IsNextBar {
			get { return isNextBar; }
			set { isNextBar = value; }
		}
	}
}
