using System;
using static System.Console;
using System.Runtime.CompilerServices;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

[assembly: InternalsVisibleTo("UnitTest")]

public struct word
{
    public char letter;
    public int depth;
};

namespace SubstitutionCipher
{
    class Program
    {

        static Dictionary<char, int> frequency_global = new Dictionary<char, int>();
        static Dictionary<char, int> frequency_local = new Dictionary<char, int>();

        static void Main(string[] args)
        {
            string input = null;
            StringBuilder text = new StringBuilder();
            if (args == null)
            {
                WriteLine("Please input strings you want to use. input [end] at the end of the line\n" +
                    "Example:\nThis is example message.\nWrite like this and put end like this.\n[end]");
                while (true)
                {
                    var tmp = ReadLine();
                    if (tmp.ToLower().Equals("[end]"))
                        break;
                    text.AppendLine(tmp.ToUpper());
                }
                File.WriteAllText("basestring.txt", text.ToString());
            }

            WriteLine("What you wanna do?\n1)encrypt\t2)decrypt");
            input = ReadLine();
            if (Int32.TryParse(input, out int choice))
            {
                switch (choice)
                {
                    case 1:
                        if (args != null)
                            Encryption(args[0]);
                        else
                            Encryption("basestring.txt");
                        break;
                    case 2:
                        if (args != null)
                            Decryption(args[0]);
                        else
                            Decryption("basestring.txt");
                        break;
                }
            }
            else if (input.ToLower().Equals("encrypt") || input.ToLower().Equals("enc"))
            {
                if (args != null)
                    Encryption(args[0]);
                else
                    Encryption("basestring.txt");
            }
            else if (input.ToLower().Equals("decrypt") || input.ToLower().Equals("dec"))
            {
                if (args != null)
                    Decryption(args[0]);
                else
                    Decryption("basestring.txt");
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
        static void Encryption(string filename)
        {
            List<char> component = new List<char>{ 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z' };
            string dictionary = "abcdefghijklmnopqrstuvwxyz";
            Dictionary<char, char> key = new Dictionary<char, char>();
            int idx = 0;

            StringBuilder text = new StringBuilder();
            text.Append(File.ReadAllLines(filename));

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
        
        static void Decryption(string filename)
        {
            List<char> component = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            string dictionary = "abcdefghijklmnopqrstuvwxyz";
            Dictionary<char, char> key = new Dictionary<char, char>();
            int succeed = 0;

            Dictionary<int[], List<string>> dict = ParsePattern("dictionary.txt", "frequency_global");
            Dictionary<int[], List<string>> enc = ParsePattern(filename, "frequency_local");

            Dictionary<char, List<char>> guessing = new Dictionary<char, List<char>>();

            var global_sort = frequency_global.Keys.ToList();
            var local_sort = frequency_local.Keys.ToList();
            global_sort.Sort();
            local_sort.Sort();

            WriteLine("Dictionary Frequency rate");
            foreach (var glokey in global_sort)
                WriteLine($"{glokey} {frequency_global[glokey]} ");
            WriteLine("Encrypted Text Frequency rate");
            foreach (var lockey in local_sort)
                WriteLine($"{lockey} {frequency_local[lockey]} ");

            //foreach(var encletters in enc)
            //{
            //    foreach(var dicletters in dict)
            //    {
            //        if (IsSame(encletters.Key,dicletters.Key))
            //        {
            //            foreach(var encletter in encletters.Value)
            //            {
            //                foreach (var dicletter in dicletters.Value)
            //                {
            //                    var len = encletter.Length;
            //                    for (int i = 0; i < len; i++)
            //                    {
            //                        if (!guessing.ContainsKey(encletter[i]))
            //                            guessing.Add(encletter[i], new List<word> { new word { letter = dicletter[i], depth = len } } );
            //                        else
            //                            guessing[encletter[i]].Add(new word { letter = dicletter[i], depth = len });
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //for (var k = 100; k > 0; k--)
            //{
            //    foreach (var i in guessing)
            //    {
            //        foreach (var j in i.Value)
            //        {
            //            //WriteLine($"{i.Key}\t{j.depth}\t{j.letter}");
            //            if(j.depth == k)

            //        }
            //    }
            //}
            Dictionary<int[], List<string>> words = MatchPattern(dict, enc);

            //foreach(var letter in words)
            //{
            //    foreach(var charchar in letter.Value)
            //    {
            //        WriteLine(charchar);
            //    }
            //}
            for (var k = 13; k != 0; k--)
                foreach (var item in words)
                    foreach (var encrypted in enc)
                        if (item.Key.Length == k && IsSame(item.Key, encrypted.Key))
                            foreach (var realitem in item.Value)
                                foreach (var realencrypted in encrypted.Value)
                                    for (int j = 0; j < k; j++)
                                        if (!guessing.ContainsKey(realencrypted[j]))
                                            guessing.Add(realencrypted[j], new List<char> { realitem[j] });
                                        else if (guessing[realencrypted[j]].Contains(realitem[j]))
                                            continue;
                                        else if (!guessing[realencrypted[j]].Contains(realitem[j]) && k != 12)
                                            break;
                                        else
                                            guessing[realencrypted[j]].Add(realitem[j]);


            foreach(var letter in guessing)
            {
                Write(letter.Key);
                foreach(var letter2 in letter.Value)
                {
                    Write(" ");
                    Write(letter2);
                }
                WriteLine();
            }

            StreamReader file2 = new StreamReader(filename);
            while (!file2.EndOfStream)
            {
                string tmp = file2.ReadLine();
                foreach (var chr in tmp)
                {
                    if (IsAlpha(chr))
                    {
                        if (guessing.ContainsKey(chr))
                            Write(guessing[chr][0]);
                        else
                            Write("_");
                    }
                    else
                        Write(chr);
                }
                WriteLine();
            }

        }
        static Dictionary<int[], List<string>> MatchPattern(Dictionary<int[], List<string>> dict, Dictionary<int[], List<string>> enc)
        {
            Dictionary<int[], List<string>> pattern = new Dictionary<int[], List<string>>();
            foreach(var letter in enc)
                foreach(var dicletter in dict)
                    if (IsSame(letter.Key, dicletter.Key) && !pattern.ContainsKey(dicletter.Key))
                        pattern.Add(dicletter.Key, dicletter.Value);
            return pattern;
        }
        static bool IsSame(int[] arr1, int[] arr2)
        {
            if (arr1.Length != arr2.Length)
                return false;
            var len = arr1.Length;
            for (int i = 0; i < len; i++)
                if (arr1[i] != arr2[i])
                    return false;
            return true;
        }
        static bool IsAlpha(char chr)
        {
            return (chr >= 'A' && chr <= 'Z');
        }
        static Dictionary<int[], List<string>> ParsePattern(string filename, string frequency)
        {
            StreamReader file = new StreamReader(filename);
            Dictionary<int[], List<string>> parsed = new Dictionary<int[], List<string>>();
            int charidx;
            int intidx;
            char[] tempchar = new char[26];
            int[] tempint = new int[100];
            int i;
            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                if (line.Contains(" "))
                    foreach (var words in line.Split(" "))
                    {
                        charidx = -1;
                        intidx = -1;
                        string final = null;
                        foreach (var chr in words)
                        {
                            if (IsAlpha(chr))
                            {
                                final += chr;
                                if (frequency.Equals("frequency_local"))
                                    if (frequency_local.ContainsKey(chr))
                                        frequency_local[chr]++;
                                    else
                                        frequency_local.Add(chr, 1);
                                else if (frequency.Equals("frequency_global"))
                                    if (frequency_global.ContainsKey(chr))
                                        frequency_global[chr]++;
                                    else
                                        frequency_global.Add(chr, 1);

                                for (i = 0; i <= charidx; i++)
                                    if (tempchar[i] == chr)
                                    {
                                        tempint[++intidx] = i;
                                        break;
                                    }
                                if (i > charidx || intidx == -1)
                                {
                                    tempint[++intidx] = ++charidx;
                                    tempchar[charidx] = chr;
                                }
                            }
                        }
                        int[] inputint = new int[intidx+1];
                        Array.Copy(tempint, inputint, intidx+1);
                        if (!parsed.ContainsKey(inputint))
                            parsed.Add(inputint, new List<string> {final});
                        else
                            parsed[inputint].Add(final);
                    }
                else
                {
                    charidx = -1;
                    intidx = -1;
                    foreach (var chr in line)
                    {
                        if (IsAlpha(chr))
                        {
                            if (frequency.Equals("frequency_local"))
                                if (frequency_local.ContainsKey(chr))
                                    frequency_local[chr]++;
                                else
                                    frequency_local.Add(chr, 1);
                            else if (frequency.Equals("frequency_global"))
                                if (frequency_global.ContainsKey(chr))
                                    frequency_global[chr]++;
                                else
                                    frequency_global.Add(chr, 1);

                            for (i = 0; i <= charidx; i++)
                                if (tempchar[i] == chr)
                                {
                                    tempint[++intidx] = i;
                                    break;
                                }
                            if (i > charidx || intidx == -1)
                            {
                                tempint[++intidx] = ++charidx;
                                tempchar[charidx] = chr;
                            }
                        }
                    }
                    //Write(line+" ");
                    //for (int j=0;j<= intidx; j++)
                    //    Write($"{tempint[j]} ");
                    //WriteLine();
                    int[] inputint = new int[intidx+1];
                    Array.Copy(tempint, inputint, intidx+1);
                    if (!parsed.ContainsKey(inputint))
                        parsed.Add(inputint, new List<string> { line });
                    else
                        parsed[inputint].Add(line);
                }
            }

            return parsed;
        }
    }
}