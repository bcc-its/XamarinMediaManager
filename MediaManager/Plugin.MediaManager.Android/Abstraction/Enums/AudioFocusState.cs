using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Plugin.MediaManager.Abstraction.Enums
{
    public enum AudioFocusState
    {
        NoFocusNoDuck = 0,
        NoFocusCanDuck = 1,
        Focused = 2
    }
}