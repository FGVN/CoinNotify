using System.Globalization;
using CoinNotify.Models;
using Newtonsoft.Json;

namespace CoinNotify.Controllers
{
    internal class UsersController
    {
        private string filepath;

        /// <summary>
        /// Parameterless controller, sets up a folder with data
        /// </summary>
        public UsersController() => filepath = Path.Combine("Data", "Users.json");
        /// <summary>
        /// Gets notification for the user
        /// </summary>
        /// <param name="chatId">Users identeficator</param>
        /// <returns>List of users notifications</returns>
        public List<Coin> GetNotifications(string chatId) => GetUsers().FirstOrDefault(x => x._id == chatId)._coins;
        /// <summary>
        /// Gets list of all users from json file
        /// </summary>
        /// <returns>List of users</returns>
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
         /// <summary>
         /// Adds notification to a certain users list
         /// </summary>
         /// <param name="chatId">users identeficator</param>
         /// <param name="coin">coin to add</param>
         /// <param name="price">coins price</param>
         // Currently always true, but can be rewrited to handle some external logic
         // E.g. number of notifications per users, subscription to some tg channel, etc
        public bool AddNotify(string chatId, string coin, string price)
        {
            var users = GetUsers();
            CultureInfo culture = new CultureInfo("en-US");

            //if user already has notifications - add to his list, else - create user and add value to list
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

        /// <summary>
        /// Deletes notification from a certain user
        /// </summary>
        /// <param name="chatId">user to delete from</param>
        /// <param name="notificationIndex">index of coin in users coin list</param>
        /// <returns>False if index is not int or true</returns>
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
