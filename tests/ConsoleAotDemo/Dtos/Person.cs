using CodeWF.NetWeaver.Base;

namespace ConsoleAotDemo.Dtos;

[NetHead(10, 1)]
public class Person : INetObject
{
    public string? Name { get; set; }

    public decimal Property { get; set; }

    public override string ToString()
    {
        return $"姓名：{Name}\r\n家产：{Property}万￥";
    }
}