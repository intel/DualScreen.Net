using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualScreenLibrary
{
	public class DSLEvents
	{
		public bool SecondaryScreenAvailable { get; private set; }
		#region Constructor and Dispose
		public DSLEvents()
		{
			// DisplaySettingsChanged is a static event so we must detach our event handler when the object is destoryed
			SystemEvents.DisplaySettingsChanged += OnRaiseDisplaySettingsChanged;
			SecondaryScreenAvailable = (ScreenManager.SecondaryScreen != null);
		}

		public void Dispose()
		{
			SystemEvents.DisplaySettingsChanged -= OnRaiseDisplaySettingsChanged;
			GC.SuppressFinalize(this);
		}
		#endregion
		#region Events
		/// <summary>
		/// <para>Notifies when the secondary screen's availability is changed.<br/>
		/// In order to use this event to move the window to a different screen you should use DelayedActionOnWindow.</para>
		/// for example: events.SecondaryScreenAvailableChanged += ((sender, e) =>
		///{
		///	if (e.isAvailable)
		///		new DelayedActionOnWindow(this, 
		///			() => this.SendToSecondary(ScreenManager.ScreenAlignment.Center | ScreenManager.ScreenAlignment.Left)).Invoke();
		///});
		///</code>
		/// </summary>
		public event EventHandler<SAvailableEventArgs> SecondaryScreenAvailableChanged;

		private void OnRaiseDisplaySettingsChanged(object sender, EventArgs e)
		{
			// change if changed
			if (SecondaryScreenAvailable && ScreenManager.SecondaryScreen == null) // true -> false
				SecondaryScreenAvailable = false;
			else if (!SecondaryScreenAvailable && ScreenManager.SecondaryScreen != null) // false -> true
				SecondaryScreenAvailable = true;
			else return; // false -> false || true -> true : no change
			
			// fire event
			var isavail = new SAvailableEventArgs(SecondaryScreenAvailable);
			EventHandler<SAvailableEventArgs> handler = SecondaryScreenAvailableChanged;
			if (handler != null)
				handler(this, isavail);
		}

		public class SAvailableEventArgs
		{
			public bool isAvailable { get; private set; }
			public SAvailableEventArgs(bool isAvailable)
			{
				this.isAvailable = isAvailable;
			}
		}
		#endregion
	}
}
