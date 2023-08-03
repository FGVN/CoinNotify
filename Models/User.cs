using System;
using System.Collections.Generic;
using System.Linq;
namespace CoinNotify.Models
{
    /// <summary>
    ///Represents users and their notifications as coin list
    /// </summary>
    internal class User
    {
        public string _id;
        public List<Coin> _coins;

        public User() 
        {
            this._id = "0";
            this._coins = new List<Coin>();
        }

        public User(string _id)
        {
            this._id = _id;
            _coins = new List<Coin>();
        }
        public User(string _id, List<Coin> _coins)
        {
            this._id = _id;
            this._coins = _coins;
        }
    }
}
