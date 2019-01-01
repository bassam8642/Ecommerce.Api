﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetwood.Core.Domain.Abstract.Repositories;
using Streetwood.Core.Domain.Entities;
using Streetwood.Infrastructure.Services.Implementations.Queries;
using Xunit;

namespace Streetwood.Infrastructure.Tests.QueryServices
{
    public class ProductCategoryDiscountTests
    {
        private readonly Mock<IProductCategoryDiscountRepository> categoryDiscountRepository;
        private readonly Mock<IProductCategoryRepository> productCategoryRepository;
        private readonly Mock<IDiscountCategoryRepository> discountCategoryRepository;
        private readonly Mock<IMapper> mapper;

        public ProductCategoryDiscountTests()
        {
            categoryDiscountRepository = new Mock<IProductCategoryDiscountRepository>();
            productCategoryRepository = new Mock<IProductCategoryRepository>();
            discountCategoryRepository = new Mock<IDiscountCategoryRepository>();
            mapper = new Mock<IMapper>();
        }

        [Fact]
        public void ApplyDiscountsToProducts_ForEmptyDiscounts_ReturnsNull()
        {
            // arrange
            var products = new List<Product>
            {
                new Product("", "", 30, "", "", true, "", "")
            };
            var discounts = new List<ProductCategoryDiscount>();
            var sut = new ProductCategoryDiscountQueryService(categoryDiscountRepository.Object,
                productCategoryRepository.Object, discountCategoryRepository.Object, mapper.Object);

            // act
            var result = sut.ApplyDiscountsToProducts(products, discounts);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public void ApplyDiscountToProducts_ReturnValidPairs()
        {
            // arrange
            var (products, discounts) = PrepareTestData();

            var expected = new List<(Product, ProductCategoryDiscount)>
            {
                (products[0], discounts[0]),
                (products[1], discounts[0]),
                (products[2], discounts[1]),
                (products[3], null)
            };

            // act
            var sut = new ProductCategoryDiscountQueryService(categoryDiscountRepository.Object,
                productCategoryRepository.Object, discountCategoryRepository.Object, mapper.Object);

            var result = sut.ApplyDiscountsToProducts(products, discounts);

            // assert
            result.Count.Should().Be(products.Count);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ApplyDiscountToProducts_ReturnProductsIfNoDiscounts()
        {
            // arrange
            var data = PrepareTestData();
            var products = data.Item1;
            var discounts = data.Item2;

            // act
            var sut = new ProductCategoryDiscountQueryService(categoryDiscountRepository.Object,
                productCategoryRepository.Object, discountCategoryRepository.Object, mapper.Object);
            var result = sut.ApplyDiscountsToProducts(products, discounts);
            var productsWithoutDiscounts = result.Where(s => s.Item2 == null);

            // assert
            productsWithoutDiscounts.Count().Should().Be(1);
        }

        private (List<Product>, List<ProductCategoryDiscount>) PrepareTestData()
        {
            var product1 = new Product("Test", "Test", 50, "Test", "Test", true, "", "");
            var product2 = new Product("Test2", "Test2", 40, "Test2", "Test2", true, "", "");
            var product3 = new Product("Test3", "Test3", 30, "Test3", "Test3", true, "", "");
            var product4 = new Product("Test4", "Test3", 30, "Test3", "Test3", true, "", "");

            var category1 = new ProductCategory("Test1", "Test1");
            var category2 = new ProductCategory("Test2", "Test2");

            product1.SetProductCategory(category1);
            product2.SetProductCategory(category1);
            product3.SetProductCategory(category2);

            var productCategoryDiscount1 = new ProductCategoryDiscount("Test1", "Test1", "Test1", "Test1", 30, true, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(10));
            var productCategoryDiscount2 = new ProductCategoryDiscount("Test2", "Test2", "Test2", "Test2", 30, true, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(10));

            var discountCategory1 = new DiscountCategory(category1, productCategoryDiscount1);
            var discountCategory2 = new DiscountCategory(category2, productCategoryDiscount2);

            productCategoryDiscount1.AddProductCategory(new[] { discountCategory1 });
            productCategoryDiscount2.AddProductCategory(new[] { discountCategory2 });

            var products = new List<Product>{product1, product2, product3, product4};
            var discounts = new List<ProductCategoryDiscount>{productCategoryDiscount1, productCategoryDiscount2};

            return (products, discounts);
        }
    }
}