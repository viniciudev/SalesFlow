using Microsoft.AspNetCore.Mvc;
using Model.Registrations;
using Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAppCommercial.Controllers
{
  [Route("api/[controller]")]
   [ApiController]
   public class FileController : ControllerBase
   {
      private readonly IFileService fileService;

      public FileController(IFileService fileService)
      {
         this.fileService = fileService;
      }
      // GET: api/<FileController>
      [HttpGet]
      public IEnumerable<string> Get()
      {
         return new string[] { "value1", "value2" };
      }

      // GET api/<FileController>/5
      [HttpGet("{id}")]
      public async Task<ActionResult<List<File>>> GetAllByIdDescriptionFiles(int id)
      {
      var data = await fileService.GetAllByIdDescriptionFiles(id);
      return Ok(data);
      }
      

      // POST api/<FileController>
      [HttpPost]
      public void Post([FromBody] string value)
      {
      }

      // PUT api/<FileController>/5
      [HttpPut("{id}")]
      public void Put(int id, [FromBody] string value)
      {
      }

      // DELETE api/<FileController>/5
      [HttpDelete("{id}")]
      public async Task<ActionResult<dynamic>> Delete(int files,int id)
      {
         try
         {
            await fileService.DeleteAsync(files);
               return true;
         }
         catch (Exception ex)
         {

            throw;
         }
      }
   }
}
