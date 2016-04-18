using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;


namespace CurrencyConvWS
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Description = "Server Variables",
                Namespace = "http://currencyconverterws.azurewebsites.net/WebService.asmx",
                Name = "Currency Converter")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService 
    {
        private List<Tuple<string, double>> list;

        private void getDataFromURL()
        {
            list = new List<Tuple<string, double>>();
            String URLString = "http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
            XmlReader xmlReader = XmlReader.Create(URLString);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "Cube"))
                {
                    if (xmlReader.HasAttributes)
                    {
                        double value = Convert.ToDouble(xmlReader.GetAttribute("rate"));
                        if (value != 0)
                            list.Add(new Tuple<string, double>(xmlReader.GetAttribute("currency"), value));
                    }
                    Debug.WriteLine(xmlReader.GetAttribute("currency") + ": " + xmlReader.GetAttribute("rate"));
                }
            }
        }

        [WebMethod(Description = "Currency List",
                    MessageName = "getCurrencyList")]
        public List<string> getCurrencyList()
        {
            getDataFromURL();
            var lst = new List<string>();
            foreach (var lst1 in list)
            {
                lst.Add(lst1.Item1);
            }
            return lst;
        }

        public class Currency
        {
            public bool Status { get; set; }
            public string Name { get; set; }
            public double Value { get; set; }
        }

        [WebMethod(Description = "Convert Currency from one to another.",
                   MessageName = "getCurrencyConvertFromTo")]
        public Currency getCurrencyConvertFromTo(string currencyFrom, string currencyTo, string value)
        {
            getDataFromURL();

            double rate1 = 1.0;
            double rate2 = 1.0;
            
            currencyFrom = currencyFrom.ToUpper();
            currencyTo = currencyTo.ToUpper();

            Currency curr = new Currency();
            curr.Status = false;

            if (!checkIsDouble(value))
            {
                curr.Name = "ERROR " + value +" is not double";
                curr.Value = 0.0;
            }
            else
            {
                var value2 = Convert.ToDouble(value);

                if (currencyFrom.Length != 3 || currencyFrom == "")
                {
                    curr.Name = "ERROR: Unknown input currency " + currencyFrom; ;
                    curr.Value = 0.0;
                    return curr;
                }

                if (currencyTo.Length != 3 || currencyTo == "")
                {
                    curr.Name = "ERROR: Unknown output currency " + currencyTo; ;
                    curr.Value = 0.0;
                    return curr;
                }


                foreach (var lst in list)
                {
                    if (currencyFrom != "EUR")
                    {
                        if (checkIfExist(currencyFrom))
                        {
                            if (lst.Item1.Equals(currencyFrom))
                            {
                                rate1 = lst.Item2;
                            }
                        }
                        else
                        {
                            curr.Name = "ERROR: Unknown input currency " + currencyFrom; ;
                            curr.Value = value2;
                            return curr;
                        }

                    }

                    if (currencyTo != "EUR")
                    {
                        if (checkIfExist(currencyTo))
                        {
                            if (lst.Item1.Equals(currencyTo))
                            {
                                rate2 = lst.Item2;
                            }
                        }
                        else
                        {
                            curr.Name = "ERROR: Unknown output currency " + currencyTo; ;
                            curr.Value = value2;
                            return curr;
                        }
                    }
                }
                curr.Status = true;
                double result = (value2 / rate1);
                result = result * rate2;

                curr.Name = currencyTo;
                curr.Value = result;

            }
            return curr;
            
        }

        private bool checkIsDouble(string number)
        {
            bool status = false;
            double dOutput = 0;

            if (Double.TryParse(number, out dOutput))
                status = true;

            return status;
        }
        
        private bool checkIfExist(string value)
        {
            bool x = false;
            foreach(var item in list)
            {
                if (item.Item1.Contains(value))
                    x = true;
            }
            return x;
        }
    }
}
