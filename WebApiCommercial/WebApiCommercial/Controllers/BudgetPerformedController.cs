using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Moves;
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
  public class BudgetPerformedController : ControllerBase
  {
    private IBudgetPerformedService budgetPerformedService;
    
    public BudgetPerformedController(IBudgetPerformedService budgetPerformedService)
    {
      this.budgetPerformedService = budgetPerformedService;
    }
    // GET: api/<BudgetPerformedController>
    [HttpGet]
    public async Task<ActionResult<BudgetPerformed>> GetComparation([FromQuery] Filters filters)
    {
      var data = await budgetPerformedService.GetByComparation(filters);
      return Ok(data);
    }

    // GET api/<BudgetPerformedController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<BudgetPerformedController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<BudgetPerformedController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<BudgetPerformedController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
