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
            List<char> pool = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
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
            foreach (var i in key) {
                WriteLine($"\t{i.Key}\t=>\t{i.Value}");
            }

            for (int i = 0; i < text.Length; i++)
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
            Dictionary<char, List<char>>[] pool = new Dictionary<char, List<char>>[max_len + 1];
            for (i = 0; i <= max_len; i++)
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
            for (i = max_len - 1; i >= 0; i--)
            {
                int unique = 1;
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
                                if (!pool[max_len][pool[i].ElementAt(j).Key].Contains(chr))
                                    if (!tmpchr.ContainsKey(pool[i].ElementAt(j).Key))
                                        tmpchr.Add(pool[i].ElementAt(j).Key, new List<char> { chr });
                                    else
                                        tmpchr[pool[i].ElementAt(j).Key].Add(chr);
                        }
                    }
                }
                foreach (var chr in tmpchr)
                    foreach (var chr2 in chr.Value)
                        pool[i][chr.Key].Remove(chr2);
                if (unique == 1)
                    foreach (var wordlist in pool[i])
                        if (!pool[max_len].ContainsKey(wordlist.Key))
                            pool[max_len].Add(wordlist.Key, wordlist.Value);
            }
            //to maximize guessing part 2
            Dictionary<int[], List<string>>[] filterlist = new Dictionary<int[], List<string>>[max_len];
            for (i = 0; i < max_len; i++)
                filterlist[i] = new Dictionary<int[], List<string>>();
            for (i = max_len - 1; i >= 0; i--)

                foreach (var strs in words[i])

                {
                    List<char> tmpchr = new List<char>();
                    foreach (var str in strs.Value)
                    {
                        var len = str.Length;

                        for (j = 0; j < len; j++)
                            foreach (var poolval in pool[i])
                            {
                                if (poolval.Value.Contains(str[j]))
                                {
                                    tmpchr.Add(str[j]);
                                }
                            }
                        if (tmpchr.Count == len)
                        {
                            if (!filterlist[i].ContainsKey(strs.Key))
                                filterlist[i].Add(strs.Key, new List<string> { str });
                            else
                                filterlist[i][strs.Key].Add(str);
                        }
                    }

                }


            WriteLine();
            //filter out everything
            for (i = max_len - 1; i >= 0; i--)
            {
                List<char> tmpchr = new List<char>();
                List<char> removal = new List<char>();
                Dictionary<char, List<char>> tmpremoval = new Dictionary<char, List<char>>();
                foreach (var filter in filterlist[i])
                    foreach (var filterword in filter.Value)
                        foreach (var chr in filterword)
                            if (!tmpchr.Contains(chr))
                                tmpchr.Add(chr);
                if (filterlist[i].Count == 0)
                    continue;
                foreach (var spool in pool[i])
                    foreach (var spoolchr in spool.Value)
                        if (!tmpchr.Contains(spoolchr))
                            removal.Add(spoolchr);
                foreach (var chr in removal)
                {
                    var change = 1;
                    foreach (var removechr in pool[i])
                        if (removechr.Value.Contains(chr))
                            if (!tmpremoval.ContainsKey(removechr.Key))
                                tmpremoval.Add(removechr.Key, new List<char> { chr });
                            else
                                tmpremoval[removechr.Key].Add(chr);

                }
                foreach (var chrs in tmpremoval)
                    foreach (var chr in chrs.Value)
                        pool[i][chrs.Key].Remove(chr);

                foreach (var chr1 in pool[i])
                    if (!pool[max_len].ContainsKey(chr1.Key))
                        pool[max_len].Add(chr1.Key, chr1.Value);

            }


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
            List<char> used = new List<char>();
            // print result
            StringBuilder decryptedhalf = new StringBuilder();
            StreamReader file2 = new StreamReader(filename);
            List<char> mustfill = new List<char>();
            int mustfillidx = 0;
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
                            {
                                decryptedhalf.Append(pool[max_len][chr][0]);
                                if (!used.Contains(pool[max_len][chr][0]))
                                    used.Add(pool[max_len][chr][0]);
                            }
                            else
                            {
                                mustfill.Add(chr);
                                decryptedhalf.Append("_");
                            }
                        }
                        else
                            decryptedhalf.Append(chr);
                    }
                    decryptedhalf.Append(" ");
                }
                decryptedhalf.Append("\n");
            }
            string halfdecrypted = decryptedhalf.ToString();
            WriteLine(decryptedhalf);
            decryptedhalf.Clear();

            for (i = 0; i < max_len; i++)
                foreach (var chrkey in pool)
                    chrkey.Clear();

            foreach (var str1 in halfdecrypted.Split("\n"))
            {
                foreach (var str2 in str1.Split(" "))
                {
                    var reallen = RealhalfLen(str2);
                    string tmpchar = null;
                    var idx = 0;
                    if (reallen == 0)
                    {
                        decryptedhalf.Append(str2 + " ");
                        continue;
                    }
                    if (str2.Contains("_"))
                    {
                        foreach (var dictletter in words[reallen - 1])
                        {
                            foreach (var dictletter2 in dictletter.Value)
                            {
                                var fail = 0;
                                idx = mustfillidx;
                                Dictionary<char, char> minimap = new Dictionary<char, char>();

                                for (i = 0; i < reallen; i++)
                                    if (str2[i] != '_' && dictletter2[i] != str2[i])
                                    {
                                        fail = 1;
                                        break;
                                    }
                                    else if (str2[i] == '_')
                                    {
                                        if (used.Contains(dictletter2[i]))
                                        {
                                            fail = 1;
                                            break;
                                        }
                                        foreach (var tmp in minimap)
                                            if (tmp.Key == mustfill.ElementAt(idx))
                                                if (tmp.Value != dictletter2[i])
                                                {
                                                    fail = 1;
                                                    break;
                                                }
                                        if (!minimap.ContainsKey(mustfill.ElementAt(idx)))
                                            minimap.Add(mustfill.ElementAt(idx++), dictletter2[i]);
                                        //else
                                        //{
                                        //    if(!minimap[mustfill.ElementAt(idx)].Equals(dictletter2[i]))
                                        //        minimap.Add(mustfill.ElementAt(idx), dictletter2[i]);
                                        //}
                                    }

                                //71 72 73 가야되는데 69 70 71
                                idx = mustfillidx;
                                if (fail == 0)
                                {
                                    for (i = 0; i < reallen; i++)
                                        if (str2[i] == '_')
                                        {

                                            if (!pool[reallen - 1].ContainsKey(mustfill.ElementAt(idx)))
                                                pool[reallen - 1].Add(mustfill.ElementAt(idx), new List<char> { dictletter2[i] });
                                            else if (!pool[reallen - 1][mustfill.ElementAt(idx)].Contains(dictletter2[i]))
                                                pool[reallen - 1][mustfill.ElementAt(idx)].Add(dictletter2[i]);
                                            idx++;
                                        }

                                    
                                }
                            }
                        }
                        for (i = 0; i < reallen; i++)
                            if (str2[i] == '_')
                                mustfillidx++;
                        // very important


                    }
                    tmpchar = str2;
                    decryptedhalf.Append(tmpchar + " ");
                }
                decryptedhalf.Append("\n");
            }

            for (i = 0; i < mustfill.Count; i++)
                for (j = 0; j < max_len; j++)
                    foreach (var spool in pool[j])
                        if (spool.Value.Count == 1 && spool.Key == mustfill.ElementAt(i))
                            if (!pool[max_len].ContainsKey(spool.Key))
                            {
                                pool[max_len].Add(spool.Key, spool.Value);
                                used.Add(spool.Value[0]);
                            }

            mustfillidx = 0;
            file2 = new StreamReader(filename);
            foreach (var chr in decryptedhalf.ToString().Split("\n")) {
                foreach (var chr2 in chr.Split(" ")) {
                    var len = RealhalfLen(chr2);
                    foreach (var chr3 in chr2) {
                        if (chr3 == '_') {
                            if (pool[max_len].ContainsKey(mustfill[mustfillidx]))
                            {
                                Write(pool[max_len][(mustfill[mustfillidx])][0]);
                                //if (!used.Contains(pool[len - 1][(mustfill[mustfillidx])][0]))
                                //    used.Add(pool[len - 1][(mustfill[mustfillidx])][0]);
                            }
                            else
                            {
                                Write("_");
                            }
                            mustfillidx++;
                        }
                        else
                            Write(chr3);
                    }
                    Write(" ");
                }
                WriteLine();
            }
            //foreach(var locked in used):

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
        static int RealhalfLen(string str)
        {
            int len = 0;
            foreach (var chr in str)
                if (IsAlpha(chr) || chr == '_')
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