﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Streetwood.Core.Domain.Abstract.Repositories;
using Streetwood.Infrastructure.Dto;
using Streetwood.Infrastructure.Services.Abstract.Queries;

namespace Streetwood.Infrastructure.Services.Implementations.Queries
{
    internal class ProductQueryService : IProductQueryService
    {
        private readonly IProductRepository productRepository;
        private readonly IProductCategoryRepository productCategoryRepository;
        private readonly IMapper mapper;

        public ProductQueryService(IProductRepository productRepository, IProductCategoryRepository productCategoryRepository, IMapper mapper)
        {
            this.productRepository = productRepository;
            this.productCategoryRepository = productCategoryRepository;
            this.mapper = mapper;
        }

        public async Task<IList<ProductListDto>> GetAsync()
        {
            var products = await productRepository.GetAsync();
            return mapper.Map<IList<ProductListDto>>(products);
        }

        public async Task<ProductDto> GetAsync(int id)
        {
            var product = await productRepository.GetAsync(id);
            return mapper.Map<ProductDto>(product);
        }

        public async Task<IList<ProductDto>> GetByCategoryIdAsync(Guid id)
        {
            var category = await productCategoryRepository.GetAndEnsureExist(id);
            return mapper.Map<IList<ProductDto>>(category.Products);
        }
    }
}
