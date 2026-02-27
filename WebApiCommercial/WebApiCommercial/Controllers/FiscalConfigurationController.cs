
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Model.Registrations;
using WebApiCommercial.Dtos;
using Model.DTO;

namespace WebApiCommercial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiscalConfigurationController : ControllerBase
    {
        private readonly IFiscalConfigurationService _service;
        private readonly IWebHostEnvironment _env;

        public FiscalConfigurationController(IFiscalConfigurationService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromHeader]int tenantid,
            [FromForm] FiscalConfigurationRequest request)
        {
            // salva arquivo se enviado (IFormFile) ou se vier base64
            if (request.CertificadoDigital != null)
            {
                var caminho = await SaveCertificadoAsync(request.CertificadoDigital);
                request.CertificadoDigital.Arquivo = caminho;
            }

            // mapear DTO para entidade
            var model = request.ToEntity();
            model.CompanyId = tenantid;
            await _service.Create(model);
            return Ok(new ResponseGeneric { Success = true, Data = model});
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id,  [FromForm] FiscalConfigurationRequest request)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            if (request.CertificadoDigital.ArquivoFile != null)
            {
                var caminho = await SaveCertificadoAsync(request.CertificadoDigital);
                request.CertificadoDigital.Arquivo = caminho;
            }

            var model = request.ToEntity();
            model.Id = id;
            model.CompanyId = existing.CompanyId;
            await _service.Alter(model);
            return Ok(new ResponseGeneric{Success=true });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _service.GetAll();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive([FromHeader]int tenantid)
        {
            var cfg = await _service.GetActiveAsync(tenantid);
            if (cfg == null) return NotFound();
            return Ok(cfg);
        }

        // Helper: salva o arquivo TSX enviado (IFormFile) ou decodifica base64 e grava no disco.
        private async Task<string?> SaveCertificadoAsync(CertificadoDigitalRequest? cert)
        {
            if (cert == null) return null;

            // pasta relativa (wwwroot/certs)
            var relativeFolder = Path.Combine("certs");
            var absoluteFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativeFolder);

            if (!Directory.Exists(absoluteFolder))
                Directory.CreateDirectory(absoluteFolder);

            // Prioriza arquivo enviado via multipart/form-data
            if (cert.ArquivoFile != null && cert.ArquivoFile.Length > 0)
            {
                var ext = Path.GetExtension(cert.ArquivoFile.FileName);
                if (string.IsNullOrEmpty(ext)) ext = ".tsx";
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(absoluteFolder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await cert.ArquivoFile.CopyToAsync(stream);

                // retorna caminho relativo para storage (ex: /certs/xxx.tsx)
                return "/" + Path.Combine(relativeFolder, fileName).Replace("\\", "/");
            }

            // Se veio base64 (campo ArquivoBase64), decodifica e salva
            if (!string.IsNullOrEmpty(cert.ArquivoBase64))
            {
                // aceita data-uri ou puro base64
                var base64 = cert.ArquivoBase64;
                var comma = base64.IndexOf(',');
                if (comma >= 0) base64 = base64[(comma + 1)..];

                byte[] bytes;
                try
                {
                    bytes = Convert.FromBase64String(base64);
                }
                catch
                {
                    // inválido
                    return null;
                }

                var ext = ".tsx";
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(absoluteFolder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                return "/" + Path.Combine(relativeFolder, fileName).Replace("\\", "/");
            }

            // sem arquivo
            return cert.Arquivo;
        }
    }
}