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
using System.Threading;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	public class FillSimulatorPhysical : FillSimulator
	{
		private static readonly Log staticLog = Factory.SysLog.GetLogger(typeof(FillSimulatorPhysical));
		private static readonly bool trace = staticLog.IsTraceEnabled;
		private static readonly bool debug = staticLog.IsDebugEnabled;
		private static readonly bool notice = staticLog.IsNoticeEnabled;
		private Log log;

		private Dictionary<string,PhysicalOrder> orderMap = new Dictionary<string, PhysicalOrder>();
		private ActiveList<PhysicalOrder> increaseOrders = new ActiveList<PhysicalOrder>();
		private ActiveList<PhysicalOrder> decreaseOrders = new ActiveList<PhysicalOrder>();
		private ActiveList<PhysicalOrder> marketOrders = new ActiveList<PhysicalOrder>();
		private NodePool<PhysicalOrder> nodePool = new NodePool<PhysicalOrder>();
		private object orderMapLocker = new object();
		private bool isOpenTick = false;
		private TimeStamp openTime;

		private Action<PhysicalFill> onPhysicalFill;
		private Action<int> onPositionChange;
		private bool useSyntheticMarkets = true;
		private bool useSyntheticStops = true;
		private bool useSyntheticLimits = true;
		private SymbolInfo symbol;
		private int actualPosition = 0;
		private TickSync tickSync;
		private TickIO currentTick = Factory.TickUtil.TickIO();
		private PhysicalOrderHandler confirmOrders;
		private bool isBarData = false;
		private bool createSimulatedFills = false;
		// Randomly rotate the partial fills but using a fixed
		// seed so that test results are reproducable.
		private Random random = new Random(1234);
		
		public FillSimulatorPhysical(string name, SymbolInfo symbol, bool createSimulatedFills)
		{
			this.symbol = symbol;
			this.tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
			this.createSimulatedFills = createSimulatedFills;
			this.log = Factory.SysLog.GetLogger(typeof(FillSimulatorPhysical).FullName + "." + symbol.Symbol.StripInvalidPathChars() + "." + name);
		}
		
		public void OnOpen(Tick tick) {
			if( trace) log.Trace("OnOpen("+tick+")");
			isOpenTick = true;
			openTime = tick.Time;
			currentTick.Inject( tick.Extract());
		}
		
		public Iterable<PhysicalOrder> GetActiveOrders(SymbolInfo symbol) {
			ActiveList<PhysicalOrder> activeOrders = new ActiveList<PhysicalOrder>();
			activeOrders.AddLast(increaseOrders);
			activeOrders.AddLast(decreaseOrders);
			activeOrders.AddLast(marketOrders);
			return activeOrders;
		}
	
		public void OnChangeBrokerOrder(PhysicalOrder order, object origBrokerOrder)
		{
			if( debug) log.Debug("OnChangeBrokerOrder( " + order + ")");
			CancelBrokerOrder( origBrokerOrder);
			CreateBrokerOrder( order);
			ProcessOrders();
			if( confirmOrders != null) confirmOrders.OnChangeBrokerOrder(order, origBrokerOrder);
		}
		
		public PhysicalOrder GetOrderById( string orderId) {
			LogOpenOrders();
			PhysicalOrder order;
			lock( orderMapLocker) {
				if( !orderMap.TryGetValue( orderId, out order)) {
					throw new ApplicationException( symbol + ": Cannot find physical order by id: " + orderId);
				}
			}
			return order;
		}
		
		private PhysicalOrder CancelBrokerOrder(object origBrokerOrder) {
			var oldOrderId = (string) origBrokerOrder;
			var oldOrder = GetOrderById(oldOrderId);
			RemoveActive( oldOrder);
			lock( orderMapLocker) {
				orderMap.Remove( oldOrderId);
			}
			LogOpenOrders();
			return oldOrder;
		}
		
		private void CreateBrokerOrder(PhysicalOrder order) {
			lock( orderMapLocker) {
				orderMap.Add((string)order.BrokerOrder,order);
			}
			SortAdjust(order);
		}
		
		public void OnCreateBrokerOrder(PhysicalOrder order)
		{
			if( debug) log.Debug("OnCreateBrokerOrder( " + order + ")");
			CreateBrokerOrder(order);
			ProcessOrders();
			if( confirmOrders != null) confirmOrders.OnCreateBrokerOrder(order);
		}
		
		public void OnCancelBrokerOrder(SymbolInfo symbol, object origBrokerOrder)
		{
			if( debug) log.Debug("OnCancelBrokerOrder( " + origBrokerOrder + ")");
			var order = CancelBrokerOrder(origBrokerOrder);
			if( confirmOrders != null) confirmOrders.OnCancelBrokerOrder(symbol, origBrokerOrder);
		}
		
		public void ProcessOrders() {
			ProcessOrdersInternal( currentTick);
		}
		
		public void StartTick(Tick lastTick)
		{
			if( trace) log.Trace("StartTick("+lastTick+")");
			currentTick.Inject( lastTick.Extract());
		}
		
		private void ProcessOrdersInternal(Tick tick) {
			if( isOpenTick && tick.Time > openTime) {
				if( trace) {
					if( isOpenTick) {
						log.Trace( "ProcessOrders( " + symbol + ", " + tick + " ) [OpenTick]") ;
					} else {
						log.Trace( "ProcessOrders( " + symbol + ", " + tick + " )") ;
					}
				}
				isOpenTick = false;
			}
			if( symbol == null) {
				throw new ApplicationException("Please set the Symbol property for the " + GetType().Name + ".");
			}
			var next = marketOrders.First;
			for( var node = next; node != null; node = node.Next) {
				var order = node.Value;
				OnProcessOrder(order, tick);
			}
			next = increaseOrders.First;
			for( var node = next; node != null; node = node.Next) {
				var order = node.Value;
				OnProcessOrder(order, tick);
			}
			next = decreaseOrders.First;
			for( var node = next; node != null; node = node.Next) {
				var order = node.Value;
				OnProcessOrder(order, tick);
			}
		}
		
		private void LogOpenOrders() {
			if( trace) {
				log.Trace( "Found " + orderMap.Count + " open orders for " + symbol + ":");
				lock( orderMapLocker) {
					foreach( var kvp in orderMap) {
						var order = kvp.Value;
						log.Trace( order.ToString());
					}
				}
			}
		}
		
		private void SortAdjust(PhysicalOrder order) {
			switch( order.Type) {
				case OrderType.BuyLimit:					
				case OrderType.SellStop:
					SortAdjust( decreaseOrders, order, (x,y) => y.Price - x.Price);
					break;
				case OrderType.SellLimit:
				case OrderType.BuyStop:
					SortAdjust( increaseOrders, order, (x,y) => x.Price - y.Price);
					break;
				case OrderType.BuyMarket:
				case OrderType.SellMarket:
					Adjust( marketOrders, order);
					break;
				default:
					throw new ApplicationException("Unexpected order type: " + order.Type);
			}
		}
		
		private void RemoveActive(PhysicalOrder order) {
			Remove( increaseOrders, order);
			Remove( decreaseOrders, order);
			Remove( marketOrders, order);
		}
		
		private void Adjust(ActiveList<PhysicalOrder> list, PhysicalOrder order) {
			if( !list.Contains(order)) {
				var node = nodePool.Create(order);
				list.AddLast(node);
			}
		}
		
		private void Remove(ActiveList<PhysicalOrder> list, PhysicalOrder order) {
			lock( list.Locker) {
				var node = list.Find(order);
				if( node != null) {
					list.Remove(node);
					nodePool.Free(node);
				}
			}
		}
		
		private void SortAdjust(ActiveList<PhysicalOrder> list, PhysicalOrder order, Func<PhysicalOrder,PhysicalOrder,double> compare) {
			if( !list.Contains(order)) {
				var newNode = nodePool.Create(order);
				bool found = false;
				var next = list.First;
				for( var node = next; node != null; node = next) {
					next = node.Next;
					var other = node.Value;
					if( object.ReferenceEquals(order,other)) {
						found = true;
						break;
					} else {
						var result = compare(order,other);
						if( result < 0) {
							list.AddBefore(node,newNode);
							found = true;
							break;
						}
					}
				}
				if( !found) {
					list.AddLast(newNode);
				}
			}
		}
		
		private void OnProcessOrder(PhysicalOrder order, Tick tick)
		{
			switch (order.Type) {
				case OrderType.SellMarket:
					ProcessSellMarket(order, tick);
					break;
				case OrderType.SellStop:
					ProcessSellStop(order, tick);
					break;
				case OrderType.SellLimit:
					ProcessSellLimit(order, tick);
					break;
				case OrderType.BuyMarket:
					ProcessBuyMarket(order, tick);
					break;
				case OrderType.BuyStop:
					ProcessBuyStop(order, tick);
					break;
				case OrderType.BuyLimit:
					ProcessBuyLimit(order, tick);
					break;
			}
		}
		private bool ProcessBuyStop(PhysicalOrder order, Tick tick)
		{
			bool retVal = false;
			long price = tick.IsTrade ? tick.lPrice : tick.lAsk;
			if (price >= order.Price.ToLong()) {
				CreatePhysicalFillHelper(order.Size, price.ToDouble(), tick.Time, tick.UtcTime, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessSellStop(PhysicalOrder order, Tick tick)
		{
			bool retVal = false;
			long price = tick.IsQuote ? tick.lBid : tick.lPrice;
			if (price <= order.Price.ToLong()) {
				CreatePhysicalFillHelper(-order.Size, price.ToDouble(), tick.Time, tick.UtcTime, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessBuyMarket(PhysicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			CreatePhysicalFillHelper(order.Size, price, tick.Time, tick.UtcTime, order);
			return true;
		}

		private bool ProcessBuyLimit(PhysicalOrder order, Tick tick)
		{
			long orderPrice = order.Price.ToLong();
			long price = tick.IsQuote ? tick.lAsk : tick.lPrice;
			bool isFilled = false;
			if (price <= orderPrice) {
				isFilled = true;
			} else if (tick.IsTrade && tick.lPrice < orderPrice) {
				price = orderPrice;
				isFilled = true;
			}
			if (isFilled) {
				CreatePhysicalFillHelper(order.Size, price.ToDouble(), tick.Time, tick.UtcTime, order);
			}
			return isFilled;
		}

		private bool ProcessSellMarket(PhysicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			CreatePhysicalFillHelper(-order.Size, price, tick.Time, tick.UtcTime, order);
			return true;
		}

		private bool ProcessSellLimit(PhysicalOrder order, Tick tick)
		{
			long orderPrice = order.Price.ToLong();
			long price = tick.IsQuote ? tick.lBid : tick.lPrice;
			bool isFilled = false;
			if (price >= orderPrice) {
				isFilled = true;
			} else if (tick.IsTrade && tick.lPrice > orderPrice) {
				price = orderPrice;
				isFilled = true;
			}
			if (isFilled) {
				CreatePhysicalFillHelper(-order.Size, price.ToDouble(), tick.Time, tick.UtcTime, order);
			}
			return isFilled;
		}
		
		private void CreatePhysicalFillHelper(int size, double price, TimeStamp time, TimeStamp utcTime, PhysicalOrder order) {
			if( debug) log.Debug("Filling order: " + order );
			var split = random.Next(3)+1;
			var partial = size / split;
			while( order.Size > 0) {
				order.Size -= Math.Abs(partial);
				if( order.Size < Math.Abs(partial)) {
					partial += Math.Sign(partial) * order.Size;
					order.Size = 0;
				}
				if( order.Size == 0) {
					CancelBrokerOrder(order.BrokerOrder);
				}
				CreateSingleFill( partial, price, time, utcTime, order);
			}
		}
	
		private void CreateSingleFill(int size, double price, TimeStamp time, TimeStamp utcTime, PhysicalOrder order) {
			if( debug) log.Debug("Changing actual position from " + this.actualPosition + " to " + (actualPosition+size) + ". Fill size is " + size);
			this.actualPosition += size;
			if( onPositionChange != null) {
				onPositionChange( actualPosition);
			}
			var fill = new PhysicalFillDefault(size,price,time,utcTime,order,createSimulatedFills);
			if( debug) log.Debug("Fill: " + fill );
			if( onPhysicalFill == null) {
				throw new ApplicationException("Please set the OnPhysicalFill property.");
			} else {
				if( SyncTicks.Enabled) tickSync.AddPhysicalFill(fill);
				onPhysicalFill(fill);
			}
		}
		
		public bool UseSyntheticLimits {
			get { return useSyntheticLimits; }
			set { useSyntheticLimits = value; }
		}
		
		public bool UseSyntheticStops {
			get { return useSyntheticStops; }
			set { useSyntheticStops = value; }
		}
		
		public bool UseSyntheticMarkets {
			get { return useSyntheticMarkets; }
			set { useSyntheticMarkets = value; }
		}
		
		public Action<PhysicalFill> OnPhysicalFill {
			get { return onPhysicalFill; }
			set { onPhysicalFill = value; }
		}
		
		public int GetActualPosition(SymbolInfo symbol) {
			return actualPosition;
		}
		
		public int ActualPosition {
			get { return actualPosition; }
			set { if( actualPosition != value) {
					if( debug) log.Debug("Setter: ActualPosition changed from " + actualPosition + " to " + value);
					actualPosition = value;
					if( onPositionChange != null) {
						onPositionChange( actualPosition);
					}
				}
			}
		}
		
		public Action<int> OnPositionChange {
			get { return onPositionChange; }
			set { onPositionChange = value; }
		}
		
		public PhysicalOrderHandler ConfirmOrders {
			get { return confirmOrders; }
			set { confirmOrders = value; }
		}
		
		public bool IsBarData {
			get { return isBarData; }
			set { isBarData = value; }
		}
	}
}

