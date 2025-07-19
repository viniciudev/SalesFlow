using Microsoft.AspNetCore.Http;
using Model;
using Model.Registrations;
using Newtonsoft.Json;
using Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

using File = Model.Registrations.File;


namespace Service
{

  public class DescriptionFilesService : BaseService<DescriptionFiles>, IDescriptionFilesService
  {
    private readonly IFileService fileService;
    public DescriptionFilesService(IGenericRepository<DescriptionFiles> repository,
       IFileService fileService) : base(repository)
    {
      this.fileService = fileService;
    }

    public async Task alter(DescriptionFiles descriptionFiles, List<IFormFile> body)
    {
      await base.Alter(descriptionFiles);

      byte[] fileBytes = null;
      var filename = string.Empty;
      var contentType = string.Empty;

      if (body.Count > 0)
      {

        var dataFiles = await fileService.GetAllByIdDescriptionFiles(descriptionFiles.Id);
        foreach (var item in dataFiles)
        {
          await fileService.Delete(item);
        }

        foreach (var item in body)
        {
          using (var memoryStream = new MemoryStream())
          {
            await item.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
          }

          filename = item.FileName;
          contentType = item.ContentType;

          File file = new File();
          file.IdDescriptionFiles = descriptionFiles.Id;
          file.Files = fileBytes;
          file.FileName = filename;
          file.ContentType = contentType;

          await fileService.Create(file);
        }
      }
    }

    public async Task save(IFormCollection dataFiles, int idCompany)
    {
      DescriptionFiles deserializedFiles = JsonConvert.DeserializeObject<DescriptionFiles>(dataFiles[key: "data"]);
      deserializedFiles.idCompany = idCompany;
      await base.Save(deserializedFiles);

      byte[] fileBytes = null;
      byte[] fileBytesThumb = null;
      var filename = string.Empty;
      var contentType = string.Empty;

      if (dataFiles.Files.Count > 0)
      {
        foreach (var item in dataFiles.Files)
        {
          using (var memoryStream = new MemoryStream())
          {
            await item.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();

            Image image = Image.FromStream(memoryStream);
            //passar imagem para thumbnail
            using (MemoryStream myResult = new MemoryStream())
            {
              Image thumb = image.GetThumbnailImage(32, 32, () => false, IntPtr.Zero);
              thumb.Save(myResult, ImageFormat.Png);
              fileBytesThumb = myResult.ToArray();
            }
          }
          filename = item.FileName;
          contentType = item.ContentType;

          File file = new File();
          file.IdDescriptionFiles = deserializedFiles.Id;
          file.Files = fileBytes;
          file.FileThumb = fileBytesThumb;
          file.FileName = filename;
          file.ContentType = contentType;
          await fileService.Save(file);
        }
      }

    }
    public async Task<PagedResult<DescriptionFiles>> GetAllPaged(Filters filters)
    {
      return await (repository as IDescriptionFilesRepository).GetAllPaged(filters);
    }
    public Task<PagedResult<DescriptionFiles>> GetSearchPaged(string name, int codGroup)
    {
      return (repository as IDescriptionFilesRepository).GetSearchPaged(name, codGroup);
    }
    public async Task Delete(int id)
    {
      DescriptionFiles data = await (repository as IDescriptionFilesRepository).GetById(id);

      foreach (var item in data.Files)
      {
        await fileService.DeleteAsync(item.Id);
      }
      await base.DeleteAsync(data.Id);
    }
    public async Task<DescriptionFiles> GetById(int id)
    {
      return await (repository as IDescriptionFilesRepository).GetById(id);
    }
  }
  public interface IDescriptionFilesService : IBaseService<DescriptionFiles>
  {
    Task save(IFormCollection dataFiles, int idCompany);
    Task<PagedResult<DescriptionFiles>> GetAllPaged(Filters filters);
    Task<PagedResult<DescriptionFiles>> GetSearchPaged(string name, int codGroup);
    Task alter(DescriptionFiles descriptionFiles, List<IFormFile> body);
    Task Delete(int id);
    Task<DescriptionFiles> GetById(int id);
  }
}


