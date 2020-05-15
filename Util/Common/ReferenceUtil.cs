using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Util.Common
{
    public class ReferenceUtil
    {
        /// <summary>
        /// 获取当前代码的程序集
        /// </summary>
        /// <returns></returns>
        public static Assembly GetDefaultAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// 根据程序集名称获取程序集
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static Assembly GetAssemblyWithName(string assemblyName)
        {
            //如果程序集名称为空，就返回当前代码的程序集
            if (string.IsNullOrEmpty(assemblyName))
            {
                return GetDefaultAssembly();
            }
            //排除名称中的空格
            assemblyName = assemblyName.Trim();
            //尝试加载程序集
            Assembly result = Assembly.Load(assemblyName);
            //如果结果为空，就认为程序集不存在
            if(result == null)
            {
                throw new FileNotFoundException(string.Format("命名空间[{0}]不存在！", assemblyName));
            }
            return result;
        }

        /// <summary>
        /// 根据Dll名称获取程序集
        /// </summary>
        /// <param name="dllName">Dll名称</param>
        /// <returns></returns>
        public static Assembly GetAssemblyWithDll(string dllName)
        {
            //如果Dll名称为空，就返回当前代码的程序集
            if (string.IsNullOrEmpty(dllName))
            {
                return GetDefaultAssembly();
            }
            //排除Dll名称中的空格
            dllName = dllName.Trim();
            //初始化返回值
            Assembly result = null;
            try
            {
                //尝试从文件中加载程序集
                result = Assembly.LoadFrom(dllName);
                //如果结果为空，就认为命名空间不存在
                if (result == null)
                {
                    throw new Exception(string.Format("命名空间[{0}]不存在！", dllName));
                }
            }
            catch (FileNotFoundException)
            {
                //如果发生了文件未找到异常，就认为Dll文件不存在
                throw new FileNotFoundException(string.Format("程序集[{0}]不存在！", dllName));
            }
            catch (Exception)
            {
                //如果发生了其他异常，如：Dll不是CLR托管的Dll等，就直接抛出异常
                throw;
            }
            return result;
        }

        /// <summary>
        /// 根据名称获取程序集
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static Assembly GetAssembly(string assemblyName)
        {
            //如果程序集名称为空，就返回当前代码的程序集
            if (string.IsNullOrEmpty(assemblyName))
            {
                return GetDefaultAssembly();
            }
            //如果程序集名称以Dll结尾，就返回根据Dll名称获取程序集
            else if (assemblyName.Trim().ToLower().EndsWith(".dll"))
            {
                return GetAssemblyWithDll(assemblyName);
            }
            //否则返回根据程序集名称获取程序集
            else
            {
                return GetAssemblyWithName(assemblyName);
            }
        }

        /// <summary>
        /// 获取一个程序集中的一个类型
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">类型名称</param>
        /// <returns></returns>
        public static Type GetType(Assembly assembly,string typeName)
        {
            //初始化返回值
            Type result = null;
            //如果类型名称为空，就认为无法获取，直接返回空
            if (string.IsNullOrEmpty(typeName))
            {
                return result;
            }
            //排除类型名称中的空格
            typeName = typeName.Trim();
            try
            {
                //尝试获取这个类
                //之所以用GetTypes().FirstOrDefault()是因为有时候代码中存在二级文件夹，则名称是
                //SubDir.ClassName这样的，而传入的却是ClassName
                //这时候GetType就会无法识别到SubDir，就会导致报错类型不存在，但其实是存在的
                //用GetTypes可以返回所有文件夹下的ClassName,然后再进行对比则可以消除这个误差
                result = assembly.GetTypes().FirstOrDefault(p => p.FullName == typeName);
                if(result == null)
                {
                    throw new EntryPointNotFoundException(string.Format("程序集[{0}]中不存在类[{1}]！", assembly.FullName, typeName));
                }
            }
            catch (Exception)
            {
                //如果发生异常就认为获取失败了
                throw;
            }
            return result;
        }

        /// <summary>
        /// 根据类型名称获取类型
        /// </summary>
        /// <param name="className">类型名称</param>
        /// <returns></returns>
        public static Type GetType(string className)
        {
            //初始化返回值
            Type result = null;
            //如果类型名称为空就认为无法获取，直接返回空
            if (string.IsNullOrEmpty(className))
            {
                return result;
            }
            //定义一个变量存放程序集的名称
            string assemblyName = string.Empty;
            //如果类型名称包含 ","
            if (className.IndexOf(',') > -1)
            {
                //根据 ","拆分类型名称
                string[] classNameCollection = className.Split(',');
                List<string> assemblyNameList = new List<string>();
                assemblyNameList.AddRange(classNameCollection);
                //重新复制类型名称，因为此时传入的类型名称的格式
                //很有可能是NameSpace.ClassName,NameSpace这样的格式
                className = assemblyNameList[0];
                assemblyNameList.RemoveAt(0);
                //得到程序集名称
                string assemblyFullName = string.Join(",", assemblyNameList.ToArray()).Trim();
                assemblyName = assemblyFullName;
            }
            //排除类型名称中的空格
            className = className.Trim();
            //获取程序集
            Assembly assembly = GetAssembly(assemblyName);
            //获取程序集中的类型
            result = GetType(assembly, className);
            return result;
        }

        /// <summary>
        /// 获取一个类型的实例
        /// </summary>
        /// <param name="className">类型名称</param>
        /// <param name="paras">构造函数所需要的参数</param>
        /// <returns></returns>
        public static object Get(string className,params object[] paras)
        {
            return Get<object>(className, paras);
        }

        /// <summary>
        /// 获取一个类型的实例
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="className">类型名称</param>
        /// <param name="paras">构造函数所需要的参数</param>
        /// <returns></returns>
        public static T Get<T>(string className,params object[] paras) where T:class
        {
            //初始化返回值
            T result = default(T);
            //获取类型
            Type classType = GetType(className);
            //如果获取不到这个类型，就返回实例类型的默认值，一般情况下是Null
            if(classType == null)
            {
                return result;
            }
            //循环获取构造函数需要的参数的所有参数类型
            List<Type> paramTypes = new List<Type>();
            for (int i = 0; i < paras.Length; i++)
            {
                object param = paras[i];
                if(param == null)
                {
                    throw new ArgumentNullException("利用构造函数生成类时，参数不能为Null！", nameof(param));
                }
                Type paramType = param.GetType();
                paramTypes.Add(paramType);
            }
            //根据参数类型和数量确定要获取的构造函数
            ConstructorInfo constructor = classType.GetConstructor(paramTypes.ToArray());
            //通过调用构造函数来实例化类型
            object classObject = constructor.Invoke(paras);
            //将类型强制转化为指定的实例类型
            result = classObject as T;
            return result;
        }
    }
}
