namespace CSharpNEAT.Core
{
    public interface IDecoder
    {
        IBlackBox Decode(IGenome genome);
    }
}
