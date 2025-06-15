
using System;
using System.Windows;
using System.Windows.Threading;

namespace EosMonitor
{
   public static class ExtensionMethods
   {
      // Refresh:  refresh a single element of the MainWindow
      private static Action EmptyDelegate = delegate() { };

      public static void Refresh(this UIElement uiElement) 
      {
         uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
      }
   }
}
