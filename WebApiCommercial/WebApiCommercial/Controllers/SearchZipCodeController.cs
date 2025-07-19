using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SearchZipCodeController : ControllerBase
  {
    // GET: api/<SearchZipCodeController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
      return new string[] { "value1", "value2" };
    }

    // GET api/<SearchZipCodeController>/5
    [HttpGet("{cep}")]
    public async Task<ActionResult<dynamic>> Get(string cep)
    {
      Cep responseCep = null;
      string CepFormatado = "https://viacep.com.br/ws/" + cep + "/json/";
      var httpClient = new HttpClient();


      HttpResponseMessage response = await httpClient.GetAsync(CepFormatado);

      if (response.IsSuccessStatusCode)
      {
        var stringResult = await response.Content.ReadAsStringAsync();
        responseCep = JsonConvert.DeserializeObject<Cep>(stringResult);
      }

      return responseCep;
    }

    // POST api/<SearchZipCodeController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<SearchZipCodeController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<SearchZipCodeController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
