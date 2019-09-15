using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using static System.Console;

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
        static int max_len;
        static void Main(string[] args)
        {
            string user_input = null;
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
            user_input = ReadLine();
            if (Int32.TryParse(user_input, out int choice))
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
            else if (user_input.ToLower().Equals("encrypt") || user_input.ToLower().Equals("enc"))
            {
                if (args != null)
                    Encryption(args[0]);
                else
                    Encryption("basestring.txt");
            }
            else if (user_input.ToLower().Equals("decrypt") || user_input.ToLower().Equals("dec"))
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

        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        static void Encryption(string filename)
        {
            List<char> pool = new List<char>{ 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z' };
            string dictionary = "abcdefghijklmnopqrstuvwxyz";
            Dictionary<char, char> key = new Dictionary<char, char>();
            int idx = 0;

            StringBuilder text = new StringBuilder();
            text.Append(File.ReadAllLines(filename));

            while (pool.Count != 0) {
                var tmp = GenKey(pool.Count);
                key.Add(dictionary[idx++], pool[tmp]);
                pool.RemoveAt(tmp);
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
            int i, j;
            //Calculate Max Word Length
            CalculateMaxLen(filename);
            //Make dictionary parse
            Dictionary<int[], List<string>>[] enc = ParsePattern(filename);
            Dictionary<int[], List<string>>[] dict = ParsePattern("dictionary.txt");

            //Make Frequency Table
            var global_sort = frequency_global.Keys.ToList();
            var local_sort = frequency_local.Keys.ToList();
            global_sort.Sort();
            local_sort.Sort();

            WriteLine("Dictionary Frequency table");
            foreach (var glokey in global_sort)
                WriteLine($"{glokey} {frequency_global[glokey]} ");
            WriteLine("Encrypted Text Frequency table");
            foreach (var lockey in local_sort)
                WriteLine($"{lockey} {frequency_local[lockey]} ");

            Dictionary<int[], List<string>>[] words = MatchPattern(dict, enc);
            Dictionary<char, List<char>>[] pool = new Dictionary<char, List<char>>[max_len+1];
            for(i=0;i<=max_len;i++)
                pool[i] = new Dictionary<char, List<char>>();

            for (i = 0; i != max_len; i++)
                foreach (var item in words[i])
                    foreach (var encrypted in enc[i])
                        if (item.Key.Length == i + 1 && IsSame(item.Key, encrypted.Key))
                            foreach (var realitem in item.Value)
                                foreach (var realencrypted in encrypted.Value)
                                {
                                    for (j = 0; j < i + 1; j++)
                                        if (!pool[i].ContainsKey(realencrypted[j]))
                                            pool[i].Add(realencrypted[j], new List<char> { realitem[j] });
                                        else if (pool[i][realencrypted[j]].Contains(realitem[j]))
                                            continue;
                                        else
                                            pool[i][realencrypted[j]].Add(realitem[j]);
                                }

            //To maximize guessing, filter out
            for(i = max_len-1; i >= 0; i--)
            {
                int unique=1;
                int len = pool[i].Count;
                Dictionary<char, List<char>> tmpchr = new Dictionary<char, List<char>>();
                unique = 1;
                for (j = 0; j < len; j++)
                {
                    if (pool[i].ElementAt(j).Value.Count != 1)
                    {
                        unique = 0;
                        if (pool[max_len].ContainsKey(pool[i].ElementAt(j).Key))
                        {
                            foreach (var chr in pool[i].ElementAt(j).Value)
                                if (pool[max_len][pool[i].ElementAt(j).Key].Contains(chr))
                                    if (!tmpchr.ContainsKey(pool[i].ElementAt(j).Key))
                                        tmpchr.Add(pool[i].ElementAt(j).Key, new List<char> { chr });
                                    else
                                        tmpchr[pool[i].ElementAt(j).Key].Add(chr);
                        }

                        //tmpchr.Clear();
                        //foreach(var chr in guessing[maxlen])
                        //    tmpchr.Add(chr.Key);
                        //foreach (var dicletter in words)
                        //    if (dicletter.Key.Length == k + 1)
                        //        foreach (var tmpstr in dicletter.Value)
                        //        {
                        //            for(int i=0;i<k+1;i++)
                        //                foreach(var comparestr in guessing[k])
                        //                    if (comparestr.Value.Contains(tmpstr[i]))
                        //                        if (comparestr.Value.Count > 1)
                        //                            comparestr.Value.Remove(tmpstr[i]);


                        //        }
                    }
                }
                foreach (var chr in tmpchr)
                    foreach(var chr2 in chr.Value)
                        pool[i][chr.Key].Remove(chr2);
                if(unique == 1)
                    foreach (var wordlist in pool[i])
                        if (!pool[max_len].ContainsKey(wordlist.Key))
                            pool[max_len].Add(wordlist.Key, wordlist.Value);
            }
            //to maximize guessing part 2
            //for (i = 0; i < max_len; i++)
            //    foreach (var strs in words[i])
            //        foreach (var str in strs.Value) {
            //            var len = str.Length;
            //            for (j = 0; j < len; j++)
            //                foreach (var poolval in pool[max_len])
            //                    if (!poolval.Value.Contains(str[j])) {

            //                    }
                                    
            //        }


            WriteLine();
            //print possible keys
            for (i = 0; i <= max_len; i++)
            {
                foreach (var letter in pool[i])
                {
                    Write(letter.Key);
                    foreach (var letter2 in letter.Value)
                    {
                        Write(" ");
                        Write(letter2);
                    }
                    WriteLine();
                }
                WriteLine();
            }
            WriteLine();

            // print result
            StreamReader file2 = new StreamReader(filename);
            while (!file2.EndOfStream)
            {
                string[] tmps = file2.ReadLine().Split(" ");
                foreach (var tmp in tmps)
                {
                    var reallen = RealLen(tmp);
                    foreach (var chr in tmp)
                    {
                        if (IsAlpha(chr))
                        {
                            if (pool[max_len].ContainsKey(chr))
                                Write(pool[max_len][chr][0]);
                            else if (pool[reallen - 1].ContainsKey(chr))
                            {
                                foreach (var chr2 in pool[reallen - 1][chr]) {
                                    if (!pool[max_len].ContainsKey(chr2))
                                    {
                                        Write(chr2);
                                        pool[max_len].Add(chr, new List<char> { chr2 });
                                        break;
                                    }
                                }
                            }
                            else
                                Write("_");
                        }
                        else
                            Write(chr);
                    }
                    Write(" ");
                }
                WriteLine();
            }

        }
        //First call to Calculate Max Length of words
        static void CalculateMaxLen(string filename)
        {
            StreamReader file = new StreamReader(filename);
            int len;
            while(!file.EndOfStream)
                foreach (var str in file.ReadLine().Split(" "))
                {
                    len = RealLen(str);
                    max_len = max_len > len ? max_len : len;
                }
        }
        //Calculate Real Length except special chars
        static int RealLen(string str)
        {
            int len = 0;
            foreach (var chr in str)
                if (IsAlpha(chr))
                    len++;
            return len;
        }
        static Dictionary<int[], List<string>>[] MatchPattern(Dictionary<int[], List<string>>[] dict, Dictionary<int[], List<string>>[] enc)
        {
            Dictionary<int[], List<string>>[] pattern = new Dictionary<int[], List<string>>[max_len];
            for (int i = 0; i < max_len; i++)
                pattern[i] = new Dictionary<int[], List<string>>();
            for(int i=0;i<max_len;i++)
                foreach(var letter in enc[i])
                    foreach(var dicletter in dict[i])
                        if (IsSame(letter.Key, dicletter.Key) && !pattern[i].ContainsKey(dicletter.Key))
                            pattern[i].Add(dicletter.Key, dicletter.Value);
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
        static Dictionary<int[], List<string>>[] ParsePattern(string filename)
        {
            StreamReader file = new StreamReader(filename);
            Dictionary<int[], List<string>>[] parsed = new Dictionary<int[], List<string>>[max_len];
            int charidx;
            int intidx;
            char[] tmpchar = new char[26];
            int[] tmpint = new int[100];
            int i;

            for (i = 0; i < max_len; i++)
                parsed[i] = new Dictionary<int[], List<string>>();

            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                if (line.Contains(" "))
                    foreach (var words in line.Split(" "))
                    {
                        charidx = -1;
                        intidx = -1;
                        string realstr = null;
                        foreach (var chr in words)
                        {
                            if (IsAlpha(chr))
                            {
                                realstr += chr;
                                if (frequency_local.ContainsKey(chr))
                                    frequency_local[chr]++;
                                else
                                    frequency_local.Add(chr, 1);

                                for (i = 0; i <= charidx; i++)
                                    if (tmpchar[i] == chr)
                                    {
                                        tmpint[++intidx] = i;
                                        break;
                                    }
                                if (i > charidx || intidx == -1)
                                {
                                    tmpint[++intidx] = ++charidx;
                                    tmpchar[charidx] = chr;
                                }
                            }
                        }
                        if (intidx == -1)
                            continue;
                        int[] inputint = new int[intidx+1];
                        Array.Copy(tmpint, inputint, intidx+1);
                        if (!parsed[intidx].ContainsKey(inputint))
                            parsed[intidx].Add(inputint, new List<string> {realstr});
                        else
                            parsed[intidx][inputint].Add(realstr);
                    }
                else
                {
                    if (line.Length > max_len)
                        continue;
                    charidx = -1;
                    intidx = -1;
                    foreach (var chr in line)
                    {
                        if (IsAlpha(chr))
                        {
                            if (frequency_global.ContainsKey(chr))
                                frequency_global[chr]++;
                            else
                                frequency_global.Add(chr, 1);

                            for (i = 0; i <= charidx; i++)
                                if (tmpchar[i] == chr)
                                {
                                    tmpint[++intidx] = i;
                                    break;
                                }
                            if (i > charidx || intidx == -1)
                            {
                                tmpint[++intidx] = ++charidx;
                                tmpchar[charidx] = chr;
                            }
                        }
                    }
                    int[] inputint = new int[intidx+1];
                    Array.Copy(tmpint, inputint, intidx+1);
                    if (!parsed[intidx].ContainsKey(inputint))
                        parsed[intidx].Add(inputint, new List<string> { line });
                    else
                        parsed[intidx][inputint].Add(line);
                }
            }

            return parsed;
        }
    }
}