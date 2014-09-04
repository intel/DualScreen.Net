// DisplayHelperLib.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "windef.h"
#include "stdlib.h"
#include "malloc.h"
//#include "stdio.h"  //for debugging purpose only


extern
    "C"
__declspec(dllexport) void  GetCurrentTopology(DISPLAYCONFIG_TOPOLOGY_ID *peCurrentTopology) {

      // Get the current topology - make sure that we aren't resetting the topology to the same that it
      // currently is - otherwise it can cause unnecessary flashing on the screen.
      LONG result;
      UINT32 NumPathArrayElements;
      UINT32 NumModeInfoArrayElements;
      DISPLAYCONFIG_PATH_INFO *pPathInfoArray;
      DISPLAYCONFIG_MODE_INFO *pModeInfoArray;
      result = GetDisplayConfigBufferSizes(QDC_DATABASE_CURRENT, &NumPathArrayElements, &NumModeInfoArrayElements);

      if (result != ERROR_SUCCESS)
            return;

      if (peCurrentTopology == NULL)
            return;

      // The only reason to allocate these structures is that the documentation says that QueryDisplayConfig
      // cannot take null for the pPathInfoArray and pModeInfoArray  elements.  I have tried to have static
      // structures that get passed in with only one element and have had things fail in the past
      // The only way that I've gotten this interface to reliably work is to call GetDisplayConfigBufferSizes
      // and then allocate AND ZERO the structures that are passed in.  This interface is incredibly
      // fragile and you are going to want to do everything that it expects, or it will return
      // "invalid parameter"
      pPathInfoArray = (DISPLAYCONFIG_PATH_INFO *)malloc(NumPathArrayElements * sizeof(DISPLAYCONFIG_PATH_INFO));
      if (!pPathInfoArray)
            return;
      pModeInfoArray = (DISPLAYCONFIG_MODE_INFO *)malloc(NumModeInfoArrayElements * sizeof(DISPLAYCONFIG_MODE_INFO));
      if (!pModeInfoArray)
      {
            if (pPathInfoArray)
                  free(pPathInfoArray);
            return;
      }
      memset(pPathInfoArray, 0, NumPathArrayElements * sizeof(DISPLAYCONFIG_PATH_INFO));
      memset(pModeInfoArray, 0, NumModeInfoArrayElements * sizeof(DISPLAYCONFIG_MODE_INFO));

      result = QueryDisplayConfig(QDC_DATABASE_CURRENT,
                                                &NumPathArrayElements,
                                                pPathInfoArray,
                                                &NumModeInfoArrayElements,
                                                pModeInfoArray,
                                                peCurrentTopology);

      // Free the structures, don't care what is in them
      free(pPathInfoArray);
      free(pModeInfoArray);
      if (result != ERROR_SUCCESS)
            return;
      return;
}



extern
    "C"
__declspec(dllexport) bool SetCurrentTopology(DISPLAYCONFIG_TOPOLOGY_ID *pDesiredTopology) {
	  
	//printf("*** DisplayHelperLib: Applying Disply State: 0x%.4x  ***\n",*pDesiredTopology);   ### Debugging
	LONG result = SetDisplayConfig(0, NULL,0, NULL,(SDC_APPLY | *pDesiredTopology));
	if (result != ERROR_SUCCESS)
		  return false;
	  return true;
}



// Get the number of displays that are currently available on the system - eliminate non-active or 
// mirroring adapters.  
extern
    "C"
__declspec(dllexport) void GetNumberofDisplays(UINT *NumDisplays)
{
    *NumDisplays = 0;
    BOOL retval;
    DEVMODE StoredDevMode;
    DISPLAY_DEVICE TempDisplayDeviceStruct;
    LPCWSTR AdapterName;
    DWORD count = 0;
    DWORD modecount = 0;

    TempDisplayDeviceStruct.cb = sizeof(DISPLAY_DEVICE);
    retval = EnumDisplayDevices(NULL, count, &TempDisplayDeviceStruct, EDD_GET_DEVICE_INTERFACE_NAME);
    while (retval)
    {
        if (TempDisplayDeviceStruct.StateFlags != DISPLAY_DEVICE_MIRRORING_DRIVER)
        {
            // A mirroring driver is just a pseudo display device that is used internally want to ignore them
            
            // The current display device is at least known to the system.  However, if you pull the plug on a 
            // device, it will still have a DisplayDeviceStruct.  But it seems that it has no modes 
            // associated with it when it cannot be accessed.  So, before we know that the device is really available
            // make sure that there are some modes that are available.
            modecount = 0;
            AdapterName = (WCHAR *)&TempDisplayDeviceStruct.DeviceName;
            StoredDevMode.dmSize = sizeof(DEVMODE);
            StoredDevMode.dmDriverExtra = 0;
            retval = EnumDisplaySettings(AdapterName, modecount, (DEVMODE *)&StoredDevMode);
            // see if there are at least a couple of modes for this display device  
            while (retval && modecount <= 2)
            {
                modecount++;
                retval = EnumDisplaySettings(AdapterName, modecount, (DEVMODE *)&StoredDevMode);
            }
            // If we exited the above loop and were still getting good data, then we have found
            // a viable display device
            if (retval)
                (*NumDisplays)++;
        }

        // Get the next display device
        count++;
        TempDisplayDeviceStruct.cb = sizeof(DISPLAY_DEVICE);
        retval = EnumDisplayDevices(NULL, count, &TempDisplayDeviceStruct, 0);
    }

    return;
}


