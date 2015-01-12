/* DelayedActionOnWindow.cs : Defines a delayed action on window.
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace DualScreenLibrary
{
	/// <summary>
	/// will invoke [action] in [window]'s thread when the application will be idle. 
	/// useful when wanting to move a window from one screen to another screen when subscribing to 
	/// DSLEvents.SecondaryScreenAvailableChanged event.
	/// </summary>
	public class DelayedActionOnWindow
	{
		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(int nIndex);

		
		#region Private Fields
		private Window window;
		private Action action;
		#endregion
		#region Properties
		public object Result { get; private set; }
		#endregion
		#region Public Constructor
		/// <summary>
		/// creates an instance that can be invoked using Invoke() method 
		/// which will invoke [action] in [window]'s thread when the application will be idle. 
		/// useful when wanting to move a window from one screen to another screen when subscribing to 
		/// DSLEvents.SecondaryScreenAvailableChanged event.
		/// <param name="window"></param>
		/// <param name="action">action to invoke</param>
		/// </summary>
		public DelayedActionOnWindow(Window window, Action action)
		{
			this.window = window;
			this.action = action;
		}
		#endregion
		#region Public Methods
		/// <summary>
		/// Invokes the delayed action
		/// </summary>
		public void Invoke()
		{
			new Thread(() =>
			{
				Result = window.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, action);
			}).Start();
		}
		#endregion
	}
}
