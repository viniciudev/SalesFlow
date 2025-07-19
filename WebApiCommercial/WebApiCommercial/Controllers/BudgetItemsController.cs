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
  public class BudgetItemsController : ControllerBase
  {
    private readonly IBudgetItemsService budgetItemsService;

    public BudgetItemsController(IBudgetItemsService budgetItemsService)
    {
      this.budgetItemsService = budgetItemsService;
    }
    // GET: api/<BudgetItemsController>
    [HttpGet("GedByIdBudget")]
    public async Task<ActionResult<PagedResult<BudgetItems>>> Get([FromQuery] Filters filter)
    {

      var data = await budgetItemsService.GeAllByIdBudget(filter);
      return Ok(data);
    }

    // POST api/<BudgetItemsController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] BudgetItems model)
    {
      var resp=await budgetItemsService.SaveItem(model);
      return Ok(resp);
    }

    // PUT api/<BudgetItemsController>/5
    [HttpPut()]
    public async Task<ActionResult<dynamic>> Put([FromBody] BudgetItems model)
    {
      await budgetItemsService.Alter(model);
      return Ok(true);
    }

    // DELETE api/<BudgetItemsController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<dynamic>> Delete(int id)
    {
      await budgetItemsService.DeleteAsync(id);
      return Ok(true);
    }
  }
}
