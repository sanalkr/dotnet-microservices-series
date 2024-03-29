﻿namespace BasketAPI.Model
{
    public class CustomerBasket
    {
        public string BuyerId { get; set; }

        public List<BasketItem> Items { get; set; } = new();
        public CustomerBasket()
        {            
        }
        public CustomerBasket(string id)
        {
            BuyerId = id;
        }
    }
}
