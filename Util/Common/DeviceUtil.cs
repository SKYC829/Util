using System;
using System.Linq;
using System.Management;
using Util.IO.Log;

namespace Util.Common
{
    /// <summary>
    /// 系统硬件帮助类
    /// <para>可以获取到一些系统信息</para>
    /// </summary>
    public class DeviceUtil
    {
        #region 私有帮助方法
        /// <summary>
        /// 创建一个WMI管理类的实例
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        protected static ManagementClass OpenWMIClass(string className)
        {
            ManagementClass result = new ManagementClass(className);
            return result;
        }

        /// <summary>
        /// 打开一个WMI实例
        /// </summary>
        /// <param name="objectPath"></param>
        /// <returns></returns>
        protected static ManagementObject OpenWMIObject(string objectPath)
        {
            ManagementObject result = new ManagementObject(objectPath);
            return result;
        }

        /// <summary>
        /// 手动释放可以Dispose的资源
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        protected static bool ReleaseResource(IDisposable resource)
        {
            bool result = false;
            try
            {
                resource.Dispose();
                result = true;
            }
            catch (Exception ex)
            {
                LogUtil.WriteException(ex.ToString());
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 获取硬盘的序列号(默认C盘)
        /// </summary>
        /// <param name="diskFlag">硬盘盘符</param>
        /// <returns></returns>
        public static string GetDiskSerialNumber(string diskFlag = "C")
        {
            //打开对应硬盘盘符的WMI实例
            ManagementObject disk = OpenWMIObject(string.Format("win32_logicaldisk.deviceid=\"{0}:\"", diskFlag.Trim(':'))); //为了防止输入错误，导致“：”重复，所以这里把“：” Trim掉
            //将WMI信息绑定到WMI实例对象
            disk.Get();
            //获取这个硬盘的WMI信息中名为VolumeSerialNumber的属性的值
            string result = disk.GetPropertyValue("volumeserialnumber").ToString();
            //手动释放资源，防止资源冗余占用
            ReleaseResource(disk);
            return result;
        }

        /// <summary>
        /// 获取CPU的序列号(默认CPU0)
        /// </summary>
        /// <returns></returns>
        public static string GetCPUSerialNumber()
        {
            return GetCPUSerialNumber(0);
        }

        /// <summary>
        /// 获取CPU的序列号
        /// </summary>
        /// <param name="cpuIndex">CPU编号(俗称第几个CPU)</param>
        /// <returns></returns>
        public static string GetCPUSerialNumber(int cpuIndex)
        {
            //打开对应CPU的WMI实例
            ManagementObject cpu = OpenWMIObject(string.Format("win32_processor.deviceid=\"cpu{0}\"", cpuIndex));
            //将WMI信息绑定到WMI实例
            cpu.Get();
            //获取这个CPU的WMI信息中名为ProcessorId的属性的值
            string result = cpu.GetPropertyValue("processorid").ToString();
            //手动释放资源
            ReleaseResource(cpu);
            return result;
        }

        /// <summary>
        /// 获取主板的序列号
        /// </summary>
        /// <returns></returns>
        public static string GetBaseBoardSerialNumber()
        {
            //打开主板的WMI实例
            ManagementObject baseBoard = OpenWMIObject("win32_baseboard.tag=\"base board\"");
            //将WMI信息绑定到实例上
            baseBoard.Get();
            //获取主板的WMI信息中名为SerialNumber的属性的值
            string result = baseBoard.GetPropertyValue("serialnumber").ToString();
            //手动释放资源
            ReleaseResource(baseBoard);
            return result;
        }

        /// <summary>
        /// 获取主板的品牌名称
        /// </summary>
        /// <returns></returns>
        public static string GetBaseBoardProductName()
        {
            //打开主板的WMI实例
            ManagementObject baseBoard = OpenWMIObject("win32_baseboard.tag=\"base board\"");
            //将WMI信息绑定到实例上
            baseBoard.Get();
            //获取主板的WMI信息中名为Product的属性的值
            string result = baseBoard.GetPropertyValue("product").ToString();
            //手动释放资源
            ReleaseResource(baseBoard);
            return result;
        }

        /// <summary>
        /// 获取当前系统登录的用户名
        /// </summary>
        /// <returns></returns>
        public static string GetSystemUserName()
        {
            //打开对应的WMI实例
            ManagementClass system = OpenWMIClass("win32_computersystem");
            //将WMI信息绑定到WMI实例
            system.Get();
            //获取WMI信息中名为username的属性的值
            string result = system.GetInstances().Cast<ManagementObject>().FirstOrDefault().GetPropertyValue("username").ToString();
            //手动释放资源
            ReleaseResource(system);
            return result;
        }

        /// <summary>
        /// 获取当前操作系统类型，例如：x64-based或x86-based
        /// </summary>
        /// <returns></returns>
        public static string GetSystemType()
        {
            //打开对应的WMI实例
            ManagementClass system = OpenWMIClass("win32_computersystem");
            //将WMI信息绑定到WMI实例
            system.Get();
            //获取WMI信息中名为systemtype的属性的值
            string result = system.GetPropertyValue("systemtype").ToString();
            //手动释放资源
            ReleaseResource(system);
            return result;
        }

        /// <summary>
        /// 获取特殊文件夹路径,等价于<see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/>
        /// </summary>
        /// <param name="specialFolder"><see cref="Environment.SpecialFolder"/></param>
        /// <returns></returns>
        public static string GetSpecialFolder(Environment.SpecialFolder specialFolder)
        {
            return GetSpecialFolder(specialFolder, Environment.SpecialFolderOption.None);
        }

        /// <summary>
        /// 获取特殊文件夹路径，等价于<see cref="Environment.GetFolderPath(Environment.SpecialFolder, Environment.SpecialFolderOption)"/>
        /// </summary>
        /// <param name="specialFolder"></param>
        /// <param name="folderOption"></param>
        /// <returns></returns>
        public static string GetSpecialFolder(Environment.SpecialFolder specialFolder, Environment.SpecialFolderOption folderOption)
        {
            return Environment.GetFolderPath(specialFolder, folderOption);
        }

        /// <summary>
        /// 获取CPU使用率(默认CPU0)
        /// </summary>
        /// <returns></returns>
        public static float GetCPUUsage()
        {
            return GetCPUUsage(0);
        }

        /// <summary>
        /// 获取CPU的使用率
        /// </summary>
        /// <param name="cpuIndex"></param>
        /// <returns></returns>
        public static float GetCPUUsage(int cpuIndex)
        {
            //打开对应CPU的WMI实例
            ManagementObject cpu = OpenWMIObject(string.Format("win32_processor.deviceid=\"cpu{0}\"", cpuIndex));
            //将WMI信息绑定到WMI实例
            cpu.Get();
            //获取这个CPU的WMI信息中名为loadpercentage的属性的值
            float.TryParse(cpu.GetPropertyValue("loadpercentage").ToString(), out float result);
            //手动释放资源
            ReleaseResource(cpu);
            return result;
        }

        /// <summary>
        /// 获取总内存大小,单位GB
        /// </summary>
        /// <returns></returns>
        public static float GetTotalMemorySize()
        {
            float result = 0;

            //获取到所有的内存实例，通常状态下，一根可用的内存条有一个实例
            ManagementClass physicalMemorys = OpenWMIClass("win32_physicalmemory");
            //遍历所有实例，计算总内存
            foreach (ManagementBaseObject memory in physicalMemorys.GetInstances())
            {
                //获取内存实例的WMI信息中名为capacity的属性的值并尝试转换为float
                bool isValue = float.TryParse(memory.GetPropertyValue("capacity").ToString(), out float memoryValue);
                if (!isValue) //如果无法转换则这个内存可能有异常，跳过计算
                {
                    continue;
                }
                result += memoryValue / 1024f / 1024f / 1024f;
            }
            //手动释放资源
            ReleaseResource(physicalMemorys);
            return result;
        }

        /// <summary>
        /// 获取可用内存大小，单位GB
        /// </summary>
        /// <returns></returns>
        public static float GetAvailableMemorySize()
        {
            float result = 0;
            //获取到所有的内存实例，通常状态下，一根可用的内存条有一个实例
            ManagementClass physicalMemorys = OpenWMIClass("win32_perfformatteddata_perfos_memory");
            //遍历所有实例，计算总内存
            foreach (ManagementBaseObject memory in physicalMemorys.GetInstances())
            {
                //获取内存实例的WMI信息中名为availablembytes的属性的值并尝试转换为float
                bool isValue = float.TryParse(memory.GetPropertyValue("availablembytes").ToString(), out float memoryValue);
                if (!isValue) //如果无法转换则这个内存可能有异常，跳过计算
                {
                    continue;
                }
                result += memoryValue / 1024f;
            }
            //手动释放资源
            ReleaseResource(physicalMemorys);
            return result;
        }

        /// <summary>
        /// 获取内存使用率，单位GB
        /// </summary>
        /// <returns></returns>
        public static float GetMemoryUsage()
        {
            float total = GetTotalMemorySize();
            float free = GetAvailableMemorySize();
            return Math.Max(0, total - free);//防止有一处出现异常或计算错误导致结果为负
        }

        /// <summary>
        /// 获取硬盘总大小,单位GB(默认C盘)
        /// </summary>
        /// <param name="diskFlag">硬盘盘符</param>
        /// <returns></returns>
        public static float GetTotalDiskSize(string diskFlag = "C")
        {
            System.IO.DriveInfo disk = new System.IO.DriveInfo(diskFlag);
            float.TryParse(disk.TotalSize.ToString(), out float diskTotalSize);
            float result = diskTotalSize / 1024f / 1024f / 1024f;
            return result;
        }

        /// <summary>
        /// 获取硬盘可用大小，单位GB(默认C盘)
        /// </summary>
        /// <param name="diskFlag">硬盘盘符</param>
        /// <returns></returns>
        public static float GetAvailableDiskSize(string diskFlag = "C")
        {
            System.IO.DriveInfo disk = new System.IO.DriveInfo(diskFlag);
            float.TryParse(disk.TotalFreeSpace.ToString(), out float freeTotalSize);
            float result = freeTotalSize / 1024f / 1024f / 1024f;
            return result;
        }

        /// <summary>
        /// 获取硬盘使用率，单位GB(默认C盘)
        /// </summary>
        /// <param name="diskFlag">硬盘盘符</param>
        /// <returns></returns>
        public static float GetDiskUsage(string diskFlag = "C")
        {
            float total = GetTotalDiskSize(diskFlag);
            float free = GetAvailableDiskSize(diskFlag);
            return Math.Max(0, total - free);//防止有一处出现异常或计算错误导致结果为负
        }
    }
}
