

namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// Dapper   MapItem for QueryMultiple
    /// </summary>
    /// <param name="type"></param>
    /// <param name="dataRetriveType"></param>
    /// <param name="propertyName"></param>
    public class MapItem(Type type, DataRetriveType dataRetriveType, string propertyName)
    {
        public Type Type { get; set; } = type;

        public DataRetriveType DataRetriveType { get; set; } = dataRetriveType;

        public string PropertyName { get; set; } = propertyName;
    }

    public enum DataRetriveType
    {
        /// <summary>
        /// single row
        /// </summary>
        FirstOrDefault,

        /// <summary>
        /// multi Rows
        /// </summary>
        List
    }
}
