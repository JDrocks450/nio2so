namespace nio2so.Data.Common
{
    /// <summary>
    /// A basic interface for Vertex object types.
    /// </summary>
    public interface IGeomVertex { }
    /// <summary>
    /// A common class for Vector3 objects that is platform independent
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Z"></param>
    public record GeomVector3(double X, double Y, double Z);
}
