/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

using Demo.UnitsOfMeasurement;
using System.Globalization;
using System.Xml;

namespace Currencies
{
    class Program
    {
        static void Main(/*string[] args*/)
        {
            Console.WriteLine(
                "Units of Measurement for C# applications. Copyright (©) Marek Anioła.\n" +
                "This program is provided to you under the terms of the license\n" +
                "as published at https://github.com/mangh/Metrology."
            );

            Console.WriteLine("\nUpdating currency exchange rates (demo application).\n");

            IEnumerable<Unit<decimal>> myCurrencies = Catalog.Units<decimal>(EUR.Family);

            foreach ((string currency, decimal rate) in ExchangeRates(@"http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml"))
            {
                Unit<decimal>? currencyUnit = myCurrencies.FirstOrDefault(c => c.Symbol.IndexOf(currency) >= 0);
                if (currencyUnit is null)
                {
                    Console.WriteLine($"    {currency} rate = {rate}");
                }
                else
                {
                    decimal oldrate = currencyUnit.Factor;
                    currencyUnit.Factor = rate;
                    char increase = rate < oldrate ? '-' : oldrate < rate ? '+' : '=';
                    Console.WriteLine($"({increase}) {currency} rate = {rate} (previously {oldrate})");
                }
            }

            Console.WriteLine("\nDone.\n");
        }

        private static IEnumerable<(string, decimal)> ExchangeRates(string url)
        {
            Console.WriteLine($"Connecting to {url}\n");

            XmlReaderSettings settings = new()
            {
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

            using (XmlReader rdr = XmlReader.Create(url, settings))
            {
                while (rdr.Read())
                {
                    if ((rdr.NodeType == XmlNodeType.Element) && (rdr.LocalName == "Cube"))
                    {
                        string? currency = rdr.GetAttribute("currency");
                        if (currency is not null)
                        {
                            string? rateString = rdr.GetAttribute("rate");
                            if (rateString is not null)
                            {
                                if (decimal.TryParse(rateString, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal rate))
                                {
                                    yield return (currency, rate);
                                }
                                else
                                {
                                    Console.WriteLine($"(?) {currency} rate = \"{rateString}\": unable to get rate number value.");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
/*
 * Sample output:
 * 
 * Updating currency exchange rates (demo application).
 * 
 * Connecting to http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml
 * 
 * (-) USD rate = 1,1278 (previously 1,3433)
 *     JPY rate = 128,19
 *     BGN rate = 1,9558
 *     CZK rate = 25,401
 *     DKK rate = 7,4362
 * (+) GBP rate = 0,85158 (previously 0,79055)
 *     HUF rate = 367,07
 * (+) PLN rate = 4,6221 (previously 4,1437)
 *     RON rate = 4,9494
 *     SEK rate = 10,2310
 *     CHF rate = 1,0418
 *     ISK rate = 148,00
 *     NOK rate = 10,1598
 *     HRK rate = 7,5210
 *     RUB rate = 82,8238
 *     TRY rate = 16,0525
 *     AUD rate = 1,5795
 *     BRL rate = 6,3190
 *     CAD rate = 1,4388
 *     CNY rate = 7,1777
 *     HKD rate = 8,7967
 *     IDR rate = 16155,54
 *     ILS rate = 3,5040
 *     INR rate = 85,4225
 *     KRW rate = 1333,60
 *     MXN rate = 23,5354
 *     MYR rate = 4,7678
 *     NZD rate = 1,6651
 *     PHP rate = 56,731
 *     SGD rate = 1,5420
 *     THB rate = 37,612
 *     ZAR rate = 17,9694
 * 
 * Done.
 * 
 */
