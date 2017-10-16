using System;
using System.Collections.Generic;

namespace DunkLang{
    public class Function{
        public string name;
        public string content;
        public string args = "";
        public int ordering = 0;

        public Function(string name, string content){
            this.name = name;
            this.content = content;
        }

        public Function(string name, string content, string args){
            this.name = name;
            this.content = content;
            this.args = args;
        }
    }
}