using System;
using System.Collections.Generic;
using System.Text;

namespace Учет.Core
{
    public static class AsyncExtensions
    {
        public static async void FireAndForgetSafeAsync(this Task task) { try { await task; } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BG ERROR] {ex}"); } }
    }
}