using System;
using System.Windows;

namespace JobScheduler.Model
{
    public static class WindowExtensions
    {
         public static T Model<T>(this FrameworkElement source)
         {
             if (source == null) throw new ArgumentNullException("source");

             if (source.DataContext  == null)
                 throw new InvalidOperationException("The view is null");

             try
             {
                 var result = (T) source.DataContext;
                 return result;
             }
             catch (InvalidCastException ex)
             {
                 throw new InvalidOperationException(
                    string.Format("The current datacontext is of type {0} (not {1} as expected",
                    source.DataContext.GetType(), typeof(T)));
             }
         }
    }
}