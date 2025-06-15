
using EDSDKLib;
using System.Drawing;

namespace EosMonitor
{
    // CreateBitMask: create a new focusPoint object from a given focusPoint parameter "focusPoint"
    public struct FocusPoint
    {
      // Focus point constructor
      internal static FocusPoint Create(EDSDK.EdsFocusPoint focusPoint) {
         return new FocusPoint {
            Bounds = new Rectangle {
               X = focusPoint.rect.x,
               Y = focusPoint.rect.y,
               Height = focusPoint.rect.height,
               Width = focusPoint.rect.width,
            },
            IsInFocus = focusPoint.justFocus != 0,
            IsSelected = focusPoint.selected != 0,
            IsValid = focusPoint.valid != 0,
         };
      }

      // Bounds
      public Rectangle Bounds { get; private set; }

      /// inFocus
      public bool IsInFocus { get; private set; }

      // IsValid
      public bool IsValid { get; private set; }

      // IsSelected
      public bool IsSelected { get; private set; }
    }
}
