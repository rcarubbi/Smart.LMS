using Carubbi.Security;
using System;

namespace SmartLMS.PasswordUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            var _crypt = new SymmetricCrypt(SymmetricCryptProvider.TripleDES); ;
            _crypt.Key = args[0];
            var password = _crypt.Decrypt(args[1]);
            Console.WriteLine(password);
            Console.ReadKey();
        }
    }
}
