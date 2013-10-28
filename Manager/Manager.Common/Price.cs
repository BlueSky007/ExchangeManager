using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Manager.Common
{
    public class Price : IComparable
    {
        private static readonly Regex regex;
        private const int _MaxPrecision = 28;
        private const int _MaxScale = 13;
        private const int _MaxDenominator = unchecked((int)1E+9);

        //private bool isReciprocal; //倒数

        public string normalizedPrice;
        public double normalizedValue;
        private int numeratorUnit;
        private int denominator;

        #region Common public properties definition
        public int NumeratorUnit
        {
            get { return this.numeratorUnit; }
        }
        public int Denominator
        {
            get { return this.denominator; }
        }
        #endregion Common public properties definition

        #region IComparable Members
        public int CompareTo(object obj)
        {
            Price price = (Price)obj;
            return this.normalizedValue.CompareTo(price.normalizedValue);
        }
        #endregion

        static Price()
        {
            string regexExpression = "\\b(?<WholePart>\\d+)(\\.(?<Numerator>\\d+)(/(?<Denominator>\\d+)|)|)\\b";
            Price.regex = new Regex(regexExpression, RegexOptions.None);
        }

        private Price()
        { }

        private Price(string normalizedPrice, double normalizedValue, int numeratorUnit, int denominator)
        {
            this.normalizedPrice = normalizedPrice;
            this.normalizedValue = normalizedValue;
            this.numeratorUnit = numeratorUnit;
            this.denominator = denominator;
        }

        public Price(string price, int numeratorUnit, int denominator)
        {
            double normalizedValue;
            string normalizedPrice = Normalize(price, numeratorUnit, denominator, out normalizedValue);
            if (normalizedPrice == null)
            {
                string info = string.Format("Cannot create price object: price={0};numeratorUnit={1};denominator={2}", price, numeratorUnit, denominator);
                throw new ArgumentException(info);
            }

            this.normalizedPrice = normalizedPrice;
            this.normalizedValue = normalizedValue;
            this.numeratorUnit = numeratorUnit;
            this.denominator = denominator;
        }

        public Price(double price, int numeratorUnit, int denominator)
        {
            double normalizedValue;
            string normalizedPrice = Normalize(price.ToString(CultureInfo.InvariantCulture), numeratorUnit, denominator, out normalizedValue);
            if (normalizedPrice == null)
            {
                string info = string.Format("Cannot create price object: price={0};numeratorUnit={1};denominator={2}", price, numeratorUnit, denominator);
                throw new ArgumentException(info);
            }

            this.normalizedPrice = normalizedPrice;
            this.normalizedValue = normalizedValue;
            this.numeratorUnit = numeratorUnit;
            this.denominator = denominator;
        }

        public static Price Create(double value, int numeratorUnit, int denominator, bool isGreatThan)
        {
            Price price = new Price(value, numeratorUnit, denominator);

            if (isGreatThan && price.normalizedValue < value)
            {
                price += 1;
            }
            else if (!isGreatThan && price.normalizedValue > value)
            {
                price -= 1;
            }

            return price;
        }

        /// <summary>
        /// Constructor throw exception but CreateInstance return null if failed.
        /// </summary>
        /// <param name="price"></param>
        /// <param name="numeratorUnit"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        public static Price CreateInstance(string price, int numeratorUnit, int denominator)
        {
            double normalizedValue;
            string normalizedPrice = Normalize(price, numeratorUnit, denominator, out normalizedValue);
            if (normalizedPrice == null) return null;

            return new Price(normalizedPrice, normalizedValue, numeratorUnit, denominator);
        }

        public static Price CreateInstance(double price, int numeratorUnit, int denominator)
        {
            return CreateInstance(price.ToString(), numeratorUnit, denominator);
        }

        public static explicit operator string(Price price)
        {
            if (price == null) return null;
            return price.normalizedPrice;
        }
        public static explicit operator double(Price price)
        {
            return price.normalizedValue;
        }
        public static explicit operator decimal(Price price)
        {
            return (decimal)price.normalizedValue;
        }

        public static Price operator +(Price price1, int points)
        {
            double price = price1.normalizedValue + (double)points / price1.denominator;
            return CreateInstance(price, price1.numeratorUnit, price1.denominator);
        }
        public static Price operator +(Price price1, Price price2)
        {
            if (price1.numeratorUnit != price2.numeratorUnit || price1.denominator != price2.denominator)
            {
                throw new ArgumentException("Prices is not the same type");
            }

            return new Price(price1.normalizedValue + price2.normalizedValue, price1.numeratorUnit, price1.denominator);
        }

        public static Price operator -(Price price1, int points)
        {
            return (price1 + (0 - points));
        }
        public static int operator -(Price price1, Price price2)
        {
            if (price1.numeratorUnit != price2.numeratorUnit || price1.denominator != price2.denominator)
            {
                throw new ArgumentException("Prices is not the same type");
            }

            return (int)Math.Round((price1.normalizedValue - price2.normalizedValue) * price1.denominator);
        }

        public static Price Avg(Price price1, Price price2)
        {
            if (price1.numeratorUnit != price2.numeratorUnit || price1.denominator != price2.denominator)
            {
                throw new ArgumentException("Prices is not the same type");
            }

            return CreateInstance((price1.normalizedValue + price2.normalizedValue) / 2, price1.numeratorUnit, price1.denominator);
        }

        public override string ToString()
        {
            return (string)this;
        }
        public override int GetHashCode()
        {
            return this.normalizedPrice.GetHashCode();
        }
        public override bool Equals(Object obj)
        {
            if (obj == null || this.GetType() != obj.GetType()) return false;

            return this == (Price)obj;
        }

        public static bool operator ==(Price price1, Price price2)
        {
            if ((object)price1 == null && (object)price2 == null) return true;
            if ((object)price1 == null ^ (object)price2 == null) return false;

            return price1.normalizedValue == price2.normalizedValue;
        }
        public static bool operator !=(Price price1, Price price2)
        {
            return !(price1 == price2);
        }
        public static bool operator >(Price price1, Price price2)
        {
            return price1.normalizedValue > price2.normalizedValue;
        }
        public static bool operator >=(Price price1, Price price2)
        {
            return price1.normalizedValue >= price2.normalizedValue;
        }
        public static bool operator <(Price price1, Price price2)
        {
            return price1.normalizedValue < price2.normalizedValue;
        }
        public static bool operator <=(Price price1, Price price2)
        {
            return price1.normalizedValue <= price2.normalizedValue;
        }

        public static Price Add(Price price1, int points, bool isReciprocal)
        {
            if (isReciprocal)
            {
                return price1 - points;
            }
            else
            {
                return price1 + points;
            }
        }

        public static Price Subtract(Price price1, int points, bool isReciprocal)
        {
            if (isReciprocal)
            {
                return price1 + points;
            }
            else
            {
                return price1 - points;
            }
        }
        public static int Subtract(Price price1,Price price2)
        {
            return Convert.ToInt32(Math.Round(price1.normalizedValue - price2.normalizedValue) * price1.Denominator);
        }

        public static int Compare(Price price1, Price price2, bool isReciprocal)
        {
            if (price1 == null && price2 == null)
            {
                return 0;
            }
            else if (price1 != null)
            {
                return price1.CompareTo(price2) * (isReciprocal ? -1 : 1);
            }
            else
            {
                return price2.CompareTo(price1) * (isReciprocal ? 1 : -1);
            }
        }

        public static string Normalize(double price, int numeratorUnit, int denominator)
        {
            return Price.Normalize(price.ToString(), numeratorUnit, denominator, out price);
        }

        protected static string Normalize(string price, int numeratorUnit, int denominator, out double normalizedValue)
        {
            normalizedValue = default(double);

            decimal normalizedPrice;
            if (!Price.TryParse(price, numeratorUnit, denominator, out normalizedPrice))
            {
                return null;
            }
            else
            {
                normalizedValue = (double)normalizedPrice;
                return Price.ToString(normalizedValue, numeratorUnit, denominator);
            }
        }

        private static bool TryParse(string priceString, int numeratorUnit, int denominator, out decimal normalizedPrice)
        {
            normalizedPrice = default(decimal);

            if (string.IsNullOrEmpty(priceString) || !Price.IsValid(numeratorUnit, denominator))
            {
                return false;
            }

            if (NumberFormatInfo.CurrentInfo.NumberDecimalSeparator != ".")
            {
                priceString = priceString.Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
            }

            decimal priceValue;
            if (!decimal.TryParse(priceString, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out priceValue))
            {
                double priceValue2;
                if (double.TryParse(priceString, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out priceValue2))
                {
                    priceValue = (decimal)priceValue2;
                }
                else
                {
                    //priceStyle4 : 1/16	127.8/16  127.9/16
                    Match match = Price.regex.Match(priceString);
                    if (!match.Success)
                    {
                        return false;
                    }

                    int priceIntegralPart;
                    int priceDenominator;

                    if (!int.TryParse(match.Groups["IntegralPart"].Value, out priceIntegralPart)
                        || !int.TryParse(match.Groups["Denominator"].Value, out priceDenominator))
                    {
                        return false;
                    }

                    string matchedNumerator = match.Groups["Numerator"].Value;
                    int priceNumerator;
                    if (string.IsNullOrEmpty(matchedNumerator))
                    {
                        priceNumerator = 0;
                    }
                    else
                    {
                        if (!int.TryParse(matchedNumerator, out priceNumerator))
                        {
                            return false;
                        }
                    }

                    if (priceNumerator > priceDenominator)
                    {
                        return false;
                    }

                    priceValue = priceIntegralPart + (decimal)priceNumerator / priceDenominator;
                }
            }

            if (denominator == 1)
            {
                //normalize value to multiple of numeratorUnit
                normalizedPrice = Math.Round(priceValue / numeratorUnit) * numeratorUnit;
            }
            else
            {
                int integralPart = (int)priceValue;
                decimal numerator = (priceValue - integralPart) * denominator;

                //normalize numerator to multiple of numeratorUnit
                decimal normalizedNumerator = Math.Round(numerator / numeratorUnit) * numeratorUnit;
                normalizedPrice = integralPart + normalizedNumerator / denominator;
            }

            normalizedPrice = Math.Round(normalizedPrice, Price._MaxScale);

            return true;// normalizedPrice > 0;
        }

        private static bool IsValid(int numeratorUnit, int denominator)
        {
            if (numeratorUnit <= 0)
            {
                return false;
            }

            if (denominator <= 0 || denominator > Price._MaxDenominator)
            {
                return false;
            }

            if (denominator > 1 && numeratorUnit >= denominator)
            {
                return false;
            }
            return true;
        }

        protected static string ToString(double normalizedValue, int numeratorUnit, int denominator)
        {
            string priceString;

            if (denominator == 1)
            {
                priceString = (Math.Round(normalizedValue, 0)).ToString();
            }
            else
            {
                int integralPart = (int)normalizedValue;
                int numerator = Math.Abs((int)Math.Round((normalizedValue - integralPart) * denominator));

                double decimalDigits = Math.Log10(denominator);
                if (decimalDigits == (uint)decimalDigits)
                {
                    //10,100,1000
                    decimal numerator2 = (decimal)numerator / denominator;
                    priceString = integralPart.ToString() + Helper.Format(numerator2, (int)decimalDigits).Substring(1);
                }
                else
                {
                    if (numerator == 0)
                    {
                        priceString = integralPart.ToString();
                    }
                    else
                    {
                        priceString = integralPart.ToString() + NumberFormatInfo.CurrentInfo.NumberDecimalSeparator
                            + numerator.ToString() + "/" + denominator.ToString();
                    }
                }
            }

            if (normalizedValue < 0 && priceString[0] != '-') priceString = "-" + priceString;
            return priceString;
        }

        public static Price Adjust(Price price1, int points)
        {
            if (price1 == null)
            {
                return null;
            }
            int pointsLen = points.ToString().Length;
            int pointsDenominator = (int)Math.Pow(10, pointsLen);

            double pointsDouble = (double)points;
            if (pointsDenominator > price1.denominator && price1.denominator != 1)
            {
                pointsDouble = Math.Floor(pointsDouble / (pointsDenominator / price1.denominator));
                pointsDenominator = price1.denominator;
            }

            double Price1 = (Math.Floor(((price1.normalizedValue * price1.denominator) / pointsDenominator) * pointsDenominator + pointsDouble)) / price1.denominator;

            //double price = (zPrice + pointsDouble) / price1._Denominator;
            //double price = price1._NormalizedValue + (double)points / price1._Denominator;
            return CreateInstance(Price1, price1.numeratorUnit, price1.denominator);
        }

        public Price ToPriceEntity()
        {
            Price entity = new Price(this.normalizedPrice, this.normalizedValue, this.numeratorUnit, this.denominator);

            return entity;
        }
    }
}
