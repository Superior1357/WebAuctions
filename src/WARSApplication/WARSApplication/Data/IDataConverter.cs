namespace WARSApplication.Data;

/// <summary>
/// Interface for converters that convert data between different representations
/// </summary>
/// <typeparam name="TI"></typeparam>
/// <typeparam name="TO"></typeparam>
public interface IDataConverter<in TI, out TO>
{
    /// <summary>
    /// Converts entity to another representation
    /// </summary>
    /// <param name="entity">input representation</param>
    /// <returns>output representation</returns>
    public TO Convert(TI entity);
}