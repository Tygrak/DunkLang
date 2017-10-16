using System;
using System.IO;
using System.Collections.Generic;

//Start using: dotnet run first.dunk -d

namespace DunkLang{
    public class Program{
        public static void Main(string[] args){
            bool debug = false;
            string path = "";
            if(args.Length > 0){
                if(args[0].Contains("/")){
                    path = args[0];
                } else{
                    path = Directory.GetCurrentDirectory()+"/"+args[0];
                }
            } else{
                Console.WriteLine("Choose file to run: ");
                string line = Console.ReadLine();
                if(line.Contains("/")){
                    path = line;
                } else{
                    path = Directory.GetCurrentDirectory()+"/"+line;
                }
                Console.WriteLine();
            }
            for (int i = 1; i < args.Length; i++){
                if(args[i] == "-d") debug = true;
            }
            DunkCompiler prog = new DunkCompiler(path, debug);
            prog.Transpile();
            //prog.RunProgram();
        }
    }
}