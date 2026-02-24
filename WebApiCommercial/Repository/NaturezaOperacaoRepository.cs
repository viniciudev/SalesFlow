

using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class NaturezaOperacaoRepository  : GenericRepository<NaturezaOperacao>, INaturezaOperacaoRepository
    {
        public NaturezaOperacaoRepository(ContextBase dbContext) : base(dbContext)
    {

    }



    public async Task<List<NaturezaOperacao>> GetAllAsync()
    {
        return await _dbContext .Set<NaturezaOperacao>()
            .AsNoTracking()
            .ToListAsync();
    }

    //public async Task<NaturezaOperacao?> GetByIdAsync(Guid id)
    //{
    //    return await _dbContext.Set<NaturezaOperacao>()
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(x => x.Id == id);
    //}

    public async Task<bool> ExistsCfopAsync(string cfop, TipoDocumentoEnum tipoDocumento)
    {
        var query = _dbContext.Set<NaturezaOperacao>()
            .AsNoTracking()
            .Where(x => x.Cfop == cfop && x.TipoDocumento == tipoDocumento);

      
        return await query.AnyAsync();
    }

    public async Task UpdateAsync(NaturezaOperacao entity)
    {
        _dbContext.Set<NaturezaOperacao>().Update(entity);
        await _dbContext.SaveChangesAsync();
    }
}
public interface INaturezaOperacaoRepository : IGenericRepository<NaturezaOperacao>
{
    //Task<NaturezaOperacao?> GetByIdAsync(Guid id);
    Task<List<NaturezaOperacao>> GetAllAsync();

    Task<bool> ExistsCfopAsync(string cfop, TipoDocumentoEnum tipoDocumento);
}
}