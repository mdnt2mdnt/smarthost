using System;
using System.Reflection;
using System.Resources;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;


[assembly: AssemblyTitle("setVersion")]
[assembly: AssemblyDescription("Update SmartHost Plugin Version Before Compiling")]
[assembly: AssemblyCompany("Tencent")]
[assembly: AssemblyProduct("setVersion")]
[assembly: AssemblyCopyright("Copyright © Mooringniu 2012")]
[assembly: AssemblyTrademark("SmartHost")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: NeutralResourcesLanguageAttribute("en")]

namespace setVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 2)
            {
                for (int i = 1, il = args.Length; i < il; i++)
                {
                    if (!File.Exists(args[i])) { continue; }
                    string[] content = File.ReadAllLines(args[i]);
                    TextWriter fp = new StreamWriter(args[i]);
                    for (int j = 0, jl = content.Length; j < jl; j++)
                    {
                        content[j] = Regex.Replace(
							content[j],
							@"((Server|Assembly|Version|VersionKey|ProductVersion).+)(\d+\.){3}\d+",
							"${1}"+args[0]
						);
                        fp.WriteLine(content[j]);
                    }
                    fp.Close();
                    Console.WriteLine("version {0}", args[i]);
                }
                Console.WriteLine("over");
            }
            else
            {
                Console.Write("Can't Find File "+args[1]);
            }
        }
    }
}
