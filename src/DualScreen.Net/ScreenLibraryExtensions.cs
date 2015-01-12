/* ScreenLibraryExtensions.cs - Dual Screen Library Extensions Methods
 *
 * Copyright (C) 2015 Intel Corporation.
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;


namespace DualScreenLibrary.Extensions
{
	public static class DSLExtensions
	{
		public static bool SendToPrimary(this Window window, int offsetX = 0, int offsetY = 0)
		{
			return ScreenManager.SendToPrimary(window, offsetX, offsetY);
		}
		public static bool SendToPrimary(this Window window, ScreenManager.ScreenAlignment align)
		{
			return ScreenManager.SendToPrimary(window, align);
		}
		public static bool SendToSecondary(this Window window, int offsetX = 0, int offsetY = 0)
		{
			return ScreenManager.SendToSecondary(window, offsetX, offsetY);
		}
		public static bool SendToSecondary(this Window window, ScreenManager.ScreenAlignment align)
		{
			return ScreenManager.SendToSecondary(window, align);
		}
		public static bool SwapScreen(this Window window, int offsetX = 0, int offsetY = 0)
		{
			return ScreenManager.SwapScreen(window, offsetX, offsetY);
		}
		public static bool SwapScreen(this Window window, ScreenManager.ScreenAlignment align)
		{
			return ScreenManager.SwapScreen(window, align);
		}
		public static bool SendToScreen(this Window window, Screen screen, int offsetX = 0, int offsetY = 0)
		{
			return ScreenManager.SendToScreen(window, screen, offsetX, offsetY);
		}
		public static bool SendToScreen(this Window window, Screen screen, ScreenManager.ScreenAlignment align)
		{
			return ScreenManager.SendToScreen(window, screen, align);
		}
		public static Screen getScreen(this Window window)
		{
			return ScreenManager.getScreenFromWindow(window);
		}
	}
}
