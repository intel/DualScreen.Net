/* ScreenManager.cs - Defines a set of methods for managing the windows on different screens.
 *
 * Copyright (C) 2015 Intel Corporation.
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;

namespace DualScreenLibrary
{
	/// <summary>
	/// Defines a set of methods for managing the windows on different screens.
	/// </summary>
	public static class ScreenManager
	{
		#region Constants
		internal const string DLL_PATH = "Intel.FlexServices.DisplayHelperLib.dll";
		// The WM_GETMINMAXINFO message is sent to a window when the size or
		// position of the window is about to change.
		// An application can use this message to override the window's
		// default maximized size and position, or its default minimum or
		// maximum tracking size.
		//	public const int WM_GETMINMAXINFO = 0x0024;
		//	public const int WM_DISPLAYCHANGE = 0x007E;
		// Constants used with MonitorFromWindow()
		/*	public enum MonitorDefaults : uint
			{
				MONITOR_DEFAULTTONULL = 0,      //Returns NULL.
				MONITOR_DEFAULTTOPRIMARY = 1,   //Returns a handle to the primary display monitor.
				MONITOR_DEFAULTTONEAREST = 2,   // Returns a handle to the display monitor that is nearest to the window.
			}*/
		#endregion
		#region Native Structs
		/// <summary>
		/// Native Windows API-compatible POINT struct
		/// </summary>
		[Serializable, StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int X;
			public int Y;
		}
		/// <summary>
		/// The RECT structure defines the coordinates of the upper-left
		/// and lower-right corners of a rectangle.
		/// </summary>
		/// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
		/// <remarks>
		/// By convention, the right and bottom edges of the rectangle
		/// are normally considered exclusive.
		/// In other words, the pixel whose coordinates are ( right, bottom )
		/// lies immediately outside of the the rectangle.
		/// For example, when RECT is passed to the FillRect function, the rectangle
		/// is filled up to, but not including,
		/// the right column and bottom row of pixels. This structure is identical
		/// to the RECTL structure.
		/// </remarks>
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			/// <summary>
			/// The x-coordinate of the upper-left corner of the rectangle.
			/// </summary>
			public int Left;

			/// <summary>
			/// The y-coordinate of the upper-left corner of the rectangle.
			/// </summary>
			public int Top;

			/// <summary>
			/// The x-coordinate of the lower-right corner of the rectangle.
			/// </summary>
			public int Right;

			/// <summary>
			/// The y-coordinate of the lower-right corner of the rectangle.
			/// </summary>
			public int Bottom;
		}

		/// <summary>
		/// The MINMAXINFO structure contains information about a window's
		/// maximized size and position and its minimum and maximum tracking size.
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/ms632605%28VS.85%29.aspx"/>
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct MINMAXINFO
		{
			/// <summary>
			/// Reserved; do not use.
			/// </summary>
			public POINT Reserved;

			/// <summary>
			/// Specifies the maximized width (POINT.x)
			/// and the maximized height (POINT.y) of the window.
			/// For top-level windows, this value
			/// is based on the width of the primary monitor.
			/// </summary>
			public POINT MaxSize;

			/// <summary>
			/// Specifies the position of the left side
			/// of the maximized window (POINT.x)
			/// and the position of the top
			/// of the maximized window (POINT.y).
			/// For top-level windows, this value is based
			/// on the position of the primary monitor.
			/// </summary>
			public POINT MaxPosition;

			/// <summary>
			/// Specifies the minimum tracking width (POINT.x)
			/// and the minimum tracking height (POINT.y) of the window.
			/// This value can be obtained programmatically
			/// from the system metrics SM_CXMINTRACK and SM_CYMINTRACK.
			/// </summary>
			public POINT MinTrackSize;

			/// <summary>
			/// Specifies the maximum tracking width (POINT.x)
			/// and the maximum tracking height (POINT.y) of the window.
			/// This value is based on the size of the virtual screen
			/// and can be obtained programmatically
			/// from the system metrics SM_CXMAXTRACK and SM_CYMAXTRACK.
			/// </summary>
			public POINT MaxTrackSize;
		}

		/// <summary>
		/// The WINDOWINFO structure contains window information.
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/ms632610%28VS.85%29.aspx"/>
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWINFO
		{
			/// <summary>
			/// The size of the structure, in bytes.
			/// The caller must set this to sizeof(WINDOWINFO).
			/// </summary>
			public uint Size;

			/// <summary>
			/// Pointer to a RECT structure
			/// that specifies the coordinates of the window.
			/// </summary>
			public RECT Window;

			/// <summary>
			/// Pointer to a RECT structure
			/// that specifies the coordinates of the client area.
			/// </summary>
			public RECT Client;

			/// <summary>
			/// The window styles. For a table of window styles,
			/// <see cref="http://msdn.microsoft.com/en-us/library/ms632680%28VS.85%29.aspx">
			/// CreateWindowEx
			/// </see>.
			/// </summary>
			public uint Style;

			/// <summary>
			/// The extended window styles. For a table of extended window styles,
			/// see CreateWindowEx.
			/// </summary>
			public uint ExStyle;

			/// <summary>
			/// The window status. If this member is WS_ACTIVECAPTION,
			/// the window is active. Otherwise, this member is zero.
			/// </summary>
			public uint WindowStatus;

			/// <summary>
			/// The width of the window border, in pixels.
			/// </summary>
			public uint WindowBordersWidth;

			/// <summary>
			/// The height of the window border, in pixels.
			/// </summary>
			public uint WindowBordersHeight;

			/// <summary>
			/// The window class atom (see
			/// <see cref="http://msdn.microsoft.com/en-us/library/ms633586%28VS.85%29.aspx">
			/// RegisterClass
			/// </see>).
			/// </summary>
			public ushort WindowType;

			/// <summary>
			/// The Windows version of the application that created the window.
			/// </summary>
			public ushort CreatorVersion;
		}

		/// <summary>
		/// The MONITORINFO structure contains information about a display monitor.
		/// The GetMonitorInfo function stores information in a MONITORINFO structure.
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/dd145065%28VS.85%29.aspx"/>
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct MONITORINFO
		{
			/// <summary>
			/// The size, in bytes, of the structure. Set this member
			/// to sizeof(MONITORINFO) (40) before calling the GetMonitorInfo function.
			/// Doing so lets the function determine
			/// the type of structure you are passing to it.
			/// </summary>
			public int Size;

			/// <summary>
			/// A RECT structure that specifies the display monitor rectangle,
			/// expressed in virtual-screen coordinates.
			/// Note that if the monitor is not the primary display monitor,
			/// some of the rectangle's coordinates may be negative values.
			/// </summary>
			public RECT Monitor;

			/// <summary>
			/// A RECT structure that specifies the work area rectangle
			/// of the display monitor that can be used by applications,
			/// expressed in virtual-screen coordinates.
			/// Windows uses this rectangle to maximize an application on the monitor.
			/// The rest of the area in rcMonitor contains system windows
			/// such as the task bar and side bars.
			/// Note that if the monitor is not the primary display monitor,
			/// some of the rectangle's coordinates may be negative values.
			/// </summary>
			public RECT WorkArea;

			/// <summary>
			/// The attributes of the display monitor.
			///
			/// This member can be the following value:
			/// 1 : MONITORINFOF_PRIMARY
			/// </summary>
			public uint Flags;
		}
		#endregion
		#region Imported methods
		/// <summary>
		/// The GetWindowInfo function retrieves information about the specified window.
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/ms633516%28VS.85%29.aspx"/>
		/// </summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="pwi">The reference to WINDOWINFO structure.</param>
		/// <returns>true on success</returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

		/// <summary>
		/// The MonitorFromWindow function retrieves a handle to the display monitor
		/// that has the largest area of intersection with the bounding rectangle
		/// of a specified window.
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/dd145064%28VS.85%29.aspx"/>
		/// </summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="dwFlags">Determines the function's return value
		/// if the window does not intersect any display monitor.</param>
		/// <returns>
		/// Monitor HMONITOR handle on success or based on dwFlags for failure
		/// </returns>
		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		/// <summary>
		/// The GetMonitorInfo function retrieves information about a display monitor
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/dd144901%28VS.85%29.aspx"/>
		/// </summary>
		/// <param name="hMonitor">A handle to the display monitor of interest.</param>
		/// <param name="lpmi">
		/// A pointer to a MONITORINFO structure that receives information
		/// about the specified display monitor.
		/// </param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
		#endregion
		#region DisplayHelperLib Declerations
		[DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void GetCurrentTopology(ref uint currentTopology);

		[DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SetCurrentTopology(ref uint currentTopology);

		[DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void GetNumberofDisplays(ref uint numDisplays);

		/// <summary>
		/// Matches the DISPLAYCONFIG_TOPOLOGY_ID, enumeration specifies the type of display topology.
		/// For more info: http://msdn.microsoft.com/en-us/library/windows/hardware/ff554001(v=vs.85).aspx
		/// </summary>
		public enum DisplayConfigTopology : uint
		{
			Internal = 0x00000001,      //DISPLAYCONFIG_TOPOLOGY_INTERNAL
			Clone = 0x00000002,         //DISPLAYCONFIG_TOPOLOGY_CLONE
			Extend = 0x00000004,        //DISPLAYCONFIG_TOPOLOGY_EXTEND
			External = 0x00000008,      //DISPLAYCONFIG_TOPOLOGY_EXTERNAL
		}

		public enum ScreenAlignment
		{
			None = 0x00000000,
			Center = 0x00000001,
			Left = 0x00000002,
			Right = 0x00000004,
			Top = 0x00000008,
			Bottom = 0x00000010,
		}
		#endregion
		#region Public Properties
		/// <summary>
		/// will return the first screen which is not a primary screen or null if there is only one screen
		/// this will work assuming the first screen that is not the primary screen is the second screen
		/// 
		/// works only on extended mode
		/// </summary>
		public static Screen SecondaryScreen
		{
			get
			{
				foreach (Screen screen in Screen.AllScreens)
					if (!screen.Primary)
						return screen;
				return null;
			}
		}
		/// <summary>
		/// will return the primary screen
		/// </summary>
		public static Screen PrimaryScreen
		{
			get
			{
				return Screen.PrimaryScreen;
			}
		}
		/// <summary>
		/// DisplayConfigTopology of current state
		/// </summary>
		/// <exception cref="Exception">unable to set/get current topology</exception>
		public static DisplayConfigTopology CurrentTopology
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				uint curState = 0;
				GetCurrentTopology(ref curState);
				DisplayConfigTopology topology = (DisplayConfigTopology)curState;

				if (topology == 0)
					throw new Exception("Error: unable to get the current topology");
				return topology;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				uint uintState = (uint)value;
				if (!SetCurrentTopology(ref uintState))
					throw new Exception("Error: unable to set the current topology");
			}
		}
		/// <summary>
		/// The number of displays, even on clone or internal/external
		/// </summary>
		public static uint NumberOfDisplays
		{
			get
			{
				uint numDisplays = 0;
				GetNumberofDisplays(ref numDisplays);
				return numDisplays;
			}
		}
		#endregion
		#region Public Functions
		#region WPF windows
		/// <summary>
		/// Sends a window to the primary screen
		/// </summary>
		/// <param name="window">to send to primary screen</param>
		/// <param name="align">Screen alignment</param>
		/// <param name="extend">change screen topology to extended if
		///	current topology is not extend. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure</returns>
		public static bool SendToPrimary(Window window, ScreenAlignment align)
		{
			return SendToScreen(window, PrimaryScreen, align);
		}
		/// <summary>
		/// Sends a window to the primary screen
		/// </summary>
		/// <param name="window">to send to primary screen</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <param name="extend">change screen topology to extended. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure</returns>
		public static bool SendToPrimary(Window window, int offsetX = 0, int offsetY = 0)
		{
			return SendToScreen(window, PrimaryScreen, offsetX, offsetY);
		}
		/// <summary>
		/// Sends a window to the secondary screen.
		/// </summary>
		/// <param name="window">to send to secondary screen</param>
		/// <param name="align">Screen alignment</param>
		/// <param name="extend">change screen topology to extended. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SendToSecondary(Window window, ScreenAlignment align)
		{
			return SendToScreen(window, SecondaryScreen, align);
		}
		/// <summary>
		/// Sends a window to the secondary screen.
		/// </summary>
		/// <param name="window">to send to secondary screen</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <param name="extend">change screen topology to extended if
		///	current topology is not extend. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SendToSecondary(Window window, int offsetX = 0, int offsetY = 0)
		{
			return SendToScreen(window, SecondaryScreen, offsetX, offsetY);
		}
		/// <summary>
		/// Sends a window to from primary screen to secondary or from secondary screen to primary screen.
		/// </summary>
		/// <param name="window">to send</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <param name="extend">change screen topology to extended. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SwapScreen(Window window, int offsetX = 0, int offsetY = 0)
		{
			return SendToScreen(window, getOtherScreen(window), offsetX, offsetY);
		}
		/// <summary>
		/// Sends a window to from primary screen to secondary or from secondary screen to primary screen.
		/// </summary>
		/// <param name="window">to send</param>
		/// <param name="align">Screen alignment</param>
		/// <param name="extend">change screen topology to extended if
		///	current topology is not extend. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SwapScreen(Window window, ScreenAlignment align)
		{
			return SendToScreen(window, getOtherScreen(window), align);
		}
		/// <summary>
		/// Sends a window to a screen
		/// </summary>
		/// <param name="window">to send to screen</param>
		/// <param name="screen">screen to send the window to</param>
		/// <param name="align">Screen alignment</param>
		/// <returns>bool to indicate success and failure.</returns>
		public static bool SendToScreen(Window window, Screen screen, ScreenAlignment align)
		{
			if (screen == null) return false;
			var maybe_offset = AlignmentToOffset(window, screen, align);
			if (maybe_offset == null) return false;
			Point offset = (Point)maybe_offset;
			return SendToScreen(window, screen, (int)offset.X, (int)offset.Y);
		}
		/// <summary>
		/// Sends a window to a screen
		/// </summary>
		/// <param name="window">to send to screen</param>
		/// <param name="screen">screen to send the window to</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <returns>bool to indicate success and failure.</returns>
		public static bool SendToScreen(Window window, Screen screen, int offsetX = 0, int offsetY = 0)
		{
			lock (window)
			{
				if (screen == null)
					return false;

				// can't send when window is maximized
				WindowState winState = window.WindowState;
				if (winState == WindowState.Maximized)
					window.WindowState = WindowState.Normal;

				var dpiRatio = getDPIRatio();

				window.Left = (screen.WorkingArea.Left + offsetX)/dpiRatio.X;
				window.Top = (screen.WorkingArea.Top + offsetY)/dpiRatio.Y;

				if (winState == WindowState.Maximized)
					window.WindowState = WindowState.Maximized;
				return true;
			}
		}
		#endregion
		#region hwnds
		/// <summary>
		/// Sends a window to the primary screen
		/// </summary>
		/// <param name="hwnd">window handle to send to primary screen</param>
		/// <param name="align">Screen alignment</param>
		/// <param name="extend">change screen topology to extended if
		///	current topology is not extend. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure</returns>
		public static bool SendToPrimary(IntPtr hwnd, ScreenAlignment align)
		{
			Point? maybe_offset = AlignmentToOffset(hwnd, PrimaryScreen, align);
			if (maybe_offset == null) return false;
			var offset = (Point)maybe_offset;
			return SendToPrimary(hwnd, (int)offset.X, (int)offset.Y);
		}
		/// <summary>
		/// Sends a window to the primary screen
		/// </summary>
		/// <param name="hwnd">window handle to send to primary screen</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <param name="extend">change screen topology to extended. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure</returns>
		public static bool SendToPrimary(IntPtr hwnd, int offsetX = 0, int offsetY = 0)
		{
			return SendToScreen(hwnd, PrimaryScreen, offsetX, offsetY);
		}
		/// <summary>
		/// Sends a window to the secondary screen.
		/// </summary>
		/// <param name="hwnd">Window handle to send to secondary screen</param>
		/// <param name="align">Screen alignment</param>
		/// <param name="extend">change screen topology to extended. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SendToSecondary(IntPtr hwnd, ScreenAlignment align)
		{
			return SendToScreen(hwnd, SecondaryScreen, align);
		}
		/// <summary>
		/// Sends a window to the secondary screen.
		/// </summary>
		/// <param name="hwnd">Window handle to send to secondary screen</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <param name="extend">change screen topology to extended if
		///	current topology is not extend. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SendToSecondary(IntPtr hwnd, int offsetX = 0, int offsetY = 0)
		{
			return SendToScreen(hwnd, SecondaryScreen, offsetX, offsetY);
		}
		/// <summary>
		/// Sends a window to from primary screen to secondary or from secondary screen to primary screen.
		/// </summary>
		/// <param name="hwnd">Window handle to send</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <param name="extend">change screen topology to extended. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SwapScreen(IntPtr hwnd, int offsetX = 0, int offsetY = 0)
		{
			return SendToScreen(hwnd, getOtherScreen(hwnd), offsetX, offsetY);
		}
		/// <summary>
		/// Sends a window to from primary screen to secondary or from secondary screen to primary screen.
		/// </summary>
		/// <param name="hwnd">Window handle to send</param>
		/// <param name="align">Screen alignment</param>
		/// <param name="extend">change screen topology to extended if
		///	current topology is not extend. if current topology is not on extend the function will fail</param>
		/// <returns>bool to indicate success and failure. failure will denote when 
		/// there aren't enough displays or the current screen topology is not extend
		/// and the extend mark is not set to true</returns>
		public static bool SwapScreen(IntPtr hwnd, ScreenAlignment align)
		{
			return SendToScreen(hwnd, getOtherScreen(hwnd), align);
		}
		/// <summary>
		/// Sends a window to a screen
		/// </summary>
		/// <param name="hwnd">window handle</param>
		/// <param name="screen">screen to send the window to</param>
		/// <param name="align">Screen alignment</param>
		/// <returns>bool to indicate success and failure.</returns>
		public static bool SendToScreen(IntPtr hwnd, Screen screen, ScreenAlignment align)
		{
			var maybe_point = AlignmentToOffset(hwnd, screen, align);
			if (maybe_point == null) return false;
			var point = (Point)maybe_point;
			return SendToScreen(hwnd, screen, (int)point.X, (int)point.Y);
		}
		/// <summary>
		/// Sends a window to a screen
		/// </summary>
		/// <param name="hwnd">window handle</param>
		/// <param name="screen">screen to send the window to</param>
		/// <param name="offsetX">x offset on the screen</param>
		/// <param name="offsetY">y offset on the screen</param>
		/// <returns>bool to indicate success and failure.</returns>
		public static bool SendToScreen(IntPtr hwnd, Screen screen, int offsetX = 0, int offsetY = 0)
		{
			if (screen == null)
				return false;

			WINDOWINFO winInfo = new WINDOWINFO();
			GetWindowInfo(hwnd, ref winInfo);

			int windowWidth = winInfo.Window.Right - winInfo.Window.Left;
			int windowHeight = winInfo.Window.Bottom - winInfo.Window.Top;

			var dpiRatio = getDPIRatio();
			double x = screen.WorkingArea.Left + offsetX;
			double y = screen.WorkingArea.Top + offsetY;
			if (1.45 < dpiRatio.X && dpiRatio.X < 1.55 &&
				1.45 < dpiRatio.Y && dpiRatio.Y < 1.55)
			{
				x /= dpiRatio.X;
				y /= dpiRatio.Y;
			}

			return MoveWindow(hwnd, (int)x, (int)y, windowWidth, windowHeight, false);
		}
		#endregion
		#region Other
		/// <summary>
		/// Extend screens
		/// </summary>
		/// <returns>success or failure</returns>
		public static bool ExtendScreens()
		{
			if (NumberOfDisplays < 2)
				return false;
			if (CurrentTopology != DisplayConfigTopology.Extend)
				CurrentTopology = DisplayConfigTopology.Extend;

			return (CurrentTopology == DisplayConfigTopology.Extend);
		}
		/// <summary>
		/// Clone screens
		/// </summary>
		/// <returns>success or failure</returns>
		public static bool CloneScreens()
		{
			if (NumberOfDisplays < 2)
				return false;
			if (CurrentTopology != DisplayConfigTopology.Clone)
				CurrentTopology = DisplayConfigTopology.Clone;

			return (CurrentTopology == DisplayConfigTopology.Clone);
		}
		/// <summary>
		/// Change topology to Internal
		/// </summary>
		/// <returns>success or failure</returns>
		public static bool SetInternal()
		{
			if (NumberOfDisplays < 2)
				return false;
			if (CurrentTopology != DisplayConfigTopology.Internal)
				CurrentTopology = DisplayConfigTopology.Internal;

			return (CurrentTopology == DisplayConfigTopology.Internal);
		}
		/// <summary>
		/// Set topology as External
		/// </summary>
		/// <returns>success or failure</returns>
		public static bool SetExternal()
		{
			if (NumberOfDisplays < 2)
				return false;
			if (CurrentTopology != DisplayConfigTopology.External)
				CurrentTopology = DisplayConfigTopology.External;

			return (CurrentTopology == DisplayConfigTopology.External);
		}
		/// <summary>
		/// Retrieves a Screen for the display that contains the largest portion of the window
		/// </summary>
		/// <param name="window">WPF window</param>
		/// <returns>Screen</returns>
		public static Screen getScreenFromWindow(Window window)
		{
			return getScreenFromWindow(new WindowInteropHelper(window).Handle);
		}
		/// <summary>
		/// Retrieves a Screen for the display that contains the largest portion of the window
		/// </summary>
		/// <param name="hwnd">window handle</param>
		/// <returns>Screen</returns>
		public static Screen getScreenFromWindow(IntPtr hwnd)
		{
			return Screen.FromHandle(hwnd);
		}
		/// <summary>
		/// Converts ScreenAlignment to Offset. alignment depends on the window and the screen
		/// </summary>
		/// <param name="window">WPF window</param>
		/// <param name="screen">Windows.Forms.Screen</param>
		/// <param name="alignment"></param>
		/// <returns>offset</returns>
		public static Point? AlignmentToOffset(Window window, Screen screen, ScreenAlignment alignment)
		{
			if (screen == null)
				return null;

			// can't send when window is maximized
			WindowState winState = window.WindowState;
			if (winState == WindowState.Maximized)
				window.WindowState = WindowState.Normal;

			var dpiRatio = getDPIRatio();

			var point = AlignmentToOffsetLogic((int)(window.ActualWidth*dpiRatio.X), 
					(int)(window.ActualHeight*dpiRatio.Y), screen.WorkingArea, alignment);
			
			if (winState == WindowState.Maximized)
				window.WindowState = WindowState.Maximized;

			return point;
		}
		/// <summary>
		/// Converts ScreenAlignment to Offset. alignment depends on the window and the screen
		/// </summary>
		/// <param name="hwnd">window handle</param>
		/// <param name="screen"></param>
		/// <param name="alignment"></param>
		/// <returns>offset</returns>
		public static Point? AlignmentToOffset(IntPtr hwnd, Screen screen, ScreenAlignment alignment)
		{
			if (screen == null) return null;
			WINDOWINFO winInfo = new WINDOWINFO();
			GetWindowInfo(hwnd, ref winInfo);

			int windowWidth = winInfo.Window.Right - winInfo.Window.Left;
			int windowHeight = winInfo.Window.Bottom - winInfo.Window.Top;
			
			var dpiRatio = getDPIRatio();
			if (1.45 < dpiRatio.X && dpiRatio.X < 1.55 &&
				1.45 < dpiRatio.Y && dpiRatio.Y < 1.55)
			{
				windowWidth = (int)((double)windowWidth*dpiRatio.X);
				windowHeight = (int)((double)windowHeight*dpiRatio.Y);
			}

			return AlignmentToOffsetLogic(windowWidth, windowHeight, screen.WorkingArea, alignment);
		}
		/// <summary>
		/// get the DPI of the system
		/// </summary>
		/// <returns>DPI of the system</returns>
		public static Point getDPI()
		{
			var point = new Point();
			using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
			{
				point.X = graphics.DpiX;
				point.Y = graphics.DpiY;
			}
			return point;
		}
		/// <summary>
		/// get the DPI ratio of the system
		/// </summary>
		/// <returns>DPI ratio of the system</returns>
		public static Point getDPIRatio()
		{
			var point = getDPI();
			point.X /= 96.0;
			point.Y /= 96.0;
			return point;
		}
		#endregion

		#region Send a window outside of the application domain
		/// <summary>
		/// Returns an array of window handles of all processes with the same name
		/// </summary>
		/// <param name="processName">the name of the process</param>
		/// <returns></returns>
		public static IntPtr[] getProcessWindowByName(string processName)
		{
			Process[] Processes = Process.GetProcessesByName(processName);
			return (from p in Processes
					select p.MainWindowHandle).ToArray<IntPtr>();
		}
		[DllImport("user32.dll")]
		static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

		/// <summary>
		/// Sends the main window of a program to screen
		/// </summary>
		/// <param name="progName">program name</param>
		/// <param name="screen">Screen to send the window to</param>
		/// <param name="align">Screen alignment</param>
		/// <returns>bool to indicate success or failure.</returns>
		public static bool SendProgramsMainWindowToScreen(string progName, Screen screen, ScreenAlignment align)
		{
			if (screen == null)
				return false;
			var hwnds = getProcessWindowByName(progName);
			bool success = false;
			foreach (var hwnd in hwnds)
				success |= SendToScreen(hwnd, screen, align);
			return success;
		}
		/// <summary>
		/// Sends all window of a program to a screen
		/// </summary>
		/// <param name="progName">program name</param>
		/// <param name="screen">screen to send the window to</param>
		/// <param name="align">Screen alignment</param>
		/// <returns>bool to indicate success or failure.</returns>
		public static bool SendProgramsWindowsToScreen(string progName, Screen screen, ScreenAlignment align)
		{
			if (screen == null)
				return false;
			var hwnds = getWindowHandlesFromProgramName(progName);
			if (hwnds.Length < 1)
				return false;
			var result = (from hwnd in hwnds
					select SendToScreen(hwnd, screen, align)).All(x => x == true);
			return result;
		}
		/// <summary>
		/// returns an array of window handles in a given screen
		/// </summary>
		/// <param name="screen">a screen</param>
		/// <returns>returns an array of window handles in given screen</returns>
		public static IntPtr[] getWindowHandlesFromScreen(Screen screen)
		{
			if (screen == null)
				return new IntPtr[] { };
			Process[] Processes = Process.GetProcesses();
			var plist = (from p in Processes
						 select EnumerateProcessWindowHandles(p.Id));

			return (from wlist in plist
					select (from hwnd in wlist
							where ScreenEquals(screen, getScreenFromWindow(hwnd))
							select hwnd).ToArray<IntPtr>()
					).SelectMany(i => i).ToArray<IntPtr>(); // flatten list
		}
		/// <summary>
		/// returns an array of window handles associated with a program name
		/// </summary>
		/// <param name="progName">program name</param>
		/// <returns></returns>
		public static IntPtr[] getWindowHandlesFromProgramName(string progName)
		{
			Process[] Processes = Process.GetProcessesByName(progName);
			var plist = (from p in Processes
						 select EnumerateProcessWindowHandles(p.Id));

			return (from wlist in plist
					select (from hwnd in wlist
							select hwnd).ToArray<IntPtr>()
					).SelectMany(i => i).ToArray<IntPtr>();
		}
		#endregion

		#endregion
		#region Private Utility Functions
		private static Screen getOtherScreen(Window window)
		{
			if (getScreenFromWindow(window).Primary)
				return SecondaryScreen;
			return PrimaryScreen;
		}
		private static Screen getOtherScreen(IntPtr hwnd)
		{
			if (getScreenFromWindow(hwnd).Primary)
				return SecondaryScreen;
			return PrimaryScreen;
		}
		private static bool Extend(bool extend)
		{
			if (CurrentTopology != DisplayConfigTopology.Extend)
			{
				if (!extend)
					return false;
				else if (!ExtendScreens())
					return false;
			}
			return true;
		}
		/// <summary>
		/// Converts ScreenAlignment to Offset. alignment depends on the window and the screen
		/// </summary>
		/// <param name="windowWidth">width of the window</param>
		/// <param name="windowHeight">height of the window</param>
		/// <param name="screen"></param>
		/// <param name="alignment"></param>
		/// <returns>offset</returns>
		private static Point AlignmentToOffsetLogic(int windowWidth, int windowHeight, System.Drawing.Rectangle workingArea, ScreenAlignment alignment)
		{
			var offsetX = 0.0;
			var offsetY = 0.0;

			// center
			if (alignment.HasFlag(ScreenAlignment.Center))
			{
				offsetX = (workingArea.Width / 2) - (windowWidth / 2);
				offsetY = (workingArea.Height / 2) - (windowHeight / 2);
			}
			// left & right
			if (alignment.HasFlag(ScreenAlignment.Left) && alignment.HasFlag(ScreenAlignment.Right))
				offsetX = (workingArea.Width / 2) - (windowWidth / 2);

			// only left
			else if (alignment.HasFlag(ScreenAlignment.Left))
				offsetX = 0;
			// only right
			else if (alignment.HasFlag(ScreenAlignment.Right))
				offsetX = workingArea.Width - windowWidth;

			// top & bottom
			if (alignment.HasFlag(ScreenAlignment.Top) && alignment.HasFlag(ScreenAlignment.Bottom))
				offsetY = (workingArea.Height / 2) - (windowHeight / 2);
			// only top
			else if (alignment.HasFlag(ScreenAlignment.Top))
				offsetY = 0;
			// only bottom
			else if (alignment.HasFlag(ScreenAlignment.Bottom))
				offsetY = workingArea.Height - windowHeight;

			return new Point(offsetX, offsetY);
		}
		private static bool ScreenEquals(Screen scr1, Screen scr2)
		{
			return scr1.DeviceName.Equals(scr2.DeviceName);
		}
		
		private delegate bool EnumThreadDelegate(IntPtr hwnd, IntPtr lParam);
		[DllImport("user32.dll")]
		static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
		private static List<IntPtr> EnumerateProcessWindowHandles(int processId)
		{
			var hwnds = new List<IntPtr>();

			foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
				EnumThreadWindows(thread.Id, (hwnd, lParam) => { hwnds.Add(hwnd); return true; }, IntPtr.Zero);

			return hwnds;
		}
		#endregion
	}
}
