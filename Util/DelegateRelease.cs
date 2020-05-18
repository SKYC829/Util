namespace Util
{
    /// <summary>
    /// 压缩、解压时处理压缩、解压进度的委托方法
    /// </summary>
    /// <param name="fileName">当前文件名</param>
    /// <param name="compressSize">已压缩大小</param>
    /// <param name="totalSize">总大小</param>
    /// <param name="isFolder">当前文件是否是文件夹</param>
    public delegate void CompressDelegateCode(string fileName, uint compressSize, float totalSize, bool isFolder);

    /// <summary>
    /// 一个无参的委托方法
    /// </summary>
    public delegate void SimpleDelegateCode();

    /// <summary>
    /// 一个无参的委托方法
    /// </summary>
    /// <typeparam name="T">委托方法的返回值</typeparam>
    /// <returns></returns>
    public delegate T SimpleDelegateCode<T>();

    /// <summary>
    /// 一个单参数的委托方法
    /// </summary>
    /// <typeparam name="T">参数的类型</typeparam>
    /// <param name="from">传入的参数</param>
    public delegate void SimpleParamDelegateCode<T>(T from);

    /// <summary>
    /// 一个多参数的委托方法
    /// </summary>
    /// <typeparam name="T1">参数的类型</typeparam>
    /// <typeparam name="T2">参数的类型</typeparam>
    /// <param name="param1">参数1</param>
    /// <param name="param2">参数2</param>
    public delegate void MultiParamDelegateCode<T1, T2>(T1 param1, T2 param2);
}
