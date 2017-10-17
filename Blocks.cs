using System;
using System.Collections.Generic;

namespace DunkLang{
    public class Function{
        public string name;
        public string content;
        public string args;
        public List<Variable> variables;
        public string returns = "";

        public Function(string name, string content){
            this.name = name;
            this.content = content;
            this.args = "";
            GetVariables();
        }

        public Function(string name, string content, string args){
            this.name = name;
            this.content = content;
            this.args = args;
            GetVariables();
        }

        private void GetVariables(){
            variables = new List<Variable>();
            string[] vargs = args.Split(",", StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < vargs.Length; i++){
                vargs[i] = vargs[i].Trim();
                int pos = vargs[i].IndexOf(" ");
                Variable nVar = new Variable(vargs[i].Substring(0, pos), vargs[i].Substring(pos+1));
                variables.Add(nVar);
            }
        }
    }

    public class Variable{
        public string name;
        public string type;

        public Variable(string type, string name){
            this.type = type;
            this.name = name;
        }
    }
}