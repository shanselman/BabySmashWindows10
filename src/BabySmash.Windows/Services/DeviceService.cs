using BabySmash.Core.Services;
using System;
using System.Collections.Generic;

using Windows.System;
using Windows.UI.Xaml;

namespace BabySmash.Windows.Services
{
	public class DeviceService : IDeviceService
	{
		public double GetScreenWidth()
		{
			return Window.Current.CoreWindow.Bounds.Width;
		}

		public double GetScreenHeight()
		{
			return Window.Current.CoreWindow.Bounds.Height;
		}
	}
}
