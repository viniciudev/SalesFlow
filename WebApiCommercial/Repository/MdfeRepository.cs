using Microsoft.EntityFrameworkCore;
using Model;
using Model.MDFe;
using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    /// <summary>
    /// Persistência do MDF-e (modelo 58).
    ///
    /// Três observações que valem para todo método aqui:
    ///
    /// 1. <b>Isolamento por empresa é manual.</b> Não há
    ///    <c>HasQueryFilter</c> nem <c>TenantQueryInterceptor</c> ativo (está
    ///    comentado em <c>ContextBase.OnConfiguring</c>), então todo método
    ///    filtra <c>IdCompany</c> explicitamente — inclusive os "GetById",
    ///    porque o <c>GenericRepository.GetByIdAsync(int)</c> herdado não filtra
    ///    e vazaria manifesto de outra empresa. Mesmo padrão do
    ///    <see cref="VehicleRepository"/>.
    ///
    /// 2. <b>A numeração é reservada em transação</b> (<see cref="AddAsync"/>),
    ///    e não por read-modify-write como o <c>NFeService.CalculateNextNumber</c>
    ///    faz. Dois pedidos simultâneos tirariam o mesmo número naquele desenho;
    ///    aqui um espera o outro.
    ///
    /// 3. <b>Este repositório também lê as NOTAS manifestáveis</b>
    ///    (<see cref="PesquisarNFeSaidaAsync"/>/<see cref="PesquisarNFeEntradaAsync"/>),
    ///    que são de outros agregados. É deliberado: a área de pesquisa da tela
    ///    cruza as duas origens com o que já está manifestado, e espalhar isso em
    ///    três repositórios faria o serviço montar a elegibilidade com três
    ///    idas ao banco em vez de uma consulta por origem.
    /// </summary>
    public class MdfeRepository : GenericRepository<MdfeEmissao>, IMdfeRepository
    {
        /// <summary>
        /// Chave do advisory lock de numeração. O par (namespace, empresa)
        /// serializa a reserva de número POR EMPRESA sem bloquear as demais — 58
        /// é o modelo do MDF-e no leiaute, usado aqui só como namespace estável.
        /// </summary>
        private const int AdvisoryLockNamespaceMdfe = 58;

        public MdfeRepository(ContextBase dbContext) : base(dbContext) { }

        // ------------------------------------------------------------------
        // Listagem e leitura
        // ------------------------------------------------------------------

        /// <summary>
        /// Listagem paginada com os filtros da tela. Traz só o cabeçalho e a
        /// placa de tração: as coleções (documentos, condutores) ficam para o
        /// detalhe, senão a listagem carregaria o manifesto inteiro por linha.
        /// </summary>
        public async Task<PagedResult<MdfeEmissao>> GetPagedAsync(Filters filter)
        {
            var query = _dbContext.Set<MdfeEmissao>()
                .Include(m => m.VeiculoTracao)
                .Where(m => m.IdCompany == filter.IdCompany);

            // Chave OU número no mesmo parâmetro (TextOption), como a placa no
            // cadastro de veículos: o usuário busca "pelo que tem em mãos". O
            // número só é comparado quando a busca é toda dígitos, porque
            // converter a coluna para texto impediria o uso do índice.
            if (!string.IsNullOrWhiteSpace(filter.MdfeChave))
            {
                var search = filter.MdfeChave.Trim();
                var searchUpper = search.ToUpper();

                if (long.TryParse(search, out var numeroBuscado))
                    query = query.Where(m => m.ChaveAcesso.Contains(searchUpper) || m.Numero == numeroBuscado);
                else
                    query = query.Where(m => m.ChaveAcesso.Contains(searchUpper));
            }

            if (!string.IsNullOrWhiteSpace(filter.TextOption))
            {
                var search = filter.TextOption.Trim().ToUpper();
                query = query.Where(m => m.ChaveAcesso.Contains(search));
            }

            if (filter.MdfeStatus.HasValue)
                query = query.Where(m => m.StatusMdfe == filter.MdfeStatus.Value);

            if (filter.MdfeTipoEmitente.HasValue)
                query = query.Where(m => m.TipoEmitente == filter.MdfeTipoEmitente.Value);

            if (filter.MdfeTipoOperacao.HasValue)
                query = query.Where(m => m.TipoOperacao == filter.MdfeTipoOperacao.Value);

            if (!string.IsNullOrWhiteSpace(filter.MdfeUfCarregamento))
            {
                var uf = filter.MdfeUfCarregamento.Trim().ToUpper();
                query = query.Where(m => m.UfCarregamento == uf);
            }

            if (!string.IsNullOrWhiteSpace(filter.MdfeUfDescarregamento))
            {
                var uf = filter.MdfeUfDescarregamento.Trim().ToUpper();
                query = query.Where(m => m.UfDescarregamento == uf);
            }

            // Período por StartDate/EndDate (strings "yyyy-MM-dd"), que é a
            // convenção já usada nas outras listagens de documento fiscal deste
            // repositório — não inventar um par de DateTime só para o MDF-e.
            if (DateTime.TryParse(filter.StartDate, out var inicio))
                query = query.Where(m => m.DataEmissao >= inicio.Date);

            if (DateTime.TryParse(filter.EndDate, out var fim))
            {
                var fimDoDia = fim.Date.AddDays(1);
                query = query.Where(m => m.DataEmissao < fimDoDia);
            }

            return await query
                .AsNoTracking()
                .WithCaseInsensitive()
                .OrderByDescending(m => m.Id)
                .GetPagedAsync(filter.PageNumber, filter.PageSize);
        }

        /// <summary>
        /// Manifesto por Id, restrito à empresa, com tudo que a tela de detalhe e
        /// o <c>MdfeBuilder</c> precisam.
        ///
        /// Os <c>Include</c> são obrigatórios, não otimização: sem
        /// <c>Veiculos.Vehicle</c> e <c>Condutores</c> o detalhe mostraria as
        /// abas de composição vazias, e sem <c>VeiculoTracao.Client</c> a RM08
        /// (RNTRC do proprietário terceiro) avaliaria um veículo proprietário
        /// como se não tivesse dono.
        ///
        /// O <c>Percurso</c> é ordenado por <c>Ordem</c> no próprio Include
        /// porque a sequência é significado no leiaute (o XSD define o trajeto na
        /// ordem de passagem) — devolver fora de ordem faria o serviço montar um
        /// percurso diferente do gravado.
        /// </summary>
        public async Task<MdfeEmissao> GetByIdAsync(int id, int idCompany)
        {
            return await _dbContext.Set<MdfeEmissao>()
                .Include(m => m.Company)
                .Include(m => m.Contratante)
                .Include(m => m.VeiculoTracao).ThenInclude(v => v.Client)
                .Include(m => m.Documentos)
                .Include(m => m.Veiculos).ThenInclude(v => v.Vehicle).ThenInclude(v => v.Client)
                .Include(m => m.Condutores).ThenInclude(c => c.Client)
                .Include(m => m.Percurso.OrderBy(p => p.Ordem))
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && m.IdCompany == idCompany);
        }

        /// <summary>
        /// Versão rastreada, para edição. Separada de <see cref="GetByIdAsync"/>
        /// de propósito: o <c>AsNoTracking</c> daquela existe para a LEITURA não
        /// poluir o change tracker, e usá-la para gravar faria o <c>Update</c>
        /// marcar a árvore inteira como modificada, zerando <c>CreatedAt</c> dos
        /// filhos e reconectando navegações desnecessariamente.
        /// </summary>
        public async Task<MdfeEmissao> GetTrackedAsync(int id, int idCompany)
        {
            return await _dbContext.Set<MdfeEmissao>()
                .Include(m => m.Documentos)
                .Include(m => m.Veiculos)
                .Include(m => m.Condutores)
                .Include(m => m.Percurso)
                .FirstOrDefaultAsync(m => m.Id == id && m.IdCompany == idCompany);
        }

        /// <summary>
        /// Manifesto pela chave de acesso. Usado para não deixar a mesma chave
        /// entrar em dois manifestos ativos (RM10).
        /// </summary>
        public async Task<MdfeEmissao> GetByChaveAsync(int idCompany, string chave)
        {
            if (string.IsNullOrWhiteSpace(chave))
                return null;

            var normalizada = chave.Trim().ToUpper();

            return await _dbContext.Set<MdfeEmissao>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdCompany == idCompany && m.ChaveAcesso == normalizada);
        }

        /// <summary>
        /// Os <see cref="Vehicle"/> escalados no manifesto, na ordem dos papéis
        /// gravados em <c>tb_mdfeVeiculo</c>, com o <c>Client</c> proprietário
        /// incluído.
        ///
        /// A ordem importa: o <c>MdfeBuilder</c> escolhe a tração varrendo a lista,
        /// e o proprietário é obrigatório quando o veículo é de terceiro — sem o
        /// Include, um comboio de terceiros não montaria o grupo <c>prop</c>.
        ///
        /// A tabela de escala guarda só o vínculo: placa, tara, rodado e carroceria
        /// vêm do cadastro VIVO, que é o que o manifesto deve refletir.
        /// </summary>
        public async Task<List<Vehicle>> GetVeiculosDoManifestoAsync(int idMdfe, int idCompany)
        {
            var ids = await _dbContext.Set<MdfeVeiculo>()
                .AsNoTracking()
                .Where(v => v.IdMdfe == idMdfe)
                .OrderBy(v => v.Tipo)
                .ThenBy(v => v.Id)
                .Select(v => v.IdVehicle)
                .ToListAsync();

            if (ids.Count == 0)
                return new List<Vehicle>();

            var veiculos = await _dbContext.Set<Vehicle>()
                .AsNoTracking()
                .Include(v => v.Client)
                .Where(v => v.IdCompany == idCompany && ids.Contains(v.Id))
                .ToListAsync();

            // Reordena pela ordem da escala: o Where não preserva a ordem dos ids,
            // e é a escala (não o cadastro) que diz quem é tração.
            return ids
                .Select(id => veiculos.FirstOrDefault(v => v.Id == id))
                .Where(v => v != null)
                .ToList();
        }

        // ------------------------------------------------------------------
        // Numeração (RM09)
        // ------------------------------------------------------------------

        /// <summary>
        /// Próximo número que será atribuído, para a tela exibir no cabeçalho
        /// ANTES de salvar. Somente leitura — NÃO reserva: reservar aqui faria
        /// cada abertura de tela queimar um número, e o manifesto nunca emitido
        /// deixaria buracos na sequência.
        ///
        /// <paramref name="numeroInicialBase"/> vem de
        /// <c>FiscalConfiguration.NumeracaoDocumentos.Mdfe.NumeroInicial</c> e só
        /// é usado quando ainda não existe manifesto — é o piso configurável.
        /// </summary>
        public async Task<long> GetProximoNumeroAsync(int idCompany, string serie, long numeroInicialBase)
        {
            var serieNormalizada = (serie ?? string.Empty).Trim();

            var ultimo = await _dbContext.Set<MdfeEmissao>()
                .AsNoTracking()
                .Where(m => m.IdCompany == idCompany && m.Serie == serieNormalizada)
                .MaxAsync(m => (long?)m.Numero);

            return ultimo.HasValue ? ultimo.Value + 1 : numeroInicialBase;
        }

        // ------------------------------------------------------------------
        // Escrita
        // ------------------------------------------------------------------

        /// <summary>
        /// Grava o manifesto reservando o número em transação.
        ///
        /// O advisory lock de transação (<c>pg_advisory_xact_lock</c>) é o que
        /// impede dois pedidos simultâneos de tirarem o mesmo número: o par
        /// (namespace, empresa) serializa a reserva por empresa e é liberado
        /// sozinho no commit/rollback, sem risco de lock preso se a requisição
        /// morrer no meio.
        ///
        /// O número é calculado DENTRO do lock — calculá-lo antes e só travar na
        /// hora de gravar não resolveria nada, porque os dois leriam o mesmo
        /// máximo.
        ///
        /// <paramref name="numeroInicialBase"/> só vale para o primeiro
        /// manifesto da série; depois o número é sempre o último + 1.
        /// </summary>
        public async Task<MdfeEmissao> AddAsync(MdfeEmissao mdfe, long numeroInicialBase)
        {
            await using var transacao = await _dbContext.Database.BeginTransactionAsync();

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0}, {1})",
                AdvisoryLockNamespaceMdfe, mdfe.IdCompany);

            mdfe.Numero = await GetProximoNumeroAsync(mdfe.IdCompany, mdfe.Serie, numeroInicialBase);

            var now = DateTime.UtcNow;
            mdfe.CreatedAt = now;
            mdfe.UpdatedAt = now;

            await _dbContext.Set<MdfeEmissao>().AddAsync(mdfe);
            await _dbContext.SaveChangesAsync();

            await transacao.CommitAsync();

            return mdfe;
        }

        /// <summary>
        /// Grava alterações de UM RASCUNHO. O serviço já recusou a edição se o
        /// manifesto tem XML gerado — a assinatura ficaria inválida, então aqui
        /// não há checagem de estado (a regra é de negócio, não de persistência).
        ///
        /// As coleções são SUBSTITUÍDAS, não mescladas: a tela manda a lista
        /// final de documentos/veículos/condutores/percurso, e mesclar exigiria
        /// casar item a item por Id só para descobrir que o usuário trocou tudo.
        /// Como são filhos de um rascunho sem referência externa, apagar e
        /// reinserir é mais simples e não perde nada. O que NÃO é substituído:
        /// <c>Numero</c> e <c>Serie</c> (reservados na criação) e
        /// <c>CreatedAt</c> (relemos do banco).
        /// </summary>
        public async Task<MdfeEmissao> UpdateAsync(MdfeEmissao mdfe)
        {
            var atual = await GetTrackedAsync(mdfe.Id, mdfe.IdCompany)
                ?? throw new InvalidOperationException($"Manifesto {mdfe.Id} não encontrado para a empresa {mdfe.IdCompany}.");

            _dbContext.Set<MdfeDocumento>().RemoveRange(atual.Documentos);
            _dbContext.Set<MdfeVeiculo>().RemoveRange(atual.Veiculos);
            _dbContext.Set<MdfeCondutor>().RemoveRange(atual.Condutores);
            _dbContext.Set<MdfePercurso>().RemoveRange(atual.Percurso);

            // Série, número e criação são do registro, não do payload: aceitar os
            // que vieram no objeto deixaria a tela reescrever a identidade fiscal
            // do manifesto (ou zerar a auditoria) mandando o campo em branco.
            mdfe.Serie = atual.Serie;
            mdfe.Numero = atual.Numero;
            mdfe.CreatedAt = atual.CreatedAt;
            mdfe.UpdatedAt = DateTime.UtcNow;
            mdfe.ChaveAcesso = atual.ChaveAcesso;
            mdfe.XmlCompleto = atual.XmlCompleto;

            foreach (var doc in mdfe.Documentos)
            {
                doc.Id = 0;
                doc.IdMdfe = mdfe.Id;
            }

            foreach (var veiculo in mdfe.Veiculos)
            {
                veiculo.Id = 0;
                veiculo.IdMdfe = mdfe.Id;
            }

            foreach (var condutor in mdfe.Condutores)
            {
                condutor.Id = 0;
                condutor.IdMdfe = mdfe.Id;
            }

            foreach (var percurso in mdfe.Percurso)
            {
                percurso.Id = 0;
                percurso.IdMdfe = mdfe.Id;
            }

            _dbContext.Set<MdfeEmissao>().Update(mdfe);
            await _dbContext.SaveChangesAsync();

            return mdfe;
        }

        /// <summary>
        /// Grava o XML assinado e o resultado da validação, sem passar pelo
        /// <c>Update</c> da entidade inteira.
        ///
        /// <c>ExecuteUpdate</c> porque este é o único campo que muda depois que o
        /// manifesto deixa de ser rascunho — carregar e regravar a árvore toda
        /// para atualizar um texto reescreveria as coleções à toa, e é justamente
        /// o que não se quer num documento já assinado.
        /// </summary>
        public async Task<int> SalvarXmlAsync(int id, int idCompany, string chaveAcesso, string xml, decimal valorTotal, decimal pesoBruto, int quantidadeNFe)
        {
            return await _dbContext.Set<MdfeEmissao>()
                .Where(m => m.Id == id && m.IdCompany == idCompany)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.ChaveAcesso, chaveAcesso)
                    .SetProperty(m => m.XmlCompleto, xml)
                    .SetProperty(m => m.ValorTotal, valorTotal)
                    .SetProperty(m => m.PesoBruto, pesoBruto)
                    .SetProperty(m => m.QuantidadeNFe, quantidadeNFe)
                    .SetProperty(m => m.StatusMdfe, Model.Enums.MdfeStatus.Validado)
                    .SetProperty(m => m.ErrorMessage, (string)null)
                    .SetProperty(m => m.UpdatedAt, DateTime.UtcNow));
        }

        /// <summary>
        /// Marca o manifesto como erro, guardando o motivo. Usado quando a
        /// montagem ou a validação de schema falha — o manifesto continua
        /// existindo e editável, com o motivo visível em vez de um erro genérico.
        /// </summary>
        public async Task<int> MarcarErroAsync(int id, int idCompany, string mensagem)
        {
            return await _dbContext.Set<MdfeEmissao>()
                .Where(m => m.Id == id && m.IdCompany == idCompany)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.StatusMdfe, Model.Enums.MdfeStatus.Erro)
                    .SetProperty(m => m.ErrorMessage, mensagem)
                    .SetProperty(m => m.UpdatedAt, DateTime.UtcNow));
        }

        /// <summary>
        /// Exclui um rascunho. Restrito à empresa e devolve quantas linhas saíram,
        /// para o serviço distinguir "não existe / é de outra empresa" de "ok".
        ///
        /// Os filhos saem junto pelo <c>DeleteBehavior</c> das FKs... que aqui é
        /// <c>Restrict</c> (a convenção do projeto rebaixa Cascade), então são
        /// removidos explicitamente antes. Só rascunho é excluível — manifesto
        /// com XML já é documento fiscal e não se apaga.
        /// </summary>
        public async Task<int> DeleteAsync(int id, int idCompany)
        {
            var mdfe = await GetTrackedAsync(id, idCompany);
            if (mdfe == null)
                return 0;

            _dbContext.Set<MdfeDocumento>().RemoveRange(mdfe.Documentos);
            _dbContext.Set<MdfeVeiculo>().RemoveRange(mdfe.Veiculos);
            _dbContext.Set<MdfeCondutor>().RemoveRange(mdfe.Condutores);
            _dbContext.Set<MdfePercurso>().RemoveRange(mdfe.Percurso);
            _dbContext.Set<MdfeEmissao>().Remove(mdfe);

            return await _dbContext.SaveChangesAsync();
        }

        // ------------------------------------------------------------------
        // Documentos manifestáveis (área de pesquisa da tela)
        // ------------------------------------------------------------------

        /// <summary>
        /// NF-e de SAÍDA manifestáveis: emitidas aqui, autorizadas
        /// (<c>Sent</c> + <c>StatusNfe.emitida</c>) e com chave de 44 dígitos.
        ///
        /// Os filtros de UF e parceiro são aplicados sobre a VENDA, porque a
        /// <c>NFeEmission</c> não guarda UF nem destinatário — quem tem isso é o
        /// <c>Sale.Client</c> ligado a ela.
        ///
        /// Traz <c>SaleItems.Product</c> porque o peso do manifesto sai de
        /// <c>Product.PesoUnitario</c> × quantidade: sem o Include, o peso de
        /// toda nota voltaria zero e o total do manifesto sairia errado sem
        /// ninguém perceber.
        /// </summary>
        public async Task<List<NFeEmission>> PesquisarNFeSaidaAsync(Filters filter, int idCompany)
        {
            var query = _dbContext.Set<NFeEmission>()
                .Include(n => n.Sale).ThenInclude(s => s.Client)
                .Include(n => n.Sale).ThenInclude(s => s.SaleItems).ThenInclude(i => i.Product)
                .Where(n => n.CompanyId == idCompany
                    && n.TipoDocumento == Model.Enums.TipoDocumentoEnum.NFE
                    && n.Sent
                    && n.StatusNfe == StatusNfe.emitida
                    && n.ChaveAcesso != null
                    && n.ChaveAcesso.Length == 44)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.DocumentoChave))
            {
                var chave = filter.DocumentoChave.Trim().ToUpper();
                query = query.Where(n => n.ChaveAcesso.Contains(chave));
            }

            if (filter.DocumentoNumero.HasValue)
                query = query.Where(n => n.Numero == filter.DocumentoNumero.Value);

            if (filter.DocumentoParceiroId.HasValue)
                query = query.Where(n => n.Sale != null && n.Sale.IdClient == filter.DocumentoParceiroId.Value);

            if (!string.IsNullOrWhiteSpace(filter.DocumentoUfDestino))
            {
                var uf = filter.DocumentoUfDestino.Trim().ToUpper();
                query = query.Where(n => n.Sale != null && n.Sale.Client != null && n.Sale.Client.Uf == uf);
            }

            if (DateTime.TryParse(filter.StartDate, out var inicio))
                query = query.Where(n => n.CreatedAt >= inicio.Date);

            if (DateTime.TryParse(filter.EndDate, out var fim))
            {
                var fimDoDia = fim.Date.AddDays(1);
                query = query.Where(n => n.CreatedAt < fimDoDia);
            }

            return await query
                .OrderByDescending(n => n.Id)
                .Take(200)
                .ToListAsync();
        }

        /// <summary>
        /// NF-e de ENTRADA manifestáveis: compras de terceiros importadas do XML
        /// (<c>PurchaseXmlImportService</c>), identificadas pela chave de 44
        /// dígitos.
        ///
        /// Não há filtro de "autorizada" como no lado da saída: a compra só existe
        /// aqui porque o XML autorizado foi importado, então a existência do
        /// registro já é essa prova. O que se checa é a chave ter 44 dígitos, que
        /// é o que o manifesto exige.
        /// </summary>
        public async Task<List<Purchase>> PesquisarNFeEntradaAsync(Filters filter, int idCompany)
        {
            var query = _dbContext.Set<Purchase>()
                .Include(p => p.Fornecedor)
                .Include(p => p.PurchaseItems).ThenInclude(i => i.Produto)
                .Where(p => p.IdCompany == idCompany
                    && p.ChaveNfe != null
                    && p.ChaveNfe.Length == 44)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.DocumentoChave))
            {
                var chave = filter.DocumentoChave.Trim().ToUpper();
                query = query.Where(p => p.ChaveNfe.Contains(chave));
            }

            if (filter.DocumentoParceiroId.HasValue)
                query = query.Where(p => p.FornecedorId == filter.DocumentoParceiroId.Value);

            if (!string.IsNullOrWhiteSpace(filter.DocumentoUfOrigem))
            {
                var uf = filter.DocumentoUfOrigem.Trim().ToUpper();
                query = query.Where(p => p.Fornecedor != null && p.Fornecedor.uf == uf);
            }

            if (DateTime.TryParse(filter.StartDate, out var inicio))
                query = query.Where(p => p.DataEntrada >= inicio.Date);

            if (DateTime.TryParse(filter.EndDate, out var fim))
            {
                var fimDoDia = fim.Date.AddDays(1);
                query = query.Where(p => p.DataEntrada < fimDoDia);
            }

            return await query
                .OrderByDescending(p => p.Id)
                .Take(200)
                .ToListAsync();
        }

        /// <summary>
        /// Chaves de NF-e já vinculadas a manifestos NÃO cancelados desta empresa
        /// — a base da RM10.
        ///
        /// "Não cancelado" e não "existente": um manifesto cancelado libera a nota
        /// para ser manifestada de novo, e é isso que o filtro de status
        /// expressa. Manifesto em erro também libera, porque em erro não houve
        /// documento válido nenhum.
        ///
        /// <paramref name="ignorarMdfeId"/> existe para a tela de EDIÇÃO: sem
        /// ele, os documentos do próprio manifesto sendo editado apareceriam
        /// como "já vinculados" e o usuário não conseguiria remontar a lista.
        /// </summary>
        public async Task<HashSet<string>> GetChavesVinculadasAsync(int idCompany, int? ignorarMdfeId = null)
        {
            var chaves = await _dbContext.Set<MdfeDocumento>()
                .AsNoTracking()
                .Where(d => d.Mdfe.IdCompany == idCompany
                    && d.Mdfe.StatusMdfe != Model.Enums.MdfeStatus.Cancelado
                    && d.Mdfe.StatusMdfe != Model.Enums.MdfeStatus.Erro
                    && (ignorarMdfeId == null || d.IdMdfe != ignorarMdfeId.Value))
                .Select(d => d.ChaveNFe)
                .Distinct()
                .ToListAsync();

            return new HashSet<string>(chaves, StringComparer.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------
        // Histórico de uso do veículo (RV14)
        // ------------------------------------------------------------------

        /// <summary>
        /// Registra o uso de cada veículo do comboio em
        /// <see cref="VehicleUsageHistory"/>, com
        /// <c>Source = VehicleUsageSource.Mdfe</c>.
        ///
        /// É o consumidor que a migration do cadastro de veículos deixou
        /// documentado como pendente: <c>SourceId</c> estava descrito como "nulo
        /// para MDF-e até que a tabela de manifesto seja criada". A tabela existe
        /// agora, e este método é quem preenche.
        ///
        /// Grava um registro por veículo (tração e reboques), não um por
        /// manifesto: a pergunta que o histórico responde é "com o que este
        /// veículo rodou", e é o veículo que é o sujeito. <c>IdCompany</c> vem do
        /// PRÓPRIO veículo, como a entidade documenta, para o histórico respeitar
        /// o isolamento por empresa mesmo se o manifesto for de outra.
        ///
        /// Idempotente por (veículo, manifesto): regravar o XML de um manifesto
        /// que já tem XML não acontece (o serviço recusa), mas a checagem evita
        /// duplicar histórico se algum caminho futuro chamar isto duas vezes.
        /// </summary>
        public async Task RegistrarUsoDosVeiculosAsync(MdfeEmissao mdfe)
        {
            var veiculos = await _dbContext.Set<MdfeVeiculo>()
                .Include(v => v.Vehicle)
                .Where(v => v.IdMdfe == mdfe.Id)
                .ToListAsync();

            if (veiculos.Count == 0)
                return;

            var idsVeiculo = veiculos.Select(v => v.IdVehicle).Distinct().ToList();

            var jaRegistrados = await _dbContext.Set<VehicleUsageHistory>()
                .AsNoTracking()
                .Where(h => h.Source == Model.Enums.VehicleUsageSource.Mdfe
                    && h.SourceId == mdfe.Id
                    && idsVeiculo.Contains(h.IdVehicle))
                .Select(h => h.IdVehicle)
                .ToListAsync();

            var referencia = string.IsNullOrWhiteSpace(mdfe.ChaveAcesso)
                ? $"{mdfe.Serie}/{mdfe.Numero}"
                : mdfe.ChaveAcesso;

            var now = DateTime.UtcNow;

            foreach (var veiculo in veiculos)
            {
                if (jaRegistrados.Contains(veiculo.IdVehicle))
                    continue;

                await _dbContext.Set<VehicleUsageHistory>().AddAsync(new VehicleUsageHistory
                {
                    IdVehicle = veiculo.IdVehicle,
                    IdCompany = veiculo.Vehicle?.IdCompany ?? mdfe.IdCompany,
                    Source = Model.Enums.VehicleUsageSource.Mdfe,
                    SourceId = mdfe.Id,
                    Reference = referencia,
                    UsedAt = mdfe.DataEmissao,
                    CreatedAt = now
                });
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    public interface IMdfeRepository : IGenericRepository<MdfeEmissao>
    {
        Task<PagedResult<MdfeEmissao>> GetPagedAsync(Filters filter);

        /// <summary>
        /// Versão com escopo de empresa. O <c>GetByIdAsync(int)</c> herdado de
        /// <see cref="IGenericRepository{TEntity}"/> NÃO filtra empresa — não o
        /// use para manifesto.
        /// </summary>
        Task<MdfeEmissao> GetByIdAsync(int id, int idCompany);

        /// <summary>Versão rastreada pelo change tracker, para edição.</summary>
        Task<MdfeEmissao> GetTrackedAsync(int id, int idCompany);

        Task<MdfeEmissao> GetByChaveAsync(int idCompany, string chave);

        /// <summary>Veículos escalados no manifesto, com o proprietário incluído, na ordem dos papéis.</summary>
        Task<List<Vehicle>> GetVeiculosDoManifestoAsync(int idMdfe, int idCompany);

        Task<long> GetProximoNumeroAsync(int idCompany, string serie, long numeroInicialBase);

        Task<MdfeEmissao> AddAsync(MdfeEmissao mdfe, long numeroInicialBase);
        Task<MdfeEmissao> UpdateAsync(MdfeEmissao mdfe);
        Task<int> SalvarXmlAsync(int id, int idCompany, string chaveAcesso, string xml, decimal valorTotal, decimal pesoBruto, int quantidadeNFe);
        Task<int> MarcarErroAsync(int id, int idCompany, string mensagem);
        Task<int> DeleteAsync(int id, int idCompany);

        Task<List<NFeEmission>> PesquisarNFeSaidaAsync(Filters filter, int idCompany);
        Task<List<Purchase>> PesquisarNFeEntradaAsync(Filters filter, int idCompany);
        Task<HashSet<string>> GetChavesVinculadasAsync(int idCompany, int? ignorarMdfeId = null);

        Task RegistrarUsoDosVeiculosAsync(MdfeEmissao mdfe);
    }
}
