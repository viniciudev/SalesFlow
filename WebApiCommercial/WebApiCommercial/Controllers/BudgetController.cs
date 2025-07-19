using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Moves;
using Repository;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class BudgetController : ControllerBase
  {
    private readonly IBudgetService budgetService;

    public BudgetController(IBudgetService budgetService)
    {
      this.budgetService = budgetService;
    }
    // GET: api/<BudgetController>
    [HttpGet]
    public async Task<ActionResult<PagedResult<Budget>>> Get([FromQuery] Filters filter,
      [FromHeader] int tenantid)
    {
      filter.idCompany = tenantid;
      var data = await budgetService.GetAllPaged(filter);
      return Ok(data);
    }

    // GET api/<BudgetController>/5
    [HttpGet("GetByDescription")]
    public async Task<ActionResult<Budget>> GetByDescription([FromQuery] Filters filters, [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      var data =await budgetService.GetByDescription(filters);
      return Ok(data);
    }

    // POST api/<BudgetController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] Budget model,[FromHeader] int tenantid)
    {
        model.IdCompany = tenantid;
        await budgetService.Create(model);
        return Ok(model);
    }

    // PUT api/<BudgetController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<BudgetController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
