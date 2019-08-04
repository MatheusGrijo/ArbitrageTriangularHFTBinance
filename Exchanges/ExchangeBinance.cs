using Binance.Net;
using Binance.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class ExchangeBinance 
{

    public static int volumeDay = 800;

    public ExchangeBinance()
    {
        try
        {
            String json = Http.get("https://api.binance.com/api/v1/exchangeInfo");
            exchangeInfo = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));
            new System.Threading.Thread(loadSocket).Start();
        }
        catch
        {

        }
    }



    public decimal getLastPrice(string pair)
    {
        try
        {
            if (lstTicker != null)
                foreach (var ticker in lstTicker)
                    if (pair.Trim().ToUpper() == ticker.symbol)
                        return ticker.tick.CurrentDayClosePrice;
        }
        catch { }

        System.Threading.Thread.Sleep(1000);
        return getLastPrice(pair);
    }

    public class Ticker
    {
        public string symbol;
        public BinanceStreamTick tick;

    }


    public static List<Ticker> lstTicker = new List<Ticker>();

    private void loadSocket()
    {
        var _client = new BinanceSocketClient();

        var successOrderBook = _client.SubscribeToAllSymbolTicker((data) =>
        {
            try
            {
                if (data != null)
                {                    
                    foreach (var item in data)
                    {
                        bool found = false;
                        for (int i = 0; i < lstTicker.Count; i++)
                        {
                            if (lstTicker[i].symbol.Trim().ToUpper() == item.Symbol.Trim().ToUpper())
                            {
                                found = true;
                                Ticker ticker = new Ticker();
                                lstTicker[i].tick = item;
                            }
                        }
                        if (!found)
                        {
                            Ticker ticker = new Ticker();
                            ticker.tick = item;
                            ticker.symbol = item.Symbol.Trim().ToUpper();
                            lstTicker.Add(ticker);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

        });

        Console.WriteLine(successOrderBook.Success);
        Console.WriteLine(successOrderBook.Error);
        Console.WriteLine(successOrderBook.Data);





    }

    public decimal[] getHighestBid(string pair, decimal amount)
    {
        try
        {
            String json = Http.get("https://api.binance.com/api/v1/depth?symbol=" + pair);
            JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));


            decimal[] arrayValue = new decimal[2];
            arrayValue[0] = arrayValue[1] = 0;
            decimal amountBook = 0;
            decimal amountAux = 0;
            decimal total = 0;
            int lines = 0;

            foreach (var item in jCointaner["bids"])
            {
                lines++;
                amountBook += decimal.Parse(item[1].ToString().Replace(".", ","));

                if (amount > amountBook)
                {
                    total += decimal.Parse(item[1].ToString().Replace(".", ",")) * decimal.Parse(item[0].ToString().Replace(".", ","));
                    amountAux += decimal.Parse(item[1].ToString().Replace(".", ","));
                }
                else if (lines == 1)
                {
                    arrayValue[0] = decimal.Parse(item[0].ToString().Replace(".", ","));
                    arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                    return arrayValue;
                }
                else
                    total += (amount - amountAux) * decimal.Parse(item[0].ToString().Replace(".", ","));

                if (amountBook >= amount)
                {
                    arrayValue[0] = total / amount;
                    arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                    return arrayValue;
                }
            }
        }
        catch
        {
        }
        return new decimal[2];
    }


    //public decimal getHighestBid(string pair, decimal amount)
    //{
    //    try
    //    {

    //        decimal amountBook = 0;
    //        decimal amountAux = 0;
    //        int lines = 0;
    //        decimal total = 0;

    //        BinanceOrderBook book = null;


    //        foreach (var item in Program.array)
    //            if ((item as Program.ClassDetailOrder).symbol == pair.ToLower())
    //            {
    //                book = (item as Program.ClassDetailOrder).book; break;
    //            }


    //        foreach (var item in book.Bids)
    //        {
    //            lines++;
    //            amountBook += item.Quantity;

    //            if (amount > amountBook)
    //            {
    //                total += item.Quantity * item.Price;
    //                amountAux += item.Quantity;
    //            }
    //            else if (lines == 1)
    //            {                    
    //                amount = amount * item.Price;
    //                amount = Math.Round(amount / getQuantity(pair)) * getQuantity(pair);
    //                amount = Math.Round(amount, getQuotePrecision(pair));
    //                return amount;
    //            }
    //            else
    //                total += (amount - amountAux) * item.Price;

    //            if (amountBook >= amount)
    //            {
    //                amount = total;
    //                amount = Math.Round(amount / getQuantity(pair)) * getQuantity(pair);
    //                amount = Math.Round(amount, getQuotePrecision(pair));
    //                return amount;
    //            }
    //        }
    //    }
    //    catch
    //    {
    //    }
    //    throw new Exception("Error Bids");
    //}




    public decimal[] getBook(string pair, decimal amount)
    {
        try
        {
            String json = Http.get("https://exchange.bitrecife.com.br/api/v3/public/getorderbook?market=BTC_BRL&type=SELL&depth=2000");
            JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

            decimal[] arrayValue = new decimal[2];
            arrayValue[0] = arrayValue[1] = 0;
            decimal orderPrice = 0;
            decimal orderAmount = 0;
            decimal totalCost = 0;
            decimal totalAmount = 0;
            decimal remaining = amount;
            decimal cost = 0;

            foreach (var item in jCointaner["result"]["sell"])
            {
                orderPrice = decimal.Parse(item["Rate"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
                orderAmount = decimal.Parse(item["Quantity"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
                cost = orderPrice * orderAmount;
                if (cost < remaining)
                {
                    remaining -= cost;
                    totalCost += cost;
                    totalAmount += orderAmount;
                }
                else
                {
                    //finished
                    remaining -= amount;
                    totalCost += amount * orderPrice;
                    totalAmount += amount;
                    arrayValue[0] = Math.Round(amount / (totalCost / totalAmount), 8);
                    arrayValue[1] = Math.Round(amount / orderPrice, 8);
                    return arrayValue;
                }
            }
        }
        catch
        {
        }
        return new decimal[2];
    }

    public decimal getBook(string pair, decimal amount, string type, string operation, bool division = true)
    {
        try
        {                      
            BinanceOrderBook book = null;

            foreach (var item in Program.array)
                if ((item as Program.ClassDetailOrder).symbol == pair.ToLower())
                {
                    book = (item as Program.ClassDetailOrder).book; break;
                }



            List<BinanceOrderBookEntry> lst = null;
            if (type == "asks")
                lst = book.Asks;
            if (type == "bids")
                lst = book.Bids;

            decimal[] arrayValue = new decimal[2];
            arrayValue[0] = arrayValue[1] = 0;
            decimal orderPrice = 0;
            decimal orderAmount = 0;
            decimal totalCost = 0;
            decimal totalAmount = 0;
            decimal remaining = amount;
            decimal cost = 0;

            foreach (var item in lst)
            {
                orderPrice = item.Price;
                orderAmount = item.Quantity;
                cost = orderPrice * orderAmount;
                if (cost < remaining)
                {
                    remaining -= cost;
                    totalCost += cost;
                    totalAmount += orderAmount;
                }
                else
                {
                    //finished
                    remaining -= amount;
                    totalCost += amount * orderPrice;
                    totalAmount += amount;
                    if (division)
                    {
                        arrayValue[0] = Math.Round(amount / (totalCost / totalAmount), 8);
                        arrayValue[1] = Math.Round(amount / orderPrice, 8);
                    }
                    else
                    {
                        arrayValue[0] = Math.Round((totalCost / totalAmount) * amount, 8);
                        arrayValue[1] = Math.Round(orderPrice * amount, 8);
                    }
                    return arrayValue[0];
                }
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message + ex.StackTrace);
            //System.Threading.Thread.Sleep(1000);
        }
        //System.Threading.Thread.Sleep(1000);
        throw new Exception("Error Asks");
    }


   

    public decimal getLowestAskAmount(string pair, decimal price)
    {
        try
        {

            decimal[] arrayValue = new decimal[2];
            arrayValue[0] = arrayValue[1] = 0;
            decimal priceBook = 0;
            decimal priceAux = 0;
            int lines = 0;
            decimal total = 0;

            BinanceOrderBook book = null;


            foreach (var item in Program.array)
                if ((item as Program.ClassDetailOrder).symbol == pair.ToLower())
                {
                    book = (item as Program.ClassDetailOrder).book; break;
                }


            foreach (var item in book.Asks)
            {
                lines++;
                priceBook += item.Price;

                if ((item.Quantity * item.Price) >= price)
                {                    
                    return price/item.Price ;
                }
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message + ex.StackTrace);
        }
        throw new Exception("Erro askamount");
    }

    public decimal[] getLowestAsk(string pair, decimal amount)
    {
        try
        {
            String json = Http.get("https://api.binance.com/api/v1/depth?symbol=" + pair);
            JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

            decimal[] arrayValue = new decimal[2];
            arrayValue[0] = arrayValue[1] = 0;
            decimal amountBook = 0;
            decimal amountAux = 0;
            int lines = 0;
            decimal total = 0;

            foreach (var item in jCointaner["asks"])
            {

                lines++;
                amountBook += decimal.Parse(item[1].ToString().Replace(".", ","));

                if (amount > amountBook)
                {
                    total += decimal.Parse(item[1].ToString().Replace(".", ",")) * decimal.Parse(item[0].ToString().Replace(".", ","));
                    amountAux += decimal.Parse(item[1].ToString().Replace(".", ","));
                }
                else if (lines == 1)
                {
                    arrayValue[0] = decimal.Parse(item[0].ToString().Replace(".", ","));
                    arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                    return arrayValue;
                }
                else
                    total += (amount - amountAux) * decimal.Parse(item[0].ToString().Replace(".", ","));

                if (amountBook >= amount)
                {
                    arrayValue[0] = total / amount;
                    arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                    return arrayValue;
                }
            }
        }
        catch
        {
        }
        return new decimal[2];
    }



    public string getName()
    {
        return "BINANCE";
    }


    public void loadBalances()
    {
        throw new NotImplementedException();
    }



    public string post(String url, String parameters, String key, String secret, Method method = Method.POST)
    {
        try
        {
            // lock (objLock)
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var client = new RestClient("https://api.binance.com");

                HMACSHA256 encryptor = new HMACSHA256();
                encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
                String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
                parameters += "&signature=" + sign;

                var request = new RestRequest("/api/v3/order?" + parameters, method);
                request.AddHeader("X-MBX-APIKEY", Key.key);
                var response = client.Execute(request);

                Logger.triangle("POST " + url + parameters +Environment.NewLine + " || RESPONSE " + response.Content.ToString() + Environment.NewLine + Environment.NewLine);
                return response.Content.ToString();
            }
        }
        catch (Exception ex)
        {
            
            return null;
        }
        finally
        {
        }
    }

    public string httppost(String url, String parameters, String key, String secret)
    {
        try
        {
            // lock (objLock)
            {



                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var client = new RestClient("https://api.binance.com");

                HMACSHA256 encryptor = new HMACSHA256();
                encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
                String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
                parameters += "&signature=" + sign;

                var request = new RestRequest(url + "?" + parameters, Method.POST);
                request.AddHeader("X-MBX-APIKEY", Key.key);
                var response = client.Execute(request);
                Console.WriteLine(response.Content);
                return response.Content.ToString();
            }
        }
        catch (Exception ex)
        {
            
            return null;
        }
        finally
        {
        }
    }

    public string order(string type, string pair, decimal amount, decimal price, bool lockQuantity = true, string timeInForce = "GTC")
    {

        amount = Math.Round(amount / getQuantity(pair)) * getQuantity(pair);
        amount = Math.Round(amount, getQuotePrecision(pair));        

        price = Math.Round(price / getPriceFilter(pair)) * getPriceFilter(pair);
        price = Math.Round(price, getPrecision(pair));


        price = Math.Round(price, 8);
        amount = Math.Round(amount, 8);

        int countTry = 0;
        string ret = post("https://api.binance.com/api/v3/order", "symbol=" + pair.ToUpper() + "&side=" + type.ToUpper() + "&type=LIMIT&timeInForce=" + timeInForce + "&quantity=" + amount.ToString().Replace(",", ".") + "&price=" + price.ToString().Replace(",", ".") + "&timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()), Key.key, Key.secret, Method.POST);
        while (ret.IndexOf("insufficient balance") >= 0 || ret.IndexOf("many new orders") >= 0)
        {
            amount -= getQuantity(pair);
            ret = post("https://api.binance.com/api/v3/order", "symbol=" + pair.ToUpper() + "&side=" + type.ToUpper() + "&type=LIMIT&timeInForce=" + timeInForce + "&quantity=" + amount.ToString().Replace(",", ".") + "&price=" + price.ToString().Replace(",", ".") + "&timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()), Key.key, Key.secret, Method.POST);
            countTry++;
            if (countTry > 1000)
            {
                amount = getBalances(pair);
                amount = Math.Round(amount / getQuantity(pair)) * getQuantity(pair);
                amount = Math.Round(amount, getQuotePrecision(pair));
                amount -= getQuantity(pair);
                amount = Math.Round(amount, 8);
                ret = post("https://api.binance.com/api/v3/order", "symbol=" + pair.ToUpper() + "&side=" + type.ToUpper() + "&type=LIMIT&timeInForce=" + timeInForce + "&quantity=" + amount.ToString().Replace(",", ".") + "&price=" + price.ToString().Replace(",", ".") + "&timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()), Key.key, Key.secret, Method.POST);
                return ret;
            }
        }
        return ret;
    }

    public string orderMarket(string type, string pair, decimal amount, bool decrease = false, bool increase = false)
    {
        //   return "";
        amount = Math.Round(amount / getQuantity(pair)) * getQuantity(pair);
        amount = Math.Round(amount, getQuotePrecision(pair));

        //if (decrease)        
        //    for (int i = 0; i < 10; i++)            
        //        amount -= getQuantity(pair);                        
        
        //if (increase)
        //    for (int i = 0; i < 10; i++)
        //        amount += getQuantity(pair);

        

        int countTry = 0;
        string ret = post("https://api.binance.com/api/v3/order", "symbol=" + pair.ToUpper() + "&side=" + type.ToUpper() + "&type=MARKET&quantity=" + amount.ToString().Replace(",", ".") + "&timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()), Key.key, Key.secret, Method.POST);        
        while (ret.IndexOf("insufficient balance") >= 0 || ret.IndexOf("many new orders") >= 0 || ret.IndexOf("LOT_SIZE") >= 0)
        {
            //amount -= getQuantity(pair);
            //amount -= getQuantity(pair);            
                amount -= getQuantity(pair);            
            ret = post("https://api.binance.com/api/v3/order", "symbol=" + pair.ToUpper() + "&side=" + type.ToUpper() + "&type=MARKET&quantity=" + amount.ToString().Replace(",", ".") + "&timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()), Key.key, Key.secret, Method.POST);
            countTry++;
            if (countTry > 900)
            {
                amount = getBalances(pair);
                amount = Math.Round(amount / getQuantity(pair)) * getQuantity(pair);
                amount = Math.Round(amount, getQuotePrecision(pair));
                amount -= getQuantity(pair);
                amount = Math.Round(amount, getQuotePrecision(pair));
                ret = post("https://api.binance.com/api/v3/order", "symbol=" + pair.ToUpper() + "&side=" + type.ToUpper() + "&type=MARKET&quantity=" + amount.ToString().Replace(",", ".") + "&timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()), Key.key, Key.secret, Method.POST);
                return ret;
            }
        }
        return ret;
    }

    public static JContainer exchangeInfo = null;
    public static decimal getQuantity(String coin)
    {

        foreach (var item in exchangeInfo["symbols"])
        {
            if (item["symbol"].ToString().Trim().ToUpper() == coin.Trim().ToUpper())
            {
                return decimal.Parse(item["filters"][3]["minNotional"].ToString().Replace(".", ","));
            }
        }

        return 1;
    }

    public static decimal getPriceFilter(String coin)
    {

        foreach (var item in exchangeInfo["symbols"])
        {
            if (item["symbol"].ToString().Trim().ToUpper() == coin.Trim().ToUpper())
            {
                return decimal.Parse(item["filters"][0]["tickSize"].ToString().Replace(".", ","));
            }
        }

        return 1;
    }



    public static int getPrecision(String coin)
    {

        foreach (var item in exchangeInfo["symbols"])
        {
            if (item["symbol"].ToString().Trim().ToUpper() == coin.Trim().ToUpper())
            {
                return int.Parse(item["baseAssetPrecision"].ToString().Replace(".", ","));
            }
        }

        return 1;
    }

    public static int getQuotePrecision(String coin)
    {

        foreach (var item in exchangeInfo["symbols"])
        {
            if (item["symbol"].ToString().Trim().ToUpper() == coin.Trim().ToUpper())
            {
                return int.Parse(item["quotePrecision"].ToString().Replace(".", ","));
            }
        }

        return 1;
    }



    public decimal getBalances(String _coin, bool btc = false, bool free = false)
    {
        try
        {
            _coin = _coin.ToUpper().Replace("BTC", "");
            if (btc)
                _coin = "BTC";
            var client = new RestClient("https://api.binance.com");
            String parameters = "timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()).Split(',')[0];
            HMACSHA256 encryptor = new HMACSHA256();
            encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
            String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
            parameters += "&signature=" + sign;
            var request = new RestRequest("/api/v3/account?" + parameters, Method.GET);
            request.AddHeader("X-MBX-APIKEY", Key.key);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            String json = response.Content.ToString();
            JContainer dt = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));


            foreach (var item in dt["balances"])
            {
                String coin = item["asset"].ToString();
                decimal value = decimal.Parse(item["free"].ToString().Replace(".", ","));

                if (value > 0)
                {
                    if (coin.ToString().ToUpper() == _coin.ToUpper())
                        return value;
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            return 0;
        }
        finally
        {
        }
    }


    public string getDetailOrder(String _coin, string orderId)
    {
        try
        {
            var client = new RestClient("https://api.binance.com");
            String parameters = "timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()).Split(',')[0] + "&symbol=" + _coin + "&orderId=" + orderId;
            HMACSHA256 encryptor = new HMACSHA256();
            encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
            String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
            parameters += "&signature=" + sign;
            var request = new RestRequest("/api/v3/order?" + parameters, Method.GET);
            request.AddHeader("X-MBX-APIKEY", Key.key);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            String json = response.Content.ToString();
            JContainer dt = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));
            return json;
        }
        catch (Exception ex)
        {
            return "";
        }
        finally
        {
        }
    }

    public bool withdrawal(string address, decimal amount)
    {
        try
        {

            var client = new RestClient("https://api.binance.com");
            String parameters = "timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()).Split(',')[0] + "&asset=BTC&address=" + address + "&amount=" + amount.ToString().Replace(",", ".");
            HMACSHA256 encryptor = new HMACSHA256();
            encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
            String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
            parameters += "&signature=" + sign;
            var request = new RestRequest("/wapi/v3/withdraw.html?" + parameters, Method.POST);
            request.AddHeader("X-MBX-APIKEY", Key.key);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            String json = response.Content.ToString();
            JContainer dt = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

            if (dt["success"].ToString().ToUpper() == "TRUE")
                return true;
            else
                return false;
        }
        catch (Exception ex)
        {
            return false;
        }
        finally
        {
        }
    }

    public string getBalancesAsync()
    {
        try
        {
            var client = new RestClient("https://api.binance.com");
            String parameters = "timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()).Split(',')[0];
            HMACSHA256 encryptor = new HMACSHA256();
            encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
            String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
            parameters += "&signature=" + sign;
            var request = new RestRequest("/api/v3/account?" + parameters, Method.GET);
            request.AddHeader("X-MBX-APIKEY", Key.key);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            String json = response.Content.ToString();
            JContainer dt = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

            int index = 0;
            decimal totalBTC = 0;

            String jsonTicker = Http.get("https://api.binance.com/api/v1/ticker/24hr");
            JContainer jCointanerTicker = (JContainer)JsonConvert.DeserializeObject(jsonTicker, (typeof(JContainer)));

            foreach (var item in dt["balances"])
            {
                String coin = item["asset"].ToString();
                decimal value = decimal.Parse(item["free"].ToString().Replace(".", ",")) + decimal.Parse(item["locked"].ToString().ToString().Replace(".", ","));

                if (value > 0)
                {
                    if (coin.ToString().ToUpper() == "BTC")
                    {
                        totalBTC += value;
                        //Key.btcInvest = Math.Round((totalBTC / 8), 13);
                        //if(Key.btcInvest > 1)
                        //Key.btcInvest = 0.05m;
                        //ClassDB.execS("update anubis_user set num_btc = '" + Key.btcInvest.ToString().Replace(",", ".") + "' where cod_user = 540 ");
                    }
                    else
                    {
                        decimal priceLast = 0;
                        foreach (var itemTicker in jCointanerTicker)
                        {
                            if (coin + "BTC" == itemTicker["symbol"].ToString())
                            {
                                priceLast = decimal.Parse(itemTicker["lastPrice"].ToString().ToString().Replace(".", ","));
                                break;
                            }
                        }
                        totalBTC += value * priceLast;
                    }
                }
                index++;
            }





            



            return response.Content.ToString();
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
        }
    }

    public string getAllOrders()
    {
        try
        {
            var client = new RestClient("https://api.binance.com");
            String parameters = "timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime());
            HMACSHA256 encryptor = new HMACSHA256();
            encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
            String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
            parameters += "&signature=" + sign;
            var request = new RestRequest("/api/v3/openOrders?" + parameters, Method.GET);
            request.AddHeader("X-MBX-APIKEY", Key.key);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            String json = response.Content.ToString();
            return json;
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
        }
    }

    public string getDeposits()
    {
        try
        {
            var client = new RestClient("https://api.binance.com");
            String parameters = "timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime());
            HMACSHA256 encryptor = new HMACSHA256();
            encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
            String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
            parameters += "&signature=" + sign;
            var request = new RestRequest("/wapi/v3/depositHistory.html?" + parameters, Method.GET);
            request.AddHeader("X-MBX-APIKEY", Key.key);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            String json = response.Content.ToString();
            return json;
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
        }
    }

    public string getAddress(String asset)
    {
        try
        {
            var client = new RestClient("https://api.binance.com");
            String parameters = "timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()) + "&status=true&asset=" + asset;
            HMACSHA256 encryptor = new HMACSHA256();
            encryptor.Key = Encoding.ASCII.GetBytes(Key.secret);
            String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
            parameters += "&signature=" + sign;
            var request = new RestRequest("/wapi/v3/depositAddress.html?" + parameters, Method.GET);
            request.AddHeader("X-MBX-APIKEY", Key.key);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            String json = response.Content.ToString();
            return json;
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
        }
    }

    public string cancelOrder(String pair, String id)
    {
        try
        {
            return post("https://api.binance.com/api/v3/order", "symbol=" + pair.ToUpper() + "&orderId=" + id + "&timestamp=" + Utils.GenerateTimeStamp(DateTime.Now.ToUniversalTime()), Key.key, Key.secret, Method.DELETE);
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
        }
    }

   

    public decimal getFee()
    {
        return 0.1m;
    }
    
}
