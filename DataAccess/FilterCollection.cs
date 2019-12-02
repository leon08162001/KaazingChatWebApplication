/// <summary>
/// Enum 的摘要描述。
/// </summary>
public class FilterCollection
{
    /// <summary>
    /// 過濾條件型別
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// 等於
        /// </summary>
        Equal = 1,
        /// <summary>
        /// 小於等於
        /// </summary>
        LessEqual = 2,
        /// <summary>
        /// 大於等於
        /// </summary>
        EqualThan = 3,
        /// <summary>
        /// 不於等
        /// </summary>
        NotEqual = 4,
        /// <summary>
        /// 小於
        /// </summary>
        Less = 5,
        /// <summary>
        /// 大於
        /// </summary>
        Than = 6,
        /// <summary>
        /// Like
        /// </summary>
        Like = 7
    }
}
