using SuperSocket.ProtoBase;

namespace client;

public sealed class MyPackage : IKeyedPackageInfo<byte>
{
    public const int HeadLength = 2;
    
    public required byte Key { get; init; }

    public string? Content { get; set; }
}