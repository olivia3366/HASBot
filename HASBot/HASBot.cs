using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class HASBot : Robot
    {

        [Parameter("Quantity (Lots)", DefaultValue = 0.1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        Position currentPosition = null;

        protected override void OnBar()
        {
            HASCandle candle1 = new HASCandle(MarketSeries, 1);
            HASCandle candle2 = new HASCandle(MarketSeries, 2);
            HASCandle candle3 = new HASCandle(MarketSeries, 3);

            currentPosition = Positions.Find("HAS", Symbol);

            if (currentPosition == null && candle1.Bull && !candle2.Bull)
            {
                ExecuteOrder(TradeType.Buy, "HAS", 100, 150);
            }

            if (currentPosition != null && currentPosition.TradeType == TradeType.Sell)
            {
                ClosePosition(currentPosition);
            }

            if (currentPosition == null && !candle1.Bull && candle2.Bull)
            {
                ExecuteOrder(TradeType.Sell, "HAS", 100, 150);
            }

            if (currentPosition != null && currentPosition.TradeType == TradeType.Buy)
            {
                ClosePosition(currentPosition);
            }
            Print(candle1.ToString());
        }

        private void ExecuteOrder(TradeType tradeType, string label, double stopLoss, double takeProfit)
        {
            var volumeInUnits = Symbol.QuantityToVolume(Quantity);
            var position = Positions.Find(label, Symbol, tradeType);
            if (position == null)
            {
                var result = ExecuteMarketOrder(tradeType, Symbol, volumeInUnits, label, stopLoss, takeProfit);

                if (result.Error == ErrorCode.NoMoney)
                    Stop();
            }
        }

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }


    }

    public class HASCandle
    {
        public double High { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Low { get; set; }
        public DateTime OpenTime { get; set; }
        public bool Bull { get; set; }
        public double BodySize { get; set; }
        public double ShadeLow { get; set; }
        public double ShadeHigh { get; set; }
        public double HighLow { get; set; }

        public HASCandle(MarketSeries marketSeries, int index)
        {
            var open = marketSeries.Open.Last(index);
            var high = marketSeries.High.Last(index);
            var low = marketSeries.Low.Last(index);
            var close = marketSeries.Close.Last(index);

            var haClose = (open + high + low + close) / 4;
            double haOpen;
            if (index > 0)
                haOpen = (marketSeries.Open.Last(index + 1) + marketSeries.Close.Last(index + 1)) / 2;
            else
                haOpen = (open + close) / 2;

            var haHigh = Math.Max(Math.Max(high, haOpen), haClose);
            var haLow = Math.Min(Math.Min(low, haOpen), haClose);

            High = haHigh;
            Open = haOpen;
            Close = haClose;
            Low = haLow;
            OpenTime = marketSeries.OpenTime.Last(index);

            Bull = Open < Close;
            BodySize = Math.Abs(Open - Close);
            ShadeHigh = High - Open;
            ShadeLow = Close - Low;
            if (Bull)
            {
                ShadeHigh = High - Close;
                ShadeLow = Open - Low;
            }
            HighLow = High - Low;
        }

        public override string ToString()
        {
            return string.Format("Candle High {0}, Open {1}, Close {2}, Low {3}, OpenTime {4}, Bull {5}, BodySize {6}, ShadeLow {7}, ShadeHigh {8}, HighLow {9}", High, Open, Close, Low, OpenTime, Bull, BodySize, ShadeLow, ShadeHigh,
            HighLow);
        }

    }
}
