using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoinNotify.Models;
using Newtonsoft.Json;

namespace CoinNotify.Controllers
{
    internal class UsersController
    {
        private string filepath;

        public UsersController()
        {
            filepath = Path.Combine("..", "..", "..", "Data", "Users.json");
        }
        public List<User> GetUsers()
        {
            using var jsonFileReader = File.OpenText(filepath);

            List<User> users;

            string jsonData = File.ReadAllText(filepath);

            users = JsonConvert.DeserializeObject<List<User>>(jsonData);

            if (users.Any())
                return users;

            return new List<User>();
        }

        public bool AddNotify(string chatId, string coin, string price)
        {
            var users = GetUsers();
            CultureInfo culture = new CultureInfo("en-US");

            if (users.Exists(x => x._id == chatId))
            {
                users.Find(x => x._id == chatId)._coins.Add(new Coin(coin, double.Parse(price, culture)));
            }
            else
            {
                users.Add(new User(chatId, new List<Coin> { new Coin(coin, double.Parse(price, culture)) }));
            }

            // Serialize the users list using Newtonsoft.Json
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);

            File.WriteAllText(filepath, json);
            return true;
        }

        public List<Coin> GetNotifications(string chatId) => GetUsers().FirstOrDefault(x => x._id == chatId)._coins;

        public bool DeleteNotification(string chatId, string coin_name)
        {
            var users = GetUsers();

            User userToDelete = users.FirstOrDefault(user => user._id == chatId);

            // Check if the User object was found
            if (userToDelete != null)
            {
                // Find the Coin object in the User's _coins list that matches the given coin name
                Coin coinToDelete = userToDelete._coins.FirstOrDefault(coin => coin._name == coin_name);

                // Check if the Coin object was found
                if (coinToDelete != null)
                {
                    // Remove the Coin object from the User's _coins list
                    userToDelete._coins.Remove(coinToDelete);

                    // If you want to delete the entire user if they have no coins left, you can add this check:
                    if (userToDelete._coins.Count == 0)
                    {
                        users.Remove(userToDelete);
                    }
                }
            }


            string json = JsonConvert.SerializeObject(users, Formatting.Indented);

            File.WriteAllText(filepath, json);

            return true;
        }
    }
}
