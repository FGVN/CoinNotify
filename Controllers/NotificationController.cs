using CoinNotify.Models;

namespace CoinNotify.Controllers
{
    /// <summary>
    /// Controller that searches for triggered notifications
    /// </summary>
    internal class NotificationController
    {
        
        /// <summary>
        /// Checks for triggered notifications in users list
        /// </summary>
        /// <param name="users">List of users</param>
        /// <param name="prices">List of current coins with prices</param>
        /// <returns>List of users with coins that has reached notification price</returns>
        public List<User> CheckNotifications(List<User> users, List<Coin> prices)
        {
            List<User> result = new List<User> ();
            foreach (User user in users)
            {
                foreach(Coin coin in user._coins)
                {
                    //if price is lower than zero - check if value is higher than current, else - vice versa
                    if (coin._price < 0)
                    {
                        if(Math.Abs(coin._price) > prices.FirstOrDefault(x => x._name == coin._name)._price)
                        {
                            result.Add(new User(user._id, new List<Coin> { new Coin(coin._name, coin._price) })) ;
                        }
                    }
                    else
                    {
                        if (coin._price < prices.FirstOrDefault(x => x._name == coin._name)._price)
                        {
                            result.Add(new User(user._id, new List<Coin> { new Coin(coin._name, coin._price) }));
                        }
                    }
                }
            }
            return result;
        }
    }
}
