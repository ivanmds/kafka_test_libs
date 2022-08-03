using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Bankly.Sdk.Kafka.Contracts
{
    /// <summary>
    /// Currency codes based on https://pt.wikipedia.org/wiki/ISO_4217 and https://en.wikipedia.org/wiki/List_of_cryptocurrencies
    /// </summary>
    [JsonConverter(typeof(CurrencyCodeConverter))]
    internal sealed class Currency : IEquatable<Currency> //TODO, analyse to use Bankly.Sdk.Contracts
    {
        #region Currencies

        private IDictionary<string, int> _iso4217Currencies;

        private ICollection<string> _cryptoCurrencies;


        #endregion

        public static readonly Currency BRL = new Currency("BRL");
        public static readonly Currency USD = new Currency("USD");
        public static readonly Currency BTC = new Currency("BTC");
        public static readonly Currency ETH = new Currency("ETH");

        public Currency(string code)
        {
            Initialize();
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code), $"{nameof(code)} can not be null or empty or whitespace");

            var cryptoCurrency = _cryptoCurrencies.FirstOrDefault(code.Equals);
            IsIso4217Currency = _iso4217Currencies.TryGetValue(code, out var number);
            IsCryptoCurrency = cryptoCurrency != null;

            if (IsIso4217Currency)
            {
                Code = code;
                Number = number;
            }
            else Code = IsCryptoCurrency ? code : throw new Exception($"The {code} currency {nameof(code)} is not allowed yet");
        }

        public Currency(int number)
        {
            Initialize();
            if (_iso4217Currencies.Values.Any(number.Equals))
            {
                var (key, value) = _iso4217Currencies.FirstOrDefault(x => x.Value == number);
                Code = key;
                Number = value;
                IsCryptoCurrency = false;
            }
            else
            {
                Code = number.ToString();
                Number = number;
                IsCryptoCurrency = false;
            }
        }

        public string Code { get; }

        public int? Number { get; }

        public bool IsCryptoCurrency { get; }
        public bool IsIso4217Currency { get; }

        public void Deconstruct(out string code, out int? number)
        {
            code = Code;
            number = Number;
        }

        public static implicit operator Currency(string code) => new Currency(code);

        public static implicit operator Currency(int number) => new Currency(number);

        public static explicit operator string(Currency currency) => currency.Code;

        public override string ToString() => Code;

        public bool Equals(Currency other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Code == other.Code
                   && Number == other.Number
                   && IsCryptoCurrency == other.IsCryptoCurrency;
        }

        public override bool Equals(object obj) => obj is Currency other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Code, Number, IsCryptoCurrency);

        public static bool operator ==(Currency left, Currency right) => Equals(left, right);

        public static bool operator !=(Currency left, Currency right) => !Equals(left, right);

        private void Initialize()
        {
            _iso4217Currencies = new Dictionary<string, int>
        {
                { "BRL", 986 },
                { "USD", 840 },

                { "AED", 784 },
                { "AFN", 971 },
                { "ALL", 008 },
                { "AMD", 051 },
                { "ANG", 532 },
                { "AOA", 973 },
                { "ARS", 032 },
                { "AUD", 036 },
                { "AWG", 533 },
                { "AZN", 944 },
                { "BAM", 977 },
                { "BBD", 052 },
                { "BDT", 050 },
                { "BGN", 975 },
                { "BHD", 048 },
                { "BIF", 108 },
                { "BMD", 060 },
                { "BOB", 068 },
                { "BOV", 984 },
                { "BSD", 044 },
                { "BTN", 044 },
                { "BWP", 072 },
                { "BYR", 974 },
                { "BZD", 084 },
                { "CAD", 124 },
                { "CDF", 976 },
                { "CHE", 947 },
                { "CLF", 990 },
                { "CLP", 152 },
                { "CNY", 156 },
                { "COP", 170 },
                { "COU", 970 },
                { "CRC", 188 },
                { "CUC", 931 },
                { "CUP", 192 },
                { "CVE", 132 },
                { "CZK", 203 },
                { "DJF", 262 },
                { "DKK", 208 },
                { "DOP", 214 },
                { "DZD", 012 },
                { "ECS", 895 },
                { "EGP", 818 },
                { "ERN", 232 },
                { "ETB", 230 },
                { "EUR", 978 },
                { "FJD", 242 },
                { "FKP", 238 },
                { "GBP", 826 },
                { "GEL", 981 },
                { "GHS", 936 },
                { "GIP", 292 },
                { "GMD", 270 },
                { "GNF", 324 },
                { "GTQ", 320 },
                { "GYD", 328 },
                { "HKD", 344 },
                { "HNL", 340 },
                { "HRK", 191 },
                { "HTG", 332 },
                { "HUF", 348 },
                { "IDR", 360 },
                { "ILS", 376 },
                { "INR", 356 },
                { "IQD", 368 },
                { "IRR", 364 },
                { "ISK", 352 },
                { "JMD", 388 },
                { "JOD", 400 },
                { "JPY", 392 },
                { "KES", 404 },
                { "KGS", 417 },
                { "KHR", 116 },
                { "KMF", 174 },
                { "KPW", 408 },
                { "KRW", 410 },
                { "KWD", 414 },
                { "KYD", 136 },
                { "KZT", 398 },
                { "LAK", 418 },
                { "LBP", 422 },
                { "LKR", 144 },
                { "LRD", 144 },
                { "LSL", 426 },
                { "LTL", 440 },
                { "LVL", 428 },
                { "LYD", 434 },
                { "MAD", 504 },
                { "MDL", 498 },
                { "MGA", 969 },
                { "MKD", 807 },
                { "MMK", 104 },
                { "MNT", 496 },
                { "MOP", 446 },
                { "MRO", 478 },
                { "MUR", 480 },
                { "MVR", 462 },
                { "MWK", 454 },
                { "MXN", 484 },
                { "MXV", 979 },
                { "MYR", 458 },
                { "NAD", 516 },
                { "NGN", 566 },
                { "NIO", 558 },
                { "NOK", 578 },
                { "NPR", 524 },
                { "NZD", 554 },
                { "OMR", 512 },
                { "PAB", 590 },
                { "PEN", 604 },
                { "PGK", 598 },
                { "PHP", 608 },
                { "PKR", 586 },
                { "PLN", 985 },
                { "PYG", 600 },
                { "QAR", 634 },
                { "RON", 946 },
                { "RSD", 941 },
                { "RUB", 643 },
                { "RWF", 646 },
                { "SAR", 682 },
                { "SDB", 090 },
                { "SCR", 690 },
                { "SDG", 938 },
                { "SEK", 752 },
                { "SGD", 702 },
                { "SHP", 654 },
                { "SLL", 694 },
                { "SOS", 706 },
                { "SRD", 968 },
                { "STN", 930 },
                { "SVC", 222 },
                { "SYP", 760 },
                { "SZL", 748 },
                { "THB", 764 },
                { "TJS", 972 },
                { "TMT", 934 },
                { "TND", 788 },
                { "TOP", 766 },
                { "TRY", 949 },
                { "TTD", 780 },
                { "TWD", 901 },
                { "TZS", 834 },
                { "UAH", 980 },
                { "UGX", 800 },
                { "USN", 997 },
                { "USS", 998 },
                { "UYI", 940 },
                { "UYU", 858 },
                { "UZS", 860 },
                { "VES", 928 },
                { "VND", 704 },
                { "VUV", 548 },
                { "WST", 882 },
                { "XAF", 950 },
                { "XAG", 961 },
                { "XAU", 959 },
                { "XBA", 955 },
                { "XBB", 956 },
                { "XBC", 957 },
                { "XBD", 958 },
                { "XCD", 951 },
                { "XDR", 960 },
                { "XOF", 952 },
                { "XPD", 964 },
                { "XPF", 953 },
                { "XPT", 962 },
                { "XTS", 963 },
                { "XXX", 999 },
                { "YER", 886 },
                { "ZAR", 710 },
                { "ZMW", 894 },
                { "ZWL", 932 }

        };

            _cryptoCurrencies = new List<string>
        {
                "BTC",
                "ETH"
        };
        }
    }
}
