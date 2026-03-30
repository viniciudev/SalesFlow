//// Model/Validations/ClientValidator.cs
//using FluentValidation;
//using System.Text.RegularExpressions;

//namespace Model.Validations
//{
//    public class ClientValidator : AbstractValidator<Client>
//    {
//        public ClientValidator()
//        {
//            RuleFor(x => x.TipoPessoa)
//                .NotNull().WithMessage("Tipo de pessoa é obrigatório")
//                .Must(x => x == "F" || x == "J").WithMessage("Tipo de pessoa deve ser 'F' ou 'J'");

//            RuleFor(x => x.NomeRazao)
//                .NotEmpty().WithMessage("Nome/Razão Social é obrigatório")
//                .MaximumLength(60).WithMessage("Nome/Razão Social deve ter no máximo 60 caracteres");

//            RuleFor(x => x.CpfCnpj)
//                .NotEmpty().WithMessage("CPF/CNPJ é obrigatório")
//                .Must(BeValidCpfOrCnpj).WithMessage("CPF/CNPJ inválido");

//            RuleFor(x => x.IndicadorIE)
//                .NotNull().WithMessage("Indicador IE é obrigatório")
//                .Must(x => x == "1" || x == "2" || x == "9").WithMessage("Indicador IE inválido");

//            RuleFor(x => x.ConsumidorFinal)
//                .NotNull().WithMessage("Consumidor final é obrigatório")
//                .Equal("1").WithMessage("Consumidor final deve ser '1'");

//            RuleFor(x => x.Logradouro)
//                .NotEmpty().WithMessage("Logradouro é obrigatório")
//                .MaximumLength(60).WithMessage("Logradouro deve ter no máximo 60 caracteres");

//            RuleFor(x => x.Numero)
//                .NotEmpty().WithMessage("Número é obrigatório")
//                .MaximumLength(60).WithMessage("Número deve ter no máximo 60 caracteres");

//            RuleFor(x => x.Bairro)
//                .NotEmpty().WithMessage("Bairro é obrigatório")
//                .MaximumLength(60).WithMessage("Bairro deve ter no máximo 60 caracteres");

//            RuleFor(x => x.Municipio)
//                .NotEmpty().WithMessage("Município é obrigatório")
//                .MaximumLength(60).WithMessage("Município deve ter no máximo 60 caracteres");

//            RuleFor(x => x.CodMunicipioIbge)
//                .NotEmpty().WithMessage("Código IBGE é obrigatório")
//                .Length(7).WithMessage("Código IBGE deve ter 7 dígitos")
//                .Matches(@"^\d+$").WithMessage("Código IBGE deve conter apenas números");

//            RuleFor(x => x.Uf)
//                .NotEmpty().WithMessage("UF é obrigatória")
//                .Length(2).WithMessage("UF deve ter 2 caracteres");

//            RuleFor(x => x.Cep)
//                .NotEmpty().WithMessage("CEP é obrigatório")
//                .Length(8).WithMessage("CEP deve ter 8 dígitos")
//                .Matches(@"^\d+$").WithMessage("CEP deve conter apenas números");

//            RuleFor(x => x.Ie)
//                .NotEmpty().When(x => x.IndicadorIE == "1")
//                .WithMessage("Inscrição Estadual é obrigatória para contribuintes");

//            RuleFor(x => x.Email)
//                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
//                .WithMessage("Email inválido");

//            RuleFor(x => x.Telefone)
//                .Matches(@"^\d*$").When(x => !string.IsNullOrEmpty(x.Telefone))
//                .WithMessage("Telefone deve conter apenas números");
//        }

//        private bool BeValidCpfOrCnpj(string document)
//        {
//            if (string.IsNullOrEmpty(document)) return false;

//            // Remove non-numeric characters
//            document = Regex.Replace(document, @"[^\d]", "");

//            return document.Length switch
//            {
//                11 => IsValidCpf(document),
//                14 => IsValidCnpj(document),
//                _ => false
//            };
//        }

//        private bool IsValidCpf(string cpf)
//        {
//            if (cpf.Length != 11 || new string(cpf[0], 11) == cpf)
//                return false;

//            int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
//            int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

//            string tempCpf = cpf.Substring(0, 9);
//            int sum = 0;

//            for (int i = 0; i < 9; i++)
//                sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

//            int remainder = sum % 11;
//            int firstDigit = remainder < 2 ? 0 : 11 - remainder;

//            tempCpf += firstDigit;
//            sum = 0;

//            for (int i = 0; i < 10; i++)
//                sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

//            remainder = sum % 11;
//            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

//            return cpf.EndsWith(firstDigit.ToString() + secondDigit.ToString());
//        }

//        private bool IsValidCnpj(string cnpj)
//        {
//            if (cnpj.Length != 14 || new string(cnpj[0], 14) == cnpj)
//                return false;

//            int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
//            int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

//            string tempCnpj = cnpj.Substring(0, 12);
//            int sum = 0;

//            for (int i = 0; i < 12; i++)
//                sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];

//            int remainder = sum % 11;
//            int firstDigit = remainder < 2 ? 0 : 11 - remainder;

//            tempCnpj += firstDigit;
//            sum = 0;

//            for (int i = 0; i < 13; i++)
//                sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];

//            remainder = sum % 11;
//            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

//            return cnpj.EndsWith(firstDigit.ToString() + secondDigit.ToString());
//        }
//    }
//}