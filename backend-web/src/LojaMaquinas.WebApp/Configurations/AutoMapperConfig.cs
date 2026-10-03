using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.WebApp.ViewModels;
using LojaMaquinas.WebApp.ViewModels.API;
using LojaMaquinas.WebApp.ViewModels.MVC;

using LojaMaquinas.WebApp.ViewModels.MVC.OrderRent;
using LojaMaquinas.WebApp.ViewModels.MVC.Contracts;

namespace LojaMaquinas.WebApp.Configurations
{
	/// <summary>
	/// Configuração de mapeamento automático entre entidades de domínio e ViewModels.
	/// Define perfis de mapeamento AutoMapper para API e MVC (bidirecional com ReverseMap).
	/// </summary>
	public class AutoMapperConfig : AutoMapper.Profile
	{
		/// <summary>
		/// Inicializa os mapeamentos AutoMapper entre Models e ViewModels.
		/// Configura mapeamentos bidirecionais para entidades (API e MVC) e ignora navegações complexas onde necessário.
		/// </summary>
		public AutoMapperConfig()
		{
			// API ViewModels
			CreateMap<Category, CategoryApiViewModel>().ReverseMap();
			CreateMap<Order, OrderApiViewModel>().ReverseMap();
			CreateMap<Client, ClientApiViewModel>().ReverseMap();
            CreateMap<User, UserApiViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());
            CreateMap<UserApiViewModel, User>();
			CreateMap<Maintenance, MaintenanceApiViewModel>().ReverseMap();
            CreateMap<Product, ProductApiViewModelWithStocks>().ReverseMap()
				.ForMember(dest => dest.Category, opt => opt.Ignore());

            // MVC ViewModels
            CreateMap<Category, CategoryViewModel>().ReverseMap();
            CreateMap<User, UserViewModel>().ReverseMap();
            CreateMap<Client, ClientViewModel>().ReverseMap();
            CreateMap<Maintenance, MaintenanceViewModel>().ReverseMap();
            CreateMap<Order, OrderViewModel>().ReverseMap();
            CreateMap<Product, ProductViewModel>().ReverseMap();
            CreateMap<Stock, StockViewModel>().ReverseMap();


			CreateMap<OrderRent, OrderRentApiViewModel>().ReverseMap();
			CreateMap<Contract, ContractApiViewModel>().ReverseMap();

			CreateMap<Product, ProductApiViewModel>().ReverseMap()
				.ForMember(dest => dest.Category, opt => opt.Ignore())
				.ForMember(dest => dest.Stocks, opt => opt.Ignore());
            CreateMap<Stock, StockApiViewModel>().ReverseMap()
				.ForMember(dest => dest.Product, opt => opt.Ignore())
				.ForMember(dest => dest.Orders, opt => opt.Ignore());

			CreateMap<Client, ClientApiViewModel>().ReverseMap();


			// MVC OrderRent - criação
			// MVC OrderRent - view geral (lista, detalhes, edit, delete)
			CreateMap<OrderRent, OrderRentViewModel>().ReverseMap();


			CreateMap<CreateOrderRentViewModel, OrderRent>()
				.ForMember(dest => dest.Id, opt => opt.Ignore()) // Id gerido por EF
				.ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
				.ForMember(dest => dest.InitialDate, opt => opt.MapFrom(src => src.StartDate))
				.ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
				.ForMember(dest => dest.CompanyImage, opt => opt.Ignore()) // controller preenche
				.ForMember(dest => dest.UserImage, opt => opt.Ignore())
				.ForMember(dest => dest.Status, opt => opt.Ignore())       // serviço mete Approved
				.ForMember(dest => dest.Contract, opt => opt.Ignore())
				.ForMember(dest => dest.Order, opt => opt.Ignore());

			// MVC Contract
			CreateMap<ContractViewModel, Contract>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.OrderRentId, opt => opt.MapFrom(src => src.OrderRentId))
				.ForMember(dest => dest.TermsConditions, opt => opt.MapFrom(src => src.TermsConditions))
				.ForMember(dest => dest.OrderRent, opt => opt.Ignore());

			CreateMap<Contract, ContractViewModel>();

		}
	}
}
