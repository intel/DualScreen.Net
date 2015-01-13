/* MainWindow.xaml.cs : The Main window of the Demo App
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DualScreenLibrary.Extensions;
using DualScreenLibrary;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Windows.Interop;

namespace WpfWindowTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		DSLEvents events = new DSLEvents();
		public MainWindow()
		{
			InitializeComponent();

			var numOfDisplays = ScreenManager.NumberOfDisplays;
			numOfDisplaysLabel.Content = numOfDisplays;
			if (numOfDisplays < 2)
				status.Content = "Number of displays is less than 2";

		}

		private void clone_Click(object sender, RoutedEventArgs e)
		{
			logStatus(ScreenManager.CloneScreens());
		}
		private void extend_Click(object sender, RoutedEventArgs e)
		{
			logStatus(ScreenManager.ExtendScreens());
		}
		private void internal_Click(object sender, RoutedEventArgs e)
		{
			logStatus(ScreenManager.SetInternal());
		}
		private void external_Click(object sender, RoutedEventArgs e)
		{
			logStatus(ScreenManager.SetExternal());
		}

		private void swapScreen_Click(object sender, RoutedEventArgs e)
		{
			logStatus(this.SwapScreen(getAlignment()));
		}
		private void sendToPrimary_Click(object sender, RoutedEventArgs e)
		{
			logStatus(this.SendToPrimary(getAlignment()));
		}
		private void sendToSecondary_Click(object sender, RoutedEventArgs e)
		{
			logStatus(this.SendToSecondary(getAlignment()));
		}

		private void align_Click(object sender, RoutedEventArgs e)
		{
			var align = getAlignment();
			logStatus(this.SendToScreen(this.getScreen(), align));

			Console.WriteLine(align);
			Console.WriteLine("DPI: " + ScreenManager.getDPI().X + " " + ScreenManager.getDPI().Y);
			Console.WriteLine("DPI Ratio: " + ScreenManager.getDPIRatio().X + " " + ScreenManager.getDPIRatio().Y);
			Console.WriteLine("Offset:" + ScreenManager.AlignmentToOffset(this, this.getScreen(), align));
		}

		private void sendProgWinToScreen_Click(object sender, RoutedEventArgs e)
		{
			if ((bool)forceExtend.IsChecked && ScreenManager.CurrentTopology != ScreenManager.DisplayConfigTopology.Extend)
			{
				var result = ScreenManager.ExtendScreens();
				logStatus(result);
				if (!result)
					return;
			}
			new DelayedActionOnWindow(this, () => logStatus(
				ScreenManager.SendProgramsWindowsToScreen(progNameTextBox.Text,
				((bool)sendProgToSecondary.IsChecked) ? ScreenManager.SecondaryScreen : ScreenManager.PrimaryScreen,
				getAlignment()) )
			).Invoke();
		}
		private void sendProgMainWinToScreen_Click(object sender, RoutedEventArgs e)
		{
			if ((bool)forceExtend.IsChecked && ScreenManager.CurrentTopology != ScreenManager.DisplayConfigTopology.Extend)
			{
				var result = ScreenManager.ExtendScreens();
				logStatus(result);
				if (!result)
					return;
			}
			new DelayedActionOnWindow(this, () => logStatus(
				ScreenManager.SendProgramsMainWindowToScreen(progNameTextBox.Text,
				((bool)sendProgToSecondary.IsChecked) ? ScreenManager.SecondaryScreen : ScreenManager.PrimaryScreen,
				getAlignment()))
			).Invoke();
		}

		private void numOfDisplay_Click(object sender, RoutedEventArgs e)
		{
			numOfDisplaysLabel.Content = ScreenManager.NumberOfDisplays;
			logStatus(true);
		}


		private void queryScreensBtn_Click(object sender, RoutedEventArgs e)
		{
			foreach (var screen in Screen.AllScreens)
			{
				// For each screen, add the screen properties to a list box.
				Console.WriteLine("Device Name: " + screen.DeviceName);
				Console.WriteLine("Bounds: " + screen.Bounds.ToString());
				Console.WriteLine("Type: " + screen.GetType().ToString());
				Console.WriteLine("Working Area: " + screen.WorkingArea.ToString());
				Console.WriteLine("Primary Screen: " + screen.Primary.ToString());
			}

			IntPtr[] hwnds = ScreenManager.getWindowHandlesFromScreen(ScreenManager.SecondaryScreen);

			foreach (var hwnd in hwnds)
				ScreenManager.SendToScreen(hwnd, ScreenManager.PrimaryScreen, getAlignment());
		}


		private ScreenManager.ScreenAlignment getAlignment()
		{
			ScreenManager.ScreenAlignment align = ScreenManager.ScreenAlignment.None;
			if ((bool)center.IsChecked)
				align |= ScreenManager.ScreenAlignment.Center;
			if ((bool)left.IsChecked)
				align |= ScreenManager.ScreenAlignment.Left;
			if ((bool)right.IsChecked)
				align |= ScreenManager.ScreenAlignment.Right;
			if ((bool)top.IsChecked)
				align |= ScreenManager.ScreenAlignment.Top;
			if ((bool)bottom.IsChecked)
				align |= ScreenManager.ScreenAlignment.Bottom;

			return align;
		}

		public void removePlaceHolderForProgName(object sender, RoutedEventArgs e)
		{
			 progNameTextBox.Text = "";
		}

		public void addPlaceHolderForProgName(object sender, RoutedEventArgs e)
		{
			 if(progNameTextBox.Text == "")
				progNameTextBox.Text = "(ex: chrome)";
		}

		private void logStatus(bool result)
		{
			status.Content = (result) ? "ok" : "bad";
		}

		private void SendFromSecondary_Click(object sender, RoutedEventArgs e)
		{
			if (ScreenManager.CurrentTopology != ScreenManager.DisplayConfigTopology.Extend)
			{
				if ((bool)forceExtend.IsChecked)
				{
					var result = ScreenManager.ExtendScreens();
					logStatus(result);
					if (result)
						return;
				}
				else
				{
					logStatus(false);
					return;
				}
			}

			new DelayedActionOnWindow(this, () => { IntPtr[] hwnds = ScreenManager.getWindowHandlesFromScreen(ScreenManager.SecondaryScreen);

			logStatus((from hwnd in hwnds
					   select ScreenManager.SendToScreen(hwnd, ScreenManager.PrimaryScreen, getAlignment()))
						.All(x => x == true)
					);
			}).Invoke();
		}
	}
}
