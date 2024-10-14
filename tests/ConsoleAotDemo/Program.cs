using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using ConsoleAotDemo;
using ConsoleAotDemo.Dtos;
using System.Reflection;

DBHeper.Test();

var student = new Person { Name = "帅哥", Property = 100 };
var properties = student.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance);
Console.WriteLine($"帅哥信息如下({properties.Length}条)：\r\n{student}");

var buffer = student.Serialize(1);
Console.WriteLine($"名下：{buffer?.Length}套房");

int index = 0;
if (buffer?.ReadHead(ref index, out NetHeadInfo head) == true)
{
    Console.WriteLine($"哇，原来你是第{head.ObjectVersion}大帅哥");
}
else
{
    Console.WriteLine("抱歉，无法详细了解你的帅");
}

Console.ReadLine();

