using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System;

namespace CharlesOlinerCommandConsole {
    public static class CSharpCompiler {
        public static CompilerErrorCollection Compile(string code, string fileName, string compilerVersion) {

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string[] assemblyNames = new string[assemblies.Length];
            for (int i = assemblies.Length - 1; i >= 0; i--) {
                assemblyNames[i] = assemblies[i].CodeBase;
            }

            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", compilerVersion } });
            var parameters = new CompilerParameters(assemblyNames, fileName, true);
            parameters.GenerateExecutable = false;
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, code);
            return results.Errors;
        }
    }
}
