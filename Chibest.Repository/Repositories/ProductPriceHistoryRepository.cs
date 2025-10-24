﻿using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class ProductPriceHistoryRepository : GenericRepository<ProductPriceHistory>, IProductPriceHistoryRepository
{
    public ProductPriceHistoryRepository(ChiBestDbContext context) : base(context) { }
}