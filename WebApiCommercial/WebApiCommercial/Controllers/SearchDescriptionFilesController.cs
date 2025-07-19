using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Model.Registrations;
using Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAppCommercial.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class SearchDescriptionFilesController : ControllerBase
   {
      private readonly IDescriptionFilesService descriptionFilesService;
      private readonly IFileService fileService;

      public SearchDescriptionFilesController(IDescriptionFilesService descriptionFilesService, IFileService fileService)
      {
         this.descriptionFilesService = descriptionFilesService;
         this.fileService = fileService;
      }
      // GET: api/<SearchDescriptionFiles>
      [HttpGet]
      public IEnumerable<string> Get()
      {
         return new string[] { "value1", "value2" };
      }

      // GET api/<SearchDescriptionFiles>/5
      [HttpGet("{name}/{codGroup}")]
      public async Task<ActionResult<DescriptionFiles>> Get(string name,int codGroup)
      {
        var data= await descriptionFilesService.GetSearchPaged(name, codGroup);
         return Ok(data);
      }

   // POST api/<SearchDescriptionFiles>
   [HttpPost]
   public void Post([FromBody] string value)
   {

   }
   // PUT api/<SearchDescriptionFiles>/5
   [HttpPut("{id}")]
   public void Put(int id, [FromBody] string value)
   {

   }

   // DELETE api/<SearchDescriptionFiles>/5
   [HttpDelete("{id}")]
   public void Delete(int id)
   {

   }
}
   }

