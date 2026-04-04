using System;
using System.Linq;
using System.Reflection;

var asm = Assembly.LoadFrom(@"C:\Users\Sam.Gomez\source\repos\ContentUnderstandingDemo\src\api\ContentUnderstanding.Api\bin\Debug\net10.0\Azure.AI.ContentUnderstanding.dll");
foreach (var t in asm.GetTypes().Where(t => t.Name.Contains("ContentField") || t.Name.Contains("AnalysisContent")).OrderBy(t => t.Name))
{
    Console.WriteLine($"=== {t.FullName} (Base: {t.BaseType?.FullName}) ===");
    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        Console.WriteLine($"  {p.PropertyType.Name} {p.Name}");
}
