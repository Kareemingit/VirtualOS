using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VirtualOS.Commuication
{

    public class KeyManager
    {
        public string RSApublicKey;
        public string RSAprivateKey;
        public byte[][] AESSessionkey = new byte[2][];
        public void GenerateRSAKeys()
        {
            using (RSA rsa = RSA.Create()) {
                string publicKey = Convert.ToBase64String(inArray: rsa.ExportRSAPublicKey());
                string privateKey = Convert.ToBase64String(inArray: rsa.ExportRSAPrivateKey());
                RSApublicKey = publicKey;
                RSAprivateKey = privateKey;
            }
        }
        public void GenerateAESSessionKeyAndIv()
        {
            byte[] key = new byte[16];
            byte[] iv = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) 
            {
                rng.GetBytes(key);
                rng.GetBytes(iv);
            }
            AESSessionkey[0] = key;
            AESSessionkey[1] = iv;
        }
    }
}
