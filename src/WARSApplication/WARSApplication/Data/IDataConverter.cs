namespace WARSApplication.Data;

public interface IDataConverter<in TI, out TO>
{
    public TO Convert(TI item);
}