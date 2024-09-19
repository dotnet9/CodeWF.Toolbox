using CodeWF.NetWeaver;
using CodeWF.NetWeaver.Base;
using ConsoleAotDemo.Dtos;

var student = new Person { Name = "帅哥", Property = 100 };

Console.WriteLine($"帅哥信息如下：\r\n{student}");

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