namespace Util.IO.Log
{
    /// <summary>
    /// 日志类型
    /// <para>注意</para>	
    /// <para>后续新增类型的值必须为上一个类型的2倍递增关系</para>
	/// <para>否则会导致输出时跳过输出类型的算法错误</para>
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 无，如果记录时类型为此项，将会记录到Normal中
        /// </summary>
        None = 2,
        /// <summary>
        /// 普通日志
        /// <para>如:</para>
        /// <para>操作步骤</para>
        /// <para>一般信息输出等</para>
        /// </summary>
        Normal = 4,
        /// <summary>
        /// 警告日志
        /// <para>如:</para>
        /// <para>可能导致后续其他功能计算发生异常的步骤记录，信息输出</para>
        /// </summary>
        Warn = 8,
        /// <summary>
        /// 异常日志
        /// <para>导致程序发生异常的错误信息</para>
        /// </summary>
        Exception = 16
    }
}
