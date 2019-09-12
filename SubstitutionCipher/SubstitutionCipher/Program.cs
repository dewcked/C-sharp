using System;
using static System.Console;
using System.Runtime.CompilerServices;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

[assembly: InternalsVisibleTo("UnitTest")]

namespace SubstitutionCipher
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = null;
            StringBuilder text = new StringBuilder();
            if (args.Length != 0)
            {
                try
                {
                    text.Append(File.ReadAllText(args[1]));
                } catch (Exception e)
                {
                    WriteLine($"Error occured {e}");
                    return;
                }
               
            }
            else
            {
                WriteLine("Please input strings you want to use. input [end] at the end of the line\n" +
                    "Example:\nThis is example message.\nWrite like this and put end like this.\n[end]");
                while (true)
                {
                    var tmp = ReadLine();
                    if (tmp.ToLower().Equals("[end]"))
                        break;
                    text.AppendLine(tmp.ToLower());
                }
            }
            WriteLine("What you wanna do?\n1)encrypt\t2)decrypt");
            input = ReadLine();
            if (Int32.TryParse(input, out int choice))
            {
                switch (choice)
                {
                    case 1:
                        Encryption(text);
                        break;
                    case 2:
                        Decryption(text);
                        break;
                }
            }
            else if (input.ToLower().Equals("encrypt") || input.ToLower().Equals("enc"))
            {
                Encryption(text);
            }
            else if (input.ToLower().Equals("decrypt") || input.ToLower().Equals("dec"))
            {
                Decryption(text);
            }
            else
            {
                WriteLine("Please input right command.");
            }
            WriteLine("Press any key to terminate this program...");
            ReadKey();
        }
        //Checking the argument input from ciphertext.txt\n
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        static void Encryption(StringBuilder text)
        {
            List<char> component = new List<char>{ 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z' };
            string dictionary = "abcdefghijklmnopqrstuvwxyz";
            Dictionary<char, char> key = new Dictionary<char, char>();
            int idx = 0;

            while (component.Count != 0) {
                var tmp = GenKey(component.Count);
                key.Add(dictionary[idx++], component[tmp]);
                component.RemoveAt(tmp);
            }
            WriteLine("The encryption key is ");
            foreach(var i in key){
                WriteLine($"\t{i.Key}\t=>\t{i.Value}");
            }

            for(int i=0;i<text.Length;i++)
            {
                if (IsAlpha(text[i]))
                    text[i] = key[text[i]];
            }
            WriteLine(text.ToString());
            File.WriteAllText("[encrypted].txt", text.ToString());
            WriteLine("Encrypted text in [encrypted].txt");
        }
        static int GenKey(int len)
        {
            byte[] randomByte = new byte[1];
            try
            {
                do
                    rngCsp.GetBytes(randomByte);
                while (randomByte[0] < byte.MaxValue / len * len);
            } catch (Exception)
            {
                WriteLine("Wrong calculation");
            }

            return randomByte[0] % len;
        }
        
        static void Decryption(StringBuilder text)
        {
            List<char> component = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            string dictionary = "abcdefghijklmnopqrstuvwxyz";
            Dictionary<char, char> key = new Dictionary<char, char>();
            int succeed = 0;
            List<char> frequency_global = new List<char> { 'e', 't', 'a', 'o', 'i', 'n', 's', 'h', 'r', 'd', 'l', 'u', 'c', 'm', 'w', 'f', 'y', 'g', 'p', 'b', 'v', 'k', 'x', 'j', 'q', 'z' };
            List<char> frequency_local = new List<char>();
            StringBuilder tmptext;
            while(succeed == 0)
            {
                ;
            }
        }
        static bool IsAlpha(char chr)
        {
            return (chr >= 'a' && chr <= 'z');
        }
    }
}