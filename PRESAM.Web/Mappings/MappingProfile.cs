//// PRESAM.Web/Mappings/MappingProfile.cs
//using AutoMapper;
//using PRESAM.Application.DTOs;
//using PRESAM.Domain.Entities;

//namespace PRESAM.Web.Mappings
//{
//    public class MappingProfile : Profile
//    {
//        public MappingProfile()
//        {
//            // Product mappings
//            CreateMap<Product, ProductDto>()
//                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

//            CreateMap<ProductDto, Product>()
//                .ForMember(dest => dest.Category, opt => opt.Ignore())
//                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

//            CreateMap<CreateProductDto, Product>()
//                .ForMember(dest => dest.Id, opt => opt.Ignore())
//                .ForMember(dest => dest.Category, opt => opt.Ignore())
//                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

//            // Category mappings: provide product count but ignore full Products collection to avoid cycles
//            CreateMap<Category, CategoryDto>()
//                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

//            CreateMap<CategoryDto, Category>()
//                .ForMember(dest => dest.Products, opt => opt.Ignore())
//                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

//            // Other mappings
//            CreateMap<Cart, CartDto>();

//            CreateMap<CartItem, CartItemDto>()
//                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
//                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product.ImageUrl))
//                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.Price))
//                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.Product.Price));

//            CreateMap<Order, OrderDto>();
//            CreateMap<OrderItem, OrderItemDto>();
//        }
//    }
//}