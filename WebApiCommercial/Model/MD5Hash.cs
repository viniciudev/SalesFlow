using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
  public class MD5Hash
  {
    public static string CalculateHash(string Senha)
    {
      try
      {
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Senha);
        byte[] hash = md5.ComputeHash(inputBytes);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
          sb.Append(hash[i].ToString("X2"));
        }
        return sb.ToString(); // Retorna senha criptografada 
      }
      catch (Exception)
      {
        return null; // Caso encontre erro retorna nulo
      }
    }

    public static bool CompareMD5(string passwordbase, string password_MD5)
    {
      using (MD5 md5Hash = MD5.Create())
      {
        var senha = ReturnMD5(passwordbase);
        if (VerifyHash(md5Hash, password_MD5, senha))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }
    public static string ReturnMD5(string password)
    {
      using (MD5 md5Hash = MD5.Create())
      {
        return ReturnHash(md5Hash, password);
      }
    }

    private static string ReturnHash(MD5 md5Hash, string input)
    {
      byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

      StringBuilder sBuilder = new StringBuilder();

      for (int i = 0; i < data.Length; i++)
      {
        sBuilder.Append(data[i].ToString("x2"));
      }

      return sBuilder.ToString();
    }

    private static bool VerifyHash(MD5 md5Hash, string input, string hash)
    {
      StringComparer compare = StringComparer.OrdinalIgnoreCase;

      if (0 == compare.Compare(input, hash))
      {
        return true;
      }
      else
      {
        return false;
      }
    }
  }
}
