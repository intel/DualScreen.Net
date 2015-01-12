/* MultiScreenLibrary.cs - Wraps dome native methods imported from DisplayHelperLib.dll & Win32 API
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

namespace DualScreenLibrary
{
	public class MultiScreenLibrary
	{
		internal const string DLL_PATH = "Intel.FlexServices.DisplayHelperLib.dll";

		#region Constants
		// The WM_GETMINMAXINFO message is sent to a window when the size or
		// position of the window is about to change.
		// An application can use this message to override the window's
		// default maximized size and position, or its default minimum or
		// maximum tracking size.
		public const int WM_GETMINMAXINFO = 0x0024;

		public const int WM_DISPLAYCHANGE = 0x007E;

		// Constants used with MonitorFromWindow()
		public enum MonitorDefaults : uint
		{
			MONITOR_DEFAULTTONULL = 0,      //Returns NULL.
			MONITOR_DEFAULTTOPRIMARY = 1,   //Returns a handle to the primary display monitor.
			MONITOR_DEFAULTTONEAREST = 2,   // Returns a handle to the display monitor that is nearest to the window.
		}
		#endregion

		#region Native Structs
		/// <summary>
		/// Native Windows API-compatible POINT struct
		/// </summary>
		[Serializable, StructLayout(LayoutKind.Sequential)]
		public struct POINT
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
		public struct RECT
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
		public struct MINMAXINFO
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
		public struct WINDOWINFO
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
		public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

		/// <summary>
		/// The MonitorFromWindow function retrieves a handle to the display monitor
		/// that has the largest area of intersection with the bounding rectangle
		/// of a specified window.
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/dd145064%28VS.85%29.aspx"/>
		/// </summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="dwFlags">Determines the function's return value
		/// ThreeDBorderWindowif the window does not intersect any display monitor.</param>
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

		/// <summary>
		/// Windows Forms const for Message on display resolution change.
		/// For more info: http://msdn.microsoft.com/en-us/library/windows/desktop/dd145210(v=vs.85).aspx
		/// </summary>
		#endregion

		#region Load Check functions
		private static Boolean? _isLoaded;

		/// <summary>
		/// Verify that the DisplayHelperLib DLL above was loded properly by calling one of the methods, hence forcing it's load and catching any error.
		/// </summary>
		/// <returns>False if loading DLL failed. True otherwise</returns>
		public static bool IsLoaded()
		{
			string ignoreMsg;
			return IsLoaded(out ignoreMsg);
		}

		/// <summary>
		/// Verify that the DisplayHelperLib DLL above was loded properly by calling one of the methods, hence forcing it's load and catching any error.
		/// </summary>
		/// <param name="errMsg"> outputs Error message into errMsg</param>
		/// <returns>False if loading DLL failed. True otherwise</returns>
		public static bool IsLoaded(out string errMsg)
		{
			errMsg = "";
			if (!_isLoaded.HasValue)
			{
				try
				{
					uint tmp = 0;
					GetCurrentTopology(ref tmp);
					_isLoaded = true;
				}
				catch (DllNotFoundException e)
				{
					errMsg = "Error Loading DisplayHelperLib DLL!\n";
					errMsg += "(Path: " + Environment.CurrentDirectory + "\\" + MultiScreenLibrary.DLL_PATH + ")";
					System.Diagnostics.Debug.WriteLine(errMsg);
					_isLoaded = false;
				}
				catch (Exception)
				{
					errMsg = "Unknown error on DisplayHelperLib DLL;\n Please contact support!";
					System.Diagnostics.Debug.WriteLine(errMsg);
					_isLoaded = false;
				}
			}
			return _isLoaded.Value;
		}
		#endregion

		#region Simple Wrappers
		/// <summary>
		/// Simple wrapper to the CPP Imported function. (Returns Int instead of our ref uint)
		/// </summary>
		/// <returns>0 if the syscall failed; otherwise - return DisplayConfigTopology of current state</returns>
		public static DisplayConfigTopology GetCurrentTopology()
		{
			uint curState = 0;
			GetCurrentTopology(ref curState);
			return (DisplayConfigTopology)curState;
		}


		/// <summary>
		/// Simple wrapper to the CPP Imported function. (Returns Int instead of our ref uint)
		/// </summary>
		/// <returns>true for success and false for failure</returns>
		public static bool SetCurrentTopology(DisplayConfigTopology newState)
		{
			uint uintState = (uint)newState;
			var result = SetCurrentTopology(ref uintState);
			return result;
		}

		/// <summary>
		/// Simple wrapper to the CPP Imported function. (Returns Int instead of our ref uint)
		/// </summary>
		/// <returns></returns>
		public static int GetNumberofDisplays()
		{
			uint numDisplays = 0;
			GetNumberofDisplays(ref numDisplays);
			return Convert.ToInt32(numDisplays);
		}

		public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
		{
			// Get the MINMAXINFO structure from memory location given by lParam
			MINMAXINFO mmi =
				(MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

			// Get the monitor that overlaps the window or the nearest
			IntPtr monitor = MonitorFromWindow(hwnd, (uint)MonitorDefaults.MONITOR_DEFAULTTONEAREST);
			if (monitor != IntPtr.Zero)
			{
				// Get monitor information
				MONITORINFO monitorInfo = new MONITORINFO();
				monitorInfo.Size = Marshal.SizeOf(typeof(MONITORINFO));
				GetMonitorInfo(monitor, ref monitorInfo);

				// The display monitor rectangle.
				// If the monitor is not the primary display monitor,
				// some of the rectangle's coordinates may be negative values
				RECT rcMonitorArea = monitorInfo.Monitor;

				// Get window information
				WINDOWINFO windowInfo = new WINDOWINFO();
				windowInfo.Size = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
				GetWindowInfo(hwnd, ref windowInfo);
				int borderWidth = (int)windowInfo.WindowBordersWidth;
				int borderHeight = (int)windowInfo.WindowBordersHeight;

				// Set the dimensions of the window in maximized state
				mmi.MaxPosition.X = -borderWidth;
				mmi.MaxPosition.Y = -borderHeight;
				mmi.MaxSize.X =
					rcMonitorArea.Right - rcMonitorArea.Left + 2 * borderWidth;
				mmi.MaxSize.Y =
					rcMonitorArea.Bottom - rcMonitorArea.Top + 2 * borderHeight;

				// Set minimum and maximum size
				// to the size of the window in maximized state
				mmi.MinTrackSize.X = mmi.MaxSize.X;
				mmi.MinTrackSize.Y = mmi.MaxSize.Y;
				mmi.MaxTrackSize.X = mmi.MaxSize.X;
				mmi.MaxTrackSize.Y = mmi.MaxSize.Y;
			}

			// Copy the structure to memory location specified by lParam.
			// This concludes processing of WM_GETMINMAXINFO.
			Marshal.StructureToPtr(mmi, lParam, true);
		}
		#endregion
	}
}
