using CoinNotify.Models;

namespace CoinNotify.Controllers
{
    internal class NotificationController
    {

        public List<User> CheckNotifications(List<User> users, List<Coin> prices)
        {
            List<User> result = new List<User> ();
            foreach (User user in users)
            {
                foreach(Coin coin in user._coins)
                {
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
