using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Registrations;
using Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAppCommercial.Controllers
{
  [Produces("application/json")]
  [Route("api/[controller]")]
  [ApiController]
  public class DescriptionFilesController : ControllerBase
  {
    private readonly IDescriptionFilesService descriptionFilesService;
    private readonly IFileService fileService;

    public DescriptionFilesController(IDescriptionFilesService descriptionFilesService, IFileService fileService)
    {
      this.descriptionFilesService = descriptionFilesService;
      this.fileService = fileService;
    }
    // GET: api/<DescriptionFilesController>

    [HttpGet]
    public async Task<ActionResult<DescriptionFiles>> GetGroup([FromQuery] Filters filter)
    {
      var data = await descriptionFilesService.GetAllPaged(filter);
      return Ok(data);
    }

    // GET api/<DescriptionFilesController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DescriptionFiles>> Get([FromQuery] Filters filter, int id)
    {
      var data = await descriptionFilesService.GetAllPaged(filter);
      return Ok(data);
    }


    [HttpPatch("{codGroup}/group")]
    public async Task<ActionResult<DescriptionFiles>> PatchGroup(Filters filter, int codGroup)
    {
      filter.codGroup = codGroup;
      var data = await descriptionFilesService.GetAllPaged(filter);
      return Ok(data);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<DescriptionFiles>> Patch(Filters filter, int id)
    {

      var data = await descriptionFilesService.GetAllPaged(filter);
      return Ok(data);
    }

    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromForm] IFormCollection dataFiles,
      [FromHeader]int tenantid)
    {
      await descriptionFilesService.save(dataFiles,tenantid);
      return Ok(true);
    }

    [HttpPut("{idDescriptionFiles}/{nameProduct}/{descriptionProduct}/{valueProduct}/{groupItems}/{id}")]
    public async Task<ActionResult<dynamic>> Put(List<IFormFile> body, int idDescriptionFiles, string nameProduct, string descriptionProduct, string valueProduct, int groupItems, int id)
    {

      try
      {

        DescriptionFiles model = new DescriptionFiles();
        model.Id = idDescriptionFiles;
        model.descriptionProduct = descriptionProduct;
        model.NameProduct = nameProduct;
        model.valueProduct = valueProduct;
        model.groupItems = groupItems;
        model.idCompany = 1;

        await descriptionFilesService.alter(model, body);

        return true;

      }
      catch (Exception ex)
      {
        return false;
      }
    }

    // PUT api/<DescriptionFilesController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<DescriptionFilesController>/5
    [HttpDelete("{idDescriptionFiles}")]
    public async Task<ActionResult<dynamic>> Delete(int idDescriptionFiles)
    {
        await descriptionFilesService.Delete(idDescriptionFiles);
        return Ok(true);
    }
  }
}
