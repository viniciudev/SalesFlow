using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Model;
using System;
using System.Security.Cryptography;

namespace Repository
{
  public class Cryptography
  {
    public string Cripto(string password)
    {

      byte[] salt = new byte[128 / 8];
      using (var rng = RandomNumberGenerator.Create())
      {
        rng.GetBytes(salt);
      }

      // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
      string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
          password: password,
          salt: salt,
          prf: KeyDerivationPrf.HMACSHA1,
          iterationCount: 10000,
          numBytesRequested: 256 / 8));
      return hashed;

    }
    public string addsEncrypted(string password)
    {

      // Gera um sal aleatório
      String salt = BCrypt.Net.BCrypt.GenerateSalt();

      // Gera a senha hasheada utilizando o sal gerado
      String hashedpassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

      return hashedpassword;
    }
    public bool authentic(User model, string passwordTyped)
    {

      bool autentic = BCrypt.Net.BCrypt.Verify(passwordTyped, model.Password);

      return autentic;

    }
  }
}
