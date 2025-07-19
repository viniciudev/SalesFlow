using GoogleApi.Entities.Interfaces;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using GoogleApi.Entities.Search.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Model;
using Model.Closure;
using Model.DTO;
using Newtonsoft.Json;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class ClosuresService : BaseService<Closures>, IClosuresService
  {
    private readonly IClosuresDetailService closuresDetailService;
    public ClosuresService(IGenericRepository<Closures> repository,
      IClosuresDetailService closuresDetailService) : base(repository)
    {
      this.closuresDetailService = closuresDetailService;
    }

    public async Task<Closures> GetCheckin(int idSalesman)
    {
      return await (repository as IClosuresRepository).GetCheckin(idSalesman);
    }
    public async Task SaveCheckout(Closures checkout)
    {
      using (var transaction = await repository.CreateTransactionAsync() )
      {
        try
        {
					//decimal KM = await DrivingDistancebyAddressAndLngLat(checkout.LatFinal, checkout.LongFinal,
					//	checkout.LatInit,checkout.LongInit);//pegar km
					await base.Alter(new Closures
					{
						Id = checkout.Id,
						IdSalesman = checkout.IdSalesman,
						DateFinal = checkout.DateFinal,
						LongInit = checkout.LongInit,
						LatInit = checkout.LatInit,
						Type = checkout.Type,
						LongFinal = checkout.LongFinal,
						LatFinal = checkout.LatFinal,
						DateInit = checkout.DateInit,
						//kilometerTraveled = KM,
						OdometerFinal= checkout.OdometerFinal,
						Odometer = checkout.Odometer,
					});
					foreach (var item in checkout.ClosuresDetails)
					{
						item.IdClosures = checkout.Id;
						await closuresDetailService.Save(item);
					}
					await transaction.CommitAsync();
        }
        catch (System.Exception ex)
        {
          await transaction.RollbackAsync();
          throw;
        }
      }
        
    }
    public async Task<decimal> DrivingDistancebyAddressAndLngLat(string latDestino,string longDestino,string latOrigem,string longOrigem)
    {
			latDestino = "50.087";
			longDestino = "14.421";
			latOrigem = "55.93";
			longOrigem = "-3.118";
			var url = "https://maps.googleapis.com/maps/api/distancematrix/json?destinations="+ $"{latDestino},{longDestino}"+
				"&origins="+ $"{latOrigem},{longOrigem}"+ "&key=AIzaSyDIdszKtJn0pDcCR3b9hu506QcRsXhP_J0";
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			using (var client = new HttpClient())
			{
				var result = await client.SendAsync(request);
				var response= await result.Content.ReadAsStreamAsync();
				Root root = await System.Text.Json.JsonSerializer.DeserializeAsync<Root>(response);
				string kmString = root.rows.FirstOrDefault().elements.FirstOrDefault().distance?.text ?? "";
				decimal Km = 0;
				if (kmString != "")
				{
					string replaceKm = kmString.Replace("km", "").TrimEnd().Replace(",",".");
				 Km = decimal.Parse(replaceKm);
				}

				return Km;
			}
		}

    public async Task<PagedResult<ClosuresResponse>> Getpaged(Filters filters)
    {
      return await (repository as IClosuresRepository).Getpaged(filters);
    }

	}
  public interface IClosuresService : IBaseService<Closures>
  {
    Task<Closures> GetCheckin(int idSalesman);
    Task SaveCheckout(Closures checkout);
    Task<decimal> DrivingDistancebyAddressAndLngLat(string latDestino, string longDestino, string latOrigem, string longOrigem);
    Task<PagedResult<ClosuresResponse>> Getpaged(Filters filters);

	}
}
public class Distance
{
	public string text { get; set; }
	public int value { get; set; }
}

public class Duration
{
	public string text { get; set; }
	public int value { get; set; }
}

public class Element
{
	public Distance distance { get; set; }
	public Duration duration { get; set; }
	public string status { get; set; }
}

public class Root
{
	public List<string> destination_addresses { get; set; }
	public List<string> origin_addresses { get; set; }
	public List<Row> rows { get; set; }
	public string status { get; set; }
}

public class Row
{
	public List<Element> elements { get; set; }
}
