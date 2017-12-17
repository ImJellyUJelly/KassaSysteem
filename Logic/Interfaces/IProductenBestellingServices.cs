﻿using Logic.Classes;

namespace Logic.Interfaces
{
    public interface IProductenBestellingServices
    {
        void AddProductToBestelling(Bestelling bestelling, Product product);
        void AddLosseVerkoop(Product product, bool isLid);
        void EditProductInBestelling(Bestelling bestelling, Product product);
        void RemoveProductFromBestelling(Bestelling bestelling, Product product);
    }
}
