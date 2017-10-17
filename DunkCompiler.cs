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
        public List<Variable> variables = new List<Variable>();
        public Function currentFunction;
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

        public Function[] ReorderFunctions(Function[] functions){
            if(debug) Console.WriteLine("Reordering functions.");
            for (int i = 0; i < functions.Length; i++){
                for (int j = 0; j < i; j++){
                    if(functions[j].content.Contains(functions[i].name)){
                        Function temp = functions[j];
                        functions[j] = functions[i];
                        functions[i] = temp;
                        if(debug){
                            for (int t = 0; t < functions.Length; t++){
                                Console.Write(functions[t].name);
                                if(t != functions.Length-1) Console.Write(" -> ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
            return functions;
        }

        public Variable GetLocalVariable(string name){
            Variable varb = variables.Find((Variable x) => {return x.name.Equals(name);});
            if(varb == null){
                varb = currentFunction.variables.Find((Variable x) => {return x.name.Equals(name);});
            }
            return varb;
        }

        public string GetBracketedValue(string str){
            return str.Substring(str.IndexOf("(")+1, str.IndexOf(")") - str.IndexOf("(")-1);
        }

        public string TranspileOp(string op){
            string trans = "";
            int value;
            if(op.StartsWith("WriteLine")){
                string bracketed = GetBracketedValue(op);
                trans += "printf(";
                if(bracketed.StartsWith("\"") && bracketed.EndsWith("\"")){
                    trans += bracketed;
                } else{
                    Variable varb = GetLocalVariable(bracketed);
                    if(varb != null){
                        if(varb.type == "int"){
                            trans += "\"%d\\n\", " + varb.name;
                        } else if(varb.type == "float"){
                            trans += "\"%f\\n\", " + varb.name;
                        }
                    } else{
                        
                    }
                }
                trans += ")";
            } else if(int.TryParse(op, out value)){
                trans += value.ToString();
            }
            return trans;
        }

        public string TranspileCommand(string line){
            string trans = "";
            if(line.Contains(" = ")){
                trans += line;
                int pos = line.IndexOf(" ");
                int pos2 = line.IndexOf(" ", pos+1) > line.IndexOf("=", pos+1) ? line.IndexOf("=", pos+1) : line.IndexOf(" ", pos+1);
                Variable nVar = new Variable(line.Substring(0, pos), line.Substring(pos+1, pos2-pos-1));
                //if(debug) Console.WriteLine("foundvar-> " + nVar.type + " : " + nVar.name);
                variables.Add(nVar);
            } else{
                //Example: Power(a, 2) -> WriteLine();
                //Target: printf((a*a));
                //Algorithm: (a*a) => printf("%d\n", (a*a))  
                string[] ops = line.Split("->", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ops.Length; i++){
                    ops[i] = ops[i].Trim();
                    ops[i] = ops[i].Trim(';');
                    //Console.WriteLine(ops[i] + ", " + (GetLocalVariable(ops[i]) != null) + ", i: " + i);
                    if(i == 0){
                        trans += TranspileOp(ops[i]);
                    } else if(GetLocalVariable(ops[i]) != null){
                        trans = trans.Insert(0, (ops[i] + " = "));
                        continue;
                    }
                }
                if(ops.Length != 0){
                    trans += ";";
                }
            }
            return trans;
        }

        public void Transpile(){
            Function[] functions = GetFunctions();
            ReorderFunctions(functions);
            transpiled = "#include<stdio.h>\n\n";
            for (int i = 0; i < functions.Length; i++){
                currentFunction = functions[i];
                if(functions[i].name == "Entry"){
                    transpiled += "int main(int argc, char *argv[]){\n";
                } else{
                    if(functions[i].returns == ""){
                        transpiled += "void ";
                    } else{
                        transpiled += functions[i].returns + " ";
                    }
                    transpiled += functions[i].name + "(" + functions[i].args + "){\n";
                }
                string[] lines = functions[i].content.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                int depth = 1;
                for (int j = 0; j < lines.Length; j++){
                    string line = lines[j].Trim();
                    if(j != 0){
                        transpiled += "\n";
                    }
                    for (int t = 0; t < depth; t++){
                        transpiled += "    ";
                    }
                    if(line.Contains("if")){
                        transpiled += "if(";
                        string bracketed = GetBracketedValue(line);
                        transpiled += bracketed + "){";
                        depth++;
                    } else if(line.Contains("}")){
                        transpiled = transpiled.Remove(transpiled.Length-4);
                        transpiled += "}";
                        depth--;
                    } else{
                        transpiled += TranspileCommand(line);
                    }
                }
                transpiled += "\n}\n\n";
            }
            if(debug){
                Console.WriteLine();
                Console.WriteLine("------Compiled to------");
                Console.WriteLine(transpiled);
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