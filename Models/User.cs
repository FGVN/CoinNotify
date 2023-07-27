using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinNotify.Models
{
    internal class User
    {
        string _id;
        List<Coin> _coins;

        public User(string id)
        {
            _id = id;
            _coins = new List<Coin>();
        }
        public User(string id, List<Coin> coins)
        {
            _id = id;
            _coins = coins;
        }
    }
}
