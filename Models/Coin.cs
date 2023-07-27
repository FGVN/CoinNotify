using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace CoinNotify.Models
{
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
        public List<Coin> GetCoins()
        {
            List<Coin> coinList = new List<Coin>();
            string dataDirectory = Path.Combine("..", "..", "..", "Data"); // Combine parent directory with "Data" folder name
            string filePath = Path.Combine(dataDirectory, "fin.txt"); // Combine data directory with "fin.txt" file name

            CultureInfo culture = new CultureInfo("en-US"); // Set the culture to use dot as the decimal separator

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string name = reader.ReadLine(); // Read the first line as a string
                        double price = double.Parse(reader.ReadLine(), culture); // Read the second line as a double


                        // Perform further operations with the variables
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
