API for DualScreenLibrary
=========================================

*   [Screen Manager](#scrman)
*   [Events](#dsleve)
*   [DSLExtensions](#dslext)

</article>
<article id="scrman">

# ScreenManager
```csharp
public static class ScreenManager
```

Defines a set of methods for managing windows on different screens.

### Enums

*   [DisplayConfigTopology](#discontop)
*   [ScreenAlignment](#scrali)

### Properties

*   [SecondaryScreen](#secscr)
*   [PrimaryScreen](#priscr)
*   [CurrentTopology](#curtop)
*   [NumberOfDisplays](#numofdis)

### Functions

*   [SendToPrimary()](#sentopri)
*   [SendToSecondary()](#sentosec)
*   [SwapScreen()](#swascr)
*   [SendToScreen()](#sentoscr)
*   [SendProgramToScreen()](#senprotoscr)
*   [ExtendScreens()](#extscr)
*   [CloneScreens()](#closcr)
*   [getScreenFromWindow()](#getscrfrowin)
*   [AlignmentToOffset()](#alitooff)
*   [getProcessWindowByName()](#getprowinbynam)
*   [DelayedActionOnWindow()](#delactonwin)

### Enums

#### DisplayConfigTopology
```csharp
enum DisplayConfigTopology
```

represents different display modes


*   Internal
*   Clone
*   Extend
*   External

#### ScreenAlignment
```csharp
enum ScreenAlignment
```

represents different screen alignments

*   None
*   Center
*   Left
*   Right
*   Top
*   Bottom

##### example:
```csharp
ScreenManager.SendToSecondary(myWindow, ScreenAlignment.Center | ScreenAlignment.Right);
```

### Properties

#### SecondaryScreen
```csharp
static Screen SecondaryScreen { get; }
```

will return the first screen which is not a primary screen or null if there is only one screen
		this will work assuming the first screen that is not the primary screen is the second screen.
		works only on extended mode

#### PrimaryScreen
```csharp
static Screen PrimaryScreen { get; }
```

will return the primary screen

#### CurrentTopology
```csharp
static DisplayConfigTopology CurrentTopology { get; set; }
```

DisplayConfigTopology of current state

##### example:
```csharp
ScreenManager.CurrentTopology = ScreenManager.DisplayConfigTopology.External;
```

#### NumberOfDisplays
```csharp
static uint NumberOfDisplays
```

The number of displays, even on clone or internal/external

### Functions

#### SendToPrimary()
```csharp
static bool SendToPrimary(Window window, ScreenAlignment align, bool extend = false)
static bool SendToPrimary(Window window, int offsetX = 0, int offsetY = 0, bool extend = false)
```

Sends a window to the primary screen

##### Parameters:

- Window window
>to send to the primary screen
- ScreenAlignment align / int offsetX+offsetY
>Screen alignment / offset
- bool extend
>change screen topology to extended. if current topology is not on extend the function will fail

##### Return value

indicates success and failure

##### example:
```csharp
ScreenManager.SendToPrimary(myWindow, ScreenAlignment.Center | ScreenAlignment.Right);
```

#### SendToSecondary()
```csharp
static bool SendToSecondary(Window window, ScreenAlignment align, bool extend = false)
static bool SendToSecondary(Window window, int offsetX = 0, int offsetY = 0, bool extend = false)</pre>
```

Sends a window to the secondary screen

##### Parameters:

- Window window
> to send to the secondary screen
- ScreenAlignment align / int offsetX+offsetY
> Screen alignment / offset
- bool extend
> change screen topology to extended. if current topology is not on extend the function will fail

##### Return value

indicates success and failure

##### example:
```csharp
ScreenManager.SendToSecondary(myWindow, ScreenAlignment.Center | ScreenAlignment.Right);
```

#### SwapScreen()
```csharp
static bool SwapScreen(Window window, ScreenAlignment align, bool extend = false)
static bool SwapScreen(Window window, int offsetX = 0, int offsetY = 0, bool extend = false)
```

Sends a window to from primary screen to secondary or from secondary screen to primary screen.

##### Parameters:

- Window window
> to send
- ScreenAlignment align / int offsetX+offsetY
> Screen alignment / offset
- bool extend
> change screen topology to extended. if current topology is not on extend the function will fail

##### Return value

indicates success and failure. failure will denote when there aren't enough displays or the current screen topology is not extend and the extend mark is set to false

##### example:
```csharp
ScreenManager.SwapScreen(myWindow, ScreenAlignment.Bottom);
```

#### SendToScreen()
```csharp
static bool SendToScreen(Window window, Screen screen, ScreenAlignment align)
static bool SendToScreen(Window window, Screen screen, int offsetX = 0, int offsetY = 0)
static bool SendToScreen(IntPtr hwnd, Screen screen, ScreenAlignment align)
static bool SendToScreen(IntPtr hwnd, Screen screen, int offsetX = 0, int offsetY = 0)
```

Sends a window to a screen.

##### Parameters:

- Window window/IntPtr hwnd
> window/window handle to send
- Screen screen
> to send to
- ScreenAlignment align / int offsetX+offsetY
> Screen alignment / offset

##### Return value

indicates success and failure.

##### example:
```csharp
ScreenManager.SendToScreen(myWindow, ScreenManager.SecondaryScreen, ScreenAlignment.Left);
```

#### SendProgramToScreen()
```csharp
static bool SendProgramToScreen(string progName, Screen screen, ScreenAlignment align)
```

Sends the main window of a program to a screen.

##### Parameters:

- String progName
> the name of the program
- Screen screen
> to send to
- ScreenAlignment align / int offsetX+offsetY
> Screen alignment / offset

##### Return value

indicates success and failure.

##### example:
```csharp
ScreenManager.SendProgramToScreen("OUTLOOK", ScreenManager.SecondaryScreen, ScreenAlignment.Top);
```	

#### ExtendScreens()
```csharp
static bool ExtendScreens()
```

Changes CurrentTopology to DisplayConfigTopology.Extend

##### Return value

indicates success and failure.

#### CloneScreens()
```csharp
static bool CloneScreens()
```

Changes CurrentTopology to DisplayConfigTopology.Clone

##### Return value

indicates success and failure.

#### getScreenFromWindow()
```csharp
static Screen getScreenFromWindow(Window window)
```

Retrieves a Screen for the display that contains the largest portion of the window

##### Parameters:

- Window window
> to get screen from

##### Return values

The Screen for the display that contains the largest portion of the window

#### getProcessWindowByName()
```csharp
static IntPtr[] getProcessWindowByName(string proccessName)
```

Returns an array of window handles of all processes with the name [processName]

##### Parameters:

- string processName
> The name of the process

##### Return values

an array of window handles of all processes with the name [processName]

#### DelayedActionOnWindow()
```csharp
public static void DelayedActionOnWindow(Window window, int delay, Action action)
```

Creates a thread, sleeps for [delay] miliseconds and runs an action in window's thread. useful when wanting to move a window from one screen to another screen when subscribing to DSLEvents.SecondaryScreenAvailableChanged event.

##### [ example:](#secscrava)

</article>
<article id="dsleve">

## DSLEvents
```csharp
public class DSLEvents : IDisposable
```

DSLEvents contains a single event (SecondaryScreenAvailable) you can subscribe to which is raised when the secondary screen becomes available or not available</summary>

### Constructor and Dispose

*   [DSLEvents()](#dsleve())
*   [Dispose()](#dsldis)

### Events

*   [SecondaryScreenAvailableChanged](#secscrava)

### Event Args

*   [SAvailableEventArgs](#savaeveargs)

#### DSLEvents()
```csharp
public DSLEvents()
```
Subscribed to system's DisplaySettingsChanged. DisplaySettingsChanged is a static event so we must detach our event handler when the object is destoryed or dispose it

#### Dispose()
```csharp
public void Dispose()
```

Detaches from system's DisplaySettingsChanged


#### SecondaryScreenAvailableChanged
```csharp
public event EventHandler<SAvailableEventArgs> SecondaryScreenAvailableChanged
```

Notifies when the secondary screen's availability is changed.

In order to use this event to move the window to a different screen you should use [ScreenManager.DelayedActionOnWindow()](#delactonwin) function.


##### example:
```csharp

events.SecondaryScreenAvailableChanged += ((sender, e) =>
{
	if (e.isAvailable)
		ScreenManager.DelayedActionOnWindow(this, 500,
			() => this.SendToSecondary(ScreenManager.ScreenAlignment.Center | ScreenManager.ScreenAlignment.Left));
});
```

#### SAvailableEventArgs
```csharp
public class SAvailableEventArgs
```

Event arguments for SecondaryScreenAvailableChanged event

##### contains:
```csharp
public bool isAvailable { get; private set; }
```

</article>
<article id="dslext">

### DSLExtensions
```csharp
public static class DSLExtensions
```

Extensions for WPF windows

#### Functions

*   SendToPrimary()
*   SendToSecondary()
*   SendToScreen()
*   SwapScreen()
*   getScreen()
</article>