namespace MiniHttpServer.Framework.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ControllerAttribute : Attribute
{
    public string? Name { get; }
    public ControllerAttribute(string? name = null) => Name = name;
}
