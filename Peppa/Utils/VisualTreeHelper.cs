using System.Collections.Generic;
using System.Windows;

namespace Peppa
{
    public class VisualTreeHelper
    {
        /// <summary>
        /// 查找父控件
        /// </summary>
        /// <typeparam name="T">父控件的类型</typeparam>
        /// <param name="obj">要找的是obj的父控件</param>
        /// <param name="name">想找的父控件的Name属性</param>
        /// <returns>目标父控件</returns>
        public static T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject parent = System.Windows.Media.VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)parent;
                }

                // 在上一级父控件中没有找到指定名字的控件，就再往上一级找
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        /// <summary>
        /// 查找子控件
        /// </summary>
        /// <typeparam name="T">子控件的类型</typeparam>
        /// <param name="obj">要找的是obj的子控件</param>
        /// <param name="name">想找的子控件的Name属性</param>
        /// <returns>目标子控件</returns>
        public static T GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            T grandChild = null;

            for (int i = 0; i <= System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)child;
                }
                else
                {
                    // 在下一级中没有找到指定名字的子控件，就再往下一级找
                    grandChild = GetChildObject<T>(child, name);
                    if (grandChild != null)
                        return grandChild;
                }
            }

            return null;

        }

        /// <summary>
        /// 查找子控件
        /// </summary>
        /// <typeparam name="T">子控件的类型</typeparam>
        /// <param name="obj">要找的是obj的子控件</param>
        /// <param name="name">想找的子控件的Name属性</param>
        /// <returns>目标子控件</returns>
        public static object GetChildObjectByTypeName(DependencyObject obj, string typeName)
        {
            DependencyObject child = null;
            object grandChild = null;

            for (int i = 0; i <= System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);

                if (child.GetType().Name == typeName)
                {
                    return child;
                }
                else
                {
                    // 在下一级中没有找到指定名字的子控件，就再往下一级找
                    grandChild = GetChildObjectByTypeName(child, typeName);
                    if (grandChild != null)
                        return grandChild;
                }
            }

            return null;

        }


        /// <summary>
        /// 获取所有同一类型的子控件
        /// </summary>
        /// <typeparam name="T">子控件的类型</typeparam>
        /// <param name="obj">要找的是obj的子控件集合</param>
        /// <param name="name">想找的子控件的Name属性</param>
        /// <returns>子控件集合</returns>
        public static List<T> GetChildObjects<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }

                childList.AddRange(GetChildObjects<T>(child, ""));
            }

            return childList;

        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        public static T GetLogicalTreeObject<T>(DependencyObject obj) where T : DependencyObject
        {
            foreach (var v in LogicalTreeHelper.GetChildren(obj))
            {
                if(v is T)
                {
                    return (T)v;
                }
                if (v is DependencyObject)
                {
                    var value = GetLogicalTreeObject<T>(v as DependencyObject);
                    if (value != null) return value;
                }
            }
            return null;
        }

    }
}
