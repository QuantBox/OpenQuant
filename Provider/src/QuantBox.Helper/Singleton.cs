using System.Collections;

namespace QuantBox.Helper
{
    public sealed class Singleton
    {
        public const string KEY_INSTRUMENT      = "Instrument";
        public const string KEY_MARKET_DATA     = "MarketData";
        public const string KEY_COMMISSION_RATE = "CommissionRate";
        public const string KEY_MARGIN_RATE     = "MarginRate";

        private static readonly Singleton instance = new Singleton();
        private Singleton()
        {
        }
        public static Singleton GetInstance()
        {
            return instance;
        }

        public Hashtable global = new Hashtable();
    }
}
