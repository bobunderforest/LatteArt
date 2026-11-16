namespace LatteArt.CoffeeTools
{
    public interface IToolActivity
    {
        bool IsActive { get; }
        float Intensity { get; }
    }
}
