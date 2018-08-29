
/*********************************************************************************************
 *	
 * 文件名称:    NotificationExtensions.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-8-28 16:50:53
 * 
 * 备    注:    WPF监听扩展
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NotificationExtensions
{
    /// <summary>
    /// wpf扩展，接口INotifyPropertyChanged不需要传入字符串了
    /// </summary>
    public static class NotificationExtensions
    {
        public static void Notify(this PropertyChangedEventHandler eventHandler, Expression<Func<object>> expression)
        {
            if (null == eventHandler)
            {
                return;
            }
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }
            var constantExpression = memberExpression.Expression as ConstantExpression;
            var propertyInfo = memberExpression.Member as PropertyInfo;

            foreach (var del in eventHandler.GetInvocationList())
            {
                del.DynamicInvoke(new object[] { constantExpression.Value, new PropertyChangedEventArgs(propertyInfo.Name) });
            }
        }
    }
}
