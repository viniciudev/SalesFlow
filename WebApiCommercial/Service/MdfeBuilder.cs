#nullable enable
using DFe.Classes.Entidades;
using DFe.Classes.Extensoes;
using DFe.Classes.Flags;
using MDFe.Classes.Flags;
using MDFe.Classes.Informacoes;
using Model;
using Model.Enums;
using Model.MDFe;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using MDFeEletronico = MDFe.Classes.Informacoes.MDFe;

namespace Service
{
    /// <summary>
    /// Monta o XML do MDF-e (modelo 58, leiaute <c>mdfe_v3.00</c>) a partir das
    /// entidades do ServiceBOX.
    ///
    /// Classe separada de propósito, como o <see cref="DpsBuilder"/>: é a única
    /// parte da emissão que pode ser verificada OFFLINE. Dá para montar o
    /// manifesto, validar contra os XSDs da SEFAZ e conferir o agrupamento de
    /// <c>infMunDescarga</c> sem certificado e sem rede. Manter isso isolado é o
    /// que permite provar o XML antes de qualquer tentativa de transmissão.
    ///
    /// Não faz I/O e não conhece repositório: recebe entidades já carregadas e
    /// devolve o objeto da biblioteca. Quem carrega é o <c>MdfeService</c>.
    ///
    /// <b>Não toca em <c>MDFeConfiguracao.Instancia</c>.</b> Toda construção passa
    /// a versão do leiaute EXPLICITAMENTE (<c>new MDFeIde(VersaoServico.Versao300)</c>)
    /// porque o construtor sem argumento cai no singleton estático — que é
    /// compartilhado entre requisições e é justamente o defeito que o
    /// <c>NFeService</c> tem. Aqui não há estado estático nenhum.
    /// </summary>
    public static class MdfeBuilder
    {
        /// <summary>
        /// Versão do leiaute. Constante nomeada para não virar literal espalhado:
        /// quando a SEFAZ publicar uma versão nova, muda-se aqui e nos XSDs
        /// vendorizados em <c>NFSchemas/</c>.
        /// </summary>
        public const MDFe.Utils.Flags.VersaoServico VersaoLayout = MDFe.Utils.Flags.VersaoServico.Versao300;

        /// <summary>Versão do processo de emissão declarada no XML (<c>ide.verProc</c>, até 20 caracteres).</summary>
        private const string VersaoProcesso = "ServiceBOX-1.0";

        /// <summary>Limite do XSD para reboques (<c>veicReboque maxOccurs="3"</c>).</summary>
        public const int MaxReboques = 3;

        /// <summary>Limite do XSD para condutores (<c>veicTracao.condutor maxOccurs="10"</c>).</summary>
        public const int MaxCondutores = 10;

        /// <summary>
        /// Monta o manifesto. <paramref name="mdfe"/> precisa vir com as coleções
        /// carregadas e <paramref name="veiculos"/> com o <see cref="Vehicle"/>
        /// resolvido — o builder não volta ao banco para buscar placa ou tara.
        ///
        /// <paramref name="ambiente"/> e <paramref name="ufEmitente"/> vêm da
        /// <see cref="FiscalConfiguration"/>: o ambiente é o do certificado
        /// (homologação x produção) e a UF do emitente define o <c>cUF</c>.
        /// </summary>
        public static MDFeEletronico Construir(
            MdfeEmissao mdfe,
            FiscalConfiguration config,
            IEnumerable<Vehicle> veiculos,
            TipoAmbiente ambiente)
        {
            if (mdfe == null) throw new ArgumentNullException(nameof(mdfe));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var emitente = config.Emitente
                ?? throw new InvalidOperationException("FiscalConfiguration sem Emitente preenchido.");

            var listaVeiculos = (veiculos ?? Enumerable.Empty<Vehicle>())
                .Where(v => v != null)
                .ToList();

            if (listaVeiculos.Count == 0)
                throw new InvalidOperationException("Manifesto sem veículo: não há como montar a composição do comboio.");

            var ufEmitente = ExigirUf(emitente.EmitenteEndereco?.Uf, "UF do emitente", "configuração fiscal da empresa");

            var documento = new MDFeEletronico
            {
                InfMDFe = new MDFeInfMDFe
                {
                    // A versão é fixada aqui, e não em Assina(): se dependesse só
                    // do Assina, um XML gerado sem assinar sairia com a versão
                    // errada — e o Valida() escolhe o XSD por este campo.
                    Versao = VersaoLayout,
                    Ide = MontarIde(mdfe, emitente, ufEmitente, ambiente),
                    Emit = MontarEmit(emitente, ufEmitente),
                    InfModal = MontarModalRodoviario(mdfe, listaVeiculos, emitente),
                    InfDoc = MontarInfDoc(mdfe),
                    ProdPred = MontarProdutoPredominante(mdfe),
                    Tot = MontarTotais(mdfe),
                    InfAdic = MontarInfAdic(mdfe)
                }
            };

            return documento;
        }

        // =====================================================================
        // ide
        // =====================================================================

        /// <summary>
        /// Bloco de identificação.
        ///
        /// O <c>cMDF</c> (código numérico de 8 dígitos) é DERIVADO do próprio
        /// manifesto, e não sorteado: ele entra na composição da chave de acesso,
        /// então precisa ser o mesmo toda vez que o XML for remontado. Um sorteio
        /// faria a chave mudar entre tentativas, e a segunda tentativa viraria um
        /// segundo documento fiscal.
        /// </summary>
        private static MDFeIde MontarIde(MdfeEmissao mdfe, Emitente emitente, Estado ufEmitente, TipoAmbiente ambiente)
        {
            var ufIni = ExigirUf(mdfe.UfCarregamento, "UF de carregamento", "manifesto");
            var ufFim = ExigirUf(mdfe.UfDescarregamento, "UF de descarregamento", "manifesto");

            var ide = new MDFeIde(VersaoLayout)
            {
                CUF = ufEmitente,
                TpAmb = ambiente,
                TpEmit = MapTipoEmitente(mdfe.TipoEmitente),
                Mod = ModeloDocumento.MDFe,
                Serie = ExigirSerie(mdfe.Serie),
                NMDF = mdfe.Numero,
                CMDF = GerarCodigoNumerico(mdfe),
                Modal = MapModal(mdfe.Modal),
                DhEmi = mdfe.DataEmissao,
                TpEmis = MDFeTipoEmissao.Normal,
                ProcEmi = MDFeIdentificacaoProcessoEmissao.EmissaoComAplicativoContribuinte,
                VerProc = VersaoProcesso,
                UFIni = ufIni,
                UFFim = ufFim,
                // A data de início da viagem é opcional no leiaute e NÃO é a data
                // de emissão: preencher com a emissão declararia à fiscalização
                // uma viagem que pode não ter começado. Fica nula até a tela ter
                // um campo para ela.
                DhIniViagem = null,
                InfMunCarrega = new List<MDFeInfMunCarrega>
                {
                    new MDFeInfMunCarrega
                    {
                        CMunCarrega = ExigirCodigoIbge(mdfe.CodMunCarregamento, "município de carregamento"),
                        XMunCarrega = ExigirTexto(mdfe.MunCarregamento, "nome do município de carregamento", 2, 60)
                    }
                }
            };

            // infPercurso é opcional (minOccurs=0) e vai na ORDEM gravada: o XSD
            // define a sequência como o trajeto real. Ordenar por Ordem aqui, e
            // não confiar na ordem de chegada, protege contra o repositório
            // devolver fora de sequência.
            var percurso = (mdfe.Percurso ?? new List<MdfePercurso>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.UfPercurso))
                .OrderBy(p => p.Ordem)
                .ToList();

            if (percurso.Count > 0)
            {
                ide.InfPercurso = percurso
                    .Select(p => new MDFeInfPercurso { UFPer = ExigirUf(p.UfPercurso, "UF de percurso", "manifesto") })
                    .ToList();
            }

            return ide;
        }

        /// <summary>
        /// Código numérico de 8 dígitos da chave de acesso.
        ///
        /// Derivado de (empresa, série, número) por um hash estável — NÃO é
        /// aleatório e NÃO é a data. Como não temos gerador de aleatório semeado
        /// por manifesto, o hash é o que garante reprodutibilidade: o mesmo
        /// manifesto sempre produz o mesmo <c>cMDF</c>, e portanto a mesma chave.
        /// FNV-1a de 32 bits porque é determinístico entre execuções e processos
        /// (o <c>string.GetHashCode()</c> do .NET é aleatorizado por processo e
        /// NÃO serviria aqui).
        /// </summary>
        private static int GerarCodigoNumerico(MdfeEmissao mdfe)
        {
            var semente = $"MDFE|{mdfe.IdCompany}|{mdfe.Serie}|{mdfe.Numero}";
            unchecked
            {
                const uint offset = 2166136261;
                const uint prime = 16777619;
                var hash = offset;
                foreach (var c in semente)
                {
                    hash ^= c;
                    hash *= prime;
                }

                return (int)(hash % 100000000);
            }
        }

        // =====================================================================
        // emit
        // =====================================================================

        /// <summary>
        /// Emitente. O <c>CNPJ</c>/<c>CPF</c> é <c>choice</c> no XSD: sai um ou
        /// outro, nunca os dois. Preferimos CNPJ e só caímos para CPF quando não
        /// houver CNPJ — é o caso do produtor rural pessoa física.
        ///
        /// A <c>IE</c> é opcional no leiaute (<c>minOccurs="0"</c>) e é omitida
        /// quando vazia em vez de sair como elemento vazio: <c>TIe</c> não aceita
        /// string vazia.
        /// </summary>
        private static MDFeEmit MontarEmit(Emitente emitente, Estado ufEmitente)
        {
            var cnpj = SomenteDigitos(emitente.Cnpj);
            var cpf = SomenteDigitos(emitente.Cpf);

            if (string.IsNullOrEmpty(cnpj) && string.IsNullOrEmpty(cpf))
                throw new InvalidOperationException("Emitente sem CNPJ nem CPF na configuração fiscal — sem isso não há chave de acesso possível.");

            var endereco = emitente.EmitenteEndereco
                ?? throw new InvalidOperationException("Emitente sem endereço na configuração fiscal: o grupo enderEmit é obrigatório no leiaute.");

            var cep = SomenteDigitos(endereco.Cep);
            var cMun = SomenteDigitos(endereco.CodigoCidade);

            if (string.IsNullOrEmpty(cMun) || cMun.Length != 7)
                throw new InvalidOperationException("Emitente sem código IBGE do município (7 dígitos) na configuração fiscal — o grupo enderEmit exige cMun.");

            var emit = new MDFeEmit
            {
                CNPJ = string.IsNullOrEmpty(cnpj) ? null : cnpj.PadLeft(14, '0'),
                CPF = string.IsNullOrEmpty(cnpj) ? cpf?.PadLeft(11, '0') : null,
                IE = SomenteDigitos(emitente.InscricaoEstadual),
                XNome = ExigirTexto(emitente.RazaoSocial, "razão social do emitente", 2, 60),
                XFant = ExigirOpcional(emitente.Fantasia, "nome fantasia do emitente", 60),
                EnderEmit = new MDFeEnderEmit
                {
                    XLgr = ExigirTexto(endereco.Logradouro, "logradouro do emitente", 2, 60),
                    Nro = ExigirTexto(endereco.Numero, "número do endereço do emitente", 1, 60),
                    XCpl = Truncar(endereco.Complemento, 60),
                    XBairro = ExigirTexto(endereco.Bairro, "bairro do emitente", 2, 60),
                    CMun = long.Parse(cMun),
                    XMun = ExigirTexto(endereco.Cidade, "município do emitente", 2, 60),
                    // O CEP entra como número e o ProxyCEP o formata em 8 dígitos.
                    // Sem CEP cadastrado, 0 vira "00000000" — que é o que o
                    // leiaute aceita para "não informado" (o campo é obrigatório
                    // no grupo, não pode ser omitido).
                    CEP = string.IsNullOrEmpty(cep) ? 0 : long.Parse(cep),
                    UF = ufEmitente,
                    Fone = SomenteDigitos(emitente.EmitenteContato?.Telefone),
                    Email = Truncar(emitente.EmitenteContato?.Email, 60)
                }
            };

            return emit;
        }

        // =====================================================================
        // infModal / rodo
        // =====================================================================

        /// <summary>
        /// Modal rodoviário — o único implementado na fase 1.
        ///
        /// A composição é 1 tração + até 3 reboques (limite do XSD, não do MOC
        /// antigo que dizia 5). Mais reboques que isso é erro de montagem e o
        /// builder recusa em vez de truncar: um comboio truncado em silêncio
        /// seria um manifesto que não corresponde ao que está na estrada.
        /// </summary>
        private static MDFeInfModal MontarModalRodoviario(MdfeEmissao mdfe, List<Vehicle> veiculos, Emitente emitente)
        {
            var tracao = EscolherTracao(mdfe, veiculos);

            var reboques = veiculos
                .Where(v => v.Id != tracao.Id)
                .ToList();

            if (reboques.Count > MaxReboques)
                throw new InvalidOperationException($"O manifesto tem {reboques.Count} reboques e o leiaute aceita no máximo {MaxReboques}.");

            var condutores = MontarCondutores(mdfe);
            if (condutores.Count == 0)
                throw new InvalidOperationException("Manifesto sem condutor: o leiaute exige de 1 a 10 condutores no veículo de tração.");
            if (condutores.Count > MaxCondutores)
                throw new InvalidOperationException($"O manifesto tem {condutores.Count} condutores e o leiaute aceita no máximo {MaxCondutores}.");

            var rodo = new MDFeRodo
            {
                VeicTracao = new MDFeVeicTracao
                {
                    // cInt é o código interno de frota, opcional e limitado a 10
                    // caracteres no XSD — truncamos porque é campo de referência
                    // interna, não dado fiscal.
                    CInt = Truncar(tracao.InternalCode, 10),
                    Placa = ExigirPlaca(tracao.LicensePlate),
                    RENAVAM = SomenteDigitos(tracao.Renavam),
                    Tara = tracao.Tare,
                    CapKG = tracao.CapacityKg,
                    // CapM3 no XSD é inteiro (0..999) e o cadastro guarda decimal.
                    // Truncar para inteiro é uma perda real: por isso só enviamos
                    // quando o valor é inteiro, senão o campo seria recusado ou
                    // arredondado sem ninguém saber. Sem valor inteiro, o campo
                    // fica de fora — é opcional no leiaute.
                    CapM3 = CapacidadeM3Inteira(tracao.CapacityM3),
                    Prop = MontarProprietario(tracao),
                    Condutor = condutores,
                    TpRod = MapTipoRodado(tracao.WheelType),
                    TpCar = MapTipoCarroceria(tracao.BodyType),
                    UF = ExigirUf(tracao.LicensingState, "UF de licenciamento do veículo de tração", "cadastro do veículo")
                },
                VeicReboque = reboques
                    .Select(v => new MDFeVeicReboque
                    {
                        CInt = Truncar(v.InternalCode, 10),
                        Placa = ExigirPlaca(v.LicensePlate),
                        RENAVAM = SomenteDigitos(v.Renavam),
                        Tara = v.Tare,
                        // No reboque o capKG é obrigatório no XSD (não tem
                        // minOccurs="0"): sem capacidade cadastrada não há como
                        // emitir, e é melhor falhar aqui do que na SEFAZ.
                        CapKG = v.CapacityKg
                            ?? throw new InvalidOperationException($"O reboque {v.LicensePlate} não tem capacidade (KG) cadastrada, e o leiaute exige capKG no reboque."),
                        CapM3 = CapacidadeM3Inteira(v.CapacityM3),
                        Prop = MontarProprietario(v),
                        TpCar = MapTipoCarroceria(v.BodyType),
                        UF = ExigirUf(v.LicensingState, $"UF de licenciamento do reboque {v.LicensePlate}", "cadastro do veículo")
                    })
                    .ToList()
            };

            // infANTT — só para PST (RM08). Em carga própria não existe contrato
            // de transporte a declarar, e omitir o grupo é o que o leiaute espera.
            if (mdfe.TipoEmitente == MdfeTipoEmitente.Pst)
                rodo.InfANTT = MontarInfANTT(mdfe, emitente, tracao);

            return new MDFeInfModal
            {
                VersaoModal = MDFeVersaoModal.Versao300,
                Modal = rodo
            };
        }

        /// <summary>
        /// Grupo <c>infANTT</c> — RNTRC do emitente, CIOT e contratante (RM08).
        ///
        /// O RNTRC aqui é o do EMITENTE (a transportadora), que é diferente do
        /// RNTRC do proprietário do veículo em <c>veicTracao.prop</c>: aquele
        /// diz de quem é o caminhão, este diz quem presta o serviço. Confundir os
        /// dois é rejeição na SEFAZ, e é por isso que o campo mora na
        /// configuração fiscal e não no cadastro do veículo.
        /// </summary>
        private static MDFeInfANTT MontarInfANTT(MdfeEmissao mdfe, Emitente emitente, Vehicle tracao)
        {
            var rntrc = SomenteDigitos(emitente.Rntrc);
            if (string.IsNullOrEmpty(rntrc))
                throw new InvalidOperationException("MDF-e de Prestador de Serviço de Transporte (PST) exige o RNTRC do emitente — preencha o RNTRC na configuração fiscal da empresa.");

            var infAntt = new MDFeInfANTT
            {
                RNTRC = rntrc.PadLeft(8, '0')
            };

            // infCIOT identifica o contrato de frete. Cada CIOT é de um
            // contratante, e o XSD exige o CPF/CNPJ junto — por isso o CIOT só
            // entra quando há contratante identificado.
            var contratante = mdfe.Contratante;
            var ciot = SomenteDigitos(mdfe.CodigoCIOT);

            if (!string.IsNullOrEmpty(ciot) && contratante != null)
            {
                var documento = DocumentoFiscal(contratante.Document, "contratante do transporte");

                infAntt.InfCIOT = new List<infCIOT>
                {
                    new infCIOT
                    {
                        CIOT = ciot.PadLeft(12, '0'),
                        CPF = documento.Cpf,
                        CNPJ = documento.Cnpj
                    }
                };
            }

            if (contratante != null)
            {
                var documento = DocumentoFiscal(contratante.Document, "contratante do transporte");

                infAntt.InfContratante = new List<MDFeInfContratante>
                {
                    new MDFeInfContratante
                    {
                        XNome = ExigirTexto(contratante.Name, "nome do contratante do transporte", 2, 60),
                        CPF = documento.Cpf,
                        CNPJ = documento.Cnpj
                    }
                };

                // infPag: contrato de frete. O XSD exige ao menos um Comp, então
                // declaramos o valor do frete como componente de pagamento.
                //
                // tpComp NÃO tem valor "frete": o domínio do leiaute é
                // 01 vale-pedágio, 02 impostos/taxas, 03 despesas bancárias e
                // 99 outros. A enumeração da biblioteca tem um MDFeTpComp.Frete
                // ("04") que o schema NÃO aceita — usar 99 com a descrição em
                // xComp é o caminho previsto pelo próprio leiaute para um
                // componente que não é nenhum dos três. (Achado pelo harness
                // offline: o XML saía com tpComp=04 e reprovava em
                // mdfeModalRodoviario_v3.00.xsd.)
                //
                // infBanc é OBRIGATÓRIO dentro de infPag, então o grupo só é
                // montado quando há destino de pagamento informado. Sem destino,
                // infPag inteiro fica de fora (é opcional) em vez de sair
                // incompleto e reprovar o schema.
                var infBanc = MontarInfBanc(mdfe);

                if (infBanc != null)
                {
                    infAntt.InfPag = new List<MDFeInfPag>
                    {
                        new MDFeInfPag
                        {
                            XNome = ExigirTexto(contratante.Name, "nome do contratante do transporte", 2, 60),
                            CPF = documento.Cpf,
                            CNPJ = documento.Cnpj,
                            Comp = new List<MDFeComp>
                            {
                                new MDFeComp
                                {
                                    TpComp = MDFeTpComp.Outros,
                                    VComp = mdfe.ValorTotal,
                                    XComp = "Frete"
                                }
                            },
                            VContratoProxy = mdfe.ValorTotal,
                            IndPag = mdfe.IndicadorPagamento == MdfeIndicadorPagamento.APrazo
                                ? MDFeIndPag.PagamentoPrazo
                                : MDFeIndPag.PagamentoVista,
                            // indAltoDesemp NÃO é preenchido: na classe da
                            // biblioteca ele está declarado DEPOIS de infBanc, mas
                            // no XSD ele vem ANTES de indPag. Preenchê-lo emitiria
                            // o elemento fora de ordem e o schema reprovaria.
                            InfBanc = infBanc
                        }
                    };
                }
            }

            return infAntt;
        }

        /// <summary>
        /// Destino do pagamento (<c>infBanc</c>), que o leiaute define como um
        /// <c>xs:choice</c> de três alternativas mutuamente exclusivas.
        ///
        /// Devolve nulo quando nada foi informado — e aí <c>infPag</c> não é
        /// emitido. Quando DOIS ramos estão preenchidos, falha em vez de escolher:
        /// o XSD aceitaria só um, e escolher por conta própria mandaria o dinheiro
        /// para o destino errado.
        /// </summary>
        private static MDFeInfBanc? MontarInfBanc(MdfeEmissao mdfe)
        {
            var banco = (mdfe.PagamentoBanco ?? string.Empty).Trim();
            var agencia = (mdfe.PagamentoAgencia ?? string.Empty).Trim();
            var ipef = SomenteDigitos(mdfe.PagamentoCnpjIpef);
            var pix = (mdfe.PagamentoChavePix ?? string.Empty).Trim();

            var temBanco = banco.Length > 0 || agencia.Length > 0;
            var temIpef = !string.IsNullOrEmpty(ipef);
            var temPix = pix.Length > 0;

            var ramos = new List<string>();
            if (temBanco) ramos.Add("banco/agência");
            if (temIpef) ramos.Add("CNPJ IPEF");
            if (temPix) ramos.Add("PIX");

            if (ramos.Count == 0)
                return null;

            if (ramos.Count > 1)
                throw new InvalidOperationException($"O destino do pagamento do frete aceita apenas uma forma e foram informadas {ramos.Count} ({string.Join(", ", ramos)}). Informe apenas uma.");

            if (temBanco)
            {
                if (banco.Length < 3 || banco.Length > 5)
                    throw new InvalidOperationException($"O código do banco deve ter de 3 a 5 caracteres (recebido: '{mdfe.PagamentoBanco}').");

                if (agencia.Length < 1 || agencia.Length > 10)
                    throw new InvalidOperationException($"O código da agência deve ter de 1 a 10 caracteres (recebido: '{mdfe.PagamentoAgencia}').");

                return new MDFeInfBanc { CodBanco = banco, CodAgencia = agencia };
            }

            if (temIpef)
            {
                if (ipef!.Length != 14)
                    throw new InvalidOperationException($"O CNPJ da instituição de pagamento deve ter 14 dígitos (recebido: '{mdfe.PagamentoCnpjIpef}').");

                return new MDFeInfBanc { CNPJIPEF = ipef };
            }

            if (pix.Length < 2 || pix.Length > 60)
                throw new InvalidOperationException($"A chave PIX deve ter de 2 a 60 caracteres (recebido: '{pix.Length}').");

            return new MDFeInfBanc { PIX = pix };
        }

        /// <summary>
        /// Proprietário do veículo (<c>veicTracao.prop</c>) — opcional no XSD, mas
        /// obrigatório na prática quando o veículo é de TERCEIRO: sem o RNTRC do
        /// proprietário a fiscalização não identifica quem cedeu o veículo.
        ///
        /// Para veículo próprio o grupo é omitido: o proprietário é o próprio
        /// emitente, já identificado em <c>emit</c>, e repetir não acrescenta.
        ///
        /// <b>IE e UF saem sempre em PAR.</b> No leiaute os dois estão dentro de
        /// um <c>xs:sequence minOccurs="0"</c>: ou vêm os dois, ou não vem nenhum
        /// — declarar só a UF (que é o que sairia, já que ela sempre existe no
        /// cadastro e a IE pode não existir) faz o schema recusar o elemento
        /// fora de ordem. Por isso, sem IE informada, os dois são omitidos.
        /// </summary>
        private static MDFeProp? MontarProprietario(Vehicle veiculo)
        {
            if (veiculo.OwnershipType == OwnershipType.Proprio)
                return null;

            var proprietario = veiculo.Client
                ?? throw new InvalidOperationException($"O veículo {veiculo.LicensePlate} é de terceiro e não tem proprietário vinculado no cadastro.");

            var documento = DocumentoFiscal(proprietario.Document, $"proprietário do veículo {veiculo.LicensePlate}");

            var rntrc = SomenteDigitos(proprietario.Rntrc);
            if (string.IsNullOrEmpty(rntrc))
                throw new InvalidOperationException($"O proprietário do veículo {veiculo.LicensePlate} não tem RNTRC cadastrado, e o grupo 'prop' o exige.");

            var prop = new MDFeProp
            {
                CPF = documento.Cpf,
                CNPJ = documento.Cnpj,
                RNTRC = rntrc.PadLeft(8, '0'),
                XNome = ExigirTexto(proprietario.Name, $"nome do proprietário do veículo {veiculo.LicensePlate}", 2, 60),
                // tpProp: 0 = TAC agregado, 1 = TAC independente, 2 = outros. O
                // cadastro usa ThirdPartyCategory com os mesmos valores; veículo
                // arrendado/locado/comodato não é TAC e cai em "outros".
                MDFeTpProp = veiculo.ThirdPartyCategory.HasValue
                    ? MapCategoriaTerceiro(veiculo.ThirdPartyCategory.Value)
                    : MDFeTpProp.Outros
            };

            // O par IE+UF: ou os dois, ou nenhum (ver o comentário do método).
            // "ISENTO" é valor válido de TIeDest, então o produtor sem inscrição
            // estadual pode ser declarado — mas quem decide isso é o cadastro, não
            // uma invenção nossa.
            var ie = SomenteDigitos(proprietario.Ie);
            if (!string.IsNullOrEmpty(ie))
            {
                prop.IE = ie;
                prop.UF = ExigirUf(proprietario.Uf, $"UF do proprietário do veículo {veiculo.LicensePlate}", "cadastro do parceiro");
            }

            return prop;
        }

        /// <summary>
        /// Condutores do manifesto. Vêm do manifesto, não do veículo: a
        /// composição motorista × caminhão muda a cada viagem (RV13), e o
        /// manifesto é o registro dessa composição.
        ///
        /// Nome e CPF saem do SNAPSHOT gravado em <c>MdfeCondutor</c>, e não do
        /// cadastro do parceiro: o que vale é o que foi declarado à fiscalização
        /// no momento da emissão.
        /// </summary>
        private static List<MDFeCondutor> MontarCondutores(MdfeEmissao mdfe)
        {
            return (mdfe.Condutores ?? new List<MdfeCondutor>())
                .Where(c => c != null)
                .Select(c => new MDFeCondutor
                {
                    XNome = ExigirTexto(c.Nome, "nome do condutor", 2, 60),
                    CPF = ExigirCpf(c.Cpf, "CPF do condutor")
                })
                .ToList();
        }

        // =====================================================================
        // infDoc — o agrupamento por município de descarga (RM03)
        // =====================================================================

        /// <summary>
        /// Documentos manifestados, AGRUPADOS POR MUNICÍPIO DE DESCARGA.
        ///
        /// Este é o ponto onde é fácil errar o leiaute: <c>infMunDescarga</c> é um
        /// grupo por MUNICÍPIO, não por UF. Duas notas do mesmo estado em cidades
        /// diferentes vão para grupos distintos, com os dados do município
        /// repetidos em cada grupo. Agrupar por UF geraria um XML que a SEFAZ
        /// recusa.
        ///
        /// A ordenação das chaves dentro do grupo é estável (ordinal) para o XML
        /// ser reproduzível: sem isso, dois builds do mesmo manifesto poderiam
        /// sair com os documentos em ordem diferente e um diff entre eles
        /// pareceria uma mudança que não houve.
        ///
        /// O <c>xMunDescarga</c> vem do próprio documento — é o snapshot do
        /// município do parceiro no momento da emissão.
        ///
        /// Como o agrupamento é por CÓDIGO e o nome é um campo à parte, dois
        /// documentos com o mesmo código e nomes divergentes cairiam no mesmo
        /// grupo e o XML sairia com um nome só — declarando à fiscalização uma
        /// cidade para uma nota que diz outra. Isso não é resolvido escolhendo um
        /// dos nomes em silêncio: é recusado.
        /// </summary>
        private static MDFeInfDoc MontarInfDoc(MdfeEmissao mdfe)
        {
            var documentos = (mdfe.Documentos ?? new List<MdfeDocumento>())
                .Where(d => d != null)
                .ToList();

            if (documentos.Count == 0)
                throw new InvalidOperationException("MDF-e sem nenhuma NF-e vinculada — o leiaute exige ao menos um grupo infMunDescarga com um documento.");

            var grupos = documentos
                .GroupBy(d => ExigirCodigoIbge(d.CodMunDescarga, $"município de descarga da NF-e {d.ChaveNFe}"))
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(MontarGrupoMunicipio)
                .ToList();

            return new MDFeInfDoc { InfMunDescarga = grupos };
        }

        private static MDFeInfMunDescarga MontarGrupoMunicipio(IGrouping<string, MdfeDocumento> grupo)
        {
            var nomes = grupo
                .Select(d => (d.MunicipioDescarga ?? string.Empty).Trim())
                .Where(n => n.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (nomes.Count == 0)
                throw new InvalidOperationException($"Nenhuma das NF-e do município {grupo.Key} tem o nome do município de descarga preenchido.");

            if (nomes.Count > 1)
                throw new InvalidOperationException($"As NF-e do município {grupo.Key} apontam nomes de município divergentes ({string.Join(", ", nomes)}). Corrija o município de descarga das notas para que o grupo {grupo.Key} tenha um nome só.");

            return new MDFeInfMunDescarga
            {
                CMunDescarga = grupo.Key,
                XMunDescarga = ExigirTexto(nomes[0], $"nome do município de descarga {grupo.Key}", 2, 60),
                InfNFe = grupo
                    .OrderBy(d => d.ChaveNFe, StringComparer.Ordinal)
                    .Select(d => new MDFeInfNFe
                    {
                        ChNFe = ExigirChaveNFe(d.ChaveNFe)
                    })
                    .ToList()
            };
        }

        // =====================================================================
        // prodPred / tot / infAdic
        // =====================================================================

        /// <summary>
        /// Produto predominante (<c>prodPred</c>). Opcional no XSD
        /// (<c>minOccurs="0"</c>), mas o MOC o trata como obrigatório para o
        /// manifesto — e um manifesto sem ele é aceito no schema e questionado na
        /// fiscalização. Como a tela tem a aba Carga/Produtos, os dois campos vêm
        /// preenchidos de lá.
        /// </summary>
        private static MDFeProdPred MontarProdutoPredominante(MdfeEmissao mdfe)
        {
            return new MDFeProdPred
            {
                TpCarga = MapTipoCarga(mdfe.TipoCarga),
                XProd = ExigirTexto(mdfe.ProdutoPredominante, "produto predominante", 1, 120)
            };
        }

        /// <summary>
        /// Totais. <c>vCarga</c> é a soma dos valores das NF-e e <c>qCarga</c> a
        /// soma dos pesos — os dois já calculados e gravados pelo serviço a partir
        /// dos documentos, para que o total do XML seja o mesmo que a tela mostrou.
        ///
        /// <c>qNFe</c> é a contagem de documentos. Os totais de CT-e e de MDF-e
        /// transportado não entram: não há CT-e nem MDF-e de terceiro neste
        /// fluxo, e declarar zero seria afirmar algo que não conferimos.
        /// </summary>
        private static MDFeTot MontarTotais(MdfeEmissao mdfe)
        {
            return new MDFeTot
            {
                QNFe = mdfe.QuantidadeNFe,
                vCarga = mdfe.ValorTotal,
                CUnid = MDFeCUnid.KG,
                QCarga = mdfe.PesoBruto
            };
        }

        /// <summary>
        /// Informações adicionais. O grupo é omitido quando as duas strings estão
        /// vazias — <c>infAdic</c> sem filhos seria elemento inválido no leiaute.
        /// </summary>
        private static MDFeInfAdic? MontarInfAdic(MdfeEmissao mdfe)
        {
            var temFisco = !string.IsNullOrWhiteSpace(mdfe.InfoAdFisco);
            var temContribuinte = !string.IsNullOrWhiteSpace(mdfe.InfoComplementar);

            if (!temFisco && !temContribuinte)
                return null;

            return new MDFeInfAdic
            {
                InfAdFisco = temFisco ? ExigirOpcional(mdfe.InfoAdFisco, "informações adicionais de interesse do fisco", 2000) : null,
                InfCpl = temContribuinte ? ExigirOpcional(mdfe.InfoComplementar, "informações complementares", 5000) : null
            };
        }

        // =====================================================================
        // Escolha da tração
        // =====================================================================

        /// <summary>
        /// Qual veículo da lista é a tração.
        ///
        /// Prefere o que o manifesto aponta em <c>IdVeiculoTracao</c>. O fallback
        /// existe porque um rascunho pode ser montado antes de a tela gravar esse
        /// campo, e aí a escolha é pelo que o CADASTRO diz que é tração. Sem
        /// nenhum dos dois, falha: adivinhar qual veículo puxa o comboio seria
        /// declarar à fiscalização algo que não sabemos.
        /// </summary>
        private static Vehicle EscolherTracao(MdfeEmissao mdfe, List<Vehicle> veiculos)
        {
            if (mdfe.IdVeiculoTracao.HasValue)
            {
                var apontada = veiculos.FirstOrDefault(v => v.Id == mdfe.IdVeiculoTracao.Value);
                if (apontada != null)
                    return apontada;
            }

            var porCadastro = veiculos.FirstOrDefault(v => v.VehicleType == VehicleType.Traction);
            if (porCadastro != null)
                return porCadastro;

            throw new InvalidOperationException("Não foi possível determinar o veículo de tração: o manifesto não aponta um, e nenhum dos veículos escalados está cadastrado como tração.");
        }

        // =====================================================================
        // Mapeamentos Model → flags do leiaute
        // =====================================================================
        //
        // Os enums do Model foram declarados com os MESMOS valores numéricos dos
        // flags do MDFe.Classes, então um cast funcionaria — mas não é usado de
        // propósito: um cast silencioso continuaria compilando se um dos lados
        // mudasse de valor, e o resultado seria um XML com o código errado. Com
        // switch explícito, um valor novo cai no caso default e FALHA.

        private static MDFeTipoEmitente MapTipoEmitente(MdfeTipoEmitente tipo)
        {
            switch (tipo)
            {
                case MdfeTipoEmitente.Pst: return MDFeTipoEmitente.PrestadorServicoDeTransporte;
                case MdfeTipoEmitente.CargaPropria: return MDFeTipoEmitente.TransportadorCargaPropria;
                default: throw new InvalidOperationException($"Tipo de emitente não suportado: {tipo}.");
            }
        }

        private static MDFeModal MapModal(MdfeModal modal)
        {
            switch (modal)
            {
                case MdfeModal.Rodoviario: return MDFeModal.Rodoviario;
                // Os demais modais existem no leiaute mas o builder não os monta
                // (só o rodo tem o grupo infModal implementado). Falhar aqui é
                // melhor do que emitir um manifesto rodoviário para uma operação
                // aérea.
                default: throw new InvalidOperationException($"O modal {modal} ainda não é suportado na emissão de MDF-e.");
            }
        }

        private static MDFeTpRod MapTipoRodado(WheelType rodado)
        {
            switch (rodado)
            {
                case WheelType.Truck: return MDFeTpRod.Truck;
                case WheelType.Toco: return MDFeTpRod.Toco;
                case WheelType.CavaloMecanico: return MDFeTpRod.CavaloMecanico;
                case WheelType.Van: return MDFeTpRod.VAN;
                case WheelType.Utilitario: return MDFeTpRod.Utilitario;
                case WheelType.Outros: return MDFeTpRod.Outros;
                default: throw new InvalidOperationException($"Tipo de rodado não mapeado para o leiaute: {rodado}.");
            }
        }

        private static MDFeTpCar MapTipoCarroceria(BodyType carroceria)
        {
            switch (carroceria)
            {
                case BodyType.NaoAplicavel: return MDFeTpCar.NaoAplicavel;
                case BodyType.Aberta: return MDFeTpCar.Aberta;
                case BodyType.FechadaBau: return MDFeTpCar.FechadaBau;
                case BodyType.Granelera: return MDFeTpCar.Granelera;
                case BodyType.PortaContainer: return MDFeTpCar.PortaContainer;
                case BodyType.Sider: return MDFeTpCar.Sider;
                default: throw new InvalidOperationException($"Tipo de carroceria não mapeado para o leiaute: {carroceria}.");
            }
        }

        private static MDFeTpProp MapCategoriaTerceiro(ThirdPartyCategory categoria)
        {
            switch (categoria)
            {
                case ThirdPartyCategory.TacAgregado: return MDFeTpProp.TacAgregado;
                case ThirdPartyCategory.TacIndependente: return MDFeTpProp.TacIndependente;
                case ThirdPartyCategory.Outros: return MDFeTpProp.Outros;
                default: throw new InvalidOperationException($"Categoria de terceiro não mapeada para o leiaute: {categoria}.");
            }
        }

        private static MDFeTpCarga MapTipoCarga(MdfeTipoCarga tipo)
        {
            switch (tipo)
            {
                case MdfeTipoCarga.GranelSolido: return MDFeTpCarga.GranelSolido;
                case MdfeTipoCarga.GranelLiquido: return MDFeTpCarga.GranelLiquido;
                case MdfeTipoCarga.Frigorificada: return MDFeTpCarga.Frigorificada;
                case MdfeTipoCarga.Conteinerizada: return MDFeTpCarga.Conteinerizada;
                case MdfeTipoCarga.CargaGeral: return MDFeTpCarga.CargaGeral;
                case MdfeTipoCarga.Neogranel: return MDFeTpCarga.Neogranel;
                case MdfeTipoCarga.PerigosaGranelSolido: return MDFeTpCarga.PerigosaGranelSolido;
                case MdfeTipoCarga.PerigosaGranelLiquido: return MDFeTpCarga.PerigosaGranelLiquido;
                case MdfeTipoCarga.PerigosaCargaFrigorificada: return MDFeTpCarga.PerigosaCargaFrigorificada;
                case MdfeTipoCarga.PerigosaCargaConteinerizada: return MDFeTpCarga.PerigosaCargaConteinerizada;
                case MdfeTipoCarga.PerigosaCargaGeral: return MDFeTpCarga.PerigosaCargaGeral;
                // ATENÇÃO: "12 - Granel pressurizada" existe no enum do MOC e na
                // biblioteca, mas NÃO está na enumeração de tpCarga do
                // mdfeTiposBasico_v3.00.xsd (que para no 11). Emitir 12 reprovaria
                // a validação local com um erro de schema genérico; recusar aqui
                // diz exatamente qual é o problema.
                case MdfeTipoCarga.GranelPressurizada:
                    throw new InvalidOperationException("O tipo de carga 'Granel pressurizada' (12) não consta na enumeração de tpCarga do leiaute mdfe_v3.00 e não pode ser emitido nesta versão do schema.");
                default: throw new InvalidOperationException($"Tipo de carga não mapeado para o leiaute: {tipo}.");
            }
        }

        // =====================================================================
        // Guardas de formato — cada uma lança com o nome do campo e de onde ele vem
        // =====================================================================

        /// <summary>
        /// UF válida no domínio do leiaute (<c>DFe.Classes.Entidades.Estado</c>).
        ///
        /// A checagem é necessária porque <c>SiglaParaEstado</c> devolve
        /// <c>default(Estado)</c> = 0 para uma sigla desconhecida, EM SILÊNCIO — e
        /// 0 não é UF válida, então o XML seria recusado sem apontar a causa.
        /// </summary>
        private static Estado ExigirUf(string? sigla, string campo, string origem)
        {
            var normalizada = (sigla ?? string.Empty).Trim().ToUpperInvariant();
            var estado = default(Estado).SiglaParaEstado(normalizada);

            if (estado == 0)
                throw new InvalidOperationException($"UF inválida em {campo}: '{sigla}'. Verifique o valor no {origem}.");

            return estado;
        }

        private static string ExigirTexto(string? valor, string campo, int minimo, int maximo)
        {
            var texto = (valor ?? string.Empty).Trim();

            if (texto.Length < minimo)
                throw new InvalidOperationException($"O campo '{campo}' é obrigatório e não foi preenchido.");

            if (texto.Length > maximo)
                throw new InvalidOperationException($"O campo '{campo}' tem {texto.Length} caracteres e o leiaute aceita no máximo {maximo}.");

            ValidarCaracteres(texto, campo);

            return texto;
        }

        /// <summary>
        /// Texto OPCIONAL, validado quando presente. Para os campos que podem
        /// ficar de fora do XML (fantasia, informações adicionais) e que por isso
        /// não passam pelo <see cref="ExigirTexto"/>.
        /// </summary>
        private static string? ExigirOpcional(string? valor, string campo, int maximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            return ExigirTexto(valor, campo, 1, maximo);
        }

        /// <summary>
        /// Domínio de caracteres do <c>TString</c> do leiaute:
        /// <c>[!-ÿ]{1}[ -ÿ]*[!-ÿ]{1}|[!-ÿ]{1}</c>.
        ///
        /// <b>O padrão tem DUAS faixas, e confundi-las é o erro fácil.</b> As
        /// pontas (primeira e última posição) exigem <c>[!-ÿ]</c> — U+0021 a
        /// U+00FF. O miolo aceita <c>[ -ÿ]</c> — U+0020 a U+00FF, ou seja, o
        /// ESPAÇO também. Ler o padrão como "U+0021 a U+00FF em todo caractere"
        /// reprova qualquer texto com espaço, isto é, reprova praticamente tudo:
        /// foi o que a primeira versão deste método fez, e o harness offline
        /// pegou reprovando "PARAMADEIRAS INDUSTRIA E COMERCIO LTDA".
        ///
        /// Na prática, o que o usuário digita e é RECUSADO são os tipográficos:
        /// travessão (—), meia-risca (–), aspas curvas (“ ” ‘ ’), reticências (…),
        /// emoji e qualquer coisa fora do Latin-1 — todos acima de U+00FF.
        /// Acentuação do português (á, ç, ã) está dentro da faixa e passa.
        ///
        /// <b>Por que recusar em vez de trocar o caractere em silêncio:</b> o
        /// texto aqui é declaração à fiscalização — na razão social, no nome do
        /// proprietário do veículo, nas informações complementares. Substituir um
        /// travessão por hífen seria reescrever o que o usuário declarou sem que
        /// ele saiba, e é exatamente a classe de decisão que este builder recusa a
        /// tomar sozinho. Recusando com o campo e os caracteres na mensagem, o
        /// usuário corrige em uma passada.
        ///
        /// Sem esta checagem, o manifesto só falharia na validação de schema — com
        /// uma mensagem em inglês sobre "Pattern constraint" e sem dizer QUAL
        /// campo nem QUAL caractere. Descoberto pelo harness offline, com um
        /// travessão em <c>infCpl</c>; a mensagem do XSD não tinha como ser
        /// melhor.
        /// </summary>
        private static void ValidarCaracteres(string texto, string campo)
        {
            List<string> invalidos = null;

            for (var i = 0; i < texto.Length; i++)
            {
                var caractere = texto[i];

                // Nas pontas o mínimo é U+0021; no miolo, U+0020 (espaço).
                var minimo = i == 0 || i == texto.Length - 1 ? '!' : ' ';

                if (caractere >= minimo && caractere <= 'ÿ')
                    continue;

                var descricao = char.IsControl(caractere)
                    ? $"U+{(int)caractere:X4} (caractere de controle)"
                    : $"'{caractere}' (U+{(int)caractere:X4})";

                invalidos ??= new List<string>();
                if (!invalidos.Contains(descricao))
                    invalidos.Add(descricao);
            }

            if (invalidos == null)
                return;

            throw new InvalidOperationException(
                $"O campo '{campo}' tem caractere que o leiaute não aceita: {string.Join(", ", invalidos)}. " +
                "O MDF-e admite de U+0020 (espaço) a U+00FF, e as pontas do texto não podem ser espaço — " +
                "o que exclui travessão, aspas curvas, reticências e emoji. Substitua-os pelos equivalentes simples.");
        }

        private static string ExigirCodigoIbge(string? codigo, string campo)
        {
            var digitos = SomenteDigitos(codigo);

            if (digitos.Length != 7)
                throw new InvalidOperationException($"O código IBGE de {campo} deve ter 7 dígitos (recebido: '{codigo}').");

            return digitos;
        }

        private static string ExigirChaveNFe(string? chave)
        {
            var digitos = SomenteDigitos(chave);

            if (digitos.Length != 44)
                throw new InvalidOperationException($"A chave da NF-e {chave} deve ter 44 dígitos (tem {digitos.Length}).");

            return digitos;
        }

        private static string ExigirCpf(string? cpf, string campo)
        {
            var digitos = SomenteDigitos(cpf);

            if (digitos == null || digitos.Length != 11)
                throw new InvalidOperationException($"O {campo} deve ter 11 dígitos (recebido: '{cpf}').");

            return digitos;
        }

        /// <summary>
        /// Decide se o documento é CPF ou CNPJ e devolve o valor no campo certo do
        /// leiaute.
        ///
        /// A decisão é pelo COMPRIMENTO (11 = CPF, 14 = CNPJ), e não por
        /// <c>Client.TipoPessoa</c>: no schema os tipos <c>TCpf</c> e <c>TCnpj</c>
        /// são definidos exatamente pelo número de dígitos, então é o comprimento
        /// que determina se o XML passa. <c>TipoPessoa</c> é rótulo de tela, e o
        /// <c>NFeService</c> já o trata como dica (cai para CPF quando não é "J");
        /// confiar nele aqui poderia pôr o partido errado no manifesto.
        /// </summary>
        private static (string? Cpf, string? Cnpj) DocumentoFiscal(string? documento, string campo)
        {
            var digitos = SomenteDigitos(documento);

            switch (digitos?.Length)
            {
                case 11:
                    return (digitos, null);
                case 14:
                    return (null, digitos);
                case null:
                    throw new InvalidOperationException($"O {campo} não tem CPF/CNPJ cadastrado.");
                default:
                    throw new InvalidOperationException($"O CPF/CNPJ do {campo} tem {digitos.Length} dígitos, e o leiaute aceita 11 (CPF) ou 14 (CNPJ).");
            }
        }

        /// <summary>
        /// Placa no padrão do leiaute: 7 caracteres, sem separador, maiúscula.
        /// O <c>TPlaca</c> do XSD aceita tanto o formato antigo (ABC1234) quanto o
        /// Mercosul (ABC1D23), então a checagem aqui é de forma, não de padrão.
        /// </summary>
        private static string ExigirPlaca(string? placa)
        {
            var normalizada = new string((placa ?? string.Empty).ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());

            if (normalizada.Length != 7)
                throw new InvalidOperationException($"Placa inválida: '{placa}'. O leiaute exige 7 caracteres (ABC1234 ou ABC1D23).");

            return normalizada;
        }

        private static short ExigirSerie(string? serie)
        {
            var texto = (serie ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(texto))
                throw new InvalidOperationException("Série do manifesto não informada — configure a série do MDF-e na configuração fiscal.");

            if (!short.TryParse(texto, out var numero))
                throw new InvalidOperationException($"A série do MDF-e deve ser numérica (recebido: '{serie}').");

            if (numero < 0 || numero > 999)
                throw new InvalidOperationException($"A série do MDF-e deve estar entre 0 e 999 (recebido: '{serie}').");

            return numero;
        }

        /// <summary>
        /// Capacidade em m³ como inteiro. O XSD aceita 0..999 inteiro e o cadastro
        /// guarda decimal com 2 casas — converter truncando mudaria o valor
        /// declarado. Quando há parte fracionária, o campo é OMITIDO (é opcional
        /// no leiaute) em vez de arredondado em silêncio.
        /// </summary>
        private static int? CapacidadeM3Inteira(decimal? capacidade)
        {
            if (!capacidade.HasValue)
                return null;

            var valor = capacidade.Value;
            if (valor != Math.Truncate(valor) || valor < 0 || valor > 999)
                return null;

            return (int)valor;
        }

        private static string? SomenteDigitos(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var digitos = new string(valor.Where(char.IsDigit).ToArray());
            return digitos.Length == 0 ? null : digitos;
        }

        private static string? Truncar(string? valor, int maximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var texto = valor.Trim();
            return texto.Length <= maximo ? texto : texto.Substring(0, maximo);
        }
    }
}
