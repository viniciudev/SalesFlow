using DFe.Classes.Flags;
using Model.Moves;
using Model.Registrations;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.Tipos;
using NFe.Classes.Informacoes.Emitente;
using System;

namespace Service
{
	public class CalculadorImpostos
	{
		private readonly ConfiguracaoTributaria _config;
		private readonly SaleItems _item;
		private readonly CRT _crt;
		private readonly decimal _valorTotalItem;

		public CalculadorImpostos(ConfiguracaoTributaria config, SaleItems item, CRT crt)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_item = item ?? throw new ArgumentNullException(nameof(item));
			_crt = crt;
			_valorTotalItem = item.Value * item.Amount;
		}

		public (decimal BaseCalculo, decimal ValorICMS, decimal Aliquota) CalcularICMSNormal()
		{
			if (!_config.AplicarICMS || _config.AliquotaICMS <= 0)
				return (0, 0, 0);
			decimal baseCalculo = _config.ReduzirBaseICMS
					? _valorTotalItem * (1 - (_config.AliquotaICMS / 100))
					: _valorTotalItem;
			decimal valorICMS = Math.Round(baseCalculo * _config.AliquotaICMS / 100, 2);
			return (Math.Round(baseCalculo, 2), valorICMS, _config.AliquotaICMS);
		}

		public Csticms ObterCSTICMS()
		{
			return ObterCSTICMS(_config.CstICMS);
		}

		public (decimal BaseST, decimal ValorICMSST) CalcularICMSST(decimal mva = 0, decimal aliquotaST = 0)
		{
			if (!_config.AplicarICMS || aliquotaST <= 0)
				return (0, 0);
			decimal baseST = mva > 0
					? _valorTotalItem * (1 + mva / 100)
					: _valorTotalItem;
			baseST = Math.Round(baseST, 2);
			decimal icmsProprio = CalcularICMSNormal().ValorICMS;
			decimal valorICMSST = Math.Round((baseST * aliquotaST / 100) - icmsProprio, 2);
			if (valorICMSST < 0) valorICMSST = 0;
			return (baseST, valorICMSST);
		}

		public PIS CalcularPIS()
		{
			var cst = ObterCSTPIS(_config.CstPIS);
			if (!_config.AplicarPIS || _config.AliquotaPIS <= 0)
			{
				return new PIS
				{
					TipoPIS = new PISOutr { CST = CSTPIS.pis99, vBC = _valorTotalItem, pPIS = 0, vPIS = 0 }
				};
			}
			decimal valorPIS = Math.Round(_valorTotalItem * _config.AliquotaPIS / 100, 2);
			return new PIS
			{
				TipoPIS = new PISOutr { CST = cst, vBC = _valorTotalItem, pPIS = _config.AliquotaPIS, vPIS = valorPIS }
			};
		}

		public COFINS CalcularCOFINS()
		{
			var cst = ObterCSTCOFINS(_config.CstCOFINS);
			if (!_config.AplicarCOFINS || _config.AliquotaCOFINS <= 0)
			{
				return new COFINS
				{
					TipoCOFINS = new COFINSOutr { CST = CSTCOFINS.cofins99, vBC = _valorTotalItem, pCOFINS = 0, vCOFINS = 0 }
				};
			}
			decimal valorCOFINS = Math.Round(_valorTotalItem * _config.AliquotaCOFINS / 100, 2);
			return new COFINS
			{
				TipoCOFINS = new COFINSOutr { CST = cst, vBC = _valorTotalItem, pCOFINS = _config.AliquotaCOFINS, vCOFINS = valorCOFINS }
			};
		}

		public IPI? CalcularIPI(ModeloDocumento modelo)
		{
			if (modelo != ModeloDocumento.NFe) return null;
			if (!_config.AplicarIPI || _config.AliquotaIPI <= 0) return null;
			var cst = ObterCSTIPI(_config.CstIPI);
			decimal valorIPI = Math.Round(_valorTotalItem * _config.AliquotaIPI / 100, 2);
			return new IPI
			{
				cEnq = 999,
				TipoIPI = new IPITrib { CST = cst, vBC = _valorTotalItem, pIPI = _config.AliquotaIPI, vIPI = valorIPI }
			};
		}

		public decimal CalcularTotalTributos()
		{
			decimal total = 0;
			if (_config.AplicarCOFINS && _config.AliquotaCOFINS > 0)
				total += Math.Round(_valorTotalItem * _config.AliquotaCOFINS / 100, 2);
			if (_config.AplicarPIS && _config.AliquotaPIS > 0)
				total += Math.Round(_valorTotalItem * _config.AliquotaPIS / 100, 2);
			if (_config.AplicarIPI && _config.AliquotaIPI > 0)
				total += Math.Round(_valorTotalItem * _config.AliquotaIPI / 100, 2);
			if (!IsSimplesNacional() && _config.AplicarICMS && _config.AliquotaICMS > 0)
				total += CalcularICMSNormal().ValorICMS;
			if (_config.AplicarIBS && _config.AliquotaIBS > 0)
				total += Math.Round(_valorTotalItem * _config.AliquotaIBS / 100, 2);
			if (_config.AplicarCBS && _config.AliquotaCBS > 0)
				total += Math.Round(_valorTotalItem * _config.AliquotaCBS / 100, 2);
			if (_config.AplicarISSQN && _config.AliquotaISSQN > 0)
				total += Math.Round(_valorTotalItem * _config.AliquotaISSQN / 100, 2);
			return Math.Round(total, 2);
		}

		public decimal CalcularIBS()
		{
			if (!_config.AplicarIBS || _config.AliquotaIBS <= 0) return 0;
			return Math.Round(_valorTotalItem * _config.AliquotaIBS / 100, 2);
		}

		public decimal CalcularCBS()
		{
			if (!_config.AplicarCBS || _config.AliquotaCBS <= 0) return 0;
			return Math.Round(_valorTotalItem * _config.AliquotaCBS / 100, 2);
		}

		public decimal CalcularIS()
		{
			if (!_config.AplicarIS || _config.AliquotaIS <= 0) return 0;
			return Math.Round(_valorTotalItem * _config.AliquotaIS / 100, 2);
		}

		public static Csticms ObterCSTICMS(string? cst)
		{
			if (string.IsNullOrWhiteSpace(cst)) return Csticms.Cst00;
			if (int.TryParse(cst, out int valor))
			{
				return valor switch
				{
					00 => Csticms.Cst00,
					02 => Csticms.Cst02,
					10 => Csticms.Cst10,
					15 => Csticms.Cst15,
					20 => Csticms.Cst20,
					30 => Csticms.Cst30,
					40 => Csticms.Cst40,
					41 => Csticms.Cst41,
					50 => Csticms.Cst50,
					51 => Csticms.Cst51,
					53 => Csticms.Cst53,
					60 => Csticms.Cst60,
					61 => Csticms.Cst61,
					70 => Csticms.Cst70,
					90 => Csticms.Cst90,
					_ => Csticms.Cst00
				};
			}
			if (Enum.TryParse<Csticms>(cst, true, out var result)) return result;
			return Csticms.Cst00;
		}

		public static Csosnicms ObterCSOSN(string? csosn)
		{
			if (string.IsNullOrWhiteSpace(csosn)) return Csosnicms.Csosn102;
			if (int.TryParse(csosn, out int valor))
			{
				return valor switch
				{
					101 => Csosnicms.Csosn101,
					102 => Csosnicms.Csosn102,
					103 => Csosnicms.Csosn103,
					201 => Csosnicms.Csosn201,
					202 => Csosnicms.Csosn202,
					203 => Csosnicms.Csosn203,
					300 => Csosnicms.Csosn300,
					400 => Csosnicms.Csosn400,
					500 => Csosnicms.Csosn500,
					900 => Csosnicms.Csosn900,
					_ => Csosnicms.Csosn102
				};
			}
			if (Enum.TryParse<Csosnicms>(csosn, true, out var result)) return result;
			return Csosnicms.Csosn102;
		}

		public static CSTPIS ObterCSTPIS(string? cst)
		{
			if (string.IsNullOrWhiteSpace(cst)) return CSTPIS.pis99;
			if (int.TryParse(cst, out int valor))
			{
				return valor switch
				{
					01 => CSTPIS.pis01,
					02 => CSTPIS.pis02,
					03 => CSTPIS.pis03,
					04 => CSTPIS.pis04,
					05 => CSTPIS.pis05,
					06 => CSTPIS.pis06,
					07 => CSTPIS.pis07,
					08 => CSTPIS.pis08,
					09 => CSTPIS.pis09,
					49 => CSTPIS.pis49,
					50 => CSTPIS.pis50,
					51 => CSTPIS.pis51,
					52 => CSTPIS.pis52,
					53 => CSTPIS.pis53,
					54 => CSTPIS.pis54,
					55 => CSTPIS.pis55,
					56 => CSTPIS.pis56,
					60 => CSTPIS.pis60,
					61 => CSTPIS.pis61,
					62 => CSTPIS.pis62,
					63 => CSTPIS.pis63,
					64 => CSTPIS.pis64,
					65 => CSTPIS.pis65,
					66 => CSTPIS.pis66,
					67 => CSTPIS.pis67,
					70 => CSTPIS.pis70,
					71 => CSTPIS.pis71,
					72 => CSTPIS.pis72,
					73 => CSTPIS.pis73,
					74 => CSTPIS.pis74,
					75 => CSTPIS.pis75,
					98 => CSTPIS.pis98,
					99 => CSTPIS.pis99,
					_ => CSTPIS.pis99
				};
			}
			if (Enum.TryParse<CSTPIS>(cst, true, out var result)) return result;
			return CSTPIS.pis99;
		}

		public static CSTCOFINS ObterCSTCOFINS(string? cst)
		{
			if (string.IsNullOrWhiteSpace(cst)) return CSTCOFINS.cofins99;
			if (int.TryParse(cst, out int valor))
			{
				return valor switch
				{
					01 => CSTCOFINS.cofins01,
					02 => CSTCOFINS.cofins02,
					03 => CSTCOFINS.cofins03,
					04 => CSTCOFINS.cofins04,
					05 => CSTCOFINS.cofins05,
					06 => CSTCOFINS.cofins06,
					07 => CSTCOFINS.cofins07,
					08 => CSTCOFINS.cofins08,
					09 => CSTCOFINS.cofins09,
					49 => CSTCOFINS.cofins49,
					50 => CSTCOFINS.cofins50,
					51 => CSTCOFINS.cofins51,
					52 => CSTCOFINS.cofins52,
					53 => CSTCOFINS.cofins53,
					54 => CSTCOFINS.cofins54,
					55 => CSTCOFINS.cofins55,
					56 => CSTCOFINS.cofins56,
					60 => CSTCOFINS.cofins60,
					61 => CSTCOFINS.cofins61,
					62 => CSTCOFINS.cofins62,
					63 => CSTCOFINS.cofins63,
					64 => CSTCOFINS.cofins64,
					65 => CSTCOFINS.cofins65,
					66 => CSTCOFINS.cofins66,
					67 => CSTCOFINS.cofins67,
					70 => CSTCOFINS.cofins70,
					71 => CSTCOFINS.cofins71,
					72 => CSTCOFINS.cofins72,
					73 => CSTCOFINS.cofins73,
					74 => CSTCOFINS.cofins74,
					75 => CSTCOFINS.cofins75,
					98 => CSTCOFINS.cofins98,
					99 => CSTCOFINS.cofins99,
					_ => CSTCOFINS.cofins99
				};
			}
			if (Enum.TryParse<CSTCOFINS>(cst, true, out var result)) return result;
			return CSTCOFINS.cofins99;
		}

		public static CSTIPI ObterCSTIPI(string? cst)
		{
			if (string.IsNullOrWhiteSpace(cst)) return CSTIPI.ipi50;
			if (int.TryParse(cst, out int valor))
			{
				return valor switch
				{
					00 => CSTIPI.ipi00,
					01 => CSTIPI.ipi01,
					02 => CSTIPI.ipi02,
					03 => CSTIPI.ipi03,
					04 => CSTIPI.ipi04,
					05 => CSTIPI.ipi05,
					49 => CSTIPI.ipi49,
					50 => CSTIPI.ipi50,
					51 => CSTIPI.ipi51,
					52 => CSTIPI.ipi52,
					53 => CSTIPI.ipi53,
					54 => CSTIPI.ipi54,
					55 => CSTIPI.ipi55,
					99 => CSTIPI.ipi99,
					_ => CSTIPI.ipi50
				};
			}
			if (Enum.TryParse<CSTIPI>(cst, true, out var result)) return result;
			return CSTIPI.ipi50;
		}

		public bool IsSimplesNacional()
		{
			return _crt == CRT.SimplesNacional || _crt == CRT.SimplesNacionalMei;
		}

		public decimal ValorTotalItem => _valorTotalItem;
		public ConfiguracaoTributaria Configuracao => _config;
	}
}
