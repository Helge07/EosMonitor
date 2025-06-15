
using EDSDKLib;
using System.Drawing;

namespace EosMonitor
{
   public struct Focus
   {
      // CreateBitMask: create a new focus object from a focus information parameter "focus"
      internal static Focus Create(EDSDK.EdsFocusInfo focus) {
         // create a FocusPoint array
         var focusPoints = new FocusPoint[focus.pointNumber];
         for (var i = 0; i < focusPoints.Length; ++i)
            focusPoints[i] = FocusPoint.Create(focus.focusPoint[i]);

         // create and return a new FocusInformation object from FocusInfo parameter "focus"
         return new Focus {
            Bounds = new Rectangle {
               X = focus.imageRect.x,
               Y = focus.imageRect.y,
               Height = focus.imageRect.height,
               Width = focus.imageRect.width,
            },
            ExecuteMode = focus.executeMode,
            FocusPoints = focusPoints
         };
      }

      // FocusInformation rectangle
      public Rectangle Bounds { get; private set; }

      // Execution mode
      public long ExecuteMode { get; private set; }

      // FocusInformation points
      public FocusPoint[] FocusPoints { get; private set; }
   }
}
