using System.Globalization;

namespace CoinNotify.Models
{
    /// <summary>
    ///Represents data structure to save data about coins
    /// </summary>
    internal class Coin
    {
        public string _name;
        public double _price;

        public Coin(string _name, double _price)
        {
            this._name = _name;
            this._price = _price;
        }

        public Coin()
        {
            _name = "";
            _price = 0;
        }

    }

    class CoinParser 
    {
        public CoinParser()
        {
        }


        /// <summary>
        ///Gets prices and names from txt file and return list of coins
        /// </summary>
        public static List<Coin> GetCoins()
        {
            List<Coin> coinList = new List<Coin>();
          
        string currentDirectory = Directory.GetCurrentDirectory();

            // Combine parent directory with "Data" folder name
            string dataDirectory = Path.Combine(currentDirectory, "Data"); 
            // Combine data directory with "fin.txt" file name
            string filePath = Path.Combine(dataDirectory, "fin.txt"); 
            // Set the culture to use dot as the decimal separator
            CultureInfo culture = new CultureInfo("en-US"); 

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        // Read the first line as a string
                        string name = reader.ReadLine();
                        // Read the second line as a double
                        double price = double.Parse(reader.ReadLine(), culture); 


                        //Adding values into the result list
                        coinList.Add(new Coin(name, price));
                    }
                }
            }
            else
            {
                Console.WriteLine("File not found.");
            }

            return coinList;
        }
    }
}
