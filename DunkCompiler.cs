using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace DunkLang{
    public class DunkCompiler{
        public string path = "";
        public string program = "";
        public string transpiled = "";
        public Process process;
        public bool debug = true;

        public DunkCompiler(string program, string path){
            if(debug) Console.WriteLine("Opening program: " + path + "\n");
            this.program = program;
            this.path = path;
        }

        public DunkCompiler(string path, bool debug){
            this.debug = debug;
            if(debug) Console.WriteLine("Opening program: " + path + "\n");
            try{
                this.program = File.ReadAllText(path);
                this.path = path;
            } catch{
                Console.WriteLine("File not found.");
                return;
            }
        }

        public string GetProgramFileName(){
            int pos = path.LastIndexOf("/");
            int pos2 = path.IndexOf(".", pos);
            return path.Substring(pos+1, pos2-pos-1);
        }

        public Function[] GetFunctions(){
            List<Function> funcs = new List<Function>();
            Function nextFunc = new Function("", "");
            int breaker = 0;
            int position = program.IndexOf("[");
            while(nextFunc != null){
                if(position == -1){
                    nextFunc = null;
                    if(debug) Console.WriteLine("End.");
                    continue;
                }
                int nextPos = program.IndexOf("]", position);
                string name = program.Substring(position+1, nextPos-position-1);
                int nextRounded = program.IndexOf("(", nextPos);
                string args = "";
                nextPos = program.IndexOf("{", nextPos)+1;
                if(nextRounded != -1 && nextRounded < nextPos){
                    args = program.Substring(nextRounded+1, program.IndexOf(")", nextRounded) - nextRounded-1);
                }
                int contentPos = nextPos;
                int depth = 1;
                int end = nextPos;
                while(depth > 0){
                    int start = program.IndexOf("{", nextPos);
                    end = program.IndexOf("}", nextPos);
                    if(start != -1 && start < end){
                        nextPos = start+1;
                        depth++;
                    } else{
                        nextPos = end+1;
                        depth--;
                    }
                }
                position = program.IndexOf("[", end);
                nextFunc = new Function(name, program.Substring(contentPos, nextPos-contentPos-1), args);
                if(debug){
                    if(args != ""){
                        Console.WriteLine(breaker.ToString() + " : " + nextFunc.name + " args: " + nextFunc.args + nextFunc.content);
                    } else{
                        Console.WriteLine(breaker.ToString() + " : " + nextFunc.name + nextFunc.content);
                    }
                }
                breaker++;
                if(breaker > int.MaxValue-1){
                    throw new Exception("Max func limit reached");
                }
                funcs.Add(nextFunc);
            }
            return funcs.ToArray();
        }

        public string[] GetCommands(Function func){
            List<string> commands = new List<string>();
            string nextCommand = "";
            int breaker = 0;
            string content = func.content.Trim();
            int position = 0;
            while(nextCommand != null){
                int nextPos = program.IndexOf(";", position);
                if(nextPos == -1){
                    nextCommand = null;
                    if(debug) Console.WriteLine("End.");
                    continue;
                }
                nextCommand = program.Substring(position, nextPos-position);
                if(debug){
                    Console.WriteLine("c" + breaker.ToString() + " : " + nextCommand);
                }
                breaker++;
                if(breaker > int.MaxValue-1){
                    throw new Exception("Max func limit reached");
                }
                position = nextPos+1;
                commands.Add(nextCommand);
            }
            return commands.ToArray();
        }

        public void Transpile(){
            Function[] functions = GetFunctions();
            for (int i = 0; i < functions.Length; i++){
                Console.WriteLine(functions[i].name + " : " + functions[i].content);
                //string[] commands = GetCommands(functions[i]);
            }
            /*transpiled = transpiled.Insert(0, "#include<stdio.h>\nint main(int argc, char *argv[]){\n");
            transpiled += program;
            transpiled += "\n}";*/
        }

        public void RunProgram(){
            string fileLocation = Directory.GetCurrentDirectory()+"/"+GetProgramFileName();
            if(debug) Console.WriteLine("Saving to: " + fileLocation + ".c");
            fileLocation += ".c";
            File.WriteAllText(fileLocation, transpiled);
            Process bProcess = new Process{
                StartInfo = new ProcessStartInfo{
                    FileName = "gcc",
                    Arguments = fileLocation + " -o " + GetProgramFileName(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            bProcess.Start();
            bProcess.WaitForExit();
            if(!debug) File.Delete(fileLocation + ".c");
            string gccOutput = bProcess.StandardOutput.ReadToEnd();
            gccOutput.Trim();
            if(gccOutput.Length > 0){
                Console.WriteLine("Compilation error:");
                Console.WriteLine(bProcess.StandardOutput.ReadToEnd());
            }
            process = new Process{
                StartInfo = new ProcessStartInfo{
                    FileName = GetProgramFileName(),
                    Arguments = fileLocation,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if(!debug) File.Delete(fileLocation);
            string output = process.StandardOutput.ReadToEnd();
            if(debug) Console.WriteLine("\nProgram output:");
            Console.WriteLine(output);
        }
    }
}