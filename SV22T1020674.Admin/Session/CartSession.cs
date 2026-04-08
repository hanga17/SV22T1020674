using Newtonsoft.Json;
using SV22T1020674.Models.Sales;

namespace SV22T1020674.Session
{
    public static class CartSession
    {
        private const string KEY = "SHOPPING_CART";

        public static List<CartItem> GetCart(ISession session)
        {
            var data = session.GetString(KEY);
            if (string.IsNullOrEmpty(data))
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(data) ?? new List<CartItem>();
        }

        public static void SaveCart(ISession session, List<CartItem> cart)
        {
            session.SetString(KEY, JsonConvert.SerializeObject(cart));
        }

        public static void AddItem(ISession session, CartItem item)
        {
            var cart = GetCart(session);

            var existing = cart.FirstOrDefault(x => x.ProductID == item.ProductID);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                cart.Add(item);
            }

            SaveCart(session, cart);
        }

        public static void Clear(ISession session)
        {
            session.Remove(KEY);
        }
        public static void ClearCart(ISession session)
        {
            session.Remove("cart");
        }
    }
}