using ReactorStringDecrypter.Context;

namespace ReactorStringDecrypter
{
    internal interface IDecrypters
    {
        string Information { get; }
        void Decrypt(FileContext ctx);
    }
}