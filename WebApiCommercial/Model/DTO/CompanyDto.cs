using System;
using System.Text.RegularExpressions;

namespace Model.DTO
{


    public class CompanyDto
    {
        public string ?CorporateName { get; set; }
        public Guid Guid { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Cnpj { get; set; }
        public string? ZipCode { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? CommercialPhone { get; set; }
        public string? City { get; set; }
        public string? Cellphone { get; set; }
        public string? Ie { get; set; }
        // Construtor padrão
        public CompanyDto() { }

        // Construtor com parâmetros
        public CompanyDto(string corporateName, Guid guid, int id, string name, string cnpj,
                      string zipCode, string address, string state, string commercialPhone,
                      string city, string cellphone)
        {
            CorporateName = corporateName;
            Guid = guid;
            Id = id;
            Name = name;
            Cnpj = RemoveSpecialCharacters(cnpj);
            ZipCode = RemoveSpecialCharacters(zipCode);
            Address = address;
            State = state;
            CommercialPhone = RemoveSpecialCharacters(commercialPhone);
            City = city;
            Cellphone = RemoveSpecialCharacters(cellphone);
        }

        // Função para remover caracteres especiais
        public static string RemoveSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove tudo que não for número, letra ou espaço
            return Regex.Replace(input, @"[^0-9a-zA-Z\s]", "");
        }

        // Função específica para CNPJ (mantém apenas números)
        public static string CleanCnpj(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj))
                return cnpj;

            return Regex.Replace(cnpj, @"[^0-9]", "");
        }

        // Função específica para CEP (mantém apenas números)
        public static string CleanZipCode(string zipCode)
        {
            if (string.IsNullOrEmpty(zipCode))
                return zipCode;

            return Regex.Replace(zipCode, @"[^0-9]", "");
        }

        // Função específica para telefones (mantém apenas números)
        public static string CleanPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            return Regex.Replace(phone, @"[^0-9]", "");
        }

        // Método para limpar todos os campos numéricos de uma vez
        public void CleanAllNumericFields()
        {
            Cnpj = CleanCnpj(Cnpj);
            ZipCode = CleanZipCode(ZipCode);
            CommercialPhone = CleanPhone(CommercialPhone);
            Cellphone = CleanPhone(Cellphone);
        }

        // Método para formatar CNPJ (XX.XXX.XXX/XXXX-XX)
        public static string FormatCnpj(string cnpj)
        {
            string cleanCnpj = CleanCnpj(cnpj);

            if (cleanCnpj.Length != 14)
                return cnpj; // Retorna original se não tiver 14 dígitos

            return $"{cleanCnpj.Substring(0, 2)}.{cleanCnpj.Substring(2, 3)}.{cleanCnpj.Substring(5, 3)}/{cleanCnpj.Substring(8, 4)}-{cleanCnpj.Substring(12)}";
        }

        // Método para formatar CEP (XXXXX-XXX)
        public static string FormatZipCode(string zipCode)
        {
            string cleanZipCode = CleanZipCode(zipCode);

            if (cleanZipCode.Length != 8)
                return zipCode; // Retorna original se não tiver 8 dígitos

            return $"{cleanZipCode.Substring(0, 5)}-{cleanZipCode.Substring(5)}";
        }

        // Método para formatar telefone ((XX) XXXXX-XXXX)
        public static string FormatPhone(string phone)
        {
            string cleanPhone = CleanPhone(phone);

            if (cleanPhone.Length == 10) // Telefone fixo
            {
                return $"({cleanPhone.Substring(0, 2)}) {cleanPhone.Substring(2, 4)}-{cleanPhone.Substring(6)}";
            }
            else if (cleanPhone.Length == 11) // Celular
            {
                return $"({cleanPhone.Substring(0, 2)}) {cleanPhone.Substring(2, 5)}-{cleanPhone.Substring(7)}";
            }
            else
            {
                return phone; // Retorna original se não tiver formato conhecido
            }
        }
    }

}
