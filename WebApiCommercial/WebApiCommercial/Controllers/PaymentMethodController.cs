using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        // GET: api/<ProductController>
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        public PaymentMethodController(IPaymentMethodRepository paymentMethodRepository)
        {
            _paymentMethodRepository = paymentMethodRepository;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllPage([FromQuery] Filters filter, [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            List<PaymentMethod> data = await _paymentMethodRepository.GetAllPage(filter);
            return Ok(data);
        }

        // GET api/<ProductController>/5
        //[HttpGet("GetListByName")]
        //public async Task<ActionResult<List<Product>>> GetListByName([FromQuery] Filters filter, [FromHeader] int tenantid)
        //{
        //    filter.IdCompany = tenantid;
        //    var data = await _paymentMethodRepository.GetAll();
        //    return Ok(data);
        //}

        // POST api/<ProductController>
        [HttpPost]
        public async Task<ActionResult<dynamic>> Post([FromBody] PaymentMethod model, [FromHeader] int tenantid)
        {
            try
            {
                model.IdCompany = tenantid;
                await _paymentMethodRepository.CreateAsync(model);
                return Ok(new { success = true, message = "Forma de pagamento cadastrado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT api/<ProductController>/5
        [HttpPut]
        public async Task<ActionResult<dynamic>> Put([FromBody] PaymentMethod model)
        {
            try
            {

                await _paymentMethodRepository.UpdateAsync(model.Id,model);
            }
            catch (Exception ex)
            {

                throw;
            }

            return true;
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _paymentMethodRepository.DeleteAsync(id);
                return Ok(new { success = true, message = "Produto excluído com sucesso" });

            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Não é possível excluir o produto!" });
            }
        }
    }
}
