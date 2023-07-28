using System.Globalization;
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

        public bool DeleteNotification(string chatId, string notificationIndex)
        {
            if(!int.TryParse(notificationIndex, out int index))
            {
                return false;
            }
            var users = GetUsers();

            users.FirstOrDefault(user => user._id == chatId)._coins.RemoveAt(Convert.ToInt32(notificationIndex)-1);

            string json = JsonConvert.SerializeObject(users, Formatting.Indented);

            File.WriteAllText(filepath, json);

            return true;
        }
    }
}
