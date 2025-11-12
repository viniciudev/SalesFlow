using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Registrations;
using Repository;
using Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly IProductService productServie;

        public ProductController(IProductService productServie)
        {
            this.productServie = productServie;
        }

        // GET: api/<ProductController>
        [HttpGet]
        public async Task<ActionResult<PagedResult<Product>>> Get([FromQuery] Filters filter, [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            var data = await productServie.GetAllPaged(filter);
            return Ok(data);
        }

        // GET api/<ProductController>/5
        [HttpGet("GetListByName")]
        public async Task<ActionResult<List<Product>>> GetListByName([FromQuery] Filters filter, [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            var data = await productServie.GetListByName(filter);
            return Ok(data);
        }

        // POST api/<ProductController>
        [HttpPost]
        public async Task<ActionResult<dynamic>> Post([FromBody] ProductCreateModelDto model, [FromHeader] int tenantid)
        {
            try
            {
                await productServie.SaveProduct(model, tenantid);
                return Ok(new { success = true, message = "Produto cadastrado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT api/<ProductController>/5
        [HttpPut]
        public async Task<ActionResult<dynamic>> Put([FromBody] Product model)
        {
            try
            {

                await productServie.Alter(model);
            }
            catch (Exception ex)
            {

                throw;
            }
      
            return true;
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
