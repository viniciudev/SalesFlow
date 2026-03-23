
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
            else
            {
                // se não enviou novo arquivo, manter o caminho existente
                request.CertificadoDigital.Arquivo = existing.CertificadoDigital?.Arquivo;
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

			// Determina o caminho correto baseado no ambiente
			string certsPath;
			if (Environment.GetEnvironmentVariable("RENDER") == "true")
			{
				certsPath = "/app/wwwroot/certs";
			}
			else
			{
				certsPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "certs");
			}

			if (!Directory.Exists(certsPath))
				Directory.CreateDirectory(certsPath);

			string? filePath = null;

			try
			{
				// Prioriza arquivo enviado via multipart/form-data
				if (cert.ArquivoFile != null && cert.ArquivoFile.Length > 0)
				{
					var ext = Path.GetExtension(cert.ArquivoFile.FileName);
					if (string.IsNullOrEmpty(ext)) ext = ".pfx"; // Altere para a extensão correta
					var fileName = $"{Guid.NewGuid()}{ext}";
					filePath = Path.Combine(certsPath, fileName);

					await using var stream = new FileStream(filePath, FileMode.Create);
					await cert.ArquivoFile.CopyToAsync(stream);
				}
				// Se veio base64
				else if (!string.IsNullOrEmpty(cert.ArquivoBase64))
				{
					var base64 = cert.ArquivoBase64;
					var comma = base64.IndexOf(',');
					if (comma >= 0) base64 = base64[(comma + 1)..];

					byte[] bytes = Convert.FromBase64String(base64);
					var fileName = $"{Guid.NewGuid()}.pfx";
					filePath = Path.Combine(certsPath, fileName);
					await System.IO.File.WriteAllBytesAsync(filePath, bytes);
				}
				else if (!string.IsNullOrEmpty(cert.Arquivo))
				{
					return cert.Arquivo;
				}

				// Valida se o arquivo foi salvo e é um certificado válido
				if (filePath != null && System.IO.File.Exists(filePath))
				{
					// Tenta carregar como certificado para validar
					try
					{
						// Verifica se o arquivo não está vazio
						var fileInfo = new FileInfo(filePath);
						if (fileInfo.Length == 0)
						{
							throw new Exception("Arquivo vazio");
						}

						// Retorna caminho relativo baseado no ambiente
						if (Environment.GetEnvironmentVariable("RENDER") == "true")
						{
							// Para o Render, retorna apenas o nome do arquivo
							return Path.GetFileName(filePath);
						}
						else
						{
							// Para desenvolvimento, retorna caminho relativo
							return "/certs/" + Path.GetFileName(filePath);
						}
					}
					catch (Exception ex)
					{
						// Se falhar ao validar, deleta o arquivo e retorna erro
						System.IO.File.Delete(filePath);
						throw new Exception($"Arquivo não é um certificado válido: {ex.Message}");
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				// Log do erro
				Console.WriteLine($"Erro ao salvar certificado: {ex.Message}");
				throw;
			}
		}
	}
}